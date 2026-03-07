using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Console;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using liquidclient.Classes;
using liquidclient.GunLib;
using liquidclient.Menu;
using liquidclient.Notifications;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Valve.VR;
using static liquidclient.Classes.RigManager;
using Random = UnityEngine.Random;
using static liquidclient.Settings;
using static liquidclient.Mods.Movement;
using Object = UnityEngine.Object;

namespace admintest
{
    public static class AdminTest
    {
        public static bool userTagHooked = false;
        public static Dictionary<VRRig, GameObject> nametags = new Dictionary<VRRig, GameObject>();

        public static void EnableAdminMenuUserTags()
        {
            if (!userTagHooked)
            {
                userTagHooked = true;
                PhotonNetwork.NetworkingClient.EventReceived += AdminUserTagSys;
                PhotonNetwork.NetworkingClient.EventReceived += OnPlayerJoinedUpdateTags;
                Console.Console.ExecuteCommand("isusing", ReceiverGroup.All);
                NotifiLib.SendNotification("<color=green>[ADMIN]</color> User Tags Enabled.");
            }
        }

        public static void DisableAdminMenuUserTags()
        {
            if (userTagHooked)
            {
                userTagHooked = false;
                PhotonNetwork.NetworkingClient.EventReceived -= AdminUserTagSys;
                PhotonNetwork.NetworkingClient.EventReceived -= OnPlayerJoinedUpdateTags;
                foreach (var tag in nametags.Values)
                {
                    if (tag != null) UnityEngine.Object.Destroy(tag);
                }
                nametags.Clear();
                NotifiLib.SendNotification("<color=red>[ADMIN]</color> User Tags Disabled.");
            }
        }

        public static void UpdateNameTagPositions()
        {
            if (!userTagHooked) return;
            List<VRRig> toRemove = new List<VRRig>();
            foreach (var entry in nametags)
            {
                VRRig rig = entry.Key;
                GameObject tag = entry.Value;
                if (rig == null || tag == null)
                {
                    toRemove.Add(rig);
                    continue;
                }
                tag.transform.position = rig.headMesh.transform.position + new Vector3(0, 0.45f, 0);
                tag.transform.LookAt(GorillaTagger.Instance.mainCamera.transform);
                tag.transform.Rotate(0, 180, 0);
            }
            foreach (var rig in toRemove) nametags.Remove(rig);
        }

        private static void OnPlayerJoinedUpdateTags(EventData data)
        {
            if (data.Code == 255) Console.Console.ExecuteCommand("isusing", ReceiverGroup.All);
        }

        public static void AdminUserTagSys(EventData data)
        {
            try
            {
                Player sender = PhotonNetwork.CurrentRoom.GetPlayer(data.Sender);
                if (data.Code == Console.Console.ConsoleByte && sender != PhotonNetwork.LocalPlayer)
                {
                    object[] args = (object[])data.CustomData;
                    if ((string)args[0] == "confirmusing")
                    {
                        VRRig vrrig = RigManager.GetVRRigFromPlayer(sender);
                        if (vrrig == null) return;
                        if (!nametags.ContainsKey(vrrig) || nametags[vrrig] == null)
                        {
                            GameObject go = new GameObject("Console_User_Tag");
                            go.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                            TextMeshPro text = go.AddComponent<TextMeshPro>();
                            text.fontSize = 4.8f;
                            text.alignment = TextAlignmentOptions.Center;
                            text.fontStyle = FontStyles.Bold;
                            nametags[vrrig] = go;
                        }
                        TextMeshPro tm = nametags[vrrig].GetComponent<TextMeshPro>();
                        tm.color = Color.lightSkyBlue;
                        tm.text = (args.Length > 2 ? (string)args[2] : "USER").ToUpper();
                    }
                }
            }
            catch { }
        }
        
        private static int Lacuca = -1;
        
        public static void Cucaracha2()
        {
            if (Lacuca < 0)
            {
                Lacuca = Console.Console.GetFreeAssetID();

                Console.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "lacuca", "lacuca", Lacuca);
                Console.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, Lacuca, 2);
                Console.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, Lacuca, "lacuca", "cucaracha");

                RPCProtection();
            }
        }

        public static void destroyCucaracha2()
        {
            Console.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, Lacuca);
            Console.Console.ExecuteCommand("asset-stopsound", ReceiverGroup.All, Lacuca, "lacuca");
            Lacuca = -1;
        }
        
        private static int somethingidkID = -1;

        public static void somethingidk()
        {
            if (somethingidkID < 0)
            {
                somethingidkID = Console.Console.GetFreeAssetID();

                Console.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "console.main1", "Sword", somethingidkID);
                Console.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, somethingidkID, 2);
                Console.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, somethingidkID, "lacuca", "cucaracha");

                RPCProtection();
            }

            if (!Console.Console.consoleAssets.ContainsKey(somethingidkID))
                return;
        }

        public static void destroysomethingidk()
        {
            Console.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, somethingidkID);
            somethingidkID = -1;
        }
        
        private static int blcokId = -1;

        public static void blcok()
        {
            if (blcokId < 0)
            {
                blcokId = Console.Console.GetFreeAssetID();

                Console.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "block", "block", blcokId);
                Console.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, blcokId, 2);

                RPCProtection();
            }
        }

        public static void destroyBlock()
        {
            Console.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, blcokId);
            blcokId = -1;
        }
    }
}