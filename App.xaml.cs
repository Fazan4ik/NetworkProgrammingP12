using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace NetworkProgrammingP12
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static String configFilename = "email-settings.json";
        private static JsonElement? settings = null;
        public static String? GetConfiguration(String name)
        {
            if (settings == null)
            {
                if (!File.Exists(configFilename))
                {
                    MessageBox.Show(
                        $"Файл конфігурації '{configFilename}' не знайдено",
                        "Операція не може бути завершена",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return null;
                }
            }

            try
            {
                settings ??= JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(configFilename));
            }
            catch
            {
                MessageBox.Show(
                        $"Файл конфігурації '{configFilename}' не підходить або зломан.",
                        "Операція не може бути завершена",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                return null;
            }

            JsonElement? jsonElement = settings;
            try
            {
                foreach (String key in name.Split(':'))
                {
                    jsonElement = jsonElement?.GetProperty(key);
                }
            }
            catch
            {
                return null;
            }

            return jsonElement?.GetString();
        }

        public static String Host => GetConfiguration("smtp:host") ?? "--";
    }
}
