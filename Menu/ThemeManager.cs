using liquidclient.Menu;
using UnityEngine;
using liquidclient.Classes;
using static liquidclient.Menu.Main;

namespace liquidclient.Menu
{
    public class ThemeManager
    {
        public enum MenuTheme { TriggerOnly, Standard }
        public static MenuTheme CurrentTheme = MenuTheme.TriggerOnly;
        public static string textHex = "white";

        public static void CycleTheme()
        {
            CurrentTheme = (CurrentTheme == MenuTheme.Standard) ? MenuTheme.TriggerOnly : MenuTheme.Standard;
            if (menu != null) RecreateMenu();
        }

        public static void CycleColor()
        {
            OnScreenGUI.CycleTheme();
            ApplyColors();
        }

        public static void ApplyColors()
        {
            Color currentAccent = GetCurrentGUIColor();
            
            if (liquidclient.Settings.backgroundColor == null)
                liquidclient.Settings.backgroundColor = new ExtGradient();

            liquidclient.Settings.backgroundColor.colors = ExtGradient.GetSolidGradient(currentAccent);

            if (OnScreenGUI.currentThemeIndex == 6)
            {
                liquidclient.Settings.textColors[0] = new Color(0.2f, 0.2f, 0.2f);
                textHex = "black";
            }
            else
            {
                liquidclient.Settings.textColors[0] = Color.white;
                textHex = "white";
            }

            if (Main.menu != null)
            {
                Main.RecreateMenu();
            }
        }
        
        public static Color GetCurrentGUIColor()
        {
            switch (OnScreenGUI.currentThemeIndex)
            {
                case 0: return new Color32(0, 0, 160, 255); // Color32(0, 0, 160, 255);
                default: return new Color32(0, 0, 160, 255);
            }
        }

        public static bool ShowSideButtons() => CurrentTheme == MenuTheme.Standard;
        public static bool UseTriggerNavigation() => CurrentTheme != MenuTheme.Standard;
        public static bool HideBackground() => false;
    }
}