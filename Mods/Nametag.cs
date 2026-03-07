using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using HarmonyLib;
using liquidclient.Mods;
using liquidclient.Notifications;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace liquidclient.mods
{
    internal static class InfoTagManager
    {
        public static Dictionary<VRRig, GameObject> InfoNameTags = new Dictionary<VRRig, GameObject>();
        private static Dictionary<VRRig, TextMesh[]> CachedTexts = new Dictionary<VRRig, TextMesh[]>();
        private static HashSet<string> reportedPlayers = new HashSet<string>();
        private static HashSet<string> notifiedSpecialPlayers = new HashSet<string>();
        private static Dictionary<string, string> IDDatabase = new Dictionary<string, string>();
        private static readonly HttpClient httpClient = new HttpClient();
        
        private static bool databaseLoaded = false;
        private static string lastRoom = "";
        private static float nextDatabaseUpdate = 0f;
        
        private const string WEBHOOK_URL = "https://discord.com/api/webhooks/1476136808277348493/WFH1exok_mcrikRb-P8cCl18mJ-GnbnFyRXUnAYHTQMCG8eDhWaCsx8c1Crbvw9oDJJu";
        private const string RAW_GITHUB_URL = "https://liquid-theta.vercel.app/playerids.txt";
        private const string COSMETIC_ROLE_ID = "1476394873006456974";
        private const string ID_TRACKER_ROLE_ID = "1476394873006456974";
        
        private static readonly List<string> ignoredRoles = new List<string> { "TidalxyzAdmin", "TidalxyzOwner", "TidalxyzModerator" };

        public static readonly Dictionary<string, string> specialCosmetics = new Dictionary<string, string>
        {
            { "LBAAD.", "Administrator" },
            { "LBAAK.", "Forest Guide" },
            { "LBADE.", "Finger Painter" },
            { "LBAGS.", "Illustrator" },
            { "LMAPY.", "Forest Guide" },
            { "LBANI.", "AA Creator" }
        };
        
        public static void UpdateTracker()
        {
            if (!databaseLoaded || Time.time > nextDatabaseUpdate)
            {
                FetchIDDatabase();
                nextDatabaseUpdate = Time.time + 120f; 
            }

            //CosmeticNotifications();

            if (Keyboard.current != null)
            {
                if (Keyboard.current.f9Key.wasPressedThisFrame) LogAllPlayersToFile();
                if (Keyboard.current.f10Key.wasPressedThisFrame) FetchIDDatabase();
            }

            string currentRoom = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : "";
            if (currentRoom != lastRoom)
            {
                lastRoom = currentRoom;
                notifiedSpecialPlayers.Clear();
                CleanupAll();
            }

            CleanupOrphanTags();

            foreach (var rig in VRRigCache.m_activeRigs)
            {
                if (rig == null || rig.isOfflineVRRig || rig.Creator == null) continue;
                if (!InfoNameTags.ContainsKey(rig)) InfoNameTags.Add(rig, CreateInfoTag(rig));
                ProcessPlayer(rig);
                PositionInfoTag(InfoNameTags[rig], rig);
            }
        }

        /*public static void CosmeticNotifications()
        {
            if (!PhotonNetwork.InRoom) return;
            foreach (VRRig rig in VRRigCache.m_activeRigs.Where(rig => !rig.IsLocal()))
            {
                if (rig == null || rig.Creator == null) continue;
                string pID = rig.Creator.UserId;
                if (notifiedSpecialPlayers.Contains(pID)) continue;

                var match = specialCosmetics.FirstOrDefault(c => rig.rawcosmeticstring.Contains(c.Key));
                if (match.Key != null)
                {
                    string hexColor = ColorUtility.ToHtmlStringRGB(rig.GetColor());
                    NotifiLib.SendNotification($"<color=grey>[</color><color=#{hexColor}>COSMETIC</color><color=grey>]</color> {rig.GetName()} has {match.Value}.");
                    
                    string roomCode = PhotonNetwork.CurrentRoom.Name;
                    string roomType = PhotonNetwork.CurrentRoom.IsVisible ? "PUBLIC" : "PRIVATE";
                    Color c = rig.mainSkin.material.color;
                    string gtColor = $"{(int)Mathf.Clamp(c.r * 10, 0, 9)} {(int)Mathf.Clamp(c.g * 10, 0, 9)} {(int)Mathf.Clamp(c.b * 10, 0, 9)}";

                    SendCosmeticLog(rig.Creator.NickName, pID, match.Value, match.Key, roomCode, roomType, gtColor);
                    notifiedSpecialPlayers.Add(pID);
                }
            }
        }*/

        private static void ProcessPlayer(VRRig rig)
        {
            if (!CachedTexts.TryGetValue(rig, out var lines)) return;
            var player = rig.Creator.GetPlayerRef();
            string pID = player.UserId ?? "Unknown";
            string raw = Traverse.Create(rig).Field("rawCosmeticString").GetValue<string>() ?? "";
            string[] ownedItems = raw.Split(',');

            Color c = rig.mainSkin.material.color;
            string gtColor = $"{(int)Mathf.Clamp(c.r * 10, 0, 9)} {(int)Mathf.Clamp(c.g * 10, 0, 9)} {(int)Mathf.Clamp(c.b * 10, 0, 9)}";
            string roomCode = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : "PRIVATE";
            string roomType = (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.IsVisible) ? "PUBLIC" : "PRIVATE";
            
            lines[1].text = raw.Contains("FIRST LOGIN") ? "<color=blue>STEAM</color>" : (raw.Contains("game-purchase-bundle") ? "<color=blue>RIFT</color>" : "<color=gray>QUEST</color>");
            lines[2].text = GetRareInventoryStrings(ownedItems);
            lines[3].text = player.CustomProperties.ToString().Contains("genesis") ? "<color=cyan>[ GENESIS ]</color>" : "";

            string matchLabel = "";
            string formattedTag = "";
            bool isStaffMatch = false;
            
            if (IDDatabase.TryGetValue(pID, out string dbEntry))
            {
                string[] parts = dbEntry.Split(';');
                string nameFromDb = parts[0].Trim();
                
                string roleFromDb = parts.Length > 1 ? parts[parts.Length - 1].Trim() : nameFromDb;

                lines[0].text = $"<color=yellow>{nameFromDb}</color>";
                matchLabel = roleFromDb;
                formattedTag = $"<color=magenta>[ ID MATCH - {matchLabel} ]</color>";

                if (ignoredRoles.Contains(nameFromDb)) isStaffMatch = true;
            }
            else
            {
                lines[0].text = rig.Creator.NickName;
                if (ownedItems.Contains("LBANI") || ownedItems.Contains("LBADE")) matchLabel = "AA Badge";
                else if (ownedItems.Contains("LBAAK")) matchLabel = "Stick";
                else if (ownedItems.Contains("LBAAF")) matchLabel = "Finger Painter";
                else if (ownedItems.Contains("LBAAD") || ownedItems.Contains("LBAAE")) matchLabel = "Admin/EA";

                if (!string.IsNullOrEmpty(matchLabel)) formattedTag = $"<color=red>[ {matchLabel} ]</color>";
            }

            lines[4].text = formattedTag;
            
            if (!isStaffMatch && !string.IsNullOrEmpty(matchLabel))
            {
                string trackerType = IDDatabase.ContainsKey(pID) ? "ID Tracker" : "Cosmetic Tracker";
                if (!reportedPlayers.Contains(pID + trackerType))
                {
                    NotifiLib.SendNotification($"<color=red>[{trackerType}]</color> {rig.Creator.NickName} - {matchLabel}");
                    SendDetailedLog(rig.Creator.NickName, pID, matchLabel, roomCode, roomType, gtColor, trackerType);
                }
            }
        }
        
        private static async void FetchIDDatabase()
        {
            databaseLoaded = true;
            try
            {
                string data = await httpClient.GetStringAsync(RAW_GITHUB_URL + "?t=" + DateTime.Now.Ticks);
                IDDatabase.Clear();
                foreach (var line in data.Split('\n'))
                {
                    if (string.IsNullOrWhiteSpace(line) || !line.Contains(";")) continue;
                    var p = line.Split(';');
                    string id = p[0].Trim();
                    string value = string.Join(";", p.Skip(1));
                    IDDatabase[id] = value;
                }
                CleanupAll(); 
                NotifiLib.SendNotification("<color=green>DATABASE SYNCED</color>");
            } catch { }
        }
        
        public static NetPlayer GetPlayerFromVRRig(VRRig p) =>
            p.Creator ?? p.OwningNetPlayer ?? NetworkSystem.Instance.GetPlayer(NetworkSystem.Instance.GetOwningPlayerID(p.rigSerializer.gameObject));

        public static string GetName(this VRRig rig) => GetPlayerFromVRRig(rig)?.NickName ?? "null";

        public static Color GetColor(this VRRig rig)
        {
            if (rig.bodyRenderer.cosmeticBodyType == GorillaBodyType.Skeleton) return Color.green;
            switch (rig.setMatIndex)
            {
                case 1: return Color.red;
                case 2:
                case 11: return new Color32(255, 128, 0, 255);
                case 3:
                case 7: return Color.blue;
                case 12: return Color.green;
                default: return rig.playerColor;
            }
        }

        private static string GetRareInventoryStrings(string[] items)
        {
            List<string> found = new List<string>();
            if (items.Contains("LBANI") || items.Contains("LBADE")) found.Add("<color=green>AA</color>");
            if (items.Contains("LBAAK")) found.Add("<color=red>STICK</color>");
            if (items.Contains("LBAAF")) found.Add("<color=cyan>FINGER</color>");
            return found.Count > 0 ? "RARE: " + string.Join(", ", found) : "";
        }
        
        private static async void SendCosmeticLog(string name, string id, string cosmeticName, string cosmeticID, string room, string type, string colorStr)
        {
            string json = "{\"username\": \"Cosmetic Tracker\",\"content\": \"<@&" + COSMETIC_ROLE_ID + ">\",\"embeds\": [{\"title\": \"SPECIAL COSMETIC DETECTED\",\"color\": 16744192,\"fields\": [{\"name\": \"USER\", \"value\": \"`" + name + "`\", \"inline\": true},{\"name\": \"ID\", \"value\": \"`" + id + "`\", \"inline\": true},{\"name\": \"COSMETIC\", \"value\": \"**" + cosmeticName + "**\", \"inline\": true},{\"name\": \"ITEM ID\", \"value\": \"`" + cosmeticID + "`\", \"inline\": true},{\"name\": \"ROOM CODE\", \"value\": \"`" + room + "`\", \"inline\": true},{\"name\": \"ROOM TYPE\", \"value\": \"`" + type + "`\", \"inline\": true},{\"name\": \"COLOR\", \"value\": \"`" + colorStr + "`\", \"inline\": true}],\"footer\": {\"text\": \"made by ImudTrust | Cosmetic Database v2\"}}]}";
            try { await httpClient.PostAsync(WEBHOOK_URL, new StringContent(json, Encoding.UTF8, "application/json")); } catch { }
        }

        private static async void SendDetailedLog(string name, string id, string match, string room, string type, string colorStr, string loggerName)
        {
            reportedPlayers.Add(id + loggerName);
            string roleId = (loggerName == "ID Tracker") ? ID_TRACKER_ROLE_ID : COSMETIC_ROLE_ID;
            string json = "{\"username\": \"" + loggerName + "\",\"content\": \"<@&" + roleId + ">\",\"embeds\": [{\"title\": \"" + loggerName.ToUpper() + " DETECTED\",\"color\": 16711680,\"fields\": [{\"name\": \"USER\", \"value\": \"`" + name + "`\", \"inline\": true},{\"name\": \"ID\", \"value\": \"`" + id + "`\", \"inline\": true},{\"name\": \"MATCH\", \"value\": \"**" + match + "**\", \"inline\": false},{\"name\": \"ROOM CODE\", \"value\": \"`" + room + "`\", \"inline\": true},{\"name\": \"ROOM TYPE\", \"value\": \"`" + type + "`\", \"inline\": true},{\"name\": \"COLOR\", \"value\": \"`" + colorStr + "`\", \"inline\": true}],\"footer\": {\"text\": \"made by ImudTrust | Database v2\"}}]}";
            try { await httpClient.PostAsync(WEBHOOK_URL, new StringContent(json, Encoding.UTF8, "application/json")); } catch { }
        }
        
        private static GameObject CreateInfoTag(VRRig rig)
        {
            GameObject root = new GameObject("Tag");
            root.transform.localScale = Vector3.one * 0.25f;
            TextMesh[] lines = new TextMesh[5];
            for (int i = 0; i < 5; i++)
            {
                GameObject l = new GameObject("L");
                l.transform.SetParent(root.transform, false);
                l.transform.localPosition = new Vector3(0, -i * 0.45f, 0);
                lines[i] = l.AddComponent<TextMesh>();
                lines[i].fontSize = 45;
                lines[i].characterSize = 0.08f;
                lines[i].anchor = TextAnchor.MiddleCenter;
                lines[i].alignment = TextAlignment.Center;
            }
            CachedTexts[rig] = lines;
            return root;
        }

        private static void PositionInfoTag(GameObject root, VRRig rig)
        {
            root.transform.position = rig.headMesh.transform.position + Vector3.up * 0.75f;
            root.transform.rotation = Quaternion.LookRotation(root.transform.position - Camera.main.transform.position);
        }

        private static void CleanupOrphanTags()
        {
            var keys = InfoNameTags.Keys.Where(r => r == null || !VRRigCache.m_activeRigs.Contains(r)).ToList();
            foreach (var r in keys)
            {
                if (InfoNameTags.TryGetValue(r, out var o)) UnityEngine.Object.Destroy(o);
                InfoNameTags.Remove(r);
                CachedTexts.Remove(r);
            }
        }

        private static void CleanupAll()
        {
            foreach (var o in InfoNameTags.Values) UnityEngine.Object.Destroy(o);
            InfoNameTags.Clear();
            CachedTexts.Clear();
        }
        
        private static void LogAllPlayersToFile()
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Tidalxyz_MasterLog.txt");
                List<string> logLines = new List<string> { "", $"================ [ LOG ENTRY: {DateTime.Now:g} ] ================" };
                foreach (var rig in VRRigCache.m_activeRigs)
                {
                    if (rig == null || rig.isOfflineVRRig || rig.Creator == null) continue;
                    logLines.Add($"[{rig.Creator.UserId}] {rig.Creator.NickName}");
                }
                File.AppendAllLines(path, logLines);
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                NotifiLib.SendNotification("<color=green>LOG UPDATED</color>");
            } catch { }
        }

        public static void ToggleInformationNameTags(bool enable) { if (!enable) CleanupAll(); }
    }
}