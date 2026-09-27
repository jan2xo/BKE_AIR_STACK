using BKE.Desktop.Licensing;
using BKE_Air_Stack.Licensing;
using BKE_Air_Stack.Notifications;
using BKE_Air_Stack.Updates;
using System;
using System.Threading;
using System.Windows.Forms;

namespace BKE_Air_Stack
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            using var single = new Mutex(
                initiallyOwned: true,
                name: @"Global\BKE_AirStack_SINGLE_INSTANCE",
                out bool isNew);

            if (!isNew)
            {
                return;
            }

            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);

            // Product broadcasts are informational and intentionally run before the
            // licensing gate so beta/trial/support-end notices remain visible even
            // when licensing subsequently denies application startup.
            NotificationCoordinator.ShowBeforeLicensing();

            bool graceActive;
            using (var gracePeriodClient = new GracePeriodClient())
            {
                graceActive = gracePeriodClient.IsActiveAsync().GetAwaiter().GetResult();
            }

            if (!graceActive)
            {
                AuthorizationResult authorization;
                using (var agentClient = new AgentClient())
                {
                    authorization = agentClient.EnsureAuthorizedAsync().GetAwaiter().GetResult();
                }

                if (authorization.Status == AuthorizationStatus.ActivationCancelled)
                {
                    return;
                }

                if (authorization.Status is AuthorizationStatus.AgentUnavailable or AuthorizationStatus.Timeout)
                {
                    AgentRecoveryDialog.ShowRecovery();
                    return;
                }

                if (authorization.Status != AuthorizationStatus.Authorized)
                {
                    MessageBox.Show(
                        "Air Stack could not establish a valid license for this installation. " +
                        "Use BKE License Center to activate or repair licensing.",
                        "Air Stack Licensing",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }

            var mainForm = new Form1();
            AddRenderDockModule(mainForm);
            UpdateCoordinator.Attach(mainForm);
            NotificationCoordinator.Attach(mainForm);
            NotificationTrayController.Attach(mainForm);
            Application.Run(mainForm);
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
