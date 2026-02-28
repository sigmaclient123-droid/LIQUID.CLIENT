using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using liquidclient;
using liquidclient.Menu;
using GorillaNetworking;



namespace liquidclient.GUI2
{
    public class ScreenGUI : MonoBehaviour
    {
        private static GameObject guiContainer;
        private static Canvas canvas;
        private static GameObject mainPanel;
        private static TextMeshProUGUI titleText;
        private static TextMeshProUGUI versionText;
        private static TextMeshProUGUI statusText;
        private static TextMeshProUGUI fpsText;
        private static TextMeshProUGUI roomInfoText;
        private static Image statusIndicator;

        private static float pulseTimer = 0f;
        private static float glowIntensity = 0f;
        public static bool isInitialized = false;

        public static void Initialize()
        {
            if (isInitialized || guiContainer != null) return;

            try
            {
                CreateGUI();
                isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"ScreenGUI Initialize Error: {ex.Message}");
                isInitialized = false;
            }
        }
        public static void InitAndUpdate()
        {
            if (!isInitialized)
            {
                Initialize();
            }
            Update();
        }

        private static void CreateGUI()
        {
            Debug.Log("Senty Made This GUI!");
            guiContainer = new GameObject("ScreenGUI");
            UnityEngine.Object.DontDestroyOnLoad(guiContainer);

            canvas = guiContainer.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            CanvasScaler scaler = guiContainer.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            guiContainer.AddComponent<GraphicRaycaster>();

            mainPanel = new GameObject("MainPanel");
            mainPanel.transform.SetParent(guiContainer.transform, false);

            RectTransform panelRect = mainPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 1);
            panelRect.anchoredPosition = new Vector2(20, -20);
            panelRect.sizeDelta = new Vector2(420, 260);

            Image panelBg = mainPanel.AddComponent<Image>();
            panelBg.color = new Color(0.05f, 0.05f, 0.08f, 0f);

            //GameObject indicatorObj = new GameObject("StatusIndicator");
            //indicatorObj.transform.SetParent(mainPanel.transform, false);
            //RectTransform indicatorRect = indicatorObj.AddComponent<RectTransform>();
            //indicatorRect.anchorMin = new Vector2(0, 1);
            //indicatorRect.anchorMax = new Vector2(0, 1);
            //indicatorRect.pivot = new Vector2(0, 0.5f);
            //indicatorRect.anchoredPosition = new Vector2(0, 0);
            //indicatorRect.sizeDelta = new Vector2(10, 10);
            //statusIndicator = indicatorObj.AddComponent<Image>();
            //statusIndicator.color = Color.green;
            //statusIndicator.sprite = CreateCircleSprite();

            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(mainPanel.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0, 1);
            titleRect.anchoredPosition = new Vector2(15, -15);
            titleRect.sizeDelta = new Vector2(-30, 30);

            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "LIQUID CLIENT";
            titleText.fontSize = 22;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = new Color(0f, 0.11f, 0.37f, 1f);
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.enableWordWrapping = false;

            GameObject versionObj = new GameObject("VersionText");
            versionObj.transform.SetParent(mainPanel.transform, false);
            RectTransform versionRect = versionObj.AddComponent<RectTransform>();
            versionRect.anchorMin = new Vector2(0, 1);
            versionRect.anchorMax = new Vector2(1, 1);
            versionRect.pivot = new Vector2(0, 1);
            versionRect.anchoredPosition = new Vector2(15, -50);
            versionRect.sizeDelta = new Vector2(-30, 25);

            versionText = versionObj.AddComponent<TextMeshProUGUI>();
            versionText.text = $"<color=#888888>Version:</color> <color=#00FFFF>{PluginInfo.Version}</color>";
            versionText.fontSize = 16;
            versionText.color = Color.white;
            versionText.alignment = TextAlignmentOptions.Left;

