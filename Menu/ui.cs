using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using liquidclient.Classes;
using static liquidclient.Menu.Buttons;
using static liquidclient.Settings;
using System.Linq;
using Photon.Pun;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using Console;
using GorillaNetworking;
using Photon.Realtime;
using liquidclient.Notifications;
using liquidclient.Mods;
using static Console.ServerData;
using GorillaNetworking;

namespace liquidclient.Menu
{
    public class OnScreenGUI : MonoBehaviour
    {
        
        public static OnScreenGUI guiInstance;
        public static bool IsVisible = false;
        public static bool IsRoomJoinerVisible = true;
        public static bool HideAllHUD = false;
        private float hudFadeAlpha = 1f;
        
        private Rect windowRect = new Rect(30, 80, 700, 460);
        private float deltaTime = 0.0f;

        public struct MenuTheme {
            public string name;
            public Color accent;
            public MenuTheme(string n, Color acc) { name = n; accent = acc; }
        }

        public static List<MenuTheme> themes = new List<MenuTheme>() {
            new MenuTheme("Neon Blue", new Color(0f, 0.8f, 1f)),
            //new MenuTheme("Pop Pink", new Color(1f, 0.07f, 0.57f)),     
            /*new MenuTheme("Matrix", new Color(0f, 1f, 0.3f)),           
            new MenuTheme("Gold", new Color(1f, 0.84f, 0f)),          
            new MenuTheme("Purple", new Color(0.6f, 0f, 1f)),           
            new MenuTheme("Blood", new Color(1f, 0f, 0f)),              
            new MenuTheme("Midnight", Color.white)       */               
        };
        
        public static int currentThemeIndex = 0;

        private float animProgress = 0f;
        private float animSpeed = 5f;
        private List<RainDrop> rainDrops = new List<RainDrop>();
        
        private int onScreenPage = 0;
        private int localCategory = 0; 
        private string searchText = "";
        private string roomCode = "";
        private Vector2 categoryScroll = Vector2.zero;
        private Texture2D solidBackground;

        private Dictionary<string, float> modFades = new Dictionary<string, float>();
        private readonly string[] hiddenCategories = { "Home", "Menu Settings", "Movement Settings", "Settings", "Main Menu" }; 
        private static string[] saveExceptions = { "Menu User Name Tags", "Menu User Tracers", "Unlock All Cosmetics" };
        private static string savePath = Path.Combine(Paths.ConfigPath, "liquidclient.cfg");
        
        private HashSet<string> lastSavedMods = new HashSet<string>();

        private GUIStyle watermarkTextStyle;
        private float fpsSmooth;
        private float fpsTimer;
        private float fpsDisplay;
        
        private bool isLoaded = false;
        
        private List<RoomInfo> cachedRooms = new List<RoomInfo>();
        
        void Awake()
        {
            guiInstance = this;
        }

        public static void JoinCode(string i)
        {
            if (PhotonNetworkController.Instance != null)
            {
                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(i, GorillaNetworking.JoinType.Solo);
            }
        }
        
        void OnDestroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        /*private HashSet<string> GetEnabledNonAdminMods()
        {
            HashSet<string> result = new HashSet<string>();
            int max = Mathf.Min(buttons.Length, categoryNames.Length);
            for (int i = 0; i < max; i++)
            {
                if (categoryNames[i].ToLower().Contains("admin")) continue;
                var categoryButtons = buttons[i];
                if (categoryButtons == null) continue;
                foreach (var btn in categoryButtons)
                {
                    if (btn == null) continue;
                    if (btn.enabled && btn.isTogglable && !btn.adminOnly) result.Add(btn.buttonText);
                }
            }
            return result;
        }*/
        
