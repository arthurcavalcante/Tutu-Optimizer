using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace TutusOptimizer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Garantia de Elevação como Administrador
            if (!IsAdministrator())
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = Application.ExecutablePath;
                    psi.UseShellExecute = true;
                    psi.Verb = "runas"; // Força solicitação UAC
                    Process.Start(psi);
                    return; // Encerra instância não-elevada
                }
                catch
                {
                    MessageBox.Show(
                        "O Tutu's Optimizer precisa ser executado com privilégios de Administrador para aplicar as alterações no Windows.",
                        "Permissão Necessária",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static bool IsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }
}
