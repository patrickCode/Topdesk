using System;
using System.Configuration;

namespace IncidentMailService
{
    public class AppSettingsProvider
    {
        public static string GetSettingValue(string settingsKey)
        {
            var appSettingValue = ConfigurationManager.AppSettings[settingsKey];
            if (!string.IsNullOrEmpty(appSettingValue))
                return appSettingValue;
            return Environment.GetEnvironmentVariable(settingsKey);
        }
    }
}