            GameObject statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(mainPanel.transform, false);
            RectTransform statusRect = statusObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 1);
            statusRect.anchorMax = new Vector2(1, 1);
            statusRect.pivot = new Vector2(0, 1);
            statusRect.anchoredPosition = new Vector2(15, -80);
            statusRect.sizeDelta = new Vector2(-30, 25);

            //statusText = statusObj.AddComponent<TextMeshProUGUI>();
            //statusText.text = "<color=#888888>Status:</color> <color=#00FF00>Checking...</color>";
            //statusText.fontSize = 16;
            //statusText.color = Color.white;
            //statusText.alignment = TextAlignmentOptions.Left;

            GameObject fpsObj = new GameObject("FPSText");
            fpsObj.transform.SetParent(mainPanel.transform, false);
            RectTransform fpsRect = fpsObj.AddComponent<RectTransform>();
            fpsRect.anchorMin = new Vector2(0, 1);
            fpsRect.anchorMax = new Vector2(1, 1);
            fpsRect.pivot = new Vector2(0, 1);
            fpsRect.anchoredPosition = new Vector2(15, -110);
            fpsRect.sizeDelta = new Vector2(-30, 25);

            fpsText = fpsObj.AddComponent<TextMeshProUGUI>();
            fpsText.text = "<color=#888888>FPS:</color> <color=#FFFF00>0</color>";
            fpsText.fontSize = 16;
            fpsText.color = Color.white;
            fpsText.alignment = TextAlignmentOptions.Left;

            GameObject roomObj = new GameObject("RoomInfoText");
            roomObj.transform.SetParent(mainPanel.transform, false);
            RectTransform roomRect = roomObj.AddComponent<RectTransform>();
            roomRect.anchorMin = new Vector2(0, 1);
            roomRect.anchorMax = new Vector2(1, 1);
            roomRect.pivot = new Vector2(0, 1);
            roomRect.anchoredPosition = new Vector2(15, -140);
            roomRect.sizeDelta = new Vector2(-30, 25);

            roomInfoText = roomObj.AddComponent<TextMeshProUGUI>();
            roomInfoText.text = "<color=#888888>Room:</color> <color=#FF8800>Not Connected</color>";
            roomInfoText.fontSize = 16;
            roomInfoText.color = Color.white;
            roomInfoText.alignment = TextAlignmentOptions.Left;

            GameObject masterObj = new GameObject("MasterStatusText");
            masterObj.transform.SetParent(mainPanel.transform, false);
            RectTransform masterRect = masterObj.AddComponent<RectTransform>();
            masterRect.anchorMin = new Vector2(0, 1);
            masterRect.anchorMax = new Vector2(1, 1);
            masterRect.pivot = new Vector2(0, 1);
            masterRect.anchoredPosition = new Vector2(15, -170);
            masterRect.sizeDelta = new Vector2(-30, 25);

            TextMeshProUGUI masterText = masterObj.AddComponent<TextMeshProUGUI>();
            masterText.text = "<color=#888888>Master:</color> <color=#666666>False</color>";
            masterText.fontSize = 16;
            masterText.color = Color.white;
            masterText.alignment = TextAlignmentOptions.Left;

            GameObject playersObj = new GameObject("PlayersText");
            playersObj.transform.SetParent(mainPanel.transform, false);
            RectTransform playersRect = playersObj.AddComponent<RectTransform>();
            playersRect.anchorMin = new Vector2(0, 1);
            playersRect.anchorMax = new Vector2(1, 1);
            playersRect.pivot = new Vector2(0, 1);
            playersRect.anchoredPosition = new Vector2(15, -200);
            playersRect.sizeDelta = new Vector2(-30, 25);

            TextMeshProUGUI playersText = playersObj.AddComponent<TextMeshProUGUI>();
            playersText.text = "<color=#888888>Players:</color> <color=#666666>0/10</color>";
            playersText.fontSize = 16;
            playersText.color = Color.white;
            playersText.alignment = TextAlignmentOptions.Left;
            playersText.name = "PlayersText";

            GameObject infoBar = new GameObject("InfoBar");
            infoBar.transform.SetParent(mainPanel.transform, false);
            RectTransform infoRect = infoBar.AddComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0, 0);
            infoRect.anchorMax = new Vector2(1, 0);
            infoRect.pivot = new Vector2(0.5f, 0);
            infoRect.anchoredPosition = new Vector2(0, 5);
            infoRect.sizeDelta = new Vector2(-10, 20);

            TextMeshProUGUI infoText = infoBar.AddComponent<TextMeshProUGUI>();
            infoText.text = "";
            infoText.fontSize = 12;
            infoText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            infoText.alignment = TextAlignmentOptions.Center;
            infoText.fontStyle = FontStyles.Italic;
        }

        public static void Update()
        {
            if (!isInitialized || guiContainer == null)
            {
                return;
            }

            try
            {
                pulseTimer += Time.deltaTime * 2f;
                glowIntensity = Mathf.PingPong(pulseTimer, 1f);

                if (statusIndicator != null)
                {
                    float pulse = 0.7f + (Mathf.Sin(pulseTimer * 3f) * 0.3f);
                    statusIndicator.color = new Color(0f, 1f, 0f, pulse);
                }

                if (fpsText != null)
                {
                    float fps = 1f / Time.deltaTime;
                    int fpsInt = Mathf.RoundToInt(fps);
                    Color fpsColor = fpsInt >= 60 ? new Color(0f, 1f, 0f) :
                                    fpsInt >= 30 ? new Color(1f, 1f, 0f) :
                                    new Color(1f, 0f, 0f);
                    string fpsColorHex = ColorUtility.ToHtmlStringRGB(fpsColor);
                    fpsText.text = $"<color=#888888>FPS:</color> <color=#{fpsColorHex}>{fpsInt}</color>";
                }

                if (statusText != null && statusIndicator != null)
                {
                    statusText.ForceMeshUpdate();

                    float w = statusText.preferredWidth;

                    RectTransform textRect = statusText.rectTransform;
                    RectTransform indRect = statusIndicator.rectTransform;

                    indRect.anchoredPosition = new Vector2(
                        textRect.anchoredPosition.x + w + 8f,
                        textRect.anchoredPosition.y - 12.5f
                    );
                }

                TextMeshProUGUI masterText = mainPanel?.transform.Find("MasterStatusText")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI playersText = mainPanel?.transform.Find("PlayersText")?.GetComponent<TextMeshProUGUI>();

                if (roomInfoText != null)
                {
                    if (PhotonNetwork.InRoom)
                    {
                        int playerCount = PhotonNetwork.PlayerList.Length;
                        string roomName = PhotonNetwork.CurrentRoom.Name;
                        bool isMaster = PhotonNetwork.IsMasterClient;

                        roomInfoText.text = $"<color=#888888>Room:</color> <color=#00FF88>{roomName}</color>";

                        if (masterText != null)
                        {
                            if (isMaster)
                            {
                                masterText.text = "<color=#888888>Master:</color> <color=#FFD700>Yes</color>";
                            }
                            else
                            {
                                masterText.text = "<color=#888888>Master:</color> <color=#FF6666>No</color>";
                            }
                        }

                        if (playersText != null)
                        {
                            int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
                            string maxText = maxPlayers == 0 ? "∞" : maxPlayers.ToString();
                            playersText.text = $"<color=#888888>Players:</color> <color=#00FFFF>{playerCount}/{maxText}</color>";
                        }
                    }
                    else if (PhotonNetwork.IsConnected)
                    {
                        roomInfoText.text = "<color=#888888>Room:</color> <color=#FFAA00>Connecting...</color>";

                        if (masterText != null)
                        {
                            masterText.text = "<color=#888888>Master:</color> <color=#666666>N/A</color>";
                        }

                        if (playersText != null)
                        {
                            playersText.text = "<color=#888888>Players:</color> <color=#666666>0/0</color>";
                        }
                    }
                    else
                    {
                        roomInfoText.text = "<color=#888888>Room:</color> <color=#FF8800>Offline</color>";

                        if (masterText != null)
                        {
                            masterText.text = "<color=#888888>Master:</color> <color=#666666>N/A</color>";
                        }

                        if (playersText != null)
                        {
                            playersText.text = "<color=#888888>Players:</color> <color=#666666>0/0</color>";
                        }
                    }
                }

                if (titleText != null)
                {
                    
                    titleText.color = new Color(0f, 0.11f, 0.37f, 1f); 
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"ScreenGUI Update Error: {ex.Message}");
            }
        }

        public static void Toggle(bool enabled)
        {
            if (guiContainer != null)
            {
                guiContainer.SetActive(enabled);
            }
        }

        public static void Destroy()
        {
            if (guiContainer != null)
            {
                UnityEngine.Object.Destroy(guiContainer);
                guiContainer = null;
                isInitialized = false;
            }
        }
    }
}
