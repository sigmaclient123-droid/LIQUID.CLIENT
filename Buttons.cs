using StupidTemplate.Classes;
using StupidTemplate.Mods;
using static StupidTemplate.Menu.Main;
using static StupidTemplate.Settings;

namespace StupidTemplate.Menu
{
    public class Buttons
    {
        /*
         * Here is where all of your buttons are located.
         * To create a button, you may use the following code:
         * 
         * Move to Category:
         *   new ButtonInfo { buttonText = "Settings", method =() => currentCategory = 1, isTogglable = false, toolTip = "Opens the main settings page for the menu."},
         *   new ButtonInfo { buttonText = "Return to Main", method =() => currentCategory = 0, isTogglable = false, toolTip = "Returns to the main page of the menu."},
         * 
         * Togglable Mod:
         *   new ButtonInfo { buttonText = "Platforms", method =() => Movement.Platforms(), toolTip = "Spawns platforms on your hands when pressing grip."},
         */

        public static ButtonInfo[][] buttons = new ButtonInfo[][]
        {
            new ButtonInfo[] { // Main Mods [0]
                new ButtonInfo { buttonText = "Settings", method = () => currentCategory = 1, isTogglable = false, toolTip = "Opens the main settings page for the menu." },

                new ButtonInfo { buttonText = "Room Mods", method = () => currentCategory = 4, isTogglable = false, toolTip = "Opens the room mods tab." },
                new ButtonInfo { buttonText = "Movement Mods", method = () => currentCategory = 5, isTogglable = false, toolTip = "Opens the movement mods tab." },
                new ButtonInfo { buttonText = "Safety Mods", method = () => currentCategory = 6, isTogglable = false, toolTip = "Opens the safety mods tab." },
                new ButtonInfo { buttonText = "Game Mods", method = () => currentCategory = 7, isTogglable = false, toolTip = "Opens the Game Mods tab." },
                new ButtonInfo { buttonText = "visual mods", method = () => currentCategory = 8, isTogglable = false, toolTip = "Opens the visual mods tab." },
                new ButtonInfo { buttonText = "fun mods", method = () => currentCategory = 9, isTogglable = false, toolTip = "Opens the fun mods mods tab." },
                new ButtonInfo { buttonText = "op mods", method = () => currentCategory = 10, isTogglable = false, toolTip = "Opens the op mods mods tab." },
            },

            new ButtonInfo[] { // Settings [1]
                new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false, toolTip = "Returns to the main page of the menu." },
                new ButtonInfo { buttonText = "Menu", method = () => currentCategory = 2, isTogglable = false, toolTip = "Opens the settings for the menu." },
                new ButtonInfo { buttonText = "Movement", method = () => currentCategory = 3, isTogglable = false, toolTip = "Opens the movement settings for the menu." },
            },

            new ButtonInfo[] { // Menu Settings [2]
                new ButtonInfo { buttonText = "Return to Settings", method = () => currentCategory = 1, isTogglable = false, toolTip = "Returns to the main settings page for the menu." },
                new ButtonInfo { buttonText = "Right Hand", enableMethod = () => rightHanded = true, disableMethod = () => rightHanded = false, toolTip = "Puts the menu on your right hand." },
                new ButtonInfo { buttonText = "Notifications", enableMethod = () => disableNotifications = false, disableMethod = () => disableNotifications = true, enabled = !disableNotifications, toolTip = "Toggles the notifications." },
                new ButtonInfo { buttonText = "FPS Counter", enableMethod = () => fpsCounter = true, disableMethod = () => fpsCounter = false, enabled = fpsCounter, toolTip = "Toggles the FPS counter." },
                new ButtonInfo { buttonText = "Disconnect Button", enableMethod = () => disconnectButton = true, disableMethod = () => disconnectButton = false, enabled = disconnectButton, toolTip = "Toggles the disconnect button." },
            },

            new ButtonInfo[] { // Movement Settings [3]
                new ButtonInfo { buttonText = "Return to Settings", method = () => currentCategory = 1, isTogglable = false, toolTip = "Returns to the main settings page for the menu." },

                new ButtonInfo { buttonText = "Change Fly Speed", overlapText = "Change Fly Speed [Normal]", method = () => Mods.Settings.Movement.ChangeFlySpeed(), isTogglable = false, toolTip = "Changes the speed of the fly mod." },
            },

            new ButtonInfo[] { // Room Mods [4]
                new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false, toolTip = "Returns to the main page of the menu." },
                new ButtonInfo { buttonText = "join random lobby", method = () => Mods.Movement.JoinRandom(), isTogglable = false, toolTip = "Returns to the main page of the menu." },
                new ButtonInfo { buttonText = "Disconnect", method = () => NetworkSystem.Instance.ReturnToSinglePlayer(), isTogglable = false, toolTip = "Disconnects you from the room." },
            },