        private HashSet<string> GetEnabledNonAdminMods()
        {
            HashSet<string> result = new HashSet<string>();
            int max = Mathf.Min(buttons.Length, categoryNames.Length);
            for (int i = 0; i < max; i++)
            {
                var categoryButtons = buttons[i];
                if (categoryButtons == null) continue;
                foreach (var btn in categoryButtons)
                {
                    if (btn == null) continue;
            
                    bool isException = saveExceptions.Contains(btn.buttonText);
                    bool isNormalMod = !categoryNames[i].ToLower().Contains("admin") && !btn.adminOnly;

                    if (btn.enabled && btn.isTogglable && (isNormalMod || isException)) 
                        result.Add(btn.buttonText);
                }
            }
            return result;
        }

        /*public static void JoinCode(string i)
        {
            if (PhotonNetworkController.Instance != null)
                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(i, 0);
        }*/

        private class RainDrop {
            public float x, y, speed, length;
            public RainDrop() { Reset(); y = Random.Range(0, Screen.height); }
            public void Reset() {
                x = Random.Range(0, Screen.width);
                y = -20;
                speed = Random.Range(200f, 500f);
                length = Random.Range(10f, 30f);
            }
        }

        public void Start() {
            for (int i = 0; i < 40; i++) rainDrops.Add(new RainDrop());
            LoadSavedMods();
            
            isLoaded = true;
            
            PhotonNetwork.AddCallbackTarget(this);
        }
        
        public void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            cachedRooms.Clear();
            foreach (var r in roomList)
                if (r.IsOpen && r.IsVisible)
                    cachedRooms.Add(r);
        }


        /*public static void SaveMods()
        {
            List<string> settings = new List<string>();
            settings.Add("ThemeIndex:" + currentThemeIndex);
            settings.Add("HideHUD:" + HideAllHUD.ToString()); 

            int max = Mathf.Min(buttons.Length, categoryNames.Length);
            for (int i = 0; i < max; i++)
            {
                if (categoryNames[i].ToLower().Contains("admin")) continue;
                var categoryButtons = buttons[i];
                if (categoryButtons == null) continue;
                foreach (var btn in categoryButtons)
                {
                    if (btn != null && btn.enabled && btn.isTogglable && !btn.adminOnly)
                        settings.Add(btn.buttonText);
                }
            }
            File.WriteAllLines(savePath, settings);
        }*/
        
        public static void SaveMods()
        {
            List<string> settings = new List<string>();
            settings.Add("ThemeIndex:" + currentThemeIndex);
            settings.Add("HideHUD:" + HideAllHUD.ToString()); 

            int max = Mathf.Min(buttons.Length, categoryNames.Length);
            for (int i = 0; i < max; i++)
            {
                var categoryButtons = buttons[i];
                if (categoryButtons == null) continue;

                foreach (var btn in categoryButtons)
                {
                    if (btn != null && btn.enabled && btn.isTogglable)
                    {
                        // Check if it's a normal mod OR if its name is in our exception list
                        bool isException = saveExceptions.Contains(btn.buttonText);
                        bool isNormalMod = !categoryNames[i].ToLower().Contains("admin") && !btn.adminOnly;

                        if (isNormalMod || isException)
                        {
                            settings.Add(btn.buttonText);
                        }
                    }
                }
            }
            File.WriteAllLines(savePath, settings);
        }

        public void LoadSavedMods()
        {
            if (!File.Exists(savePath)) return;
            string[] saved = File.ReadAllLines(savePath);
            int enabledCount = 0;
            
            foreach (var line in saved)
            {
                if (line.StartsWith("ThemeIndex:"))
                {
                    if (int.TryParse(line.Split(':')[1], out int index))
                        currentThemeIndex = Mathf.Clamp(index, 0, themes.Count - 1);
                }
                if (line.StartsWith("HideHUD:"))
                {
                    if (bool.TryParse(line.Split(':')[1], out bool hidden))
                        HideAllHUD = hidden;
                }
            }

            int max = Mathf.Min(buttons.Length, categoryNames.Length);
            foreach (var line in saved)
            {
                if (line.Contains(":") ) continue;

                for (int i = 0; i < max; i++)
                {
                    var categoryButtons = buttons[i];
                    if (categoryButtons == null) continue;

                    foreach (var btn in categoryButtons)
                    {
                        if (btn != null && btn.buttonText == line && btn.isTogglable && !btn.enabled)
                        {
                            bool isException = saveExceptions.Contains(btn.buttonText);
                            bool isNormalMod = !categoryNames[i].ToLower().Contains("admin") && !btn.adminOnly;

                            if (isNormalMod || isException)
                            {
                                Main.Toggle(btn.buttonText);
                                enabledCount++;
                                if (btn.buttonText == "First Person Camera") Important.EnableFPC();
                            }
                        }
                    }
                }
            }
            ThemeManager.ApplyColors();
            StartCoroutine(NotifyModsLoaded(enabledCount));
        }
        
