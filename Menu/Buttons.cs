using GorillaLocomotion;
using GorillaNetworking;
using liquidclient.Mods;
using liquid.client.Patches.Internal;
using liquidclient.Classes;
using liquidclient.GunLib;
using liquidclient.Managers;
using liquidclient.mods;
using liquidclient.Notifications;
using liquidclient.Patches.Menu;
using Photon.Pun;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using static liquidclient.Menu.Main;
using static liquidclient.Mods.Movement;
using static liquidclient.Settings;

namespace liquidclient.Menu
{
    public class Buttons
    {
        public static int GetModCount()
        {
            int count = 0;
            foreach (var category in buttons)
            {
                foreach (var button in category)
                {
                    if (button.method != null &&
                        button.buttonText != "Return to Main" &&
                        !button.buttonText.Contains("Return to") &&
                        !button.buttonText.Contains("Mods") &&
                        button.buttonText != "Settings")
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public static List<ButtonInfo> GetActiveMods()
        {
            List<ButtonInfo> active = new List<ButtonInfo>();
            foreach (var category in buttons)
            {
                foreach (var btn in category)
                {
                    if (btn.enabled && btn.isTogglable && !btn.buttonText.Contains("Return"))
                    {
                        active.Add(btn);
                    }
                }
            }
            return active;
        }

        public static string[] categoryNames = new string[]
        {
            "Main Menu",          // 0
            "Settings",           // 1
            "Menu Settings",      // 2
            "Movement Settings",  // 3
            "Enabled Mods",       // 4
            "Room Mods",          // 5
            "Movement Mods",      // 6
            "Safety Mods",        // 7
            "Game Mods",          // 8
            "Visual Mods",        // 9
            "Fun Mods",           // 10
            "OP Mods",            // 11
            "Sounds",             // 12
            "GunLib Settings",    // 13
            "Tag Mods",           // 14
        };

        public static ButtonInfo[][] buttons = new ButtonInfo[][]
        {
            new ButtonInfo[] { // Main Mods [0]
                new ButtonInfo { buttonText = "Enabled Mods", method =() => currentCategory = 4, isTogglable = false },
                new ButtonInfo { buttonText = "Join Discord", method = () => Process.Start(serverLink), isTogglable = false },
                new ButtonInfo { buttonText = "Room Mods", method = () => currentCategory = 5, isTogglable = false },
                new ButtonInfo { buttonText = "Movement Mods", method = () => currentCategory = 6, isTogglable = false },
                new ButtonInfo { buttonText = "Safety Mods", method = () => currentCategory = 7, isTogglable = false },
                new ButtonInfo { buttonText = "Game Mods", method = () => currentCategory = 8, isTogglable = false },
                new ButtonInfo { buttonText = "Tag Mods", method = () => currentCategory = 14, isTogglable = false },
                new ButtonInfo { buttonText = "Visual Mods", method = () => currentCategory = 9, isTogglable = false },
                new ButtonInfo { buttonText = "Fun Mods", method = () => currentCategory = 10, isTogglable = false },
                new ButtonInfo { buttonText = "OP Mods", method = () => currentCategory = 11, isTogglable = false },
                new ButtonInfo { buttonText = "Sounds", method = () => currentCategory = 12, isTogglable = false },
            },

            new ButtonInfo[] { // Settings [1] 
                new ButtonInfo { buttonText = "Menu", method = () => currentCategory = 2, isTogglable = false },
                new ButtonInfo { buttonText = "GunLib", method = () => currentCategory = 13, isTogglable = false },
                new ButtonInfo { buttonText = "Movement", method = () => currentCategory = 3, isTogglable = false },
            },

            new ButtonInfo[] { // Menu Settings [2]
                new ButtonInfo { buttonText = "Right Hand", enableMethod = () => rightHanded = true, disableMethod = () => rightHanded = false },
                new ButtonInfo { buttonText = "Notifications", enableMethod = () => disableNotifications = false, disableMethod = () => disableNotifications = true, enabled = !disableNotifications },
                new ButtonInfo { buttonText = "FPS Counter", enableMethod = () => fpsCounter = true, disableMethod = () => fpsCounter = false, enabled = fpsCounter },
                new ButtonInfo { buttonText = "Disconnect Button", enableMethod = () => disconnectButton = true, disableMethod = () => disconnectButton = false, enabled = disconnectButton },
            },

            new ButtonInfo[] { // Movement Settings [3]
                // Add movement settings here if needed
            },

            new ButtonInfo[] { // Enabled Mods [4]
                // This category will be populated dynamically
            },

            new ButtonInfo[] { // Room Mods [5]
                new ButtonInfo { buttonText = "join random lobby", method = () => Mods.Movement.JoinRandom(), isTogglable = false },
                new ButtonInfo { buttonText = "Disconnect", method = () => NetworkSystem.Instance.ReturnToSinglePlayer(), isTogglable = false },
                new ButtonInfo { buttonText = "LT DISCONNECT", method = () => Mods.Movement.LTdisconnet() },
                new ButtonInfo { buttonText = "Anti AFK", enableMethod =() => PhotonNetworkController.Instance.disableAFKKick = true, disableMethod =() => PhotonNetworkController.Instance.disableAFKKick = false, toolTip = "Doesn't let you get kicked for being AFK."},
                new ButtonInfo { buttonText = "Join Code Mods", method = () => Joincode("MODS"), isTogglable = false},
                new ButtonInfo { buttonText = "Join Code 459", method = () => Joincode("459"), isTogglable = false},
                new ButtonInfo { buttonText = "Join Code Mod", method = () => Joincode("MOD"), isTogglable = false},
                new ButtonInfo { buttonText = "Join Code PBBV", method = () => Joincode("PBBV"), isTogglable = false},
            },

            new ButtonInfo[] { // Movement Mods [6]
                new ButtonInfo { buttonText = "Platforms", method = () => Movement.PlatformModbysigmaboy() },
                new ButtonInfo { buttonText = "Trigger Platforms", method = () => Movement.Triggerplats() },
                new ButtonInfo { buttonText = "Sticky Platforms", method = () => Movement.StickyPlatforms() },
                new ButtonInfo { buttonText = "Frozone", method = () => Movement.Frozone() },
                new ButtonInfo { buttonText = "Fly", method = () => Movement.Fly() },
                new ButtonInfo { buttonText = "Speedboost", method = () => Movement.speedboost() },
                new ButtonInfo { buttonText = "Checkpoint", method = () => Checkpoint(), disableMethod = () => GameObject.Destroy(CheckPoint), isTogglable = true },
                new ButtonInfo { buttonText = "WASD Fly", method = () => WASDFLY.WASDFly(), isTogglable = true },
                new ButtonInfo { buttonText = "Tp Stump", method = () => Movement.TP_Stump(), isTogglable = false },
                new ButtonInfo { buttonText = "Fast SpeedBoost", method = () => Movement.fastspeedboost() },
                new ButtonInfo { buttonText = "Fast Grip SpeedBoost", method = () => Movement.rightgripspeedboost() },
                new ButtonInfo { buttonText = "fast fly", method = () => Movement.fastFly() },
                new ButtonInfo { buttonText = "Ghost Monke", method = () => Movement.Ghost() },
                new ButtonInfo { buttonText = "Up and Down", method = () => Movement.UpAndDown() },
                new ButtonInfo { buttonText = "Noclip", method = () => Movement.NoClip() },
                new ButtonInfo { buttonText = "grab rig", method = () => Movement.GrabRig() },
                new ButtonInfo { buttonText = "trigger fly", method = () => Movement.triggerFly() },
                new ButtonInfo { buttonText = "fast trigger fly", method = () => Movement.fasttriggerFly() },
                new ButtonInfo { buttonText = "Slingshot Fly", method = () => Movement.Slingshotfly(false, false) },
                new ButtonInfo { buttonText = "Zero Gravity Slingshot Fly", method = () => Movement.Slingshotfly(true, false) },
                new ButtonInfo { buttonText = "Low Gravity Slingshot Fly", method = () => Movement.Slingshotfly(false, true) },
                new ButtonInfo { buttonText = "Bird Fly", method = Movement.BirdFly, toolTip = "Makes you fly like a bird when you flap your wings."},
                new ButtonInfo { buttonText = "Slide Control", method = () => Movement.Slidemangerr(10f) },
                //new ButtonInfo { buttonText = "Long arms", method = () => Longarms(), toolTip = "Long Arms"},
            },

            new ButtonInfo[] { // Safety Mods [7]
                new ButtonInfo { buttonText = "Anti Report", method = () => Safety.AntiReportDisconnect() },
                //new ButtonInfo { buttonText = "Visualize Anti Report", method = Safety.VisualizeAntiReport, toolTip = "Visualizes the distance threshold for the anti report mods."},
                new ButtonInfo { buttonText = "Flush Rpcs Dont Spam", method = () => Movement.FlushRPCs(), isTogglable = false },
                new ButtonInfo { buttonText = "Auto Clear Cache", method = Safety.AutoClearCache, toolTip = "Automatically clears your game's cache (garbage collector) every minute to prevent memory leaks."},
                new ButtonInfo { buttonText = "Disable Fingers", method = () => Movement.DisableFingers(), toolTip = "Good for plats"},
                new ButtonInfo { buttonText = "Panic(RG)", method = () => Movement.RightgripPanic(), toolTip = "Turns off all of the mods when holding right grip."},
                new ButtonInfo { buttonText = "Panic", method = () => Movement.Panic(), isTogglable = false, toolTip = "Turns off all of the mods."},
            },

            new ButtonInfo[] { // Game Mods [8]
                new ButtonInfo { buttonText = "PC Button Click", method = Important.PCButtonClick, disableMethod = Important.DisablePCButtonClick },
                new ButtonInfo { buttonText = "First Person Camera", enableMethod = Important.EnableFPC, method = Important.MoveFPC, disableMethod = Important.DisableFPC },
                new ButtonInfo { buttonText = "Get ID Self", method = Important.CopySelfID, isTogglable = false, toolTip = "Gets your player ID and copies it to the clipboard."},
                new ButtonInfo { buttonText = "Unlock FPS", method = Important.UncapFPS, disableMethod =() => Application.targetFrameRate = 144 },
                new ButtonInfo { buttonText = "Disable Wind Barrier", overlapText = "Disable Wind Barriers", enableMethod =() => { ForcePatches.enabled = true; GetObject("Environment Objects/LocalObjects_Prefab/Forest/Environment/Forest_ForceVolumes/").SetActive(false); GetObject("Environment Objects/LocalObjects_Prefab/ForestToHoverboard/TurnOnInForestAndHoverboard/ForestDome_CollisionOnly").SetActive(false); }, disableMethod =() => { ForcePatches.enabled = false; GetObject("Environment Objects/LocalObjects_Prefab/Forest/Environment/Forest_ForceVolumes/").SetActive(true); GetObject("Environment Objects/LocalObjects_Prefab/ForestToHoverboard/TurnOnInForestAndHoverboard/ForestDome_CollisionOnly").SetActive(true); }, toolTip = "Disables the wind barriers in every map." },
                new ButtonInfo { buttonText = "Unlock Fan Club Subscription", enableMethod =() => SubscriptionPatches.enabled = true, disableMethod =() => SubscriptionPatches.enabled = false, toolTip = "Unlocks the Gorilla Tag fan club subscription." },
                new ButtonInfo { buttonText = "PC Controller Emulation", method = Important.PCControllerEmulation },
                new ButtonInfo { buttonText = "Close Application", method = () => Application.Quit(), isTogglable = false },
                new ButtonInfo { buttonText = "120 FPS", method = () => Mods.Movement.ofps(), disableMethod = Movement.FixFPS},
                new ButtonInfo { buttonText = "60 FPS", method = () => Mods.Movement.sfps(), disableMethod = Movement.FixFPS },
                new ButtonInfo { buttonText = "40 FPS", method = () => Mods.Movement.Ffps(), disableMethod = Movement.FixFPS },
                new ButtonInfo { buttonText = "20 FPS", method = () => Mods.Movement.Tfps(), disableMethod = Movement.FixFPS },
                new ButtonInfo { buttonText = "5 FPS", method = () => Mods.Movement.ffps(), disableMethod = Movement.FixFPS },
                new ButtonInfo { buttonText = "Fps Boost", method = () => Mods.Movement.FPSBoostIndev() },
                new ButtonInfo { buttonText = "Grab Bug(If M Then SS)", method = () => Grabbug()},
                new ButtonInfo { buttonText = "Grab Bat(If M Then SS)", method = () => GrabBat()},
                new ButtonInfo { buttonText = "Unlock comp", enableMethod = () => GorillaComputer.instance.allowedInCompetitive = true, disableMethod = () => GorillaComputer.instance.allowedInCompetitive = false},
            },

            new ButtonInfo[] { // Visual Mods [9]
                new ButtonInfo { buttonText = "Tracers", method = () => Movement.Tracer() },
                new ButtonInfo { buttonText = "Beacons", method = () => Movement.beacons() },
                new ButtonInfo { buttonText = "Box ESP", method = () => Movement.Box_ESP() },
                new ButtonInfo { buttonText = "Bat ESP", method = () => Movement.Bat_ESP() },
                new ButtonInfo { buttonText = "Bug ESP", method = () => Movement.Bug_ESP() },
                new ButtonInfo { buttonText = "Circle ESP", method = () => Movement.Circle_ESP() },
                new ButtonInfo { buttonText = "Chams", method = () => Movement.chams(), disableMethod = Movement.ChamsOff},
                new ButtonInfo { buttonText = "Nametags",  method = () => InfoTagManager.UpdateTracker(), disableMethod = () => InfoTagManager.ToggleInformationNameTags(false), isTogglable = true},
                new ButtonInfo { buttonText = "Clear Notifs", method = () => NotifiLib.ClearAllNotifications(), isTogglable = false },
            },

            new ButtonInfo[] { // Fun Mods [10]
                new ButtonInfo { buttonText = "Fix Rig", method = () => Movement.FixRig(), isTogglable = false},
                new ButtonInfo { buttonText = "Spaz Head", method = () => Movement.SpazHead()},
                new ButtonInfo { buttonText = "Bounce", method = () => Movement.Bouncy(), disableMethod = Movement.ResetBouncy },
                new ButtonInfo { buttonText = "Platfom Spam", method = () => Movement.PlatformSpam() },
                new ButtonInfo { buttonText = "Upsidedown Head", method = () => Movement.UpsideDownHead(), disableMethod = Movement.FixRig },
                new ButtonInfo { buttonText = "Broken Neck", method = () => Movement.BrokenNeck(), disableMethod = Movement.FixRig },
                new ButtonInfo { buttonText = "Rainbow Bracelet", enableMethod = () => Movement.RainbowBracelet(), disableMethod = () => Movement.disableRainbowBracelet() },
                new ButtonInfo { buttonText = "Set Quest Score To 100k", method = () => Movement.addqueststuff(100000), disableMethod = () => Mods.Movement.Resetqueststuff() },
                new ButtonInfo { buttonText = "Spawn Hoverboard", method = Movement.SSHoverboardSpawn, disableMethod = Movement.DisableHoverboard, toolTip = "Gives you the hoverboard no matter where you are."},
                new ButtonInfo { buttonText = "Zero Gravity(CS)", method = () => Gravityhelper(true, false)},
                new ButtonInfo { buttonText = "High Gravity(CS)", method = () => Gravityhelper(false, false)},
                new ButtonInfo { buttonText = "Low Gravity(CS)", method = () => Gravityhelper(false, true)},
                new ButtonInfo { buttonText = "Rainbow Hoverboard", method = () => HoverboardColor(Color.black, true)},
                new ButtonInfo { buttonText = "Navy Blue Color Hoverboard", method = () => HoverboardColor(Color.navyBlue, false)},
                new ButtonInfo { buttonText = "J3VU Color Hoverboard", method = () => HoverboardColor(Color.green, false)},
            },
            
            new ButtonInfo[] { // OP Mods [11]-
                // ButtonInfo { buttonText = "Check Matser", method = () => Movement.Checkmaster(), isTogglable = false },
                new ButtonInfo { buttonText = "Ghost Money", method = () => Movement.AddCurrencySelf() },
                 new ButtonInfo { buttonText = "Anti Ban(OP)", method = () => Movement.AntiBan() },
                new ButtonInfo { buttonText = "Lag server 1", method = () => Movement.LagServer(24), toolTip = "Lags the server"},
                new ButtonInfo { buttonText = "Lag server 2", method = () => Movement.LagServer(23), toolTip = "Lags the server"},
                new ButtonInfo { buttonText = "Freeze server", method = () => Movement.LagServer(149), toolTip = "Attempts to freeze the server"},
                new ButtonInfo { buttonText = "Light Lag Server", method = () => Movement.LagServer(1), toolTip = "Lightly lags the server"},
                new ButtonInfo { buttonText = "Lag Server 3", method = () => Movement.LagServer(10), toolTip = "Lags the server"},
                new ButtonInfo { buttonText = "Flash Gray Screen", method = () => Movement.FlashGrayScreenSSAll(), isTogglable = true },
                new ButtonInfo { buttonText = "Gray Screen/No gravity(SS)(M)", enableMethod = () => Movement.GrayScreenThing(true), disableMethod  = () => Movement.GrayScreenThing(false)},
                new ButtonInfo { buttonText = "Guardian Self(SS)(M)", enableMethod = () => Guardianhelper(true, false, false), disableMethod = () => Guardianhelper(false, false, false), toolTip = "Makes you guardian"},
                new ButtonInfo { buttonText = "Guardian All(SS)(M)", enableMethod = () => Guardianhelper(false, true, false), disableMethod = () => Guardianhelper(false, false, false), toolTip = "Makes everyone guardian"},
                new ButtonInfo { buttonText = "Guardian Others(SS)(M)", enableMethod = () => Guardianhelper(false, false, true), disableMethod = () => Guardianhelper(false, false, false), toolTip = "Makes everyone guardian"},
                //new ButtonInfo { buttonText = "Wind Push(M)(RG)", method = () => Movement.Windmod(), toolTip = "Pushes whoever you point it at"},
                //new ButtonInfo { buttonText = "Fling Gun(M)(RG)", method = () => Movement.Flinggun(), toolTip = "Flings emmmm"},
            },

            new ButtonInfo[] { // Sounds [12]
                new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false },
                new ButtonInfo { buttonText = "Soon", method = () => currentCategory = 0, isTogglable = false},
            },

            new ButtonInfo[] { // GunLib Settings [13]
                new ButtonInfo { buttonText = "Test Gun", method = () => Important.TestGun(), isTogglable = true },

                new ButtonInfo { buttonText = "Style: " + GunLibTEst.AthrionGunLibrary.currentLineStyle, isTogglable = false, method = null },
                new ButtonInfo { buttonText = "Next Style →", method = () => GunLibTEst.AthrionGunLibrary.ChangeGunStyle(true), isTogglable = false },
                new ButtonInfo { buttonText = "← Prev Style", method = () => GunLibTEst.AthrionGunLibrary.ChangeGunStyle(false), isTogglable = false },

                new ButtonInfo { buttonText = "Line Size: " + GunLibTEst.AthrionGunLibrary.GunLineWidth.ToString("F3"), isTogglable = false, method = null },
                new ButtonInfo { buttonText = "Increase Line", method = () => GunLibTEst.AthrionGunLibrary.ChangeGunLineSize(true), isTogglable = false },
                new ButtonInfo { buttonText = "Decrease Line", method = () => GunLibTEst.AthrionGunLibrary.ChangeGunLineSize(false), isTogglable = false },

                new ButtonInfo { buttonText = "Sphere Size: " + GunLibTEst.AthrionGunLibrary.SphereSize.ToString("F2"), isTogglable = false, method = null },
                new ButtonInfo { buttonText = "Increase Sphere", method = () => GunLibTEst.AthrionGunLibrary.ChangeGunSphereScale(true), isTogglable = false },
                new ButtonInfo { buttonText = "Decrease Sphere", method = () => GunLibTEst.AthrionGunLibrary.ChangeGunSphereScale(false), isTogglable = false },

                new ButtonInfo { buttonText = "Wave Freq: " + GunLibTEst.AthrionGunLibrary.GunConfig.WaveFrequency.ToString("F1"), isTogglable = false, method = null },
                new ButtonInfo { buttonText = "Increase Freq", method = () => GunLibTEst.AthrionGunLibrary.ChangeWaveFrequency(true), isTogglable = false },
                new ButtonInfo { buttonText = "Decrease Freq", method = () => GunLibTEst.AthrionGunLibrary.ChangeWaveFrequency(false), isTogglable = false },
                new ButtonInfo { buttonText = "Wave Amp: " + GunLibTEst.AthrionGunLibrary.GunConfig.WaveAmplitude.ToString("F3"), isTogglable = false, method = null },
                new ButtonInfo { buttonText = "Increase Amp", method = () => GunLibTEst.AthrionGunLibrary.ChangeWaveAmplitude(true), isTogglable = false },
                new ButtonInfo { buttonText = "Decrease Amp", method = () => GunLibTEst.AthrionGunLibrary.ChangeWaveAmplitude(false), isTogglable = false },

                new ButtonInfo { buttonText = "Particles", enableMethod = () => GunLibTEst.AthrionGunLibrary.GunConfig.EnableParticles = true, disableMethod = () => GunLibTEst.AthrionGunLibrary.ToggleParticles(), enabled = GunLibTEst.AthrionGunLibrary.GunConfig.EnableParticles },
                new ButtonInfo { buttonText = "Box ESP", enableMethod = () => GunLibTEst.AthrionGunLibrary.GunConfig.EnableBoxESP = true, disableMethod = () => GunLibTEst.AthrionGunLibrary.GunConfig.EnableBoxESP = false, enabled = GunLibTEst.AthrionGunLibrary.GunConfig.EnableBoxESP },
                new ButtonInfo { buttonText = "Animations", enableMethod = () => GunLibTEst.AthrionGunLibrary.GunConfig.EnableAnimations = true, disableMethod = () => GunLibTEst.AthrionGunLibrary.GunConfig.EnableAnimations = false, enabled = GunLibTEst.AthrionGunLibrary.GunConfig.EnableAnimations },
            },

            new ButtonInfo[] { // TagMods Category [14]
                new ButtonInfo { buttonText = "Tag All", method = () => Movement.tg() },
                new ButtonInfo { buttonText = "Tag Gun", method = () => Important.TagGun() },
            },
        };
    }
}
