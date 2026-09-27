using BKE.Notifications;
using BKE_Air_Stack.Licensing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BKE_Air_Stack.Notifications
{
    internal static class NotificationCoordinator
    {
        private const string ProductId = "bke-air-stack";
        private static readonly object ShownGate = new();
        private static readonly HashSet<string> ShownThisProcess = new(StringComparer.Ordinal);

        internal static void ShowBeforeLicensing()
        {
            ShowUnreadAsync(owner: null).GetAwaiter().GetResult();
        }

        internal static void Attach(Form form)
        {
            ArgumentNullException.ThrowIfNull(form);
            form.Shown += async (_, __) =>
                await ShowUnreadAsync(form).ConfigureAwait(true);
        }

        private static async Task ShowUnreadAsync(IWin32Window? owner)
        {
            try
            {
                using var client = BkeNotificationInboxClient.Create(
                    ProductId,
                    CurrentVersion(),
                    InstallationIdentity.GetOrCreate());

                var feed = await client.GetFeedAsync(
                    new NotificationFeedQuery(limit: 10, includeDismissed: false)).ConfigureAwait(true);

                if (!feed.Succeeded)
                {
                    Debug.WriteLine(
                        $"Air Stack notification inbox failed: {feed.Error?.Code}: {feed.Error?.Message}");
                    return;
                }

                foreach (var item in feed.Items)
                {
                    if (item.State != NotificationState.Unread)
                    {
                        continue;
                    }

                    if (owner is Form form && form.IsDisposed)
                    {
                        return;
                    }

                    lock (ShownGate)
                    {
                        if (!ShownThisProcess.Add(item.Id))
                        {
                            continue;
                        }
                    }

                    if (owner is null)
                    {
                        MessageBox.Show(
                            item.Body,
                            item.Title,
                            MessageBoxButtons.OK,
                            IconFor(item.Severity));
                    }
                    else
                    {
                        MessageBox.Show(
                            owner,
                            item.Body,
                            item.Title,
                            MessageBoxButtons.OK,
                            IconFor(item.Severity));
                    }

                    var markRead = await client.MarkReadAsync(item.Id).ConfigureAwait(true);
                    if (markRead.Status != NotificationOperationStatus.Succeeded)
                    {
                        Debug.WriteLine(
                            $"Air Stack notification {item.Id} could not be marked read: " +
                            $"{markRead.Status}: {markRead.Error?.Code}: {markRead.Error?.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Notifications are informational and must never block or terminate Air Stack.
                Debug.WriteLine($"Air Stack notification inbox did not complete: {ex.Message}");
            }
        }

        private static string CurrentVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version == null
                ? "0.0.0"
                : $"{version.Major}.{version.Minor}.{version.Build}";
        }

        private static MessageBoxIcon IconFor(NotificationSeverity severity) =>
            severity switch
            {
                NotificationSeverity.Error => MessageBoxIcon.Error,
                NotificationSeverity.Warning => MessageBoxIcon.Warning,
                NotificationSeverity.Success => MessageBoxIcon.Information,
                _ => MessageBoxIcon.Information
            };
    }
}