        private IEnumerator NotifyModsLoaded(int count)
        {
            yield return new WaitForSeconds(5f);
            var notif = FindObjectOfType<NotifiLib>();
            if (notif != null)
            {
                if (count > 0) notif.SendMessage($"Loaded {count} mods");
                else notif.SendMessage("No saved mods to load");
            }
        }

        public void Update() {
            //HandleAutoJoinCycle();
            
            //if (Keyboard.current.insertKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame)
              //  IsVisible = !IsVisible;
              
              if (Keyboard.current.insertKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame)
              {
                  bool wasVisible = IsVisible;
                  
                  IsVisible = !IsVisible;
                  
                  if (IsVisible)
                  {
                      MenuAudioHandler.PlayOpeningSound();
                  }
                  else
                  {
                      MenuAudioHandler.PlayClosesound();
                  }
              }
            
            if (Keyboard.current.backslashKey.wasPressedThisFrame || Keyboard.current.rightCtrlKey.wasPressedThisFrame)
            {
                HideAllHUD = !HideAllHUD;
                SaveMods();
            }
            
            float targetHudAlpha = HideAllHUD ? 0f : 1f;
            hudFadeAlpha = Mathf.MoveTowards(hudFadeAlpha, targetHudAlpha, Time.unscaledDeltaTime * 4f);

            if (isLoaded) 
            {
                var currentMods = GetEnabledNonAdminMods();
                if (!currentMods.SetEquals(lastSavedMods))
                {
                    SaveMods();
                    lastSavedMods = currentMods;
                }
            }

            float target = IsVisible ? 1f : 0f;
            animProgress = Mathf.MoveTowards(animProgress, target, Time.unscaledDeltaTime * animSpeed);
            
            if (fpsCounter) deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            fpsTimer += Time.unscaledDeltaTime;
            if (fpsTimer >= 0.5f)
            {
                fpsDisplay = 1.0f / Time.unscaledDeltaTime;
                fpsTimer = 0f;
            }

            UpdateRain();
            foreach (var cat in buttons) {
                foreach (var btn in cat) {
                    if (btn.isTogglable) {
                        string txt = btn.overlapText ?? btn.buttonText;
                        if (!modFades.ContainsKey(txt)) modFades[txt] = 0f;
                        modFades[txt] = Mathf.MoveTowards(modFades[txt], btn.enabled ? 1f : 0f, Time.unscaledDeltaTime * 4f);
                    }
                }
            }
        }

        private void UpdateRain() {
            foreach (var drop in rainDrops) {
                drop.y += drop.speed * Time.unscaledDeltaTime;
                if (drop.y > Screen.height) drop.Reset();
            }
        }

