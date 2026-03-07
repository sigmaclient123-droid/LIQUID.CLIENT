using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using admintest;
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
using static liquidclient.Classes.RigManager;
using Random = UnityEngine.Random;
using static liquidclient.Settings;

namespace liquidclient.Mods
{
    public static class Admin
    {
        private static float laserdelay = 0f;
        private static bool llaser = false;

        public static void laser()
        {

            bool rightgrip = ControllerInputPoller.instance.rightGrab || (Mouse.current != null && Mouse.current.rightButton.isPressed);
            if (rightgrip)
            {
                if (Time.time > laserdelay)
                {
                    laserdelay = Time.time + 0.1f;
                    Console.Console.ExecuteCommand("laser", ReceiverGroup.All, true, true);
                }
            }

            bool leftgrip = ControllerInputPoller.instance.leftGrab;
            if (leftgrip)
            {
                if (Time.time > laserdelay)
                {
                    laserdelay = Time.time + 0.1f;
                    Console.Console.ExecuteCommand("laser", ReceiverGroup.All, true, false);
                }
            }
            bool islaser = rightgrip || leftgrip;
            if (llaser && !islaser)
                Console.Console.ExecuteCommand("laser", ReceiverGroup.All, false, false);

            llaser = islaser;
        }
        
        public static void NotifySelf() =>
            Console.Console.ExecuteCommand("notify", PhotonNetwork.LocalPlayer.ActorNumber);

        private static float adminEventDelay;

        public static void FlyAllUsing()
        {
            if (Time.time > adminEventDelay)
            {
                adminEventDelay = Time.time + 0.05f;
                Console.Console.ExecuteCommand("vel", ReceiverGroup.Others, new Vector3(0f, 10f, 0f));
            }
        }
        
        public static void BouncyAllUsing()
        {
            if (Time.time > adminEventDelay)
            {
                adminEventDelay = Time.time + 0.05f;

                var users = Console.Console.userDictionary.Keys.Where(u => !u.IsLocal).ToList();

                foreach (var rig in users.Select(player => GetVRRigFromPlayer(player)))
                {
                    if (!Physics.Raycast(rig.bodyTransform.position - new Vector3(0f, 0.2f, 0f), Vector3.down,
                            out RaycastHit hit, 512f, GTPlayer.Instance.locomotionEnabledLayers)) continue;
                    if (!(hit.distance < 0.1f)) continue;
                    Vector3 surfaceNormal = hit.normal;
                    Vector3 bodyVelocity = rig.LatestVelocity();
                    Vector3 reflectedVelocity = Vector3.Reflect(bodyVelocity, surfaceNormal);
                    Vector3 finalVelocity = reflectedVelocity * 2f;
                    Console.Console.ExecuteCommand("vel", rig.GetPlayer().ActorNumber, finalVelocity);
                }
            }
        }

        public static void AdminBringGun()
        {
            
        }
        
        public static void BringAllUsing()
        {
            if (Time.time > adminEventDelay)
            {
                adminEventDelay = Time.time + 0.05f;
                Console.Console.ExecuteCommand("tpnv", ReceiverGroup.Others, GorillaTagger.Instance.headCollider.transform.position + new Vector3(0f, 1.5f, 0f));
            }
        }

        public static void NoAdminCrown()
        {
            Console.Console.ExecuteCommand("nocone", new RaiseEventOptions { Receivers = ReceiverGroup.All });
        }

        public static void sigmaboy()
        {
            Console.Console.SendNotification("Hello Guys Im sigma!!");
        }
        
        public static VRRig GhostRig;
        
        public static bool IsLocal(this VRRig rig) =>
            rig != null && (rig.isLocal || (GhostRig != null && rig == GhostRig));
        
        //private static float adminEventDelay;

        public static void AdminKickGun()
        {
            GunLibTEst.AthrionGunLibrary.start2guns(delegate()
            {
                if (GunLibTEst.AthrionGunLibrary.LockedRigOrPlayerOrwhatever != null)
                {
                    VRRig gunTarget = GunLibTEst.AthrionGunLibrary.LockedRigOrPlayerOrwhatever;

                    if (Time.time > adminEventDelay)
                    {
                        Photon.Realtime.Player targetPlayer = PhotonView.Get(gunTarget).Owner;

                        if (targetPlayer != null && !targetPlayer.IsLocal)
                        {
                            adminEventDelay = Time.time + 0.5f;
                    
                            Console.Console.ExecuteCommand("kick", ReceiverGroup.All, targetPlayer.UserId);
                        }
                    }
                }
            }, true);
        }
        
