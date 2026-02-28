using System.Collections;
using BepInEx;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaLocomotion.Gameplay;
using GorillaNetworking;
using Oculus.Interaction.Grab.GrabSurfaces;
using Oculus.Interaction.Input;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using liquidclient.Classes;
using liquidclient.Notifications;
using liquidclient.Patches.Internal;
using System.Collections.Generic;
using System.Linq;
using GorillaLocomotion.Swimming;
using liquid.client.Patches.Internal;
using liquidclient.Menu;
using liquidclient.mods;
using Photon.Voice;
using Photon.Voice.PUN;
using TMPro;
using UnityEngine;
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
        public static void Fly()
        {
            if (ControllerInputPoller.instance.rightControllerPrimaryButton)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * Time.deltaTime * Settings.Movement.flySpeed;
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            }
        }
        
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
                        GorillaLocomotion.GTPlayer.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    }
                    else
                    {
                        CheckPoint.GetComponent<Renderer>().material.color = Color.yellow;
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
        public static void DisableCheckpoint()
        {
            if (CheckPoint != null)
            {
                UnityEngine.Object.Destroy(CheckPoint);
                CheckPoint = null;
            }
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

        public static bool previousTeleportTrigger;
        public static void TeleportGun()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (ControllerInputPoller.TriggerFloat(XRNode.RightHand) > 0.5f && !previousTeleportTrigger)
                {
                    GTPlayer.Instance.TeleportTo(NewPointer.transform.position + Vector3.up, GTPlayer.Instance.transform.rotation);
                    GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
                }

                previousTeleportTrigger = ControllerInputPoller.TriggerFloat(XRNode.RightHand) > 0.5f;
            }
        }

        public static void speedboost()
        {
            GTPlayer.Instance.maxJumpSpeed = 7.5f;
            GTPlayer.Instance.jumpMultiplier = 1.1f;
        }

        public static float startX = -1f;
        public static float startY = -1f;

        public static float subThingy;
        public static float subThingyZ;

        public static void WASDFly()
        {
            GorillaLocomotion.GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity = new Vector3(0f, 0.067f, 0f);

            bool W = UnityInput.Current.GetKey(KeyCode.W);
            bool A = UnityInput.Current.GetKey(KeyCode.A);
            bool S = UnityInput.Current.GetKey(KeyCode.S);
            bool D = UnityInput.Current.GetKey(KeyCode.D);
            bool Space = UnityInput.Current.GetKey(KeyCode.Space);
            bool Ctrl = UnityInput.Current.GetKey(KeyCode.LeftControl);

            if (Mouse.current.rightButton.isPressed)
            {
                Transform parentTransform = GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.parent;
                Quaternion currentRotation = parentTransform.rotation;
                Vector3 euler = currentRotation.eulerAngles;

                if (startX < 0)
                {
                    startX = euler.y;
                    subThingy = Mouse.current.position.value.x / UnityEngine.Screen.width;
                }
                if (startY < 0)
                {
                    startY = euler.x;
                    subThingyZ = Mouse.current.position.value.y / UnityEngine.Screen.height;
                }

                float newX = startY - ((((Mouse.current.position.value.y / UnityEngine.Screen.height) - subThingyZ) * 360) * 1.33f);
                float newY = startX + ((((Mouse.current.position.value.x / UnityEngine.Screen.width) - subThingy) * 360) * 1.33f);

                newX = (newX > 180f) ? newX - 360f : newX;
                newX = Mathf.Clamp(newX, -90f, 90f);

                parentTransform.rotation = Quaternion.Euler(newX, newY, euler.z);
            }
            else
            {
                startX = -1;
                startY = -1;
            }

            float speed = Settings.Movement.flySpeed;
            if (UnityInput.Current.GetKey(KeyCode.LeftShift))
                speed *= 2f;
            if (W)
            {
                GorillaTagger.Instance.rigidbody.transform.position += GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.parent.forward * Time.deltaTime * speed;
            }

            if (S)
            {
                GorillaTagger.Instance.rigidbody.transform.position += GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.parent.forward * Time.deltaTime * -speed;
            }

            if (A)
            {
                GorillaTagger.Instance.rigidbody.transform.position += GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.parent.right * Time.deltaTime * -speed;
            }

            if (D)
            {
                GorillaTagger.Instance.rigidbody.transform.position += GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.parent.right * Time.deltaTime * speed;
            }

            if (Space)
            {
                GorillaTagger.Instance.rigidbody.transform.position += new Vector3(0f, Time.deltaTime * speed, 0f);
            }

            if (Ctrl)
            {
                GorillaTagger.Instance.rigidbody.transform.position += new Vector3(0f, Time.deltaTime * -speed, 0f);
            }
            VRRig.LocalRig.head.rigTarget.transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;
        }

        public static void TP_Stump()
        {
            GTPlayer.Instance.TeleportTo(new Vector3(-68.647f, 12.406f, -83.699f), GTPlayer.Instance.transform.rotation);
            GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
        }
        
        public static GameObject airSwimPart;
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
        }
        
        private static float flapTime;
        public static void BirdFly()
        {
            if (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, GorillaTagger.Instance.headCollider.transform.position) < 0.63f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.headCollider.transform.position) < 0.63f)
                return;

            if (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, GorillaTagger.Instance.rightHandTransform.position) < 1f)
                return;
            if (Physics.Raycast(GorillaTagger.Instance.bodyCollider.attachedRigidbody.position, Vector3.down, hitInfo: out _, Physics.AllLayers))
                return;

            UnityEngine.XR.InputDevice LeftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            UnityEngine.XR.InputDevice RightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

            if (LeftHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out Vector3 leftVel) && RightHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out Vector3 rightVel))
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
        }

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
        public static void Ghost()
        {
            if (ControllerInputPoller.instance.rightControllerPrimaryButton && ghosted == false)
            {
                ghosted = true;
            }

            VRRig.LocalRig.enabled = !ghosted;

            if (ControllerInputPoller.instance.rightControllerSecondaryButton && ghosted)
            {
                ghosted = false;
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
            bool disablecolliders2 = ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f;
            MeshCollider[] colliders = Resources.FindObjectsOfTypeAll<MeshCollider>();

            foreach (MeshCollider collider in colliders)
            {
                collider.enabled = !disablecolliders2;
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


        public static void FixHead()
        {
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.x = 0f;
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.y = 0f;
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.z = 0f;
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
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
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

        public static void closegame()
        {
            Application.Quit();
        }

        public static void beacons()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
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

        public static void upsidedownhead()
        {
            VRRig.LocalRig.head.trackingRotationOffset.z = 180f;
        }
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
        
        public static void MuteTarget(object target)
        {
            RaiseEventOptions raiseOptions = new RaiseEventOptions();

            if (target is ReceiverGroup group)
                raiseOptions.Receivers = group;
            else if (target is int[] actors)
                raiseOptions.TargetActors = actors;
            else if (target is RaiseEventOptions options)
                raiseOptions = options;
            else
                return;

            SendOptions sendOptions = new SendOptions
            {
                Reliability = false,
                Channel = 0
            };


            Dictionary<byte, object> voiceData = new Dictionary<byte, object>
            {
                { 1, 255 },
                { 2, 48000 },
                { 3, 2 },
                { 4, 20000 },
                { 5, 30000 },
                { 10, null },
                { 11, (byte)0 },
                { 12, Codec.AudioOpus }
            };

            object[] eventData =
            {
                (byte)0,
                (byte)1,
                new object[] { voiceData }
            };

            PhotonVoiceNetwork.Instance.Client.OpRaiseEvent(
                202,
                eventData,
                raiseOptions,
                sendOptions
            );
        }

        public static void ServerMuteAll()
        {
            for (int i = 0; i < 2; i++)
                MuteTarget(ReceiverGroup.All);
        }


        public static void GrabRig()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                var Player = GorillaLocomotion.GTPlayer.Instance;
                GorillaTagger.Instance.offlineVRRig.enabled = false;
                GorillaTagger.Instance.offlineVRRig.transform.position = Player.RightHand.controllerTransform.position;
                GorillaTagger.Instance.offlineVRRig.transform.rotation = Player.RightHand.controllerTransform.rotation;
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
        }
        
        public static string serverLink = "https://discord.gg/3erAJbNem9";


        public static Coroutine kickCoroutine;
        public static IEnumerator KickMasterClient()
        {
            //ButtonInfo button = Buttons.GetIndex("Kick Master Client");
            if (!NetworkSystem.Instance.InRoom)
            {
                NotifiLib.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not in a room.");
                kickCoroutine = null;
                yield break;
            }

            if (NetworkSystem.Instance.IsMasterClient)
            {
                NotifiLib.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are master client! You have no one to kick.");
				kickCoroutine = null;
                yield break;
            }

            SerializePatch.OverrideSerialization = () => false;

            Player player = PhotonNetwork.MasterClient;
            VRRig rig = RigManager.GetVRRigFromPlayer(PhotonNetwork.MasterClient);
            string name = $"<color=#{(rig != null ? ColorUtility.ToHtmlStringRGBA(rig.GetColor()) : "white")}>{player.NickName}</color>";
            NotifiLib.SendNotification($"<color=grey>[</color><color=purple>KICK</color><color=grey>]</color> Kicking {name}.");
            float time;
            RPCProtection();
            kick:
            {
                time = Time.time + 10f;
                int view = PhotonNetwork.AllocateViewID(0);
                for (int i = 0; i < 3965; i++)
                {
                    PhotonNetwork.NetworkingClient.OpRaiseEvent(202, new Hashtable
                    {
                        { 0, "GameMode" },
                        { 6, PhotonNetwork.ServerTimestamp },
                        { 7, view }
                    }, new RaiseEventOptions
                    {
                        Receivers = ReceiverGroup.MasterClient
                    }, SendOptions.SendReliable);
                }
            }
            

            while (PhotonNetwork.PlayerList.Contains(player))
            {
                if (Time.time > time)
                {
                    NotifiLib.SendNotification($"<color=grey>[</color><color=purple>KICK</color><color=grey>]</color> Could not kick {name}. Trying again..");
                    yield return null;
                    goto kick;
                }
                yield return null;
            }

            SerializePatch.OverrideSerialization = null;
            NotifiLib.SendNotification($"<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> {name} has been kicked!");
            kickCoroutine = null;
        }

        public static IEnumerator KickAll()
        {
            //ButtonInfo button = Buttons.GetIndex("Kick Master Client");

            if (!NetworkSystem.Instance.InRoom)
            {
                NotifiLib.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not in a room.");
                kickCoroutine = null;
                yield break;
            }

            if (NetworkSystem.Instance.IsMasterClient)
            {
                kickCoroutine = null;
                yield break;
            }

            SerializePatch.OverrideSerialization = () => false;

            while (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                Player player = PhotonNetwork.MasterClient;
                if (player == null)
                    break;

                VRRig rig = RigManager.GetVRRigFromPlayer(player);
                string name = $"<color=#{(rig != null ? ColorUtility.ToHtmlStringRGBA(rig.GetColor()) : "white")}>{player.NickName}</color>";

                NotifiLib.SendNotification($"<color=grey>[</color><color=purple>KICK</color><color=grey>]</color> Kicking {name}.");
                RPCProtection();
                float time;
                kick:
                {
                    time = Time.time + 10f;
                    int view = PhotonNetwork.AllocateViewID(0);
                    for (int i = 0; i < 3965; i++)
                    {
                        PhotonNetwork.NetworkingClient.OpRaiseEvent(202, new Hashtable
                        {
                            { 0, "GameMode" },
                            { 6, PhotonNetwork.ServerTimestamp },
                            { 7, PhotonNetwork.AllocateViewID(0) }
                        }, new RaiseEventOptions
                        {
                            Receivers = ReceiverGroup.MasterClient
                        }, SendOptions.SendReliable);
                    }
                }

                while (PhotonNetwork.MasterClient == player)
                {
                    if (Time.time > time)
                    {
                        NotifiLib.SendNotification($"<color=grey>[</color><color=red>KICK</color><color=grey>]</color> Could not kick {name}, trying again..");
                        yield return null;
                        goto kick;
                    }
                    yield return null;
                }

                if (!PhotonNetwork.InRoom)
                {
                    NotifiLib.SendNotification($"<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> Kicking {name} failed. :(");
                    kickCoroutine = null;
                    yield break;
                }

                int left = (Time.time - (time - 10f)) < 2.5f ? 10 : 5;

                NotifiLib.SendNotification($"<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> {name} has been kicked! Waiting {left} seconds to kick the next person..");
                yield return new WaitForSeconds(left);

            }

            SerializePatch.OverrideSerialization = null;

            NotifiLib.SendNotification($"<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> Kicked all successfully!");

            kickCoroutine = null;
        }
        
        private static bool hasGrabbedHoverboard;
        public static void SpawnHowerdBoard()
        {
            if (!hasGrabbedHoverboard)
            {
                GTPlayer.Instance.GrabPersonalHoverboard(false, Vector3.zero, Quaternion.identity, Color.black);
                hasGrabbedHoverboard = true;
            }

            GTPlayer.Instance.SetHoverAllowed(true);
            GTPlayer.Instance.SetHoverActive(true);
            VRRig.LocalRig.hoverboardVisual.gameObject.SetActive(true);
        }

        public static void DisableHoverboard()
        {
            hasGrabbedHoverboard = false;

            GTPlayer.Instance.SetHoverAllowed(false);
            GTPlayer.Instance.SetHoverActive(false);
            VRRig.LocalRig.hoverboardVisual.gameObject.SetActive(false);
        }

        public static void Rocket()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                GorillaTagger.Instance.rigidbody.linearVelocity += Vector3.up * (Time.deltaTime * Settings.Movement.flySpeed * 5f);
            }
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
        
        public static void InvisibleMod()
        {
            bool rightControllerSecondaryButton = ControllerInputPoller.instance.rightControllerSecondaryButton;
            if (rightControllerSecondaryButton)
            {
                GorillaTagger.Instance.offlineVRRig.headBodyOffset.x = 180f;
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.headBodyOffset.x = 0f;
            }
        }
        
        private static float freezeAllDelay;
        public static bool muteOnFreeze;
        public static void FreezeServer(float delay = 0.1f, int eventCount = 11, RaiseEventOptions options = null)
        {
            if (!PhotonNetwork.InRoom) return;

            options ??= new RaiseEventOptions
            {
                Flags = new WebFlags(byte.MaxValue),
                TargetActors = new[] { -1 }
            };

            if (muteOnFreeze)
            {
                for (int i = 0; i < 10; i++)
                    MuteTarget(options);
            }

            if (Time.time > freezeAllDelay)
            {
                for (int i = 0; i < eventCount; i++)
                    PhotonNetwork.RaiseEvent(51, new object[] { serverLink }, options, SendOptions.SendUnreliable);

                RPCProtection();
                freezeAllDelay = Time.time + delay;
            }
        }

        public static void GhostMonke()
        {
            bool rightControllerPrimaryButton = ControllerInputPoller.instance.rightControllerPrimaryButton;
            if (rightControllerPrimaryButton)
            {
                GorillaTagger.Instance.offlineVRRig.enabled = false;
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
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
            catch { Debug.Log("RPC protection failed, are you in a lobby?"); }
        }

        public static void FlushRPCs()
        {
            RPCProtection();
        }

        // Giant thanks to Visor Paid
        private static float delay = 5f;
        public static void stutterall()
        {
            if (Time.time > delay)
            {
                for (int i = 0; i < 925; i++)
                {
                    PhotonNetwork.NetworkingClient.OpRaiseEvent(201, new object[] { float.NaN, 777 }, new RaiseEventOptions
                    {
                        Receivers = ReceiverGroup.Others
                    }, new SendOptions()
                    {
                        DeliveryMode = DeliveryMode.UnreliableUnsequenced,
                        Encrypt = true,
                        Reliability = false
                    });
                }
                delay = Time.time + 2f;
            }
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

        public static void bodyTracer()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig)
                {
                    GameObject line = new GameObject("Line");
                    LineRenderer liner = line.AddComponent<LineRenderer>();
                    UnityEngine.Color thecolor = vrrig.playerColor;
                    liner.startColor = thecolor; liner.endColor = thecolor; liner.startWidth = 0.010f; liner.endWidth = 0.010f; liner.positionCount = 2; liner.useWorldSpace = true;
                    liner.SetPosition(0, GorillaTagger.Instance.rigidbody.transform.position);
                    liner.SetPosition(1, vrrig.transform.position);
                    liner.material.shader = Shader.Find("GUI/Text Shader");
                    UnityEngine.Object.Destroy(line, Time.deltaTime);
                }
            }
        }




        public static void Invismonk()
        {
            bool invis = false;
            var Rig = GorillaTagger.Instance.offlineVRRig;
            if (ControllerInputPoller.instance.rightControllerSecondaryButton && !invis)
            {
                invis = true;
                Rig.enabled = false;
                Rig.transform.position = new Vector3(0f, -100f, 0f);

            }

            if (ControllerInputPoller.instance.rightControllerPrimaryButton && invis)
            {
                invis = false;
                Rig.enabled = true;
                VRRig.LocalRig.enabled = true;
            }
            else
            {
                VRRig.LocalRig.enabled = true;
            }
        }

        public static float isDirtyDelay;
        // thanks to iidk for the code
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


        // Giant thanks to Visor Paid
        private static float delay1 = 1.70f;
        public static void stutterallunsafe()
        {
            if (Time.time > delay1)
            {
                for (int i = 0; i < 925; i++)
                {
                    PhotonNetwork.NetworkingClient.OpRaiseEvent(201, new object[] { float.NaN, 777 }, new RaiseEventOptions
                    {
                        Receivers = ReceiverGroup.Others
                    }, new SendOptions()
                    {
                        DeliveryMode = DeliveryMode.UnreliableUnsequenced,
                        Encrypt = true,
                        Reliability = false
                    });
                }
                delay1 = Time.time + 2f;
            }
        }

        public static void SetBraceletState(bool enable, bool isLeftHand) =>
            GorillaTagger.Instance.myVRRig.SendRPC("EnableNonCosmeticHandItemRPC", RpcTarget.All, enable, isLeftHand);


        public static void CrystalSoundSpam()
        {
            int[] sounds = {
                Random.Range(40,54),
                Random.Range(214,221)
            };
            SoundSpam(sounds[Random.Range(0, 1)]);
        }


        private static float soundSpamDelay;
        public static void SoundSpam(int soundId, bool constant = false)
        {
            if (ControllerInputPoller.instance.rightGrab || constant)
            {
                if (Time.time > soundSpamDelay)
                    soundSpamDelay = Time.time + 0.1f;
                else
                    return;

                if (PhotonNetwork.InRoom)
                {
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, soundId, false, 999999f);
                    RPCProtection();
                }
                else
                    VRRig.LocalRig.PlayHandTapLocal(soundId, false, 999999f);
            }
        }

        public static void JmancurlySoundSpam() =>
            SoundSpam(Random.Range(336, 338));


        private static bool squeakToggle;
        public static void SqueakSoundSpam()
        {
            if (Time.time > soundSpamDelay)
                squeakToggle = !squeakToggle;

            SoundSpam(squeakToggle ? 75 : 76);
        }


        private static bool sirenToggle;
        public static void SirenSoundSpam()
        {
            if (Time.time > soundSpamDelay)
                sirenToggle = !sirenToggle;

            SoundSpam(sirenToggle ? 48 : 50);
        }

        public static int soundId;


        public static void Frozone()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                Color frozonecolor = Color.navyBlue;
                GameObject FrozoneCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                FrozoneCube.AddComponent<GorillaSurfaceOverride>().overrideIndex = 61;
                FrozoneCube.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                FrozoneCube.transform.position = TrueRightHand().position - new Vector3(0, .05f, 0);
                FrozoneCube.transform.rotation = TrueRightHand().rotation;

                FrozoneCube.GetComponent<Renderer>().material.color = frozonecolor;
                GameObject.Destroy(FrozoneCube, 5);
            }

            if (ControllerInputPoller.instance.leftGrab)
            {
                Color frozonecolor = Color.darkBlue;
                GameObject FrozoneCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                FrozoneCube.AddComponent<GorillaSurfaceOverride>().overrideIndex = 61;
                FrozoneCube.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                FrozoneCube.transform.position = TrueLeftHand().position - new Vector3(0, .05f, 0);
                FrozoneCube.transform.rotation = TrueLeftHand().rotation;

                FrozoneCube.GetComponent<Renderer>().material.color = frozonecolor;
                GameObject.Destroy(FrozoneCube, 5);
            }
        }

        public static void tg()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig)
                {
                    if (!vrrig.mainSkin.material.name.Contains("fected") && GorillaTagger.Instance.offlineVRRig.mainSkin.material.name.Contains("fected"))
                    {
                        GorillaTagger.Instance.offlineVRRig.enabled = true;
                        GorillaTagger.Instance.offlineVRRig.transform.position = vrrig.transform.position;
                        GameMode.ReportTag(vrrig.OwningNetPlayer);
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
            foreach (VRRig Vrrigsss in GorillaParent.instance.vrrigs)
            {
                if (!Vrrigsss.isOfflineVRRig && !Vrrigsss.isMyPlayer)
                {
                    if (Vrrigsss.mainSkin.material.name.Contains("fected"))
                    {
                        Vrrigsss.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                        Vrrigsss.mainSkin.material.color = Color.blue;
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
            foreach (VRRig rigs in GorillaParent.instance.vrrigs)
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
            foreach (VRRig rigs in GorillaParent.instance.vrrigs)
            {
                if (!rigs.isOfflineVRRig && !rigs.isMyPlayer)
                {
                    GameObject rigsnametag = rigs.transform.Find("NameTags")?.gameObject;
                    GameObject nametags = new GameObject("NameTags");
                    TextMeshPro TMP = nametags.AddComponent<TextMeshPro>();
                    TMP.text = rigs.OwningNetPlayer.NickName;
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

        public static void Disablesubdoor()
        {
            GameObject Subdoor = GameObject.Find("City_Pretty/CosmeticsRoomAnchor/outsidestores_prefab/Top Layer/Huts/SubscribersHut/SubscriberExclusionZone/");
            Subdoor.SetActive(false);
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
                GreyZoneManager.Instance.ActivateGreyZoneAuthority();
            }
            else if (!greyzonestatus)
            {
                GreyZoneManager.Instance.DeactivateGreyZoneAuthority();
            }
        }

    public static void SSHoverboardSpawn()
        {
            Color color = new Color(0, 0, 0);
            Vector3 spawnPos = GorillaTagger.Instance.rightHandTransform.position;
            Quaternion spawnRot = GorillaTagger.Instance.rightHandTransform.rotation;

            FreeHoverboardManager.instance.SendDropBoardRPC(spawnPos, spawnRot, Vector3.zero, Vector3.zero, color);
        }

    }
}

    



