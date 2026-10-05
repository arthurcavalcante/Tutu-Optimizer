using System;
using System.IO;
using System.Xml.Serialization;

namespace TutusOptimizer
{
    public static class SettingsStore
    {
        private static string SettingsPath
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TutusOptimizer", "settings.xml"); }
        }

        public static AppSettings Load(bool defaultDark)
        {
            AppSettings settings = null;
            try
            {
                using (FileStream stream = File.OpenRead(SettingsPath))
                    settings = (AppSettings)new XmlSerializer(typeof(AppSettings)).Deserialize(stream);
            }
            catch (IOException) { }
            catch (InvalidOperationException) { }
            catch (UnauthorizedAccessException) { }
            if (settings == null) settings = new AppSettings { DarkMode = defaultDark };
            if (settings.RefreshIntervalMs != 1000 && settings.RefreshIntervalMs != 2000 && settings.RefreshIntervalMs != 5000)
                settings.RefreshIntervalMs = 2000;
            try { System.Drawing.ColorTranslator.FromHtml(settings.AccentHex); }
            catch { settings.AccentHex = "#6366F1"; }
            if (string.IsNullOrEmpty(settings.AccentHex)) settings.AccentHex = "#6366F1";
            return settings;
        }

        public static void Save(AppSettings settings)
        {
            string path = SettingsPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (FileStream stream = File.Create(temporary))
                    new XmlSerializer(typeof(AppSettings)).Serialize(stream, settings);
                if (File.Exists(path)) File.Replace(temporary, path, null);
                else File.Move(temporary, path);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
    }
}
