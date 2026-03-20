using BepInEx;
using ExitGames.Client.Photon;
using Fusion;
using g3;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaLocomotion.Gameplay;
using GorillaLocomotion.Swimming;
using GorillaNetworking;
using liquid.client.Patches.Internal;
using liquidclient.Classes;
using liquidclient.GunLib;
using liquidclient.Menu;
using liquidclient.mods;
using liquidclient.Notifications;
using liquidclient.Patches.Internal;
using Oculus.Interaction.Grab.GrabSurfaces;
using Oculus.Interaction.Input;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice;
using Photon.Voice.PUN;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Transactions;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Valve.VR.InteractionSystem;
using static liquidclient.Menu.Main;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Player = Photon.Realtime.Player;


namespace liquidclient.Mods
{
    public class Movement
    {
        // Player
        public static NetPlayer Photon_local_player = PhotonNetwork.LocalPlayer;
        public static NetPlayer Network_local_player = NetworkSystem.Instance.LocalPlayer;
        public static VRRig Rig = GorillaTagger.Instance.offlineVRRig;
        public static Rigidbody Rig_Rigidbody = GorillaTagger.Instance.rigidbody;
        public static VRRig Vrrig = VRRig.LocalRig;
        // Guardian Varibles
        public static GorillaGuardianManager GuardianManager = (GorillaGuardianManager)GorillaGameManager.instance;
        public static void Fly()
        {
            if (ControllerInputPoller.instance.rightControllerPrimaryButton)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * Time.deltaTime * Settings.Movement.flySpeed;
                Rig_Rigidbody.linearVelocity = Vector3.zero;
            }
        }

        public static void Slingshotfly(bool Iszerogravity, bool Islowgravsling)
        {
            if (ControllerInputPoller.instance.rightControllerPrimaryButton)
            {
                if (!Iszerogravity)
                {
                    Rig_Rigidbody.linearVelocity += GorillaTagger.Instance.headCollider.transform.forward * Time.deltaTime * Settings.Movement.flySpeed;
                }
                else
                {
                    if (Islowgravsling)
                    {
                        Gravityhelper(false, true);
                        Rig_Rigidbody.linearVelocity += GorillaTagger.Instance.headCollider.transform.forward * Time.deltaTime * Settings.Movement.flySpeed;
                    }
                    else
                    {
                        Gravityhelper(true, false);
                        Rig_Rigidbody.linearVelocity += GorillaTagger.Instance.headCollider.transform.forward * Time.deltaTime * Settings.Movement.flySpeed;
                    }
                }
            }
        }





        public static void Gravityhelper(bool isZerograv, bool Islowgrav)
        {
            if (isZerograv)
            {
                Rig_Rigidbody.AddForce(-Physics.gravity, ForceMode.Acceleration);
            }

            if (!isZerograv)
            {
                if (Islowgrav)
                {
                    Rig_Rigidbody.AddForce(Vector3.up * 5.36f, ForceMode.Acceleration);
                }
                else
                {
                    Rig_Rigidbody.AddForce(Vector3.down * 6.93f, ForceMode.Acceleration);
                }
            }
        }

        public static void Checkmaster()
        {
            if (!NetworkSystem.Instance.IsMasterClient && PhotonNetwork.InRoom)
            {
                NotifiLib.SendNotification("You are not master client!");
                return;
            }

            if (NetworkSystem.Instance.IsMasterClient && PhotonNetwork.InRoom)
            {
                NotifiLib.SendNotification("You are master client!");
                return;
            }
        }

   

        public static void Joincode(string Code) =>
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(Code, GorillaNetworking.JoinType.Solo);

        public static void unguardianall()
        {
            foreach (var ZoneManager in GorillaGuardianZoneManager.zoneManagers.Where(gorillaGuardianZoneManager => gorillaGuardianZoneManager.enabled && gorillaGuardianZoneManager.IsZoneValid()))
            {
                ZoneManager.SetGuardian(null);
            }
        }


        public static void guardianall()
        {
            int Players = 0;

            foreach (var ZoneManager in GorillaGuardianZoneManager.zoneManagers.Where(gorillaGuardianZoneManager => gorillaGuardianZoneManager.enabled && gorillaGuardianZoneManager.IsZoneValid()))
            {
                ZoneManager.SetGuardian(PhotonNetwork.PlayerList[Players]);
                Players++;
            }
        }

        public static void Guardianothers()
        {
            int others = 0;
            foreach (var ZoneManager in GorillaGuardianZoneManager.zoneManagers.Where(gorillaGuardianZoneManager => gorillaGuardianZoneManager.enabled && gorillaGuardianZoneManager.IsZoneValid()))
            {
                ZoneManager.SetGuardian(PhotonNetwork.PlayerListOthers[others]);
                others++;
            }
        }

        public static void HoverboardColor(Color color, bool IsRGB)
        {
            HoverboardVisual Hoverboardvisual = Vrrig.hoverboardVisual;
            if (Vrrig.hoverboardVisual.IsHeld && Vrrig.hoverboardVisual != null)
            {
                float frames = Time.time / 3f % 1f;
                Color RGB = Color.HSVToRGB(frames, 1f, 1f);
                if (IsRGB)
                {
                    Vrrig.hoverboardVisual.SetIsHeld(Hoverboardvisual.IsLeftHanded, Hoverboardvisual.NominalLocalPosition, Hoverboardvisual.NominalLocalRotation, RGB);
                }
                else
                {
                    Vrrig.hoverboardVisual.SetIsHeld(Hoverboardvisual.IsLeftHanded, Hoverboardvisual.NominalLocalPosition, Hoverboardvisual.NominalLocalRotation, color);
                }
            }
        }

        public static void Guardianhelper(bool Makeguardian, bool Everyoneisguardian, bool guardothers)
        {
            if (NetworkSystem.Instance.InRoom && NetworkSystem.Instance.IsMasterClient)
            {
                if (Makeguardian)
                {

                    foreach (TappableGuardianIdol Guardianidol in GetAllType<TappableGuardianIdol>())
                    {
                        GorillaGuardianZoneManager zoneManager = Guardianidol.zoneManager;
                        if (Guardianidol.manager.photonView && Guardianidol.manager && !Guardianidol.isChangingPositions)
                        {
                            if (zoneManager.IsZoneValid() && Guardianidol.manager && zoneManager.CurrentGuardian == null)
                            {
                                zoneManager.SetGuardian(Photon_local_player);
                                return;
                            }
                        }
                    }
                }

                if (!Makeguardian)
                {
                    unguardianall();
                }

                if (Everyoneisguardian)
                {
                    guardianall();
                }

                if (!Everyoneisguardian)
                {
                    unguardianall();
                }

                if (guardothers)
                {
                    Guardianothers();
                }

                if (!guardothers)
                {
                    unguardianall();
                }
            }
        }