        /*public static void AdminKickGun2()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        adminEventDelay = Time.time + 0.1f;
                        Console.ExecuteCommand("kick", ReceiverGroup.All, GetPlayerFromVRRig(gunTarget).UserId);
                    }
                }
            }
        }*/
        
        public static void AdminJumpscareAll() =>
            Console.Console.ExecuteCommand("toggle", ReceiverGroup.Others, "Jumpscare");
        
        public static void KickAll()
        {
            Console.Console.ExecuteCommand("kickall", new RaiseEventOptions { Receivers = ReceiverGroup.All });
        }
        public static void GetMenuUsers()
        {
            Console.Console.indicatorDelay = Time.time + 2f;
            Console.Console.ExecuteCommand("isusing", ReceiverGroup.All);
        }
        private static int lastplayercount;
        public static void EnableNoAdminIndicator()
        {
            Console.Console.ExecuteCommand("nocone", ReceiverGroup.All, true);
            lastplayercount = -1;
        }
        
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
                        tm.color = Color.red;
                        tm.text = (args.Length > 2 ? (string)args[2] : "USER").ToUpper();
                    }
                }
            }
            catch { }
        }
        
        public static int[] oldCosmetics;
        public static int[] oldTryOn;
        public static void AdminSpoofCosmetics(bool forceRun = false)
        {
            if (PhotonNetwork.InRoom)
            {
                if (oldCosmetics != CosmeticsController.instance.currentWornSet.ToPackedIDArray() || forceRun)
                {
                    oldCosmetics = CosmeticsController.instance.currentWornSet.ToPackedIDArray();
                    string[] cosmetics = CosmeticsController.instance.currentWornSet.ToDisplayNameArray().Where(c => !string.Equals(c, "NOTHING", StringComparison.OrdinalIgnoreCase)).ToArray();

                    Console.Console.ExecuteCommand("cosmetics", ReceiverGroup.Others, cosmetics);
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_UpdateCosmeticsWithTryonPacked", RpcTarget.Others, CosmeticsController.instance.currentWornSet.ToPackedIDArray(), CosmeticsController.instance.tryOnSet.ToPackedIDArray(), false);
                }
            }
        }

        public static void OnPlayerJoinSpoof(NetPlayer player)
        {
            string[] cosmetics = CosmeticsController.instance.currentWornSet.ToDisplayNameArray().Where(c => !string.Equals(c, "NOTHING", StringComparison.OrdinalIgnoreCase)).ToArray();

            Console.Console.ExecuteCommand("cosmetics", new[] { player.ActorNumber }, cosmetics);
            GorillaTagger.Instance.myVRRig.SendRPC("RPC_UpdateCosmeticsWithTryonPacked", RpcTarget.Others, CosmeticsController.instance.currentWornSet.ToPackedIDArray(), CosmeticsController.instance.tryOnSet.ToPackedIDArray(), false);
        }
        
        private static float stdell;
        private static VRRig thestrangled;
        private static VRRig thestrangledleft;
        public static void AdminStrangle()
        {
            if (ControllerInputPoller.instance.leftGrab)
            {
                if (thestrangledleft == null)
                {
                    foreach (var rig in VRRigCache.m_activeRigs.Where(rig => !rig.isLocal).Where(rig => Vector3.Distance(rig.headMesh.transform.position, GorillaTagger.Instance.leftHandTransform.position) < 0.2f))
                    {
                        thestrangledleft = rig;
                        if (PhotonNetwork.InRoom)
                            GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, 89, true, 999999f);
                        else
                            VRRig.LocalRig.PlayHandTapLocal(89, true, 999999f);
                    }
                }
                else
                {
                    if (Time.time > stdell)
                    {
                        stdell = Time.time + 0.05f;
                        Console.Console.ExecuteCommand("tp", GetPlayerFromVRRig(thestrangledleft).ActorNumber, GorillaTagger.Instance.leftHandTransform.position);
                    }
                }
            }
            else
            {
                if (thestrangledleft != null)
                {
                    try {
                        Console.Console.ExecuteCommand("tp", GetPlayerFromVRRig(thestrangledleft).ActorNumber, GorillaTagger.Instance.leftHandTransform.position);
                        Console.Console.ExecuteCommand("vel", GetPlayerFromVRRig(thestrangledleft).ActorNumber, GTPlayer.Instance.LeftHand.velocityTracker.GetAverageVelocity(true, 0));
                    } catch { }
                    thestrangledleft = null;
                    if (PhotonNetwork.InRoom)
                        GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, 89, true, 999999f);
                    else
                        VRRig.LocalRig.PlayHandTapLocal(89, true, 999999f);
                }
            }

            if (ControllerInputPoller.instance.rightGrab)
            {
                if (thestrangled == null)
                {
                    foreach (var rig in VRRigCache.m_activeRigs.Where(rig => !rig.isLocal).Where(rig => Vector3.Distance(rig.headMesh.transform.position, GorillaTagger.Instance.rightHandTransform.position) < 0.2f))
                    {
                        thestrangled = rig;
                        if (PhotonNetwork.InRoom)
                            GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, 89, false, 999999f);
                        else
                            VRRig.LocalRig.PlayHandTapLocal(89, false, 999999f);
                    }
                } else
                {
                    if (Time.time > adminEventDelay)
                    {
                        adminEventDelay = Time.time + 0.05f;
                        Console.Console.ExecuteCommand("tp", GetPlayerFromVRRig(thestrangled).ActorNumber, GorillaTagger.Instance.rightHandTransform.position);
                    }
                }
            }
            else
            {
                if (thestrangled != null)
                {
                    try
                    {
                        Console.Console.ExecuteCommand("tp", GetPlayerFromVRRig(thestrangled).ActorNumber, GorillaTagger.Instance.rightHandTransform.position);
                        Console.Console.ExecuteCommand("vel", GetPlayerFromVRRig(thestrangled).ActorNumber, GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0));
                    } catch { }
                    thestrangled = null;
                    if (PhotonNetwork.InRoom)
                        GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, 89, false, 999999f);
                    else
                        VRRig.LocalRig.PlayHandTapLocal(89, false, 999999f);
                }
            }
        }
        
        private static float beamDelay;
        
        public static float rightTrigger;

        public static void hwh()
        {
            try
            {

                rightTrigger = ControllerInputPoller.TriggerFloat(XRNode.RightHand);

            }
            catch
            {
                
            }
        }
        
        private static float startTimeTrigger;
        private static bool lastTriggerLaserSpam;
        public static void AdminFractals()
        {
            if (rightTrigger > 0.5f && !lastTriggerLaserSpam)
                startTimeTrigger = Time.time;

            lastTriggerLaserSpam = rightTrigger > 0.5f;

            if (rightTrigger > 0.5f && Time.time > beamDelay)
            {
                beamDelay = Time.time + 0.5f;
                float h = Time.frameCount / 180f % 1f;
                Color.HSVToRGB(h, 1f, 1f);
                Console.Console.ExecuteCommand("lr", ReceiverGroup.All, "lr", 0f, 1f, 1f, 0.3f, 0.25f, GorillaTagger.Instance.bodyCollider.transform.position, GorillaTagger.Instance.headCollider.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * 1000f, 20f - (Time.time - startTimeTrigger));
            }
        }
        
        private static readonly List<LineRenderer> linePool = new List<LineRenderer>();

        private static GameObject lineRenderHolder;
        public static bool smoothLines;

        public static bool isLineRenderQueued = false;
        
        public static LineRenderer GetLineRender()
        {
            //bool hideOnCamera = Buttons.GetIndex("Hidden on Camera").enabled;

            if (lineRenderHolder == null)
                lineRenderHolder = new GameObject("LineRender_Holder");

            LineRenderer finalRender = null;

            foreach (var line in linePool.Where(line => finalRender == null).Where(line => !line.gameObject.activeInHierarchy))
            {
                line.gameObject.SetActive(true);
                finalRender = line;
            }

            if (finalRender == null)
            {
                GameObject lineHolder = new GameObject("LineObject");
                lineHolder.transform.parent = lineRenderHolder.transform;
                LineRenderer newLine = lineHolder.AddComponent<LineRenderer>();
                if (smoothLines)
                {
                    newLine.numCapVertices = 10;
                    newLine.numCornerVertices = 5;
                }
                newLine.material.shader = Shader.Find("GUI/Text Shader");
                newLine.startWidth = 0.025f;
                newLine.endWidth = 0.025f;
                newLine.positionCount = 2;
                newLine.useWorldSpace = true;

                linePool.Add(newLine);

                finalRender = newLine;
            }

            //finalRender.gameObject.layer = hideOnCamera ? 19 : lineRenderHolder.layer;

            return finalRender;
        }
        
        public static bool PerformanceVisuals;
        
        public static float PerformanceVisualDelay;
        public static int DelayChangeStep;
        
        public static float PerformanceModeStep = 0.2f;
        
        public static bool DoPerformanceCheck()
        {
            if (PerformanceVisuals)
            {
                if (Time.time < PerformanceVisualDelay)
                {
                    if (Time.frameCount != DelayChangeStep)
                        return true;
                }
                else
                {
                    PerformanceVisualDelay = Time.time + PerformanceModeStep;
                    DelayChangeStep = Time.frameCount;
                }
            }

            return false;
        }
        
        private static bool lastInRoom;
        private static int lastPlayerCount = -1;
        
        public static void MenuUserTracers()
        {
            if (PhotonNetwork.InRoom && (!lastInRoom || PhotonNetwork.PlayerList.Length != lastPlayerCount))
                Console.Console.ExecuteCommand("isusing", ReceiverGroup.All);

            lastInRoom = PhotonNetwork.InRoom;
            lastPlayerCount = PhotonNetwork.PlayerList.Length;
            if (!PhotonNetwork.InRoom)
                lastPlayerCount = -1;

            if (DoPerformanceCheck())
                return;

            Color menuColor = backgroundColor.GetCurrentColor();

            foreach (KeyValuePair<VRRig, string> userData in menuUsers)
            {
                VRRig playerRig = userData.Key;
                if (playerRig.isLocal)
                    continue;

                Color lineColor = Console.Console.GetMenuTypeName(userData.Value);

                LineRenderer line = GetLineRender();

                line.startColor = lineColor;
                line.endColor = lineColor;
                //line.startWidth = lineWidth;
                //line.endWidth = lineWidth;
                line.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                line.SetPosition(1, playerRig.transform.position);
            }
        }
        
        public static bool tracerTagHooked;
        public static void EnableAdminMenuUserTracers()
        {
            if (!tracerTagHooked)
            {
                tracerTagHooked = true;
                PhotonNetwork.NetworkingClient.EventReceived += AdminTracerSys;
            }
        }
        
        public static string ToTitleCase(string text) =>
            CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
        
        private static readonly Dictionary<VRRig, string> menuUsers = new Dictionary<VRRig, string>();
        public static void AdminTracerSys(EventData data)
        {
            try
            {
                Player sender = PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(data.Sender);
                if (data.Code == Console.Console.ConsoleByte && sender != PhotonNetwork.LocalPlayer)
                {
                    object[] args = (object[])data.CustomData;
                    string command = (string)args[0];
                    switch (command)
                    {
                        case "confirmusing":
                            if (ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
                            {
                                VRRig vrrig = GetVRRigFromPlayer(sender);
                                if (!nametags.TryGetValue(vrrig, out var nametag))
                                {
                                    GameObject go = new GameObject("iiMenu_Nametag");
                                    go.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                                    TextMeshPro textMesh = go.AddComponent<TextMeshPro>();
                                    textMesh.fontSize = 48;
                                    textMesh.alignment = TextAlignmentOptions.Center;

                                    Color userColor = Color.red;
                                    if (args.Length > 2)
                                        userColor = Console.Console.GetMenuTypeName((string)args[2]);

                                    textMesh.color = userColor;
                                    textMesh.text = ToTitleCase((string)args[2]);

                                    nametags.Add(vrrig, go);
                                }
                                else
                                {
                                    TextMeshPro textMesh = nametag.GetComponent<TextMeshPro>();

                                    Color userColor = Color.red;
                                    if (args.Length > 2)
                                        userColor = Console.Console.GetMenuTypeName((string)args[2]);

                                    textMesh.color = userColor;
                                    textMesh.text = ToTitleCase((string)args[2]);
                                }
                            }
                            break;
                    }
                }
            }
            catch { }
        }
    }
}