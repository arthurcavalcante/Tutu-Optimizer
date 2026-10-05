using System;
using System.Text.RegularExpressions;
namespace TutusOptimizer
{
    // NEW: Regex-powered input validator utility class
    public static class InputValidator
    {
        // Validate CPF format (###.###.###-##)
        public static bool ValidateCpfFormat(string cpf)
        {
            if (string.IsNullOrEmpty(cpf)) return false;
            return Regex.IsMatch(cpf, @"^\d{3}\.\d{3}\.\d{3}-\d{2}$");
        }
        
        // Validate IP address format
        public static bool ValidateIpAddress(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return false;
            return Regex.IsMatch(ip, @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
        }
        
        // Validate DNS address
        public static bool ValidateDnsAddress(string dns)
        {
            return ValidateIpAddress(dns);
        }
        
        // Validate executable filename (something.exe)
        public static bool ValidateExeFileName(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return false;
            return Regex.IsMatch(filename, @"^[a-zA-Z0-9_\-\. ]+\.exe$", RegexOptions.IgnoreCase);
        }
        
        // Validate Windows registry path
        public static bool ValidateRegistryPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return Regex.IsMatch(path, @"^(HKEY_LOCAL_MACHINE|HKLM|HKEY_CURRENT_USER|HKCU|HKEY_CLASSES_ROOT|HKCR|HKEY_USERS|HKU|HKEY_CURRENT_CONFIG|HKCC)\\[a-zA-Z0-9_\-\.\\]+$", RegexOptions.IgnoreCase);
        }
        
        // Validate Windows service name
        public static bool ValidateServiceName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return Regex.IsMatch(name, @"^[a-zA-Z][a-zA-Z0-9_]*$");
        }
        
        // Sanitize string for log output (remove control characters)
        public static string SanitizeLogOutput(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return Regex.Replace(input, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");
        }
        
        // Extract version number from string
        public static string ExtractVersion(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            Match match = Regex.Match(input, @"\d+\.\d+(?:\.\d+)*");
            if (match.Success)
            {
                return match.Value;
            }
            return string.Empty;
        }
        
        // Validate game name (no special dangerous chars)
        public static bool ValidateGameName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return Regex.IsMatch(name, @"^[a-zA-Z0-9_\-\. ]+$");
        }
        
        // Parse temperature value from string (e.g. "42 °C" -> 42)
        public static int ParseTemperature(string tempStr)
        {
            if (string.IsNullOrEmpty(tempStr)) return 0;
            Match match = Regex.Match(tempStr, @"(-?\d+)");
            if (match.Success)
            {
                int temp;
                if (int.TryParse(match.Value, out temp))
                {
                    return temp;
                }
            }
            return 0;
        }
    }

}
