using GorillaNetworking;
using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace liquidclient.Menu
{
    internal class BoardManager
    {
        private static string MOTD = "[ <color=#3DA6FF>Liquid.Client</color> ]";

        public static string MOTDtext =
            "Welcome to [ <color=#3DA6FF>Liquid.Client</color> ]\n" +
            "============================================================\n" +
            "Status: <color=green>Active</color>\n" +
            "User: <color=#3DA6FF>{0}</color>\n" +
            "------------------------------------------------------------\n" +
            "<color=#FF0000>DISCLAIMER: Use at your own risk. We are not responsible for bans, or any account actions.</color>\n";
        

        private static string CoCTitle = "[ <color=#3DA6FF>Liquid.Client</color> ]";

        private static string CoCText =
            "<color=#3DA6FF>Liquid.Client</color>\n\n" +
            "Version: {0}\n\n" +
            "================ Credits ================\n" +
            "<color=#3DA6FF>Senty - Made sum mods</color>\n" +
            "<color=#3DA6FF>cdev - owner of the menu</color>\n" +
            "<color=#3DA6FF>sigmaboy - owner of the menu</color>\n" +
            "<color=#3DA6FF>imudtrust - Made the GUI, Stump text, And more</color>\n" +
            "=========================================\n\n" +
            "<color=grey>Thank you for supporting Liquid.Client</color>";

        private static string RemoteMOTD = "[ <color=#3DA6FF>Liquid.Client</color> ]";

        private static string RemoteText =
            "<color=#3DA6FF>Remote Access Initialized</color>\n" +
            "------------------------------------------\n" +
            "Location: <color=white>{0}</color>\n" +
            "Status: <color=green>Encrypted</color>";

        public static Dictionary<string, GameObject> customPlanes = new Dictionary<string, GameObject>();

        public static void CreateCustomBoards()
        {
            UpdateStumpBranding();
            string currentScene = SceneManager.GetActiveScene().name;

            if (BoardInformations.TryGetValue(currentScene, out var info))
                ApplyMapBoard(currentScene, info);
        }

        private static void UpdateStumpBranding()
        {
            try
            {
                GameObject motdHead = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/motdHeadingText");
                if (motdHead != null)
                {
                    var tmp = motdHead.GetComponent<TextMeshPro>();
                    tmp.richText = true;
                    tmp.text = MOTD;
                }

                GameObject motdBody = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/motdBodyText");
                if (motdBody != null)
                {
                    var tmp = motdBody.GetComponent<TextMeshPro>();
                    tmp.richText = true;
                    tmp.text = string.Format(MOTDtext, PhotonNetwork.LocalPlayer.NickName);
                }

                GameObject cocHead = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/CodeOfConductHeadingText");
                if (cocHead != null)
                {
                    var tmp = cocHead.GetComponent<TextMeshPro>();
                    tmp.richText = true;
                    tmp.text = CoCTitle;
                }

                GameObject cocBody = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/COCBodyText_TitleData");
                if (cocBody != null)
                {
                    var tmp = cocBody.GetComponent<TextMeshPro>();
                    tmp.richText = true;
                    tmp.text = string.Format(CoCText, PluginInfo.Version);
                }
            }
            catch { }
        }

        private static void ApplyMapBoard(string sceneName, BoardInfo info)
        {
            if (customPlanes.ContainsKey(sceneName))
            {
                if (customPlanes[sceneName] != null)
                    UnityEngine.Object.Destroy(customPlanes[sceneName]);

                customPlanes.Remove(sceneName);
            }

            GameObject parent = GameObject.Find(info.Path);
            if (parent != null)
            {
                GameObject board = GameObject.CreatePrimitive(PrimitiveType.Plane);
                board.transform.SetParent(parent.transform, false);
                board.transform.localPosition = info.Pos;
                board.transform.localRotation = Quaternion.Euler(info.Rot);
                board.transform.localScale = info.Scale;
                UnityEngine.Object.Destroy(board.GetComponent<Collider>());

                var renderer = board.GetComponent<Renderer>();
                renderer.material.shader = Shader.Find("GorillaTag/UberShader");
                renderer.material.color = new Color32(15, 15, 15, 255);

                GameObject textObj = new GameObject("LiquidRemoteText");
                textObj.transform.SetParent(board.transform, false);
                textObj.transform.localPosition = new Vector3(0f, 0.1f, 0f);
                textObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                textObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

                TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
                tmp.richText = true;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 2f;
                tmp.text = string.Format(RemoteText, sceneName);

                customPlanes.Add(sceneName, board);
            }
        }

        private struct BoardInfo
        {
            public string Path;
            public Vector3 Pos;
            public Vector3 Rot;
            public Vector3 Scale;

            public BoardInfo(string p, Vector3 pos, Vector3 rot, Vector3 s)
            {
                Path = p;
                Pos = pos;
                Rot = rot;
                Scale = s;
            }
        }

        private static readonly Dictionary<string, BoardInfo> BoardInformations = new Dictionary<string, BoardInfo>
        {
            ["Canyon2"] = new BoardInfo("Canyon/CanyonScoreboardAnchor/GorillaScoreBoard", new Vector3(-24.5f, -28.7f, 0.1f), new Vector3(270f, 0f, 0f), new Vector3(21.5f, 1f, 22.1f)),
            ["Skyjungle"] = new BoardInfo("skyjungle/UI/Scoreboard/GorillaScoreBoard", new Vector3(-21.2f, -32.1f, 0f), new Vector3(270f, 0f, 0f), new Vector3(21.6f, 0.1f, 20.4f)),
            ["Beach"] = new BoardInfo("BeachScoreboardAnchor/GorillaScoreBoard", new Vector3(-22.1f, -33.7f, 0.1f), new Vector3(270f, 0f, 0f), new Vector3(21.2f, 2f, 21.6f)),
            ["City"] = new BoardInfo("City_Pretty/CosmeticsScoreboardAnchor/GorillaScoreBoard", new Vector3(-22.1f, -34.9f, 0.5f), new Vector3(270f, 0f, 0f), new Vector3(21.6f, 2.4f, 22f)),
            ["Basement"] = new BoardInfo("Basement/BasementScoreboardAnchor/GorillaScoreBoard/", new Vector3(-22.1f, -24.5f, 0.5f), new Vector3(270f, 0f, 0f), new Vector3(21.6f, 1.2f, 20.8f))
        };
    }
}