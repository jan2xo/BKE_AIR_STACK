using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BKE_Air_Stack.Licensing
{
    internal static class AgentUpdateCoordinator
    {
        private static readonly string[] EnterpriseProducts = { "bke-air-stack", "bke-render-dock" };

        internal static void Attach(Form form)
        {
            form.Shown += async (_, __) => await CheckAfterShownAsync(form);
        }

        private static async Task CheckAfterShownAsync(Form form)
        {
            var client = new AgentUpdateClient();
            form.FormClosed += (_, __) => client.Dispose();
            var statuses = new List<UpdateStatus>();
            foreach (var product in EnterpriseProducts)
            {
                var status = await client.StatusAsync(product);
                if (status == null) continue;
                if (status.State == "never_checked")
                {
                    await client.QueueRefreshAsync(status);
                    continue;
                }
                if (status.Available) statuses.Add(status);
            }
            if (statuses.Count == 0 || form.IsDisposed) return;
            form.BeginInvoke(new Action(() => ShowBanner(form, client, statuses)));
        }

        private static void ShowBanner(Form form, AgentUpdateClient client, IReadOnlyList<UpdateStatus> statuses)
        {
            var banner = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.FromArgb(28, 54, 92), Padding = new Padding(12, 7, 12, 7) };
            var names = string.Join(" and ", statuses.Select(item => item.ProductId == "bke-air-stack" ? "Air Stack" : "Render Dock"));
            var label = new Label { AutoSize = true, ForeColor = Color.White, Text = $"Updates are available for {names}.", Top = 13, Left = 12 };
            var later = new Button { Text = "Later", Width = 72, Height = 30, Dock = DockStyle.Right };
            var update = new Button { Text = "Update", Width = 82, Height = 30, Dock = DockStyle.Right };
            later.Click += (_, __) => { form.Controls.Remove(banner); banner.Dispose(); client.Dispose(); };
            update.Click += async (_, __) =>
            {
                update.Enabled = false;
                try
                {
                    foreach (var status in statuses)
                    {
                        var result = await client.OpenUpdateCenterAsync(status);
                        MessageBox.Show(result, "BKE Update Center", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("The Licensing Agent Update Center is unavailable.", "BKE Update Center", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                finally { if (!update.IsDisposed) update.Enabled = true; }
            };
            banner.Controls.Add(later); banner.Controls.Add(update); banner.Controls.Add(label);
            form.Controls.Add(banner); banner.BringToFront();
        }
    }
}
