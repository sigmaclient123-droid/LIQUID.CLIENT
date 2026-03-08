using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Linq;
using static Console.Console;
using HarmonyLib;
using liquidclient.Classes;
using liquidclient.Menu;
using liquidclient.Notifications;

namespace Console
{
    public class ServerData : MonoBehaviour
    {
        public static bool ServerDataEnabled = true;
        public static bool DisableTelemetry = true;
        public static string ServerEndpoint = "Removed";
        public static readonly string ServerDataEndpoint = $"{ServerEndpoint}/serverdata";
        public static string LatestMenuVersion = "0.0.0";
        public static string DiscordInvite = "";
        public static string Status = "UNKNOWN";
        public static string MOTD = "";
        public static List<string> OwnerNames = new List<string>();
        public static string DefaultStatusColor = "#FFFFFF";

        public static readonly Dictionary<string, string> StatusColors = new Dictionary<string, string>();
        public static readonly Dictionary<string, string> Administrators = new Dictionary<string, string>();
        public static readonly List<string> SuperAdministrators = new List<string>();
        public static readonly List<string> BlacklistedIds = new List<string>();
        public static readonly Dictionary<string, string> Owners = new Dictionary<string, string>();

        public static ServerData instance;
        private static readonly List<string> DetectedModsLabelled = new List<string>();
        private static float DataLoadTime = -1f;
        private static float ReloadTime = -1f;
        private static int LoadAttempts;
        private static bool GivenAdminMods;
        public static bool OutdatedVersion;
        private static float DataSyncDelay;
        public static int PlayerCount;

        public static void SetupAdminPanel(string playername)
        {
            List<ButtonInfo> homeButtons = new List<ButtonInfo>(Buttons.buttons[0]);

            homeButtons.RemoveAll(b =>
                b.buttonText == "Admin" ||
                b.buttonText == "SuperAdmin" ||
                b.buttonText == "Owner"
            );

            string userId = PhotonNetwork.LocalPlayer.UserId;
            if (userId == null) return;

            bool isOwner = Owners.TryGetValue(userId, out string ownerName);
            bool isAdmin = Administrators.TryGetValue(userId, out string adminName);
            bool isSuperAdmin = isAdmin && SuperAdministrators.Contains(adminName);

            int originalDecay = NotifiLib.NoticationThreshold;
            NotifiLib.NoticationThreshold = 600;

            if (isOwner)
            {
                homeButtons.Add(new ButtonInfo
                {
                    buttonText = "Owner",
                    method = () => Main.currentCategory = 15,
                    isTogglable = false,
                    toolTip = "Owner tools"
                });

                NotifiLib.SendNotification(
                    $"(<color=gold>WELCOME OWNER</color>) Welcome, {ownerName}! Owner tools have been enabled."
                );
            }
            else if (isSuperAdmin)
            {
                homeButtons.Add(new ButtonInfo
                {
                    buttonText = "SuperAdmin",
                    method = () => Main.currentCategory = 14,
                    isTogglable = false,
                    toolTip = "Super admin tools"
                });

                NotifiLib.SendNotification(
                    $"(<color=magenta>WELCOME</color>) Welcome, {adminName}! Super admin mods have been enabled."
                );
            }
            else if (isAdmin)
            {
                homeButtons.Add(new ButtonInfo
                {
                    buttonText = "Admin",
                    method = () => Main.currentCategory = 13,
                    isTogglable = false,
                    toolTip = "Admin tools"
                });

                NotifiLib.SendNotification(
                    $"(<color=purple>WELCOME</color>) Welcome, {adminName}! Admin mods have been enabled."
                );
            }

            Buttons.buttons[0] = homeButtons.ToArray();
            NotifiLib.NoticationThreshold = originalDecay;
        }

        public static string GetDisplayCategoryName(string internalName)
        {
            return internalName switch
            {
                "Admin Panel" => "Admin",
                "Admin Test" => "Super Admin",
                "Owner Panel" => "Owner",
                _ => internalName
            };
        }

        public static bool IsOwner(string userId)
        {
            return !string.IsNullOrEmpty(userId) && Owners.ContainsKey(userId);
        }

