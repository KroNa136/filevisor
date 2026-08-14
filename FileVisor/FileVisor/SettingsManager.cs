using FileVisor.Models;
using System;
using System.IO;
using System.Text.Json;

using static FileVisor.CustomDialogBox.DialogBox;

namespace FileVisor
{
    internal static class SettingsManager
    {
        static string savePath = "settings.json";
        
        internal static Settings GetSettings()
        {
            if (File.Exists(savePath))
            {
                try
                {
                    return JsonSerializer.Deserialize<Settings>(File.ReadAllText(savePath));
                }
                catch (Exception)
                {
                    ShowDialogBox("Не удалось загрузить настройки программы. Применены параметры по умолчанию.", null, DialogBoxType.Error, DialogBoxButtons.OK);
                    return new Settings();
                }
            }
            else
            {
                return new Settings();
            }
        }

        internal static void SaveSettings(Settings settings)
        {
            File.WriteAllText(savePath, JsonSerializer.Serialize(settings));
        }
    }
}