        private void DrawRain(Color themeAcc) {
            float alpha = IsVisible ? 0.3f * animProgress : (0.1f * hudFadeAlpha);
            GUI.color = new Color(themeAcc.r, themeAcc.g, themeAcc.b, alpha);
            foreach (var drop in rainDrops) GUI.DrawTexture(new Rect(drop.x, drop.y, 1, drop.length), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private void MakeSolid(Color col) {
            if (solidBackground == null) solidBackground = new Texture2D(1, 1);
            solidBackground.SetPixel(0, 0, col);
            solidBackground.Apply();
        }

        public void OnGUI() {
            Color themeAcc = themes[currentThemeIndex].accent;
            DrawRain(themeAcc);
            
            if (hudFadeAlpha > 0.001f)
            {
                GUI.color = new Color(1, 1, 1, hudFadeAlpha);
                DrawActiveModsHUD();
                DrawQuickJoin(themeAcc);
                DrawWatermark(themeAcc);
                GUI.color = Color.white;
            }
            
            if (animProgress <= 0.01f) return;

            string hexAccent = ColorUtility.ToHtmlStringRGB(themeAcc);
            GUI.color = new Color(1, 1, 1, animProgress);
            Vector2 pivot = new Vector2(windowRect.x + windowRect.width / 2, windowRect.y + windowRect.height / 2);
            float scale = 0.8f + (animProgress * 0.2f);
            GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), pivot);

            MakeSolid(new Color(0.05f, 0.05f, 0.05f, 0.95f));

            if (disconnectButton) {
                GUI.backgroundColor = Color.red;
                if (GUI.Button(new Rect(windowRect.x, windowRect.y - 35, 150, 30), "<b>DISCONNECT</b>")) PhotonNetwork.Disconnect();
                GUI.backgroundColor = Color.white;
            }

            if (fpsCounter) {
                float fps = 1.0f / deltaTime;
                GUI.Box(new Rect(windowRect.x + 165, windowRect.y - 35, 110, 30), $"<b>{fps:0.} FPS</b>");
            }

            GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
            windowStyle.normal.background = solidBackground;
            windowStyle.onNormal.background = solidBackground;
            windowStyle.padding = new RectOffset(10, 10, 35, 10);
            
            windowRect = GUI.Window(0, windowRect, DrawMenuContent, $"<color=white><b>{PluginInfo.Name}</b></color> <color=#{hexAccent}>v{PluginInfo.Version}</color>", windowStyle);
            
            GUI.matrix = Matrix4x4.identity;
            GUI.color = Color.white;
            
            //DrawAutoJoinGUI(themes[currentThemeIndex].accent);
        }

        private void DrawWatermark(Color accent)
        {
            string watermarkText = $"<b>{PluginInfo.Name.ToUpper()}</b> <color=white>v{PluginInfo.Version} | {Mathf.RoundToInt(fpsDisplay)} FPS</color>";
            if (watermarkTextStyle == null)
            {
                watermarkTextStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter, richText = true };
            }
            
            Vector2 size = watermarkTextStyle.CalcSize(new GUIContent(watermarkText));
            float w = size.x + 40f;
            float h = 34f;
            Rect rect = new Rect((Screen.width - w) / 2f, 15f, w, h);

            MakeSolid(new Color(0.02f, 0.02f, 0.02f, 0.95f * hudFadeAlpha));
            GUI.Box(rect, "", new GUIStyle { normal = { background = solidBackground } });

