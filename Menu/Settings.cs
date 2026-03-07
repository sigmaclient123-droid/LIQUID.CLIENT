using liquidclient.Classes;
using liquidclient.Utilities;
using UnityEngine;

namespace liquidclient
{
    public class Settings
    {
        /*
         * Updated Colors:
         * Buttons = dark blue
         * Text = violet blue
         */
        
        // violet blue for text
        private static Color violetblue = new Color(0.5f, 0f, 0f);

        // Background color (sky blue)
        public static ExtGradient backgroundColor = new ExtGradient
        {
            colors = ExtGradient.GetSolidGradient(Color.navyBlue)
            //colors = ExtGradient.GetSolidGradient(ColorLib.DarkBlue)
        };

        // Button colors (disabled & enabled both black)
        public static ExtGradient[] buttonColors = new ExtGradient[]
        {
            new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.blue) }, // Disabled
            new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.dodgerBlue) }  // Enabled
        };

        // Text colors (dark blue)
        public static Color[] textColors = new Color[]
        {
            Color.aquamarine, // Disabled
            Color.aquamarine  // Enabled
        };

        // Font
        public static Font currentFont = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 50);

        // Button layout settings
        public static bool fpsCounter = true;
        public static bool disconnectButton = true;
        public static bool rightHanded = false;
        public static bool disableNotifications = false;
        public static bool Enablecustomboards = true;

        // Keybind
        public static KeyCode keyboardButton = KeyCode.M;

        // Menu size (depth, width, height)
        public static Vector3 menuSize = new Vector3(0.1f, 0.3f, 0.3825f);

        // Buttons per page
        public static int buttonsPerPage = 7;

        // Gradient animation speed
        public static float gradientSpeed = 0.5f;

        public static bool settingsButton = true;
        public static bool homeButton = true;
    }
}