using Xunit;

namespace AirStack.Tests;

public sealed class NotificationTrayContractTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Fact]
    public void StartupPopupRemainsProcessDeduplicatedAndAgentBacked()
    {
        var coordinator = Read("BKE AirStack", "Notifications", "NotificationCoordinator.cs");

        Assert.Contains("ProductId = \"bke-air-stack\"", coordinator);
        Assert.Contains("BkeNotificationInboxClient.Create", coordinator);
        Assert.Contains("ShownThisProcess", coordinator);
        Assert.Contains("NotificationState.Unread", coordinator);
        Assert.Contains("MarkReadAsync(item.Id)", coordinator);
        Assert.Contains("Notifications are informational and must never block", coordinator);
        Assert.DoesNotContain("HttpClient", coordinator, StringComparison.Ordinal);
        Assert.DoesNotContain("127.0.0.1:43873", coordinator, StringComparison.Ordinal);
        Assert.DoesNotContain("/v1/notifications/", coordinator, StringComparison.Ordinal);
    }

    [Fact]
    public void TrayUsesFeedUnreadAndLifecycleContractsWithoutOwningCadence()
    {
        var tray = Read("BKE AirStack", "Notifications", "NotificationTrayController.cs");
        var program = Read("BKE AirStack", "Program.cs");

        Assert.Contains("NotificationTrayController.Attach(mainForm)", program);
        Assert.Contains("ProductId = \"bke-air-stack\"", tray);
        Assert.Contains("BkeNotificationInboxClient.Create", tray);
        Assert.Contains("GetUnreadCountAsync", tray);
        Assert.Contains("GetFeedAsync", tray);
        Assert.Contains("includeDismissed: false", tray);
        Assert.Contains("NotificationState.Unread", tray);
        Assert.Contains("MarkReadAsync", tray);
        Assert.Contains("DismissAsync", tray);
        Assert.Contains("Notifications are temporarily unavailable.", tray);
        Assert.DoesNotContain("ShownThisProcess", tray, StringComparison.Ordinal);
        Assert.DoesNotContain("HttpClient", tray, StringComparison.Ordinal);
        Assert.DoesNotContain("127.0.0.1:43873", tray, StringComparison.Ordinal);
        Assert.DoesNotContain("/v1/notifications/", tray, StringComparison.Ordinal);
        Assert.DoesNotContain("EVERY_LAUNCH", tray, StringComparison.OrdinalIgnoreCase);
    }

    private static string Read(params string[] path) =>
        File.ReadAllText(Path.Combine(new[] { RepositoryRoot }.Concat(path).ToArray()));

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "BKE_AIR_STACK.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Air Stack repository root.");
    }
}