        // 143 23 24 1
        public static void LagServer(byte b)
        {
            bool inroom = !PhotonNetwork.InRoom;
            if (!inroom)
            {
                bool freezedelay_ = Time.time > Lagdelay;
                if (freezedelay_)
                {
                    for (int i = 0; i < 11; i++)
                    {
                        WebFlags flags = new WebFlags(byte.MaxValue);
                        NetEventOptions netEventOptions = new NetEventOptions
                        {
                            Flags = flags,
                            TargetActors = new int[]
                            {
                -1
                            }
                        };
                        NetworkSystemRaiseEvent.RaiseEvent(b, new object[]
                        {
            "Slkyy!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!"
                        }, netEventOptions, false);
                    }
                    Lagdelay = Time.time + 1f;
                    RPCProtection();
                }
            }
        }











        public static float Lagdelay;

        public static float delay = 0f;



        public static void IronMan()
        {
            if (ControllerInputPoller.instance.leftControllerGripFloat > 0.5f)
            {
                Rigidbody rb = GTPlayer.Instance.bodyCollider.attachedRigidbody;
                Transform leftHand = GorillaTagger.Instance.leftHandTransform;

                rb.AddForce(13 * leftHand.up, ForceMode.Acceleration);
                rb.AddForce(2 * -leftHand.right, ForceMode.Acceleration);

                GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tapHapticStrength, GorillaTagger.Instance.tagHapticDuration);

                CreateFireEffect(leftHand.position);
            }

            if (ControllerInputPoller.instance.rightControllerGripFloat > 0.5f)
            {
                Rigidbody rb = GTPlayer.Instance.bodyCollider.attachedRigidbody;
                Transform rightHand = GorillaTagger.Instance.rightHandTransform;

                rb.AddForce(13 * rightHand.up, ForceMode.Acceleration);
                rb.AddForce(2 * rightHand.right, ForceMode.Acceleration);

                GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength, GorillaTagger.Instance.tagHapticDuration);

