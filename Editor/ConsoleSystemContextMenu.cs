using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Application = UnityEngine.Device.Application;

namespace ModularConsole.Editor.Editor
{
    public static class ConsoleSystemContextMenu
    {
        private const string FOLDER_NAME = "ModularConsole";
        private const string RESOURCES_PATH = "Assets/Resources";
        private const string CONSOLE_FOLDER = "Assets/Resources/ModularConsole";
        
        [MenuItem("Assets/Fixer33/ModularConsole/Regenerate console panel settings override")]
        private static void RegenerateConsolePanelSettingsOverride()
        {
            ValidateConsoleFolder();

            string path = $"{CONSOLE_FOLDER}/{ConsoleSystem.CONSOLE_STYLE_PANEL_SETTINGS_OVERRIDE_NAME}.asset";
            if (AssetDatabase.LoadAssetAtPath<PanelSettings>(path))
            {
                if (EditorUtility.DisplayDialog("File already exists", "Panel settings asset already exists in Resources folder. \n" +
                                                                       "Are you sure that you want to delete it and create new one?", "Yes", "Abort") == false)
                    return;

                AssetDatabase.DeleteAsset(path);
                AssetDatabase.SaveAssets();
            }
            
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.sortingOrder = 9999;
            settings.name = ConsoleSystem.CONSOLE_STYLE_PANEL_SETTINGS_OVERRIDE_NAME;
            settings.themeStyleSheet = Resources.Load<ThemeStyleSheet>(ConsoleSystem.THEME_STYLESHEET_FILE_NAME);

            AssetDatabase.CreateAsset(settings, path);
            AssetDatabase.SaveAssets();
            
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
        }
        
        [MenuItem("Assets/Fixer33/ModularConsole/Regenerate console style override")]
        private static void RegenerateConsoleStyleOverride()
        {
            RegenerateStyleFile(ConsoleSystem.CONSOLE_STYLE_OVERRIDE_NAME);
        }
        
        [MenuItem("Assets/Fixer33/ModularConsole/Regenerate console style mobile override")]
        private static void RegenerateConsoleStyleMobileOverride()
        {
            RegenerateStyleFile(ConsoleSystem.CONSOLE_STYLE_MOBILE_OVERRIDE_NAME);
        }

        private static void RegenerateStyleFile(string consoleStyleOverrideName)
        {
            ValidateConsoleFolder();

            var templateAsset = Resources.Load<StyleSheet>(ConsoleSystem.INTERNAL_STYLES_FILE_NAME);
            string templatePath = Application.dataPath + AssetDatabase.GetAssetPath(templateAsset).Replace("Assets", "");
            string templateText = File.ReadAllText(templatePath);

            string targetAssetPath = $"{CONSOLE_FOLDER}/{consoleStyleOverrideName}";
            string targetPath = Application.dataPath + targetAssetPath.Replace("Assets", "") + ".uss";

            if (File.Exists(targetPath))
            {
                if (EditorUtility.DisplayDialog("File already exists", "Style override file already exists in Resources folder. \n" +
                                                                       "Are you sure that you want to delete it and create new one?", "Yes", "Abort") == false)
                    return;
                
                File.Delete(targetPath);
                File.Delete(targetPath + ".meta");
            }
            
            File.Create(targetPath).Close();
            File.WriteAllText(targetPath, templateText);
            
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(CONSOLE_FOLDER);
        }

        private static void ValidateConsoleFolder()
        {
            if (AssetDatabase.IsValidFolder(RESOURCES_PATH) == false)
                AssetDatabase.CreateFolder("Assets", "Resources");
            
            if (AssetDatabase.IsValidFolder(CONSOLE_FOLDER) == false)
                AssetDatabase.CreateFolder(RESOURCES_PATH, FOLDER_NAME);
        }
    }
}