        public static bool IsAdmin
        {
            get
            {
                string userId = PhotonNetwork.LocalPlayer?.UserId;
                return userId != null && (Administrators.ContainsKey(userId) || Owners.ContainsKey(userId));
            }
        }

        public static bool IsSuperAdmin
        {
            get
            {
                string userId = PhotonNetwork.LocalPlayer?.UserId;
                if (userId == null) return false;

                if (Owners.ContainsKey(userId))
                    return true;

                if (!Administrators.TryGetValue(userId, out string adminName))
                    return false;

                return SuperAdministrators.Contains(adminName);
            }
        }

        public static bool IsLocalPlayerOwner
        {
            get
            {
                string userId = PhotonNetwork.LocalPlayer?.UserId;
                return userId != null && Owners.ContainsKey(userId);
            }
        }

        public static string GetStatusColor(string status)
        {
            if (string.IsNullOrEmpty(status))
                return DefaultStatusColor;

            if (StatusColors.TryGetValue(status.ToUpper(), out string color))
                return color;

            return DefaultStatusColor;
        }

        public void Awake()
        {
            instance = this;
            DataLoadTime = Time.time + 5f;

            NetworkSystem.Instance.OnJoinedRoomEvent += OnJoinRoom;
            NetworkSystem.Instance.OnPlayerJoined += UpdatePlayerCount;
            NetworkSystem.Instance.OnPlayerLeft += UpdatePlayerCount;

            instance.StartCoroutine(HeartbeatLoop());
        }

        public void Update()
        {
            if (DataLoadTime > 0f && Time.time > DataLoadTime && GorillaComputer.instance.isConnectedToMaster)
            {
                DataLoadTime = Time.time + 5f;

                LoadAttempts++;
                if (LoadAttempts >= 3)
                {
                    Console.Log("Server data could not be loaded");
                    DataLoadTime = -1f;
                    return;
                }

                Console.Log("Attempting to load web data");
                instance.StartCoroutine(LoadServerData());
            }

            if (ReloadTime > 0f)
            {
                if (Time.time > ReloadTime)
                {
                    ReloadTime = Time.time + 60f;
                    instance.StartCoroutine(LoadServerData());
                }
            }
            else
            {
                if (GorillaComputer.instance.isConnectedToMaster)
                    ReloadTime = Time.time + 5f;
            }

            if (Time.time > DataSyncDelay || !PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.InRoom && PhotonNetwork.PlayerList.Length != PlayerCount)
                    instance.StartCoroutine(PlayerDataSync(PhotonNetwork.CurrentRoom.Name, PhotonNetwork.CloudRegion));

                PlayerCount = PhotonNetwork.InRoom ? PhotonNetwork.PlayerList.Length : -1;
            }
        }

        public static void OnJoinRoom()
        {
            instance.StartCoroutine(TelemetryRequest(PhotonNetwork.CurrentRoom.Name, PhotonNetwork.NickName, PhotonNetwork.CloudRegion, PhotonNetwork.LocalPlayer.UserId, PhotonNetwork.CurrentRoom.IsVisible, PhotonNetwork.PlayerList.Length, NetworkSystem.Instance.GameModeString));
            instance.StartCoroutine(SendHeartbeat());
        }

        public static string CleanString(string input, int maxLength = 12)
        {
            input = new string(Array.FindAll(input.ToCharArray(), c => Utils.IsASCIILetterOrDigit(c)));

            if (input.Length > maxLength)
                input = input[..(maxLength - 1)];

            input = input.ToUpper();
            return input;
        }

        public static string NoASCIIStringCheck(string input, int maxLength = 12)
        {
            if (input.Length > maxLength)
                input = input[..(maxLength - 1)];

            input = input.ToUpper();
            return input;
        }

        public static bool IsVersionOutdated(string localVersion, string serverVersion)
        {
            if (Version.TryParse(localVersion, out Version local) && Version.TryParse(serverVersion, out Version server))
            {
                return local.CompareTo(server) < 0;
            }
            return false;
        }