                CreateFireEffect(rightHand.position);
            }
        }

        private static void CreateFireEffect(Vector3 position)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.transform.localScale = Vector3.one * 0.2f;
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetColor("_Color", Color.red);
            sphere.GetComponent<Renderer>().material = mat;
            Object.Destroy(sphere, 0.3f);
        }

        public static GameObject CheckPoint;
        // NO THIS IS NOT SKIDDED I MADE THIS
        public static void Checkpoint()
        {

            {
                if (ControllerInputPoller.instance.rightGrab)
                {
                    if (CheckPoint == null)
                    {
                        CheckPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        UnityEngine.Object.Destroy(CheckPoint.GetComponent<Rigidbody>());
                        UnityEngine.Object.Destroy(CheckPoint.GetComponent<SphereCollider>());
                        CheckPoint.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    }
                    CheckPoint.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                }
                if (CheckPoint != null)
                {
                    if (ControllerInputPoller.instance.rightControllerPrimaryButton)
                    {
                        CheckPoint.GetComponent<Renderer>().material.color = Color.gray;
                        TeleportPlayer(CheckPoint.transform.position);
                        GorillaLocomotion.GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                    }
                    else
                    {
                        CheckPoint.GetComponent<Renderer>().material.color = Color.navyBlue;
                    }
                }
            }
        }

        public static void TeleportPlayer(Vector3 pos)
        {
            pos = World2Player(pos);

            GorillaLocomotion.GTPlayer.Instance.GetComponent<Rigidbody>().transform.position = pos;
            typeof(GorillaLocomotion.GTPlayer).GetField("lastPosition").SetValue(GorillaLocomotion.GTPlayer.Instance, pos);
            typeof(GorillaLocomotion.GTPlayer).GetField("velocityHistory").SetValue(GorillaLocomotion.GTPlayer.Instance, new Vector3[GorillaLocomotion.GTPlayer.Instance.velocityHistorySize]);

            GorillaLocomotion.GTPlayer.Instance.lastHeadPosition = GorillaLocomotion.GTPlayer.Instance.headCollider.transform.position;
            typeof(GorillaLocomotion.GTPlayer).GetField("lastLeftHandPosition").SetValue(GorillaLocomotion.GTPlayer.Instance, pos);
            typeof(GorillaLocomotion.GTPlayer).GetField("lastRightHandPosition").SetValue(GorillaLocomotion.GTPlayer.Instance, pos);

        }

        public static Vector3 World2Player(Vector3 world)
        {
            return world - GorillaTagger.Instance.bodyCollider.transform.position + GorillaTagger.Instance.transform.position;
        }

        // Right hand
        public static GameObject RightHandBottom;
        public static GameObject RightHandTop;
        public static GameObject RightHandLeft;
        public static GameObject RightHandRight;
        public static GameObject RightHandFront;
        public static GameObject RightHandBack;
        // Left hand
        public static GameObject LeftHandBottom;
        public static GameObject LeftHandTop;
        public static GameObject LeftHandLeft;
        public static GameObject LeftHandRight;
        public static GameObject LeftHandFront;
        public static GameObject LeftHandBack;

        public static void StickyPlatforms()
        {
            if (ControllerInputPoller.instance.rightGrab && RightHandBottom == null)
            {
                // create the shit righthand
                RightHandBottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
                RightHandBottom.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                RightHandBottom.transform.position = TrueRightHand().position - new Vector3(0, 0.05f, 0);
                RightHandBottom.transform.rotation = TrueRightHand().rotation;
                RightHandBottom.AddComponent<ColorChanger>().colors = liquidclient.Settings.backgroundColor;
                // If color changer fails do black
                RightHandBottom.GetComponent<Renderer>().material.color = Color.black;

                RightHandTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
                RightHandTop.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                RightHandTop.transform.position = TrueRightHand().position + new Vector3(0, 0.05f, 0);
                RightHandTop.transform.rotation = TrueRightHand().rotation;
                RightHandTop.GetComponent<Renderer>().enabled = false;

                RightHandRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
                RightHandRight.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                RightHandRight.transform.position = TrueRightHand().position + new Vector3(0.05f, 0, 0);
                RightHandRight.transform.eulerAngles = TrueRightHand().rotation.eulerAngles + new Vector3(0, 0, 90);
                RightHandRight.GetComponent<Renderer>().enabled = false;

                RightHandLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
                RightHandLeft.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                RightHandLeft.transform.position = TrueRightHand().position - new Vector3(0.05f, 0, 0);
                RightHandLeft.transform.eulerAngles = TrueRightHand().rotation.eulerAngles + new Vector3(0, 0, 90);
                RightHandLeft.GetComponent<Renderer>().enabled = false;

                RightHandFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
                RightHandFront.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                RightHandFront.transform.position = TrueRightHand().position + new Vector3(0, 0, 0.05f);
                RightHandFront.transform.eulerAngles = TrueRightHand().rotation.eulerAngles + new Vector3(90, 0, 0);
                RightHandFront.GetComponent<Renderer>().enabled = false;

                RightHandBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
                RightHandBack.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                RightHandBack.transform.position = TrueRightHand().position - new Vector3(0, 0, 0.05f);
                RightHandBack.transform.eulerAngles = TrueRightHand().rotation.eulerAngles + new Vector3(90, 0, 0);
                RightHandBack.GetComponent<Renderer>().enabled = false;
            }
            else if (!ControllerInputPoller.instance.rightGrab)
            {
                // destroy it if they are not holding rightgrip
                GameObject.Destroy(RightHandBottom, Time.deltaTime);
                GameObject.Destroy(RightHandTop, Time.deltaTime);
                GameObject.Destroy(RightHandLeft, Time.deltaTime);
                GameObject.Destroy(RightHandRight, Time.deltaTime);
                GameObject.Destroy(RightHandFront, Time.deltaTime);
                GameObject.Destroy(RightHandBack, Time.deltaTime);
            }

            if (ControllerInputPoller.instance.leftGrab && LeftHandBottom == null)
            {
                // left hand shit
                LeftHandBottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
                LeftHandBottom.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                LeftHandBottom.transform.position = TrueLeftHand().position - new Vector3(0, 0.05f, 0);
                LeftHandBottom.transform.rotation = TrueLeftHand().rotation;
                LeftHandBottom.AddComponent<ColorChanger>().colors = liquidclient.Settings.backgroundColor;
                LeftHandBottom.GetComponent<Renderer>().material.color = Color.black;

                LeftHandTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
                LeftHandTop.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                LeftHandTop.transform.position = TrueLeftHand().position + new Vector3(0, 0.05f, 0);
                LeftHandTop.transform.rotation = TrueLeftHand().rotation;
                LeftHandTop.GetComponent<Renderer>().enabled = false;

                LeftHandRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
                LeftHandRight.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                LeftHandRight.transform.position = TrueLeftHand().position + new Vector3(0.05f, 0, 0);
                LeftHandRight.transform.eulerAngles = TrueLeftHand().rotation.eulerAngles + new Vector3(0, 0, 90);
                LeftHandRight.GetComponent<Renderer>().enabled = false;

                LeftHandLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
                LeftHandLeft.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                LeftHandLeft.transform.position = TrueLeftHand().position - new Vector3(0.05f, 0, 0);
                LeftHandLeft.transform.eulerAngles = TrueLeftHand().rotation.eulerAngles + new Vector3(0, 0, 90);
                LeftHandLeft.GetComponent<Renderer>().enabled = false;

                LeftHandFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
                LeftHandFront.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                LeftHandFront.transform.position = TrueLeftHand().position + new Vector3(0, 0, 0.05f);
                LeftHandFront.transform.eulerAngles = TrueLeftHand().rotation.eulerAngles + new Vector3(90, 0, 0);
                LeftHandFront.GetComponent<Renderer>().enabled = false;

                LeftHandBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
                LeftHandBack.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                LeftHandBack.transform.position = TrueLeftHand().position - new Vector3(0, 0, 0.05f);
                LeftHandBack.transform.eulerAngles = TrueLeftHand().rotation.eulerAngles + new Vector3(90, 0, 0);
                LeftHandBack.GetComponent<Renderer>().enabled = false;
            }
            else if (!ControllerInputPoller.instance.leftGrab)
            {
                // same thing
                GameObject.Destroy(LeftHandBottom, Time.deltaTime);
                GameObject.Destroy(LeftHandTop, Time.deltaTime);
                GameObject.Destroy(LeftHandLeft, Time.deltaTime);
                GameObject.Destroy(LeftHandRight, Time.deltaTime);
                GameObject.Destroy(LeftHandFront, Time.deltaTime);
                GameObject.Destroy(LeftHandBack, Time.deltaTime);
            }
        }

        private static GameObject leftplat = null;
        private static GameObject rightplat = null;
        private static GameObject CreatePlatformOnHand(Transform handTransform)
        {
            GameObject plat = GameObject.CreatePrimitive(PrimitiveType.Cube);

            plat.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);

            plat.transform.position = handTransform.position;
            plat.transform.rotation = handTransform.rotation;
            return plat;
        }
        public static void PlatformModbysigmaboy()

        {

            if (ControllerInputPoller.instance.leftGrab && leftplat == null)
            {
                leftplat = CreatePlatformOnHand(GorillaTagger.Instance.leftHandTransform);
                ColorChanger colorChanger = leftplat.AddComponent<ColorChanger>();
                colorChanger.colors = liquidclient.Settings.backgroundColor;
            }

            if (ControllerInputPoller.instance.rightGrab && rightplat == null)
            {
                rightplat = CreatePlatformOnHand(GorillaTagger.Instance.rightHandTransform);
                ColorChanger colorChanger = rightplat.AddComponent<ColorChanger>();
                colorChanger.colors = liquidclient.Settings.backgroundColor;
            }

            if (ControllerInputPoller.instance.rightGrabRelease && rightplat != null)
            {
                rightplat.Disable();
                rightplat = null;

            }

            if (ControllerInputPoller.instance.leftGrabRelease && leftplat != null)
            {
                leftplat.Disable();
                leftplat = null;
            }
        }

        public static void Triggerplats()
        {

            if (ControllerInputPoller.instance.leftControllerTriggerButton && leftplat == null)
            {
                leftplat = CreatePlatformOnHand(GorillaTagger.Instance.leftHandTransform);
                ColorChanger colorChanger = leftplat.AddComponent<ColorChanger>();
                colorChanger.colors = liquidclient.Settings.backgroundColor;
            }

            if (ControllerInputPoller.instance.rightControllerTriggerButton && rightplat == null)
            {
                rightplat = CreatePlatformOnHand(GorillaTagger.Instance.rightHandTransform);
                ColorChanger colorChanger = rightplat.AddComponent<ColorChanger>();
                colorChanger.colors = liquidclient.Settings.backgroundColor;
            }

            if (!ControllerInputPoller.instance.rightControllerTriggerButton && rightplat != null)
            {
                rightplat.Disable();
                rightplat = null;
            }

            if (!ControllerInputPoller.instance.leftControllerTriggerButton && leftplat != null)
            {
                leftplat.Disable();
                leftplat = null;
            }
        }







        public static bool previousTeleportTrigger;
        public static void TeleportGun()
        {
            /*if (ControllerInputPoller.instance.rightGrab)
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (ControllerInputPoller.TriggerFloat(XRNode.RightHand) > 0.5f && !previousTeleportTrigger)
                {
                    GTPlayer.Instance.TeleportTo(NewPointer.transform.position + Vector3.up, GTPlayer.Instance.transform.rotation);
                    GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
                }

                previousTeleportTrigger = ControllerInputPoller.TriggerFloat(XRNode.RightHand) > 0.5f;
            }*/


            GunLibTEst.AthrionGunLibrary.StartPcGun(delegate ()
            {
                var teleportposint = GunLibTEst.AthrionGunLibrary.GetPointerPos();
                var Teleportrotation = GorillaTagger.Instance.transform.rotation;
                GTPlayer.Instance.TeleportTo(teleportposint + Vector3.up, Teleportrotation, false, false);
            }, false);
        }

        public static void speedboost()
        {
            GTPlayer.Instance.maxJumpSpeed = 7.5f;
            GTPlayer.Instance.jumpMultiplier = 1.1f;
        }





        public static void TP_Stump()
        {
            GTPlayer.Instance.TeleportTo(new Vector3(-68.647f, 12.406f, -83.699f), GTPlayer.Instance.transform.rotation, false, true);
            GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
        }

        /*public static GameObject airSwimPart;
        public static void AirSwim()
        {
            if (airSwimPart == null)
            {
                airSwimPart = Object.Instantiate(GetObject("Environment Objects/LocalObjects_Prefab/ForestToBeach/ForestToBeach_Prefab_V4/CaveWaterVolume"));
                airSwimPart.transform.localScale = new Vector3(5f, 5f, 5f);
                airSwimPart.GetComponent<Renderer>().enabled = false;
            }
            else
            {
                GTPlayer.Instance.audioManager.UnsetMixerSnapshot();
                airSwimPart.transform.position = GorillaTagger.Instance.headCollider.transform.position + new Vector3(0f, 2.5f, 0f);
            }
        }

        public static void DisableAirSwim()
        {
            if (airSwimPart != null)
                Object.Destroy(airSwimPart);
        }

        public static void SetSwimSpeed(float speed = 3f) =>
            GTPlayer.Instance.swimmingParams.swimmingVelocityOutOfWaterDrainRate = speed;

        private static float? waterSurfaceJumpAmount;
        private static float? waterSurfaceJumpMaxSpeed;
        public static void WaterRunHelper(bool enable)
        {
            if (enable)
            {
                waterSurfaceJumpAmount = GTPlayer.Instance.swimmingParams.waterSurfaceJumpAmount;
                waterSurfaceJumpMaxSpeed = GTPlayer.Instance.swimmingParams.waterSurfaceJumpMaxSpeed;

                GTPlayer.Instance.swimmingParams.waterSurfaceJumpAmount = 1.25f;
                GTPlayer.Instance.swimmingParams.waterSurfaceJumpMaxSpeed = 4.333f;
            }
            else
            {
                GTPlayer.Instance.swimmingParams.waterSurfaceJumpAmount = waterSurfaceJumpAmount ?? 0.6f;
                GTPlayer.Instance.swimmingParams.waterSurfaceJumpMaxSpeed = waterSurfaceJumpMaxSpeed ?? 1f;
            }
        }

        public static void DisableWater()
        {
            foreach (WaterVolume waterVolume in GetAllType<WaterVolume>())
            {
                GameObject v = waterVolume.gameObject;
                v.layer = LayerMask.NameToLayer("TransparentFX");
            }
        }*/

        private static float flapTime;
        public static void BirdFly()
        {
            UnityEngine.XR.InputDevice lefthand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            UnityEngine.XR.InputDevice righthand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, GorillaTagger.Instance.headCollider.transform.position) < 0.63f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.headCollider.transform.position) < 0.63f)
                return;

            if (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, GorillaTagger.Instance.rightHandTransform.position) < 1f)
                return;
            if (Physics.Raycast(GorillaTagger.Instance.bodyCollider.attachedRigidbody.position, Vector3.down, hitInfo: out _, Physics.AllLayers))
                return;



            if (lefthand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out Vector3 leftVel) && righthand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out Vector3 rightVel))
            {
                if (Time.time - flapTime < 0.4f) return;

                if (leftVel.y < -1.2f && rightVel.y < -1.2f)
                {
                    float force = Mathf.Min(6f * ((Mathf.Abs(leftVel.y) + Mathf.Abs(rightVel.y)) / 2f) / 1.2f, 9f);
                    GorillaTagger.Instance.bodyCollider.attachedRigidbody.AddForce(Vector3.up * force, ForceMode.VelocityChange);

                    flapTime = Time.time;
                }
            }
        }
        /*
        public static void SolidWater()
        {
            foreach (WaterVolume waterVolume in GetAllType<WaterVolume>())
            {
                GameObject v = waterVolume.gameObject;
                v.layer = LayerMask.NameToLayer("Default");
            }
        }

        public static void FixWater()
        {
            foreach (WaterVolume waterVolume in GetAllType<WaterVolume>())
            {
                GameObject v = waterVolume.gameObject;
                v.layer = LayerMask.NameToLayer("Water");
            }
        }*/

        public static void AddCurrencySelf()
        {
            int moneygiven = 1000;
            if (!NetworkSystem.Instance.IsMasterClient)
            {
                return;
            }
            GRPlayer.Get(PhotonNetwork.LocalPlayer.actorNumber).shiftCreditCache = +moneygiven;
            Notifications.NotifiLib.SendNotification("Succses fully added" + moneygiven + "Credits");
        }

        public static void fastspeedboost()
        {
            GTPlayer.Instance.maxJumpSpeed = 11.3f;
            GTPlayer.Instance.jumpMultiplier = 1.3f;
        }
        public static void rightgripspeedboost()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                GTPlayer.Instance.maxJumpSpeed = 11.3f;
                GTPlayer.Instance.jumpMultiplier = 1.3f;
            }
        }

        public static void fastFly()
        {
            if (ControllerInputPoller.instance.rightControllerPrimaryButton)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * Time.deltaTime * 67;
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            }
        }

        private static bool ghosted = false;
        //private static GameObject Rballhand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //private static GameObject Lballhand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        public static void Ghost()
        {
            if (ControllerInputPoller.instance.rightControllerPrimaryButton && ghosted == false)
            {
                ghosted = true;
                /*Lballhand.SetActive(true);
                Rballhand.SetActive(true);
                Lballhand.AddComponent<ColorChanger>().colors = liquidclient.Settings.backgroundColor;
                Rballhand.AddComponent<ColorChanger>().colors = liquidclient.Settings.backgroundColor;
                Lballhand.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                Rballhand.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                Lballhand.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                Rballhand.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;*/
            }

            VRRig.LocalRig.enabled = !ghosted;

            if (ControllerInputPoller.instance.rightControllerSecondaryButton && ghosted)
            {
                ghosted = false;
                //Lballhand.SetActive(false);
                //Rballhand.SetActive(false);
            }
        }

        public static void UpAndDown()
        {
            if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.4f)
            {
                GorillaTagger.Instance.rigidbody.AddForce(GorillaTagger.Instance.offlineVRRig.transform.up * 5000f);
            }
            else if (ControllerInputPoller.instance.leftControllerIndexFloat > 0.4f)
            {
                GorillaTagger.Instance.rigidbody.AddForce(GorillaTagger.Instance.offlineVRRig.transform.up * -5000f);
            }
        }

        public static void NoClip()
        {
            bool DisableColliders = ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f;
            MeshCollider[] colliders = Resources.FindObjectsOfTypeAll<MeshCollider>();

            foreach (MeshCollider collider in colliders)
            {
                collider.enabled = !DisableColliders;
            }

        }

        public static void Bouncy()
        {
            GorillaTagger.Instance.bodyCollider.material.bounciness = 1f;
            GorillaTagger.Instance.bodyCollider.material.bounceCombine = (PhysicsMaterialCombine)3;
            GorillaTagger.Instance.bodyCollider.material.dynamicFriction = 0f;
        }
        public static void ResetBouncy()
        {
            GorillaTagger.Instance.bodyCollider.material.bounciness = 0f;
            GorillaTagger.Instance.bodyCollider.material.bounceCombine = 0;
            GorillaTagger.Instance.bodyCollider.material.dynamicFriction = 0f;
        }


        public static void SpazHead()
        {
            VRMap head = GorillaTagger.Instance.offlineVRRig.head;
            head.trackingRotationOffset.z = head.trackingRotationOffset.z + 10f;
            head.trackingRotationOffset.y = head.trackingRotationOffset.y + 10f;
            head.trackingRotationOffset.x = head.trackingRotationOffset.x + 10f;
        }


        public static void FixRig()
        {
            Rig.head.trackingRotationOffset.x = 0f;
            Rig.head.trackingRotationOffset.y = 0f;
            Rig.head.trackingRotationOffset.z = 0f;
            Rig.headBodyOffset.y = 0;
            Rig.headBodyOffset.x = 0;
            Rig.headBodyOffset.z = 0;
            Rig.enabled = true;
        }



        public static void UpsideDownHead()
        {
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.z = 180f;
        }

        public static void BackwardsHead()
        {
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.y = 180f;
        }

        public static void SpinHeadX()
        {
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.x += 10f;
        }

        public static void SpinHeadY()
        {
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.y += 10f;
        }

        public static void SpinHeadZ()
        {
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.z += 10f;
        }










        public static void PlatformSpam()
        {

            bool rightGrip = ControllerInputPoller.instance.rightControllerGripFloat > 0.8f;

            if (rightGrip)
            {
                GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                UnityEngine.Object.Destroy(gameObject.GetComponent<BoxCollider>());

                Renderer renderer = gameObject.GetComponent<Renderer>();
                renderer.material.color = Color.black;
                renderer.material.shader = Shader.Find("GorillaTag/UberShader");

                gameObject.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                gameObject.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                gameObject.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                gameObject.AddComponent<ColorChanger>();
                gameObject.GetComponent<ColorChanger>().colors = liquidclient.Settings.backgroundColor;
                UnityEngine.Object.Destroy(gameObject, 1f);
            }
        }

        public static void Tracer()
        {
            foreach (VRRig vrrig in VRRigCache.m_activeRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig)
                {
                    GameObject line = new GameObject("Line");
                    LineRenderer liner = line.AddComponent<LineRenderer>();
                    UnityEngine.Color thecolor = vrrig.playerColor;
                    liner.startColor = thecolor; liner.endColor = thecolor; liner.startWidth = 0.010f; liner.endWidth = 0.010f; liner.positionCount = 2; liner.useWorldSpace = true;
                    liner.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                    liner.SetPosition(1, vrrig.transform.position);
                    liner.material.shader = Shader.Find("GUI/Text Shader");
                    UnityEngine.Object.Destroy(line, Time.deltaTime);
                }
            }
        }


        public static void Ffps()
        {
            Application.targetFrameRate = 40;
            QualitySettings.vSyncCount = 0;
        }

        public static void Tfps()
        {
            Application.targetFrameRate = 20;
            QualitySettings.vSyncCount = 0;
        }

        public static void FixFPS()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 120;
        }



        public static void beacons()
        {
            foreach (VRRig vrrig in VRRigCache.m_activeRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig)
                {
                    GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    beacon.transform.position = vrrig.transform.position + new Vector3(0f, 2f, 0f);
                    UnityEngine.Object.Destroy(beacon.GetComponent<CapsuleCollider>());
                    beacon.transform.localScale = new Vector3(0.2f, 6f, 0.001f);
                    beacon.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    beacon.GetComponent<Renderer>().material.color = vrrig.playerColor;
                    UnityEngine.Object.Destroy(beacon, Time.deltaTime);
                }
            }
        }

        public static void Box_ESP()
        {
            foreach (VRRig vrrig in VRRigCache.m_activeRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig)
                {
                    GameObject Box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Box.transform.position = vrrig.transform.position;
                    UnityEngine.Object.Destroy(Box.GetComponent<Collider>());
                    Box.transform.localScale = new Vector3(0.40f, 0.40f, 0.40f);
                    Box.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    Box.GetComponent<Renderer>().material.color = vrrig.playerColor;
                    UnityEngine.Object.Destroy(Box, Time.deltaTime);
                }
            }
        }

        public static void Circle_ESP()
        {
            foreach (VRRig vrrig in VRRigCache.m_activeRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig)
                {
                    GameObject Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Sphere.transform.position = vrrig.transform.position;
                    UnityEngine.Object.Destroy(Sphere.GetComponent<Collider>());
                    Sphere.transform.localScale = new Vector3(0.40f, 0.40f, 0.40f);
                    Sphere.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    Sphere.GetComponent<Renderer>().material.color = vrrig.playerColor;
                    UnityEngine.Object.Destroy(Sphere, Time.deltaTime);
                }
            }
        }

        public static void Bug_ESP()
        {
            var Bug = GameObject.Find("Floating Bug Holdable");
            if (Bug != null)
            {
                GameObject Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Sphere.transform.position = Bug.transform.position;
                UnityEngine.Object.Destroy(Sphere.GetComponent<Collider>());
                Sphere.transform.localScale = new Vector3(0.40f, 0.40f, 0.40f);
                Sphere.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                Sphere.GetComponent<Renderer>().material.color = Color.burlywood;
                UnityEngine.Object.Destroy(Sphere, Time.deltaTime);
            }
        }

        public static void Bat_ESP()
        {
            var Bat = GameObject.Find("Cave Bat Holdable");
            if (Bat != null)
            {
                GameObject Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Sphere.transform.position = Bat.transform.position;
                UnityEngine.Object.Destroy(Sphere.GetComponent<Collider>());
                Sphere.transform.localScale = new Vector3(0.40f, 0.40f, 0.40f);
                Sphere.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                Sphere.GetComponent<Renderer>().material.color = Color.rebeccaPurple;
                UnityEngine.Object.Destroy(Sphere, Time.deltaTime);
            }
        }
        // E
        public static void Grabbug()
        {
            var Bug = GameObject.Find("Floating Bug Holdable");
            //PhotonView Bugss = Bug.GetPhotonView();
            if (ControllerInputPoller.instance.rightGrab && Bug != null)
            {
                //Bugss.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                //Bugss.transform.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                Bug.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                Bug.transform.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
            }
        }

        public static void GrabBat()
        {
            var Bat = GameObject.Find("Cave Bat Holdable");
            //PhotonView Batss = Bat.GetPhotonView();
            if (ControllerInputPoller.instance.rightGrab && Bat != null)
            {
                //Batss.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                //Batss.transform.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                Bat.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                Bat.transform.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
            }
        }

        /*public static void GrabSIItemwood()
        {
            if (ControllerInputPoller.instance.rightGrab && NetworkSystem.Instance.IsMasterClient)
            {
                
            }
        }*/
        public static void ffps()
        {
            Application.targetFrameRate = 5;
            QualitySettings.vSyncCount = 0;
        }

        public static void sfps()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
        }

        public static void ofps()
        {
            Application.targetFrameRate = 120;
            QualitySettings.vSyncCount = 0;
        }






        public static void GrabRig()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                GTPlayer Player = GTPlayer.Instance;
                GorillaTagger.Instance.offlineVRRig.enabled = false;
                GorillaTagger.Instance.offlineVRRig.transform.position = Player.RightHand.controllerTransform.position;
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
        }

        public static string serverLink = "https://discord.gg/v744fGYkyn";





        

        public static void DisableHoverboard()
        {
            GTPlayer.Instance.SetHoverAllowed(false);
            GTPlayer.Instance.SetHoverActive(false);
            VRRig.LocalRig.hoverboardVisual.gameObject.SetActive(false);
        }



        public static void JoinRandom()
        {
            if (PhotonNetwork.InRoom)
            {
                NetworkSystem.Instance.ReturnToSinglePlayer();
                GorillaNetworkJoinTrigger trigger = PhotonNetworkController.Instance.currentJoinTrigger ?? GorillaComputer.instance.GetJoinTriggerForZone("forest");
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(trigger);
            }

            else
            {
                GorillaNetworkJoinTrigger trigger = PhotonNetworkController.Instance.currentJoinTrigger ?? GorillaComputer.instance.GetJoinTriggerForZone("forest");
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(trigger);
            }
        }
        public static void triggerFly()
        {
            if (ControllerInputPoller.instance.rightControllerTriggerButton)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * Time.deltaTime * Settings.Movement.flySpeed;
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            }
        }

        public static void fasttriggerFly()
        {
            if (ControllerInputPoller.instance.rightControllerTriggerButton)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * Time.deltaTime * 67;
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            }
        }


        public static void FPSBoostIndev()
        {
            QualitySettings.vSyncCount = 0;
            QualitySettings.terrainDetailDensityScale = 0.10f;
            QualitySettings.globalTextureMipmapLimit = 1;
        }

        public static void BrokenNeck()
        {
            VRRig.LocalRig.head.trackingRotationOffset.y = 90f;
        }



        public static void RPCProtection()
        {
            if (!PhotonNetwork.InRoom)
                return;

            try
            {
                MonkeAgent.instance.rpcErrorMax = int.MaxValue;
                MonkeAgent.instance.rpcCallLimit = int.MaxValue;
                MonkeAgent.instance.logErrorMax = int.MaxValue;

                PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
                PhotonNetwork.QuickResends = int.MaxValue;

                PhotonNetwork.SendAllOutgoingCommands();
            }
            catch { UnityEngine.Debug.Log("RPC protection failed, are you in a lobby?"); }
        }

        public static void FlushRPCs()
        {
            RPCProtection();
        }

        public static GameObject Forestwind = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/Environment/Forest_ForceVolumes");
        public static GameObject Canyonwind = GameObject.Find("Environment Objects/LocalObjects_Prefab/Canyon/Canyon/Canyon_ForceVolumes");
        public static GameObject Beachwind = GameObject.Find("Environment Objects/LocalObjects_Prefab/Beach/ForceVolumesOcean_Combo_V2");
        public static GameObject cloudswind = GameObject.Find("Environment Objects/LocalObjects_Prefab/skyjungle/Force Volumes");
        public static GameObject basementwind = GameObject.Find("Environment Objects/LocalObjects_Prefab/Basement/DungeonRoomAnchor/DungeonBasement/BasementMouseHoleWindPrefab");
        public static GameObject basement2wind = GameObject.Find("Environment Objects/LocalObjects_Prefab/Basement/DungeonRoomAnchor/DungeonBasement/BasementMouseHoleWindPrefab (1)");
        public static GameObject basement3wind = GameObject.Find("Environment Objects/LocalObjects_Prefab/Basement/DungeonRoomAnchor/DungeonBasement/BasementMouseHoleWindPrefab (2)");

        public static void Destroywind()
        {
            // Thanks sentry for the name of these!


            Forestwind.SetActive(false);
            Canyonwind.SetActive(false);
            Beachwind.SetActive(false);
            cloudswind.SetActive(false);
            basementwind.SetActive(false);
            basement2wind.SetActive(false);
            basement3wind.SetActive(false);
        }

        public static void Enablewind()
        {
            Forestwind.SetActive(true);
            Canyonwind.SetActive(true);
            Beachwind.SetActive(true);
            cloudswind.SetActive(true);
            basementwind.SetActive(true);
            basement2wind.SetActive(true);
            basement3wind.SetActive(true);
        }

        public static void LTdisconnet()
        {
            if (ControllerInputPoller.instance.leftControllerTriggerButton)
            {
                NetworkSystem.Instance.ReturnToSinglePlayer();
            }
        }



        public static float isDirtyDelay;
        // Thank you ii
        public static void RainbowBracelet()
        {
            Patches.Internal.BraceletPatch.enabled = true;
            if (!VRRig.LocalRig.nonCosmeticRightHandItem.IsEnabled)
            {
                SetBraceletState(true, false);
                RPCProtection();

                VRRig.LocalRig.nonCosmeticRightHandItem.EnableItem(true);
            }
            List<Color> rgbColors = new List<Color>();
            for (int i = 0; i < 10; i++)
                rgbColors.Add(Color.HSVToRGB((Time.frameCount / 180f + i / 10f) % 1f, 1f, 1f));

            VRRig.LocalRig.reliableState.isBraceletLeftHanded = false;
            VRRig.LocalRig.reliableState.braceletSelfIndex = 99;
            VRRig.LocalRig.reliableState.braceletBeadColors = rgbColors;
            VRRig.LocalRig.friendshipBraceletRightHand.UpdateBeads(rgbColors, 99);

            if (Time.time > isDirtyDelay)
            {
                isDirtyDelay = Time.time + 0.1f;
                VRRig.LocalRig.reliableState.SetIsDirty();
            }
        }
        // thanks to iidk for the code
        public static void disableRainbowBracelet()
        {
            BraceletPatch.enabled = false;
            if (!VRRig.LocalRig.nonCosmeticRightHandItem.IsEnabled)
            {
                SetBraceletState(false, false);
                RPCProtection();

                VRRig.LocalRig.nonCosmeticRightHandItem.EnableItem(false);
            }

            VRRig.LocalRig.reliableState.isBraceletLeftHanded = false;
            VRRig.LocalRig.reliableState.braceletSelfIndex = 0;
            VRRig.LocalRig.reliableState.braceletBeadColors.Clear();
            VRRig.LocalRig.UpdateFriendshipBracelet();

            VRRig.LocalRig.reliableState.SetIsDirty();
        }

        // Me Cdev made dis
        public static void Resetqueststuff()
        {
            VRRig.LocalRig.SetQuestScore(10);
        }

        // Me Cdev made dis
        public static void addqueststuff(int questint)
        {
            VRRig.LocalRig.SetQuestScore(questint);
        }

        public static void SetBraceletState(bool enable, bool isLeftHand) =>
            GorillaTagger.Instance.myVRRig.SendRPC("EnableNonCosmeticHandItemRPC", RpcTarget.All, enable, isLeftHand);





        public static void Frozone()
        {
            Color frozonecolor = Color.navyBlue;
            if (ControllerInputPoller.instance.rightGrab)
            {
                GameObject FrozoneCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                FrozoneCube.AddComponent<GorillaSurfaceOverride>().overrideIndex = 61;
                FrozoneCube.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                FrozoneCube.transform.position = TrueRightHand().position - new Vector3(0, .05f, 0);
                FrozoneCube.transform.rotation = TrueRightHand().rotation;

                FrozoneCube.GetComponent<Renderer>().material.color = frozonecolor;
                GameObject.Destroy(FrozoneCube, 5);
            }
            // hi
            if (ControllerInputPoller.instance.leftGrab)
            {
                GameObject FrozoneCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                FrozoneCube.AddComponent<GorillaSurfaceOverride>().overrideIndex = 61;
                FrozoneCube.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                FrozoneCube.transform.position = TrueLeftHand().position - new Vector3(0, .05f, 0);
                FrozoneCube.transform.rotation = TrueLeftHand().rotation;

                FrozoneCube.GetComponent<Renderer>().material.color = frozonecolor;
                GameObject.Destroy(FrozoneCube, 5);
            }
        }


        public static float antibandelay = 0f;
        // yay
        public static void AntiBan()
        {
            if (Time.time > antibandelay)
            {
                WebFlags flags = new WebFlags(3);

                PhotonNetwork.RaiseEvent(
                   8,
                   new object[3],
                   new RaiseEventOptions
                   {
                       CachingOption = EventCaching.RemoveFromRoomCache,
                       TargetActors = new int[] { PhotonNetwork.LocalPlayer.actorNumber },
                       Receivers = ReceiverGroup.MasterClient,
                       Flags = flags
                   },
                   SendOptions.SendReliable
                );

                PhotonNetwork.RaiseEvent(
                   50,
                   new object[3],
                   new RaiseEventOptions
                   {
                       CachingOption = EventCaching.RemoveFromRoomCache,
                       TargetActors = new int[] { PhotonNetwork.LocalPlayer.actorNumber },
                       Receivers = ReceiverGroup.MasterClient,
                       Flags = flags
                   },
                   SendOptions.SendReliable
                );

                MonkeAgent.instance.rpcErrorMax = int.MaxValue;
                MonkeAgent.instance.logErrorMax = int.MaxValue;
                PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
                PhotonNetwork.SendAllOutgoingCommands();
                Hashtable rpcfiltershit = new Hashtable();
                rpcfiltershit[0] = GorillaTagger.Instance.myVRRig.ViewID;
                PhotonNetwork.NetworkingClient.OpRaiseEvent(200, rpcfiltershit, new RaiseEventOptions
                {
                    CachingOption = EventCaching.RemoveFromRoomCache,
                    TargetActors = new int[]
                    {
                      PhotonNetwork.LocalPlayer.ActorNumber
                    }
                }, SendOptions.SendReliable);
                antibandelay = Time.time + 0.05f;
            }
        }
        public static void tg()
        {
            foreach (VRRig vrrig in VRRigCache.m_activeRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig)
                {
                    if (!vrrig.mainSkin.material.name.Contains("fected") && GorillaTagger.Instance.offlineVRRig.mainSkin.material.name.Contains("fected"))
                    {
                        GorillaTagger.Instance.offlineVRRig.enabled = true;
                        GorillaTagger.Instance.offlineVRRig.transform.position = vrrig.transform.position;
                        GorillaGameModes.GameMode.ReportTag(vrrig.Creator);
                    }
                }
                else
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                }
            }
        }

        

        public static void chams()
        {
            foreach (VRRig Vrrigsss in VRRigCache.m_activeRigs)
            {
                if (!Vrrigsss.isOfflineVRRig && !Vrrigsss.isMyPlayer)
                {
                    if (Vrrigsss.mainSkin.material.name.Contains("fected"))
                    {
                        Vrrigsss.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                        Vrrigsss.mainSkin.material.color = Color.red;
                    }
                    else
                    {
                        Vrrigsss.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                        Vrrigsss.mainSkin.material.color = Color.darkBlue;
                    }
                }
            }
        }

        public static void ChamsOff()
        {
            foreach (VRRig rigs in VRRigCache.m_activeRigs)
            {
                if (!rigs.isMyPlayer && !rigs.isOfflineVRRig)
                {
                    rigs.mainSkin.material.shader = Shader.Find("GorillaTag/UberShader");
                    rigs.mainSkin.material.color = rigs.playerColor;
                }

            }
        }

        public static void NameTags()
        {
            foreach (VRRig rigs in VRRigCache.m_activeRigs)
            {
                if (!rigs.isOfflineVRRig && !rigs.isMyPlayer)
                {
                    GameObject rigsnametag = rigs.transform.Find("NameTags")?.gameObject;
                    GameObject nametags = new GameObject("NameTags");
                    TextMeshPro TMP = nametags.AddComponent<TextMeshPro>();
                    TMP.text = rigs.Creator.NickName;
                    TMP.fontSize = 2.5f;
                    TMP.color = rigs.playerColor;
                    TMP.font = GameObject.Find("motdtext").GetComponent<TextMeshPro>().font;
                    nametags.transform.SetParent(rigs.transform);
                    nametags.transform.LookAt(Camera.main.transform.position);
                    nametags.GetComponent<TextMeshPro>().renderer.material.shader = Shader.Find("GUI/Text Shader");
                    nametags.transform.Rotate(0f, 180f, 0f);
                }
            }
        }






        public static bool screenToggleState;
        public static float nextToggleTime;

        public static void FlashGrayScreenSSAll()
        {
            if (Time.time > nextToggleTime)
            {
                screenToggleState = !screenToggleState;
                GrayScreenThing(screenToggleState);
                nextToggleTime = Time.time + 0.1f;
            }
        }

        public static void GrayScreenThing(bool greyzonestatus)
        {
            if (!NetworkSystem.Instance.InRoom)
            {
                return;
            }
            if (!NetworkSystem.Instance.IsMasterClient)
            {
                NotifiLib.SendNotification("Your not master client");
                return;
            }
            if (greyzonestatus)
            {
                // I'll do low grav next
                GreyZoneManager.Instance.gravityFactorOptionSelection = int.MaxValue;


                GreyZoneManager.Instance.ActivateGreyZoneAuthority();
            }
            else if (!greyzonestatus)
            {
                GreyZoneManager.Instance.DeactivateGreyZoneAuthority();
                GreyZoneManager.Instance.gravityFactorOptionSelection = 0;
            }
        }

        public static void SSHoverboardSpawn()
        {
            Color color = new Color(0, 0, 0);

            FreeHoverboardManager.instance.SendDropBoardRPC(GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.rotation, Vector3.zero, Vector3.zero, color);
        }


        

        #region Visual




        /*public static void Flingongrab(float Offset)
        {
            if (ControllerInputPoller.instance.leftControllerSecondaryButton)
            {
                Rig.enabled = false;
                Rig.headBodyOffset.x = Offset;
            }
            else
            {
                Rig.enabled = true;
                Rig.headBodyOffset.y = 0;
                Rig.headBodyOffset.x = 0;
                Rig.headBodyOffset.z = 0;
            }
        }*/


        #endregion
        /* Soon
        public static void Copygun()
        {
            VRRig rig = GorillaTagger.Instance.offlineVRRig;
            GunLibTEst.AthrionGunLibrary.StartPcGun(delegate ()
            {
                rig.enabled = false;
                rig.transform.position = GunLibTEst.AthrionGunLibrary.LockedRigOrPlayerOrwhatever.transform.position;
            }, true);
            rig.enabled = true;
        }*/

        public static void ResetRig()
        {
            GorillaTagger.Instance.offlineVRRig.enabled = true;
        }

        public static void DisableFingers()
        {
            ControllerInputPoller.instance.leftControllerGripFloat = 0f;
            ControllerInputPoller.instance.rightControllerGripFloat = 0f;
            ControllerInputPoller.instance.leftControllerIndexFloat = 0f;
            ControllerInputPoller.instance.rightControllerIndexFloat = 0f;
            ControllerInputPoller.instance.leftControllerPrimaryButton = false;
            ControllerInputPoller.instance.leftControllerSecondaryButton = false;
            ControllerInputPoller.instance.rightControllerPrimaryButton = false;
            ControllerInputPoller.instance.rightControllerSecondaryButton = false;
            ControllerInputPoller.instance.leftControllerPrimaryButtonTouch = false;
            ControllerInputPoller.instance.leftControllerSecondaryButtonTouch = false;
            ControllerInputPoller.instance.rightControllerPrimaryButtonTouch = false;
            ControllerInputPoller.instance.rightControllerSecondaryButtonTouch = false;
        }

        public static void Panic()
        {
            foreach (ButtonInfo Enabled in Buttons.GetActiveMods())
            {
                Enabled.enabled = false;
            }
        }


        public static void RightgripPanic()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                foreach (ButtonInfo Enabled in Buttons.GetActiveMods())
                {
                    Enabled.enabled = false;
                }
            }
        }

        public static void Slidemangerr(float amount)
        {
            GTPlayer.Instance.slideControl = amount;
        }

        public static void Windmod()
        {
            GameObject Wind = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/sky jungle entrance 2/JunkToDisable/Wind Tunnels/WindTunnelRibbons_Prefab (5)");
            if (Wind != null && PhotonNetwork.IsMasterClient && ControllerInputPoller.instance.rightGrab)
            {
                var WindSS = Wind.GetPhotonView();
                //GameObject clone = GameObject.Instantiate(Wind);
                //clone.GetPhotonView().transform.position = GorillaTagger.Instance.rightHandTransform.position;
                Wind.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                Wind.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
            }
        }

        public static void Flinggun()
        {
            GameObject Wind = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/sky jungle entrance 2/JunkToDisable/Wind Tunnels/WindTunnelRibbons_Prefab (5)");
            if (Wind != null && NetworkSystem.Instance.IsMasterClient && ControllerInputPoller.instance.rightGrab)
            {
                GameObject clone = GameObject.Instantiate(Wind);
                GunLibTEst.AthrionGunLibrary.start2guns(delegate ()
                {
                    clone.transform.position = lockTarget.transform.position - new Vector3(0, 1, 0);
                }, true);
            }
        }
    }
}

    