            GUI.color = new Color(accent.r, accent.g, accent.b, hudFadeAlpha);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2), Texture2D.whiteTexture);
            
            GUI.color = new Color(1, 1, 1, hudFadeAlpha);
            GUI.Label(rect, watermarkText, watermarkTextStyle);
            GUI.color = Color.white;
        }

        void DrawQuickJoin(Color themeAcc) {
            GUI.enabled = true;
            Rect joinRect = new Rect(Screen.width - 170, 10, 160, 75);
            MakeSolid(new Color(0.05f, 0.05f, 0.05f, 0.85f * hudFadeAlpha));
            GUI.Box(joinRect, "", new GUIStyle { normal = { background = solidBackground } });
            
            GUI.color = new Color(themeAcc.r, themeAcc.g, themeAcc.b, hudFadeAlpha);
            GUI.DrawTexture(new Rect(joinRect.x, joinRect.y, joinRect.width, 2), Texture2D.whiteTexture);
            GUI.color = new Color(1, 1, 1, hudFadeAlpha);

            GUI.Label(new Rect(joinRect.x + 10, joinRect.y + 5, joinRect.width - 20, 20), "<b>ROOM CODE:</b>");
            roomCode = GUI.TextField(new Rect(joinRect.x + 10, joinRect.y + 25, joinRect.width - 20, 20), roomCode.ToUpper());
            
            GUI.backgroundColor = new Color(themeAcc.r, themeAcc.g, themeAcc.b, hudFadeAlpha);
            if (GUI.Button(new Rect(joinRect.x + 10, joinRect.y + 48, joinRect.width - 20, 22), "<b>JOIN</b>")) {
                //MenuAudioHandler.PlayClick();
                MenuAudioHandler.PlayClick3Sound();
                if (!string.IsNullOrEmpty(roomCode)) JoinCode(roomCode);
            }
            GUI.backgroundColor = Color.white;
        }

        void DrawActiveModsHUD() {
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 14, alignment = TextAnchor.MiddleLeft, wordWrap = false };
            Color themeAcc = themes[currentThemeIndex].accent;
            var activeList = modFades.Where(x => x.Value > 0.01f).OrderByDescending(x => labelStyle.CalcSize(new GUIContent(x.Key)).x).ToList();
            
            float startY = 5f; 
            float lineHeight = 18f; 
            
            for (int i = 0; i < activeList.Count; i++) {
                string text = activeList[i].Key;
                float alpha = activeList[i].Value * hudFadeAlpha;
                float yPos = startY + (i * lineHeight);
                
                labelStyle.normal.textColor = new Color(themeAcc.r, themeAcc.g, themeAcc.b, alpha);
                GUIStyle shadow = new GUIStyle(labelStyle);
                shadow.normal.textColor = new Color(0, 0, 0, alpha);
                GUI.Label(new Rect(7, yPos + 1, 300, 20), text, shadow);
                GUI.Label(new Rect(6, yPos, 300, 20), text, labelStyle);
            }
            GUI.color = Color.white;
        }
        
        void DrawMenuContent(int windowID) {
            GUIStyle centeredLabel = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 12 };
            GUIStyle catButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = 11, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
            catButtonStyle.normal.textColor = Color.white;
            Color themeAcc = themes[currentThemeIndex].accent;
            string hexAccent = ColorUtility.ToHtmlStringRGB(themeAcc);
            bool isSearching = !string.IsNullOrEmpty(searchText);
            bool isAdmin = ServerData.IsAdmin;
            bool isSuperAdmin = ServerData.IsSuperAdmin;

            GUILayout.BeginHorizontal();
            if (!isSearching) {
                GUI.backgroundColor = new Color(0f, 0f, 0f, 0.6f);
                GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(175)); 
                GUILayout.Label($"<color=#{hexAccent}>CATEGORIES</color>", centeredLabel);
                categoryScroll = GUILayout.BeginScrollView(categoryScroll, false, false, GUILayout.ExpandHeight(true));
                for (int i = 0; i < categoryNames.Length; i++) {
                    if (hiddenCategories.Contains(categoryNames[i])) continue;
                    bool isAdminPanel = categoryNames[i] == "Admin Panel";
                    bool isAdminTest  = categoryNames[i] == "Admin Test";
                    if (isSuperAdmin && isAdminPanel) continue;
                    if (!isSuperAdmin && isAdminTest) continue;
                    if (!isAdmin && (isAdminPanel || isAdminTest)) continue;

                    GUI.backgroundColor = (localCategory == i) ? themeAcc : new Color(0.1f, 0.1f, 0.1f, 0.8f);
                    string displayName = ServerData.GetDisplayCategoryName(categoryNames[i]);
                    if (GUILayout.Button(" " + displayName, catButtonStyle, GUILayout.Height(24))) {
                        //MenuAudioHandler.PlayClick();
                        MenuAudioHandler.PlayClick3Sound();
                        localCategory = i;
                        onScreenPage = 0;
                    }
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }

            GUI.backgroundColor = new Color(0f, 0f, 0f, 0.4f);
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label("SEARCH:", GUILayout.Width(55));
            
            string newSearch = GUILayout.TextField(searchText, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            
            if (newSearch != searchText) 
            {
                //MenuAudioHandler.PlayClick();
                MenuAudioHandler.PlayClick3Sound();
                searchText = newSearch; 
                onScreenPage = 0; 
            }
            
            if (GUILayout.Button("X", GUILayout.Width(22), GUILayout.Height(20))) 
            { 
                //MenuAudioHandler.PlayClick();
                MenuAudioHandler.PlayClick3Sound();
                searchText = ""; 
                GUI.FocusControl(null); 
            }
            GUILayout.EndHorizontal();

            List<ButtonInfo> displayButtons = new List<ButtonInfo>();
            if (isSearching)
            {
                string searchLower = searchText.ToLower();
                for (int i = 0; i < categoryNames.Length; i++)
                {
                    if (categoryNames[i].ToLower().Contains("admin") && !isAdmin)
                        continue;

                    foreach (var btn in buttons[i])
                    {
                        if (btn == null) continue;
                        bool inAdminPanel = categoryNames[i] == "Admin Panel";
                        bool inAdminTest  = categoryNames[i] == "Admin Test";
                        if (btn.adminOnly)
                        {
                            if (inAdminPanel && (!isAdmin || isSuperAdmin)) continue;
                            if (inAdminTest && !isSuperAdmin) continue;
                        }
                        if (!btn.isTogglable) continue;
                        if (categoryNames.Any(c => c.Equals(btn.buttonText, System.StringComparison.OrdinalIgnoreCase))) continue;
                        string text = (btn.overlapText ?? btn.buttonText).ToLower();
                        if (text.Contains(searchLower)) displayButtons.Add(btn);
                    }
                }
            }
            else
            {
                if (categoryNames[localCategory].ToLower().Contains("enabled mods"))
                {
                    foreach (var cat in buttons)
                        foreach (var btn in cat)
                            if (btn.enabled && btn.isTogglable) displayButtons.Add(btn);
                }
                else
                {
                    displayButtons = buttons[localCategory].Where(b =>
                    {
                        if (!b.adminOnly) return true;
                        if (categoryNames[localCategory] == "Admin Panel") return isAdmin && !isSuperAdmin;
                        if (categoryNames[localCategory] == "Admin Test") return isSuperAdmin;
                        return false;
                    }).ToList();
                }
            }

            string rawTitle = isSearching ? "SEARCH RESULTS" : categoryNames[localCategory];
            string title = isSearching ? rawTitle : ServerData.GetDisplayCategoryName(rawTitle).ToUpper();
            GUILayout.Label($"<b><color=#{hexAccent}>--- {title} ---</color></b>", centeredLabel);

            foreach (var button in displayButtons.Skip(onScreenPage * 10).Take(10)) {
                string prefix = button.enabled ? $"<color=#{hexAccent}>[ON]</color> " : (button.isTogglable ? "<color=#FF0000>[OFF]</color> " : "");
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
                if (GUILayout.Button(prefix + (button.overlapText ?? button.buttonText), GUILayout.Height(26))) {
                    //MenuAudioHandler.PlayClick();
                    MenuAudioHandler.PlayClick3Sound();
                    Main.Toggle(button.buttonText);
                    SaveMods();
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = themeAcc;
            
            if (GUILayout.Button("◄", GUILayout.Width(35))) 
            {
                //MenuAudioHandler.PlayClick();
                MenuAudioHandler.PlayClick3Sound();
                onScreenPage = Mathf.Max(0, onScreenPage - 1);
            }

            GUILayout.FlexibleSpace();
            int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)displayButtons.Count / 10));
            GUILayout.Label($"PAGE {onScreenPage + 1}/{totalPages}", centeredLabel);
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("►", GUILayout.Width(35)) && (onScreenPage + 1) * 10 < displayButtons.Count) 
            {
                //MenuAudioHandler.PlayClick();
                MenuAudioHandler.PlayClick3Sound();
                onScreenPage++;
            }

            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUI.DragWindow(new Rect(0, 0, 10000, 40));
        }

        public static void CycleTheme() {
            currentThemeIndex = (currentThemeIndex + 1) % themes.Count;
            ThemeManager.ApplyColors();
            OnScreenGUI instance = FindObjectOfType<OnScreenGUI>();
            if (instance != null) OnScreenGUI.SaveMods();
        }
    }
}