using System.Linq;
using System.Reflection;
using BepInEx;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaNetworking;
using liquid.client.Patches.Internal;
using liquid.Patches.Menu;
using liquidclient;
using liquidclient.Classes;
using liquidclient.GunLib;
using liquidclient.Menu;
using liquidclient.Notifications;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using static liquidclient.Menu.Main;
using static liquid.Patches.Menu.PostGetData;

namespace liquidclient.Mods
{
    public class Important
    {
        private static Vector3 oldLocalPosition;
        private static bool isClicking;

        public static void PCButtonClick()
        {
            if (Mouse.current == null)
                return;
            
            if (GorillaTagger.Instance == null ||
                GorillaTagger.Instance.rightHandTriggerCollider == null)
                return;

            var hand = GorillaTagger.Instance.rightHandTriggerCollider.transform;
            var follow = hand.GetComponent<TransformFollow>();

            if (follow == null)
                return;

            if (!Mouse.current.leftButton.isPressed)
            {
                if (isClicking)
                    RestoreHand(hand, follow);

                return;
            }

            if (!isClicking)
            {
                isClicking = true;
                oldLocalPosition = hand.localPosition;
                follow.enabled = false;
            }
            
            Ray ray = Main.TPC.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 512f, Main.NoInvisLayerMask()))
            {
                hand.position = hit.point;
            }
        }

        private static void RestoreHand(Transform hand, TransformFollow follow)
        {
            hand.localPosition = oldLocalPosition;
            follow.enabled = true;
            isClicking = false;
        }
        
        public static void DisablePCButtonClick()
        {
            if (GorillaTagger.Instance == null) return;

            var hand = GorillaTagger.Instance.rightHandTriggerCollider?.transform;
            if (hand == null) return;

            var follow = hand.GetComponent<TransformFollow>();
            if (follow == null) return;

            RestoreHand(hand, follow);
        }
        
        private static bool wasenabled = true;

        public static void EnableFPC()
        {
            if (TPC != null)
                wasenabled = TPC.gameObject.transform.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>().enabled;
        }

        public static float zoomFOV = 35f;
        public static void MoveFPC()
        {
            if (TPC != null)
            {
                if (menu != null && !XRSettings.isDeviceActive)
                    return;

                float FOV = 90f;
                if (Keyboard.current.cKey.isPressed)
                {
                    Vector2 scroll = Mouse.current.scroll.ReadValue();
                    zoomFOV += -scroll.y * 5f;
                    zoomFOV = Mathf.Clamp(zoomFOV, 10f, 90f);
                    TPC.fieldOfView = Mathf.Lerp(TPC.fieldOfView, zoomFOV, 0.1f);
                }
                else
                {
                    zoomFOV = 35f;
                    TPC.fieldOfView = Mathf.Lerp(TPC.fieldOfView, FOV, 0.1f);
                }
                TPC.gameObject.transform.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>().enabled = false;
                TPC.gameObject.transform.position = Keyboard.current.cKey.isPressed ? Vector3.Lerp(TPC.transform.position, GorillaTagger.Instance.headCollider.transform.position, 0.1f) : GorillaTagger.Instance.headCollider.transform.position;
                TPC.gameObject.transform.rotation = Quaternion.Lerp(TPC.transform.rotation, GorillaTagger.Instance.headCollider.transform.rotation, 0.075f);
            }
        }

        public static void DisableFPC()
        {
            if (TPC != null)
            {
                TPC.fieldOfView = 60f;
                
                GameObject vcam = TPC.gameObject.transform.Find("CM vcam1").gameObject;
                vcam.GetComponent<CinemachineVirtualCamera>().enabled = wasenabled;
                
                TPC.transform.localPosition = Vector3.zero;
                TPC.transform.localRotation = Quaternion.identity;
                
                vcam.SetActive(false);
                vcam.SetActive(true);
            }
        }
        
        public static NetPlayer GetPlayerFromID(string id) =>
            PhotonNetwork.PlayerList.FirstOrDefault(player => player.UserId == id);
        
        public static VRRig GetVRRigFromPlayer(NetPlayer p) =>
            GorillaGameManager.StaticFindRigForPlayer(p);
        
        public static NetPlayer GetPlayerFromVRRig(VRRig p) =>
            p.Creator ?? NetworkSystem.Instance.GetPlayer(NetworkSystem.Instance.GetOwningPlayerID(p.rigSerializer.gameObject));
        
        public static void ConsoleBeacon(string id, string version, string menuName)
        {
            NetPlayer sender = GetPlayerFromID(id);
            VRRig vrrig = GetVRRigFromPlayer(sender);

            Color userColor = Color.red;

            NotifiLib.SendNotification("<color=grey>[</color><color=purple>ADMIN</color><color=grey>]</color> " + sender.NickName + " is using " + menuName + " version " + version + ".");
            VRRig.LocalRig.PlayHandTapLocal(29, false, 99999f);
            VRRig.LocalRig.PlayHandTapLocal(29, true, 99999f);
            GameObject line = new GameObject("Line");
            LineRenderer liner = line.AddComponent<LineRenderer>();
            liner.startColor = userColor; liner.endColor = userColor; liner.startWidth = 0.25f; liner.endWidth = 0.25f; liner.positionCount = 2; liner.useWorldSpace = true;

            liner.SetPosition(0, vrrig.transform.position + new Vector3(0f, 9999f, 0f));
            liner.SetPosition(1, vrrig.transform.position - new Vector3(0f, 9999f, 0f));
            liner.material.shader = Shader.Find("GUI/Text Shader");
            Object.Destroy(line, 3f);
        }
        
        public static void CopyIDAll()
        {
            foreach (var id in VRRigCache.m_activeRigs.Select(vrrig => GetPlayerFromVRRig(vrrig).UserId))
            {
                NotifiLib.SendNotification("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> " + id);
                GUIUtility.systemCopyBuffer = id;
            }
        }
        
        public static void UncapFPS()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = int.MaxValue;
        }
        
        public static void CopySelfID()
        {
            string id = PhotonNetwork.LocalPlayer.UserId;
            NotifiLib.SendNotification("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> " + id);
            GUIUtility.systemCopyBuffer = id;
        }
        
        public static void PCControllerEmulation()
        {
            ControllerInputPoller.instance.rightControllerPrimaryButton |= UnityInput.Current.GetKey(KeyCode.E);
            ControllerInputPoller.instance.rightControllerSecondaryButton |= UnityInput.Current.GetKey(KeyCode.R);

            ControllerInputPoller.instance.leftControllerPrimaryButton |= UnityInput.Current.GetKey(KeyCode.F);
            ControllerInputPoller.instance.leftControllerSecondaryButton |= UnityInput.Current.GetKey(KeyCode.G);

            ControllerInputPoller.instance.leftGrab |= UnityInput.Current.GetKey(KeyCode.LeftBracket);
            ControllerInputPoller.instance.leftControllerGripFloat += UnityInput.Current.GetKey(KeyCode.LeftBracket) ? 1f : 0f;

            ControllerInputPoller.instance.rightGrab |= UnityInput.Current.GetKey(KeyCode.RightBracket);
            ControllerInputPoller.instance.rightControllerGripFloat += UnityInput.Current.GetKey(KeyCode.RightBracket) ? 1f : 0f;

            ControllerInputPoller.instance.rightControllerTriggerButton |= UnityInput.Current.GetKey(KeyCode.Equals);
            ControllerInputPoller.instance.rightControllerIndexFloat += UnityInput.Current.GetKey(KeyCode.Equals) ? 1f : 0f;

            ControllerInputPoller.instance.leftControllerTriggerButton |= UnityInput.Current.GetKey(KeyCode.Minus);
            ControllerInputPoller.instance.leftControllerIndexFloat += UnityInput.Current.GetKey(KeyCode.Minus) ? 1f : 0f;

            ControllerInputPoller.instance.rightControllerTriggerButton |= UnityInput.Current.GetKey(KeyCode.Equals);
            ControllerInputPoller.instance.rightControllerIndexFloat += UnityInput.Current.GetKey(KeyCode.Equals) ? 1f : 0f;
        }

        public static void Test(Transform pointerOrigin)
        {
            
        }
        
        public static void RemoveFlicklimit()
        {
            GorillaTagger.Instance.maxTagDistance = 2.5f;
        }
        
        public static void TestGun()
        {
            GunLibTEst.AthrionGunLibrary.start2guns(delegate()
            {
                Debug.Log("Working");
            }, true);
        }
        
        public static void TagGun()
        {
            VRRig rig = GorillaTagger.Instance.offlineVRRig;
            GunLibTEst.AthrionGunLibrary.start2guns(delegate()
            {
                rig.enabled = false;
                rig.transform.position = GunLibTEst.AthrionGunLibrary.LockedRigOrPlayerOrwhatever.transform.position + new Vector3(0f, -2f, 0f);
                GameMode.ReportTag(GunLibTEst.AthrionGunLibrary.LockedRigOrPlayerOrwhatever.Creator);
            }, true);
            rig.enabled = true;
        }
        
        public static bool hasGivenCosmetics;
        public static void UnlockAllCosmetics()
        {
            CosmeticPatch.enabled = true;
            if (!PostGetData.CosmeticsInitialized || hasGivenCosmetics) return;
            hasGivenCosmetics = true;
            MethodInfo unlockItem = typeof(CosmeticsController).GetMethod("UnlockItem", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var item in CosmeticsController.instance.allCosmetics.Where(item => !CosmeticsController.instance.concatStringCosmeticsAllowed.Contains(item.itemName)))
            {
                try
                {
                    unlockItem.Invoke(CosmeticsController.instance, new object[] { item.itemName, false });
                }
                catch
                {
                }
            }
        }
        
        
    }
}