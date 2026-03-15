using admintest;
using GorillaLocomotion;
using GorillaNetworking;
using liquid.client.Mods;
using liquid.client.Patches.Internal;
using liquid.client.SoundManager;
using liquidclient.Classes;
using liquidclient.GunLib;
using liquidclient.Managers;
using liquidclient.mods;
using liquidclient.Mods;
using liquidclient.Notifications;
using liquidclient.Patches.Menu;
using Photon.Pun;
using System.Collections.Generic;
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
        
        public static class ButtonTextUpdater
        {
            public static void UpdateClickButtonText(ButtonInfo button)
            {
                button.buttonText = $"Change Click Sound ({ChangeSoundManager.CurrentClickSoundName()})";
            }

            public static void UpdateOpenButtonText(ButtonInfo button)
            {
                button.buttonText = $"Change Open Sound ({ChangeSoundManager.CurrentOpenSoundName()})";
            }
        }
        
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
            "Admin Panel",        // 13
            "Admin Test",         // 14
            "GunLib Settings",    // 15
            "Tag Mods",            // 16
            "Owner Panel"            // 16
        };

        public static ButtonInfo[][] buttons = new ButtonInfo[][]
        {
            new ButtonInfo[] { // Main Mods [0]
                new ButtonInfo { buttonText = "Enabled Mods", method =() => currentCategory = 4, isTogglable = false },
                new ButtonInfo { buttonText = "Room Mods", method = () => currentCategory = 5, isTogglable = false },
                new ButtonInfo { buttonText = "Movement Mods", method = () => currentCategory = 6, isTogglable = false },
                new ButtonInfo { buttonText = "Safety Mods", method = () => currentCategory = 7, isTogglable = false },
                new ButtonInfo { buttonText = "Game Mods", method = () => currentCategory = 8, isTogglable = false },
                new ButtonInfo { buttonText = "Tag Mods", method = () => currentCategory = 16, isTogglable = false },
                new ButtonInfo { buttonText = "Visual Mods", method = () => currentCategory = 9, isTogglable = false },
                new ButtonInfo { buttonText = "Fun Mods", method = () => currentCategory = 10, isTogglable = false },
                new ButtonInfo { buttonText = "OP Mods", method = () => currentCategory = 11, isTogglable = false },
                new ButtonInfo { buttonText = "Sounds", method = () => currentCategory = 12, isTogglable = false },
            },

            new ButtonInfo[] { // Settings [1]
                //new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false },
                new ButtonInfo { buttonText = "Menu", method = () => currentCategory = 2, isTogglable = false },
                new ButtonInfo { buttonText = "GunLib", method = () => currentCategory = 15, isTogglable = false },
                new ButtonInfo { buttonText = "Movement", method = () => currentCategory = 3, isTogglable = false },
            },
            
            new ButtonInfo[] { // Menu Settings [2]
                //new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false },
                new ButtonInfo { buttonText = "Right Hand", enableMethod = () => rightHanded = true, disableMethod = () => rightHanded = false },
                new ButtonInfo { buttonText = "Notifications", enableMethod = () => disableNotifications = false, disableMethod = () => disableNotifications = true, enabled = !disableNotifications },
                new ButtonInfo { buttonText = "FPS Counter", enableMethod = () => fpsCounter = true, disableMethod = () => fpsCounter = false, enabled = fpsCounter },
                new ButtonInfo { buttonText = "Disconnect Button", enableMethod = () => disconnectButton = true, disableMethod = () => disconnectButton = false, enabled = disconnectButton },
                new ButtonInfo { buttonText = "Stump Text", method = () => StumpText.Stumpy(), disableMethod = () => StumpText.STUMPY_DESTROY() ,isTogglable = true, enabled = true},
            },

            new ButtonInfo[] { // Movement Settings [3]
                
                //new ButtonInfo { buttonText = "Return to Settings", method = () => currentCategory = 1, isTogglable = false },
                //new ButtonInfo { buttonText = "Change Fly Speed", method = () => Mods.Settings.Movement.ChangeFlySpeed(), isTogglable = false },
            },

            new ButtonInfo[] { // Enabled Mods [4]
                //new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false },
            },

            new ButtonInfo[] { // Room Mods [5]
                //new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false },
                new ButtonInfo { buttonText = "join random lobby", method = () => Mods.Movement.JoinRandom(), isTogglable = false },
                new ButtonInfo { buttonText = "Disconnect", method = () => NetworkSystem.Instance.ReturnToSinglePlayer(), isTogglable = false },
                new ButtonInfo { buttonText = "LT DISCONNECT", method = () => Mods.Movement.LTdisconnet() },
                new ButtonInfo { buttonText = "Anti AFK", enableMethod =() => PhotonNetworkController.Instance.disableAFKKick = true, disableMethod =() => PhotonNetworkController.Instance.disableAFKKick = false, toolTip = "Doesn't let you get kicked for being AFK."},
            },

            new ButtonInfo[] { // Movement Mods [6]
                //new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false },
                new ButtonInfo { buttonText = "Platforms", method = () => Movement.PlatformModbysigmaboy() },
                new ButtonInfo { buttonText = "Sticky Platforms", method = () => Movement.StickyPlatforms() },
                new ButtonInfo { buttonText = "Frozone", method = () => Movement.Frozone() },
                new ButtonInfo { buttonText = "Fly", method = () => Movement.Fly() },
                //new ButtonInfo { buttonText = "Teleport Gun", method = () => Movement.TeleportGun() },
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
                new ButtonInfo { buttonText = "Slingshot Fly", method = () => Movement.Slingshotfly(false) },
                new ButtonInfo { buttonText = "Zero Slingshot Fly", method = () => Movement.Slingshotfly(true) },
                //new ButtonInfo { buttonText = "invs monke", method = () => Movement.Invismonk() },
                new ButtonInfo { buttonText = "Long Arms", method = () => Movement.Longarms(11f), toolTip = "Makes Your Arms Longer!" },

                new ButtonInfo { buttonText = "Bird Fly", method = Movement.BirdFly, toolTip = "Makes you fly like a bird when you flap your wings."},
                //new ButtonInfo { buttonText = "Iron Monke", method = () => IronMan(), isTogglable = true },
                
                //new ButtonInfo { buttonText = "Solid Water", aliases = new[] { "Jesus" }, enableMethod = Movement.SolidWater, disableMethod = Movement.FixWater, toolTip = "Makes the water solid in the beach map." },
                //new ButtonInfo { buttonText = "Disable Water", enableMethod = Movement.DisableWater, disableMethod = Movement.FixWater, toolTip = "Disables the water in the beach map." },
                //new ButtonInfo { buttonText = "Air Swim", aliases = new[] { "Fish" }, method = Movement.AirSwim, disableMethod = Movement.DisableAirSwim, toolTip = "Puts you in a block of water, letting you swim in the air." },
                //new ButtonInfo { buttonText = "Fast Swim", method =() => Movement.SetSwimSpeed(10f), disableMethod =() => Movement.SetSwimSpeed(), toolTip = "Lets you swim faster in water." },
                //new ButtonInfo { buttonText = "Water Run Helper", overlapText = "Water Run", enableMethod =() => Movement.WaterRunHelper(true), disableMethod =() => Movement.WaterRunHelper(false), toolTip = "Adds back water running to the game." },
            },

            new ButtonInfo[] { // Safety Mods [7]
                //new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false },
                new ButtonInfo { buttonText = "Anti Report", method = () => Safety.AntiReportDisconnect() },
                new ButtonInfo { buttonText = "Visualize Anti Report", method = Safety.VisualizeAntiReport, toolTip = "Visualizes the distance threshold for the anti report mods."},
                new ButtonInfo { buttonText = "Anti Report Join Random", method = () => Safety.AntiReportJoinRand() },
                new ButtonInfo { buttonText = "Flush Rpcs Dont Spam", method = () => Movement.FlushRPCs(), isTogglable = false },
                new ButtonInfo { buttonText = "Auto Clear Cache", method = Safety.AutoClearCache, toolTip = "Automatically clears your game's cache (garbage collector) every minute to prevent memory leaks."},
                new ButtonInfo { buttonText = "Disable Fingers", method = () => Movement.DisableFingers(), toolTip = "Good for plats"},
                new ButtonInfo { buttonText = "Panic", method = () => Movement.Panic(), isTogglable = false, toolTip = "Turns off all of the mods."},
            },

            new ButtonInfo[] { // Game Mods [8]
                //new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false },
                new ButtonInfo { buttonText = "PC Button Click", method = Important.PCButtonClick, disableMethod = Important.DisablePCButtonClick },
                new ButtonInfo { buttonText = "First Person Camera", enableMethod = Important.EnableFPC, method = Important.MoveFPC, disableMethod = Important.DisableFPC },
                new ButtonInfo { buttonText = "Get ID Self", method = Important.CopySelfID, isTogglable = false, toolTip = "Gets your player ID and copies it to the clipboard."},
                new ButtonInfo { buttonText = "Unlock FPS", method = Important.UncapFPS, disableMethod =() => Application.targetFrameRate = 144 },
                new ButtonInfo { buttonText = "Disable Air", overlapText = "Disable Wind Barriers", enableMethod =() => { ForcePatches.enabled = true; GetObject("Environment Objects/LocalObjects_Prefab/Forest/Environment/Forest_ForceVolumes/").SetActive(false); GetObject("Environment Objects/LocalObjects_Prefab/ForestToHoverboard/TurnOnInForestAndHoverboard/ForestDome_CollisionOnly").SetActive(false); }, disableMethod =() => { ForcePatches.enabled = false; GetObject("Environment Objects/LocalObjects_Prefab/Forest/Environment/Forest_ForceVolumes/").SetActive(true); GetObject("Environment Objects/LocalObjects_Prefab/ForestToHoverboard/TurnOnInForestAndHoverboard/ForestDome_CollisionOnly").SetActive(true); }, toolTip = "Disables the wind barriers in every map." },
                new ButtonInfo { buttonText = "Unlock Fan Club Subscription", enableMethod =() => SubscriptionPatches.enabled = true, disableMethod =() => SubscriptionPatches.enabled = false, toolTip = "Unlocks the Gorilla Tag fan club subscription." },
                new ButtonInfo { buttonText = "PC Controller Emulation", method = Important.PCControllerEmulation },
                new ButtonInfo { buttonText = "Close Application", method = () => Mods.Movement.closegame(), isTogglable = false },
                new ButtonInfo { buttonText = "120 FPS", method = () => Mods.Movement.ofps(), disableMethod = Movement.FixFPS},
                new ButtonInfo { buttonText = "60 FPS", method = () => Mods.Movement.sfps(), disableMethod = Movement.FixFPS },
                new ButtonInfo { buttonText = "40 FPS", method = () => Mods.Movement.Ffps(), disableMethod = Movement.FixFPS },
                new ButtonInfo { buttonText = "20 FPS", method = () => Mods.Movement.Tfps(), disableMethod = Movement.FixFPS },
                new ButtonInfo { buttonText = "5 FPS", method = () => Mods.Movement.ffps(), disableMethod = Movement.FixFPS },
                new ButtonInfo { buttonText = "Fps Boost", method = () => Mods.Movement.FPSBoostIndev() },
            },

            new ButtonInfo[] { // Visual Mods [9]
                //new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false },
                new ButtonInfo { buttonText = "Tracers", method = () => Movement.Tracer() },
                new ButtonInfo { buttonText = "Beacons", method = () => Movement.beacons() },
                new ButtonInfo { buttonText = "Cubes", method = () => Movement.Box_ESP() },
                new ButtonInfo { buttonText = "Chams", method = () => Movement.chams(), disableMethod = Movement.ChamsOff},
                new ButtonInfo { buttonText = "Nametags",  method = () => InfoTagManager.UpdateTracker(), disableMethod = () => InfoTagManager.ToggleInformationNameTags(false), isTogglable = true},
                new ButtonInfo { buttonText = "Clear Notifs", method = () => NotifiLib.ClearAllNotifications(), isTogglable = false },
            },

            new ButtonInfo[] { // Fun Mods [10]
                //new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false },
                new ButtonInfo { buttonText = "Fix Rig", method = () => Movement.FixRig(), isTogglable = false},
                new ButtonInfo { buttonText = "Spaz Head", method = () => Movement.SpazHead()},
                new ButtonInfo { buttonText = "Bounce", method = () => Movement.Bouncy(), disableMethod = Movement.ResetBouncy },
                new ButtonInfo { buttonText = "Platfom Spam", method = () => Movement.PlatformSpam() },
                new ButtonInfo { buttonText = "Upsidedown Head", method = () => Movement.UpsideDownHead(), disableMethod = Movement.FixRig },
                new ButtonInfo { buttonText = "Broken Neck", method = () => Movement.BrokenNeck(), disableMethod = Movement.FixRig },
                new ButtonInfo { buttonText = "Rainbow Bracelet", enableMethod = () => Movement.RainbowBracelet(), disableMethod = () => Movement.disableRainbowBracelet() },
                new ButtonInfo { buttonText = "Set Quest Score To 100k", method = () => Movement.addqueststuff(100000), disableMethod = () => Mods.Movement.Resetqueststuff() },
                new ButtonInfo { buttonText = "Spawn Hoverboard", method = Movement.SpawnHowerdBoard, disableMethod = Movement.DisableHoverboard, toolTip = "Gives you the hoverboard no matter where you are."},
               // new ButtonInfo { buttonText = "Copy Gun", method = () => Movement.Copygun(), toolTip = "Lock on to a player to copy them!"}
               new ButtonInfo { buttonText = "Zero Gravity(CS)", method = () => Gravityhelper(true)},
               new ButtonInfo { buttonText = "High Gravity(CS)", method = () => Gravityhelper(false)},
               //new ButtonInfo { buttonText = "Fast Hoverboard", method = () => Movement.Fasthoverboarderr()},
               //new ButtonInfo { buttonText = "Fling on grab(OP)", method = () => Movement.Flingongrab(100f)},
            },

            new ButtonInfo[] { // OP Mods [11]
                //new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false },
                new ButtonInfo { buttonText = "Ghost Money", method = () => Movement.AddCurrencySelf() },
                new ButtonInfo { buttonText = "Lag server", method = () => Movement.LagServer(), toolTip = "Lags the server"},
                
                
                //new ButtonInfo { buttonText = "Unlock VIM door", method = () => Movement.Disablesubdoor(), isTogglable = false },
                new ButtonInfo { buttonText = "Flash Gray Screen", method = () => Movement.FlashGrayScreenSSAll(), isTogglable = true },
                new ButtonInfo { buttonText = "Gray Screen/No gravity(SS)(M)", enableMethod = () => Movement.GrayScreenThing(true), disableMethod  = () => Movement.GrayScreenThing(false)},
                new ButtonInfo { buttonText = "Guardian Self(SS)(M)", enableMethod = () => Guardianhelper(true, false), disableMethod = () => Guardianhelper(false, false), toolTip = "Makes you guardian"},
                new ButtonInfo { buttonText = "Guardian All(SS)(M)", enableMethod = () => Guardianhelper(false, true), disableMethod = () => Guardianhelper(false, false), toolTip = "Makes you guardian"},
            },

            new ButtonInfo[] { // Sounds [12]s
                new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false },
                new ButtonInfo { buttonText = "Soon", method = () => currentCategory = 0, isTogglable = false},
                //new ButtonInfo { buttonText = "Crystal sound spam", method = () => Movement.CrystalSoundSpam() },
                //new ButtonInfo { buttonText = "Squeak sound spam", method = () => Movement.SqueakSoundSpam() },
                //new ButtonInfo { buttonText = "Siren sound spam", method = () => Movement.SirenSoundSpam() },
            },

            new ButtonInfo[] { // Admin Mods [13]
                new ButtonInfo { buttonText = "Roblox Sword", method = () => AdminTest.somethingidk(), disableMethod = () => AdminTest.destroysomethingidk(), isTogglable = true },
                new ButtonInfo { buttonText = "Cucaracha", method = () => AdminTest.Cucaracha2(), disableMethod = () => AdminTest.destroyCucaracha2(), isTogglable = true, toolTip = ""},
                new ButtonInfo { buttonText = "Block", enableMethod =() => AdminTest.blcok(), disableMethod =() => AdminTest.destroyBlock(), isTogglable = true, toolTip = "Spawns a block cuz why not"},
                new ButtonInfo { buttonText = "Menu User Name Tags", enableMethod = AdminTest.EnableAdminMenuUserTags, method = AdminTest.UpdateNameTagPositions, disableMethod = AdminTest.DisableAdminMenuUserTags, adminOnly = true, toolTip = "Puts nametags on menu users.", isTogglable = true},
                new ButtonInfo { buttonText = "Unlock All Cosmetics", method = Important.UnlockAllCosmetics, toolTip = "Unlocks every cosmetic in the game. This mod is client-sided." },
                new ButtonInfo { buttonText = "Admin Levitate All", method = Admin.FlyAllUsing, adminOnly = true },
                new ButtonInfo { buttonText = "GetMenuUsers", method = Admin.GetMenuUsers, isTogglable = false, adminOnly = true },
                new ButtonInfo { buttonText = "Send message to all", method = Admin.sigmaboy, isTogglable = false, adminOnly = true },
                new ButtonInfo { buttonText = "Admin Laser", method = Admin.laser, adminOnly = true },
                //new ButtonInfo { buttonText = "Menu User Name Tags", enableMethod = AdminTest.EnableAdminMenuUserTags, method = AdminTest.UpdateNameTagPositions, disableMethod = AdminTest.DisableAdminMenuUserTags, toolTip = "Puts nametags on menu users."},
                new ButtonInfo { buttonText = "Menu User Tracers", enableMethod = Admin.EnableAdminMenuUserTracers, method = Admin.MenuUserTracers, disableMethod =() => {Admin.isLineRenderQueued = true;}, toolTip = "Puts tracers on your right hand to menu users.", isTogglable = true},
            },

            new ButtonInfo[] { // SuperAdmin Mods [14]
                //new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false },
                new ButtonInfo { buttonText = "Admin Kick Gun", method = Admin.AdminKickGun, adminOnly = true, isTogglable = true},
                new ButtonInfo { buttonText = "Admin Kick All", method = Admin.KickAll, isTogglable = false, adminOnly = true },
                new ButtonInfo { buttonText = "Admin Bring All", method = Admin.BringAllUsing, adminOnly = true },
                new ButtonInfo { buttonText = "Admin Bring Gun", method = Admin.AdminBringGun, adminOnly = true },
                new ButtonInfo { buttonText = "Admin Bouncy All", method = Admin.BouncyAllUsing, adminOnly = true },
                new ButtonInfo { buttonText = "Admin Levitate All", method = Admin.FlyAllUsing, adminOnly = true },
                new ButtonInfo { buttonText = "GetMenuUsers", method = Admin.GetMenuUsers, isTogglable = false, adminOnly = true },
                new ButtonInfo { buttonText = "Send message to all", method = Admin.sigmaboy, isTogglable = false, adminOnly = true },
                new ButtonInfo { buttonText = "Admin Laser", method = Admin.laser, adminOnly = true },
                new ButtonInfo { buttonText = "Menu User Name Tags", enableMethod = AdminTest.EnableAdminMenuUserTags, method = AdminTest.UpdateNameTagPositions, disableMethod = AdminTest.DisableAdminMenuUserTags, toolTip = "Puts nametags on menu users.", isTogglable = true},
                new ButtonInfo { buttonText = "Menu User Tracers", enableMethod = Admin.EnableAdminMenuUserTracers, method = Admin.MenuUserTracers, disableMethod =() => {Admin.isLineRenderQueued = true;}, toolTip = "Puts tracers on your right hand to menu users.", isTogglable = true},
                
                //new ButtonInfo { buttonText = "Admin Laser", method = Important.ConsoleBeacon(), adminOnly = true },
                new ButtonInfo { buttonText = "Admin Fake Cosmetics", overlapText = "Admin Spoof Cosmetics", method =() => Admin.AdminSpoofCosmetics(), enableMethod =() => { NetworkSystem.Instance.OnPlayerJoined += Admin.OnPlayerJoinSpoof; Admin.AdminSpoofCosmetics(true); }, disableMethod =() => { NetworkSystem.Instance.OnPlayerJoined -= Admin.OnPlayerJoinSpoof; Admin.oldCosmetics = null; }, toolTip = "Makes everyone using the menu see whatever cosmetics you have on as if you owned them."},
                new ButtonInfo { buttonText = "Unlock All Cosmetics", method = Important.UnlockAllCosmetics, toolTip = "Unlocks every cosmetic in the game. This mod is mod-sided." },
                new ButtonInfo { buttonText = "Admin Strangle", method = Admin.AdminStrangle, toolTip = "Strangles whoever you grab if they're using the menu."},
                
                new ButtonInfo { buttonText = "Admin Jumpscare All", method = Admin.AdminJumpscareAll, isTogglable = false, toolTip = "Jumpscares everyone using the menu."},
                new ButtonInfo { buttonText = "Admin Fractals <color=grey>[</color><color=green>T</color><color=grey>]</color>", method = Admin.AdminFractals, toolTip = "Shines white lines out of your body when holding <color=green>trigger</color>."},
            },
            
            new ButtonInfo[] { // GunLib Settings [15]
                //new ButtonInfo { buttonText = "Return to Settings", method = () => currentCategory = 1, isTogglable = false },
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
            
            new ButtonInfo[] { // TagMods Category [16]
                //new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false },
                new ButtonInfo { buttonText = "Tag All", method = () => Movement.tg() },
                new ButtonInfo { buttonText = "Tag Gun", method = () => Important.TagGun() },
                new ButtonInfo { buttonText = "Tag Self", method = () => OverPowred.TagSelf() },
            },
            
            new ButtonInfo[] { // Owner Category [17]
                //new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false },
                new ButtonInfo { buttonText = "Tag All", method = () => Movement.tg() },
                new ButtonInfo { buttonText = "Tag Gun", method = () => Important.TagGun() },
                //new ButtonInfo { buttonText = "Tag Self", method = () => OverPowred.TagSelf() },
            },
        };
    }
}
