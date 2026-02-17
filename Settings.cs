using StupidTemplate.Classes;
using UnityEngine;

namespace StupidTemplate
{
    public class Settings
    {
        /*
         * Updated Colors:
         * Buttons = Black
         * Text = Dark Red
         */

        // Dark Red for text
        private static Color darkRed = new Color(0.5f, 0f, 0f);

        // Background color (black)
        public static ExtGradient backgroundColor = new ExtGradient
        {
            colors = ExtGradient.GetSolidGradient(Color.black)
        };

        // Button colors (disabled & enabled both black)
        public static ExtGradient[] buttonColors = new ExtGradient[]
        {
            new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) }, // Disabled
            new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.softRed) }  // Enabled
        };

        // Text colors (dark red)
        public static Color[] textColors = new Color[]
        {
            darkRed, // Disabled
            darkRed  // Enabled
        };

        // Font
        public static Font currentFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Button layout settings
        public static bool fpsCounter = true;
        public static bool disconnectButton = true;
        public static bool rightHanded = false;
        public static bool disableNotifications = false;

        // Keybind
        public static KeyCode keyboardButton = KeyCode.Q;

        // Menu size (depth, width, height)
        public static Vector3 menuSize = new Vector3(0.1f, 1f, 1f);

        // Buttons per page
        public static int buttonsPerPage = 7;

        // Gradient animation speed
        public static float gradientSpeed = 0.5f;
    }
}