        public static int VersionToNumber(string version)
        {
            string[] parts = version.Split('.');
            if (parts.Length != 3)
                return -1;

            return int.Parse(parts[0]) * 100 + int.Parse(parts[1]) * 10 + int.Parse(parts[2]);
        }

        public static IEnumerator LoadServerData()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(ServerDataEndpoint))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Console.Log("Failed to load server data: " + request.error);
                    yield break;
                }

                string json = request.downloadHandler.text;
                DataLoadTime = -1f;

                JObject data = JObject.Parse(json);

                if (data["menu-version"] != null)
                {
                    LatestMenuVersion = data["menu-version"].ToString().Trim();

                    OutdatedVersion = IsVersionOutdated(Console.ConsoleVersion.Trim(), LatestMenuVersion);
                    Console.IsOutdated = OutdatedVersion;
                }

                if (data["discord-invite"] != null)
                {
                    DiscordInvite = data["discord-invite"].ToString();
                }
                else
                {
                    DiscordInvite = "";
                }

                if (data["status"] != null)
                {
                    Status = data["status"].ToString();
                }
                else
                {
                    Status = "UNKNOWN";
                }

                if (data["motd"] != null)
                {
                    MOTD = data["motd"].ToString();
                }
                else
                {
                    MOTD = "";
                }

                DefaultStatusColor = data["default-status-color"]?.ToString() ?? "#FFFFFF";

                StatusColors.Clear();
                if (data["status-colors"] != null)
                {
                    JObject colorsJson = (JObject)data["status-colors"];
                    foreach (var pair in colorsJson)
                    {
                        string statusKey = pair.Key.ToUpper();
                        string colorHex = pair.Value.ToString();
                        StatusColors[statusKey] = colorHex;
                    }
                }

                Administrators.Clear();
                SuperAdministrators.Clear();
                BlacklistedIds.Clear();
                Owners.Clear();

                JArray owners = (JArray)data["owners"];
                if (owners != null)
                {
                    foreach (var owner in owners)
                    {
                        string name = owner["name"].ToString();
                        string userId = owner["user-id"].ToString();
                        Owners[userId] = name;
                    }
                }

                JArray admins = (JArray)data["admins"];
                foreach (var admin in admins)
                {
                    string name = admin["name"].ToString();
                    string userId = admin["user-id"].ToString();
                    Administrators[userId] = name;
                }

                JArray superAdmins = (JArray)data["super-admins"];
                foreach (var superAdmin in superAdmins)
                    SuperAdministrators.Add(superAdmin.ToString());

                JArray blacklist = (JArray)data["blacklisted-ids"];
                if (blacklist != null)
                {
                    foreach (var id in blacklist)
                        BlacklistedIds.Add(id.ToString());
                }

                string localUserId = PhotonNetwork.LocalPlayer?.UserId;

                if (!string.IsNullOrEmpty(localUserId) && BlacklistedIds.Contains(localUserId))
                {
                    Console.Log("User is blacklisted. Closing application.");
                    Application.Quit();

#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif

                    yield break;
                }

                if (!GivenAdminMods && !string.IsNullOrEmpty(localUserId) && Owners.ContainsKey(localUserId))
                {
                    GivenAdminMods = true;
                    SetupAdminPanel(Owners[localUserId]);
                }
                else if (!GivenAdminMods && !string.IsNullOrEmpty(localUserId) && Administrators.TryGetValue(localUserId, out var administrator))
                {
                    GivenAdminMods = true;
                    SetupAdminPanel(administrator);
                }
            }

            yield return null;
        }

        public static IEnumerator SendHeartbeat()
        {
            if (DisableTelemetry || !PhotonNetwork.InRoom)
                yield break;

            UnityWebRequest request = new UnityWebRequest($"{ServerEndpoint}/heartbeat", "POST");

            string json = JsonConvert.SerializeObject(new
            {
                user_id = PhotonNetwork.LocalPlayer?.UserId ?? "unknown",
                consoleVersion = Console.ConsoleVersion,
                menuName = Console.MenuName,
                menuVersion = Console.MenuVersion,
                directory = PhotonNetwork.CurrentRoom?.Name ?? "none",
                region = PhotonNetwork.CloudRegion,
                playerCount = PhotonNetwork.PlayerList?.Length ?? 0
            });

            byte[] raw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(raw);
            request.SetRequestHeader("Content-Type", "application/json");
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();
        }

        public static IEnumerator TrackCommand(string command, params object[] parameters)
        {
            if (DisableTelemetry)
                yield break;

            UnityWebRequest request = new UnityWebRequest($"{ServerEndpoint}/command", "POST");

            string json = JsonConvert.SerializeObject(new
            {
                command = command,
                user_id = PhotonNetwork.LocalPlayer?.UserId ?? "unknown",
                parameters = parameters
            });

            byte[] raw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(raw);
            request.SetRequestHeader("Content-Type", "application/json");
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();
        }

        public static IEnumerator HeartbeatLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(30f);

                if (PhotonNetwork.InRoom && !DisableTelemetry)
                {
                    instance.StartCoroutine(SendHeartbeat());
                }
            }
        }

        public static IEnumerator TelemetryRequest(string directory, string identity, string region, string userid, bool isPrivate, int playerCount, string gameMode)
        {
            if (DisableTelemetry)
                yield break;

            UnityWebRequest request = new UnityWebRequest(ServerEndpoint + "/telemetry", "POST");

            string json = JsonConvert.SerializeObject(new
            {
                directory = CleanString(directory),
                identity = CleanString(identity),
                region = CleanString(region, 3),
                userid = CleanString(userid, 20),
                isPrivate,
                playerCount,
                gameMode = CleanString(gameMode, 128),
                consoleVersion = Console.ConsoleVersion,
                menuName = Console.MenuName,
                menuVersion = Console.MenuVersion
            });

            byte[] raw = Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(raw);
            request.SetRequestHeader("Content-Type", "application/json");
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            yield return SendHeartbeat();
        }

        public static void UpdatePlayerCount(NetPlayer Player) =>
            PlayerCount = -1;

        public static bool IsPlayerSteam(VRRig Player)
        {
            string concat = Traverse.Create(Player)
                .Field("rawCosmeticString")
                .GetValue<string>() ?? "";

            int customPropsCount = Player.Creator.GetPlayerRef().CustomProperties.Count;

            if (concat.Contains("S. FIRST LOGIN")) return true;
            if (concat.Contains("FIRST LOGIN") || customPropsCount >= 2) return true;
            if (concat.Contains("LMAKT.")) return false;

            return false;
        }

        public static IEnumerator PlayerDataSync(string directory, string region)
        {
            if (DisableTelemetry)
                yield break;

            DataSyncDelay = Time.time + 3f;
            yield return new WaitForSeconds(3f);

            if (!PhotonNetwork.InRoom)
                yield break;

            Dictionary<string, Dictionary<string, string>> data = new Dictionary<string, Dictionary<string, string>>();

            foreach (Player identification in PhotonNetwork.PlayerList)
            {
                VRRig rig = Console.GetVRRigFromPlayer(identification) ?? VRRig.LocalRig;

                string cosmetics = Traverse.Create(rig)
                    .Field("rawCosmeticString")
                    .GetValue<string>() ?? "";

                data.Add(
                    identification.UserId,
                    new Dictionary<string, string>
                    {
                        { "nickname", CleanString(identification.NickName) },
                        { "cosmetics", cosmetics },
                        { "color", $"{Math.Round(rig.playerColor.r * 255)} {Math.Round(rig.playerColor.g * 255)} {Math.Round(rig.playerColor.b * 255)}" },
                        { "platform", IsPlayerSteam(rig) ? "STEAM" : "QUEST" }
                    }
                );
            }

            UnityWebRequest request = new UnityWebRequest(ServerEndpoint + "/syncdata", "POST");

            string json = JsonConvert.SerializeObject(new
            {
                directory = CleanString(directory),
                region = CleanString(region, 3),
                data
            });

            byte[] raw = Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(raw);
            request.SetRequestHeader("Content-Type", "application/json");
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
        }
    }

}
