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

            AuthorizationResult authorization;
            using (var agentClient = new AgentClient())
            {
                authorization = agentClient.AuthorizeAsync().GetAwaiter().GetResult();
            }

            if (authorization.Status != AuthorizationStatus.Allowed)
            {
                MessageBox.Show(
                    authorization.Message,
                    "AIRSTACK Licensing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Application.Run(new Form1());
        }
    }
}