            new ButtonInfo[] { // Movement Mods [5]
                new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false, toolTip = "Returns to the main page of the menu." },
                new ButtonInfo { buttonText = "platforms", method = () => Movement.PlatformModbysigmaboy(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "Fly", method = () => Movement.Fly(), toolTip = "Sends you forward when holding A." },
                new ButtonInfo { buttonText = "Teleport Gun", method = () => Movement.TeleportGun(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "Speedboost", method = () => Movement.speedboost(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "wasd fly", method = () => Movement.WASDFly(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "tp stump", method = () => Movement.TP_Stump(), isTogglable = false, toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "fastspeedboost", method = () => Movement.fastspeedboost(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "fastgripspeedboost", method = () => Movement.rightgripspeedboost(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "fast fly", method = () => Movement.fastFly(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "ghost monke", method = () => Movement.Ghost(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "up and down", method = () => Movement.UpAndDown(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "noclip", method = () => Movement.NoClip(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "grab rig", method = () => Movement.GrabRig(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "rockets", method = () => Movement.Rocket(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "trigger fly", method = () => Movement.triggerFly(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "fast trigger fly", method = () => Movement.fasttriggerFly(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
            },


            new ButtonInfo[] { // Safety Mods [6]
                new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false, toolTip = "Returns to the main page of the menu." },
                new ButtonInfo { buttonText = "clear all Notifications", method = () => Safety.Clearnotis(), toolTip = "Disconnects you when someone tries to report you." },
                new ButtonInfo { buttonText = "Anti Report", method = () => Safety.AntiReportDisconnect(), toolTip = "Disconnects you when someone tries to report you." },
                new ButtonInfo { buttonText = "flush rpcs DO NOT SPAM", method = () => Movement.FlushRPCs(), isTogglable = false, toolTip = "Disconnects you when someone tries to report you." },
            },


            new ButtonInfo[] { // Game Mods
                new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false, toolTip = "Returns to the main page of the menu." },


                new ButtonInfo { buttonText = "FixFPS", method = () => Mods.Movement.FixFPS(), isTogglable = false, toolTip = "Resets Your Frames To Normal." },
                new ButtonInfo { buttonText = "120 FPS", method = () => Mods.Movement.ofps(), toolTip = "Sets Your Game To 120 FPS" },
                 new ButtonInfo { buttonText = "60 FPS", method = () => Mods.Movement.sfps(), toolTip = "Sets Your Game To 60 FPS" },
                new ButtonInfo { buttonText = "40 FPS", method = () => Mods.Movement.Ffps(), toolTip = "Sets Your Game To 40 FPS" },
                new ButtonInfo { buttonText = "20 FPS", method = () => Mods.Movement.Tfps(), toolTip = "Sets Your Game To 20 FPS" },
                 new ButtonInfo { buttonText = "5 FPS", method = () => Mods.Movement.ffps(), toolTip = "Sets Your Game To 5 FPS" },
                  new ButtonInfo { buttonText = "fps boost", method = () => Mods.Movement.FPSBoostIndev(), toolTip = "Sets Your Game To 5 FPS" },
                new ButtonInfo { buttonText = "close gtag", method = () => Mods.Movement.closegame(), isTogglable = false, toolTip = "closes Your Game " },
                new ButtonInfo { buttonText = "disable wind", method = () => Movement.Destroywind(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
            },
            new ButtonInfo[] { // visual mods
                new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false, toolTip = "Returns to the main page of the menu." },

                new ButtonInfo { buttonText = "tracers", method = () => Movement.Tracer(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "beacons", method = () => Movement.beacons(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
            },

            new ButtonInfo[] { // fun mods
                new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false, toolTip = "Returns to the main page of the menu." },
                new ButtonInfo { buttonText = "spaz head", method = () => Movement.SpazHead(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "fix head", method = () => Movement.FixHead(), isTogglable = false, toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "bounce", method = () => Movement.Bouncy(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "reset bounce", method = () => Movement.ResetBouncy(), isTogglable = false, toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "platfom spam", method = () => Movement.PlatformSpam(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "upsidedown head", method = () => Movement.upsidedownhead(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "broken neck", method = () => Movement.BrokenNeck(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
                new ButtonInfo { buttonText = "be doug", method = () => Movement.BeDougTheBug(), toolTip = "Teleports you to wherever your pointer is when pressing trigger." },
            },
            new ButtonInfo[] { //op mods
 new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false, toolTip = "Returns to the main page of the menu." },


 new ButtonInfo { buttonText = "ghost money", method =() => Movement.AddCurrencySelf(), toolTip = "Teleports you to wherever your pointer is when pressing trigger."},
 new ButtonInfo { buttonText = "lag all maybe(D?)", method =() => Movement.LagMaybe(), toolTip = "Teleports you to wherever your pointer is when pressing trigger."},

            },
        };
    }
}
