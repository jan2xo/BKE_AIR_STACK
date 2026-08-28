using System;
using BKE_Air_Stack.Licensing;
using System.Threading;
using System.Windows.Forms;

namespace BKE_Air_Stack
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);

            using var single = new Mutex(
                initiallyOwned: true,
                name: @"Global\BKE_AirStack_SINGLE_INSTANCE",
                out bool isNew);

            if (!isNew)
            {
                return;
            }

            bool graceActive;
            using (var gracePeriodClient = new GracePeriodClient())
            {
                graceActive = gracePeriodClient.IsActiveAsync().GetAwaiter().GetResult();
            }

            if (!graceActive && !AuthorizeOrRecover())
            {
                return;
            }

            var mainForm = new Form1();
            AddRenderDockModule(mainForm);
            AgentUpdateCoordinator.Attach(mainForm);
            Application.Run(mainForm);
        }

        private static bool AuthorizeOrRecover()
        {
            using var agentClient = new AgentClient();
            var authorization = agentClient.EnsureAuthorizedAsync().GetAwaiter().GetResult();

            if (authorization.Status == AuthorizationStatus.Cancelled)
            {
                return false;
            }

            if (authorization.Status == AuthorizationStatus.AgentUnavailable)
            {
                AgentRecoveryDialog.ShowRecovery();
                return false;
            }

            if (authorization.Status != AuthorizationStatus.Allowed)
            {
                MessageBox.Show(
                    authorization.Message,
                    "Air Stack Licensing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private static void AddRenderDockModule(Form mainForm)
        {
            var button = new Button
            {
                Text = "Render Dock",
                Width = 120,
                Height = 34,
                Top = 12,
                Left = Math.Max(12, mainForm.ClientSize.Width - 132),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TabStop = false
            };

            button.Click += async (_, __) =>
            {
                button.Enabled = false;
                try
                {
                    var result = await new ModuleLaunchClient().LaunchRenderDockAsync();
                    if (!result.Launched)
                    {
                        MessageBox.Show(
                            result.Message,
                            "Air Stack — Render Dock",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
                finally
                {
                    button.Enabled = true;
                }
            };

            mainForm.Controls.Add(button);
            button.BringToFront();
        }
    }
}
