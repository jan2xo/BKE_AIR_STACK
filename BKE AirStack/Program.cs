using System;
using System.Threading;
using System.Windows.Forms;
using static BKE_Air_Stack.Form1;

namespace BKE_Air_Stack
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);
            using var single = new Mutex(initiallyOwned: true, name: @"Global\BKE_AirStack_SINGLE_INSTANCE", out bool isNew);
            if (!isNew) return; // already running → exit quietly
            if (ExpiryLite.TryHandleCli()) return;     // optional owner shortcut
            ExpiryLite.InitializeOrCreateTrial(trialDays: 0);  // or 0 if you’ll set via CLI
            Application.Run(new Form1());
        }
    }
}
