using System;
using System.Drawing;
using System.Windows.Forms;

namespace TutusOptimizer
{
    public partial class MainForm
    {
        private bool forceExit;

        private static Color ContrastText(Color background)
        {
            double brightness = background.R * 0.299 + background.G * 0.587 + background.B * 0.114;
            return brightness >= 155 ? Color.FromArgb(15, 23, 42) : Color.White;
        }

        private void ApplySavedAccent()
        {
            Color accent = ColorTranslator.FromHtml(settings.AccentHex);
            theme.Accent = accent;
            theme.AccentHover = Color.FromArgb(Math.Max(0, accent.R - 20), Math.Max(0, accent.G - 20), Math.Max(0, accent.B - 20));
            theme.NavActiveText = accent;
        }

        private bool CanChangeAppearance()
        {
            if (diagnosticCancellation == null && standaloneOperations == 0 && (worker == null || !worker.IsBusy)) return true;
            MessageBox.Show(this, "Aguarde a operação atual terminar ou cancele o teste na página de benchmark.", "Operação em andamento", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        private void SavePreferences()
        {
            try { SettingsStore.Save(settings); }
            catch (Exception ex) { MessageBox.Show(this, "Não foi possível salvar as preferências: " + ex.Message, "Preferências", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
    }
}
