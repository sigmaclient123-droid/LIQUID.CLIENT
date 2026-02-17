using BepInEx;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using Oculus.Interaction.Input;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using StupidTemplate.Classes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using static StupidTemplate.Menu.Main;
using StupidTemplate.Notifications;
using GorillaNetworking;


namespace StupidTemplate.Mods
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

        public static GameObject platl;
        public static GameObject platr;
        public static void PlatformModbysigmaboy()

        {

            if (ControllerInputPoller.instance.leftGrab && leftplat == null)
            {
                leftplat = CreatePlatformOnHand(GorillaTagger.Instance.leftHandTransform);
                ColorChanger colorChanger = leftplat.AddComponent<ColorChanger>();
                colorChanger.colors = StupidTemplate.Settings.backgroundColor;
            }

            if (ControllerInputPoller.instance.rightGrab && rightplat == null)
            {
                rightplat = CreatePlatformOnHand(GorillaTagger.Instance.rightHandTransform);
                ColorChanger colorChanger = rightplat.AddComponent<ColorChanger>();
                colorChanger.colors = StupidTemplate.Settings.backgroundColor;
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
            GTPlayer.Instance.maxJumpSpeed = 6.4f;
            GTPlayer.Instance.jumpMultiplier = 6.3f;
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
            GTPlayer.Instance.jumpMultiplier = 8.1f;

        }
        public static void rightgripspeedboost()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                GTPlayer.Instance.maxJumpSpeed = 11.3f;
                GTPlayer.Instance.jumpMultiplier = 8.1f;
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



        public static void PlatformMod()

        {
            if (ControllerInputPoller.instance.leftGrab && leftplat == null)
            {
                leftplat = CreatePlatformOnHand(GorillaTagger.Instance.leftHandTransform);
            }

            if (ControllerInputPoller.instance.rightGrab && rightplat == null)
            {
                rightplat = CreatePlatformOnHand(GorillaTagger.Instance.rightHandTransform);
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
        private static GameObject leftplat = null;
        private static GameObject rightplat = null;
        private static GameObject CreatePlatformOnHand(Transform handTransform)
        {
            GameObject plat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plat.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);

            plat.transform.position = handTransform.position;
            plat.transform.rotation = handTransform.rotation;

            float h = (Time.frameCount / 180f) % 1f;
            plat.GetComponent<Renderer>().material.color = Color.darkGoldenRod;
            return plat;
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

                UnityEngine.Object.Destroy(gameObject, 1f);

                PhotonNetwork.RaiseEvent(69, new object[]
                {
gameObject.transform.position,
gameObject.transform.rotation
                },
                new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.Others
                },
                SendOptions.SendReliable);
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

        //become doug the bug by Senty, not required to credit but appreciated!


        public static void BeDougTheBug()
        {
            if (ControllerInputPoller.instance.leftGrab || ControllerInputPoller.instance.rightGrab)
            {
                foreach (MeshCollider mesh in Resources.FindObjectsOfTypeAll<MeshCollider>())
                {
                    mesh.enabled = false;
                }

                GameObject bug = GameObject.Find("Floating Bug Holdable");
                if (bug == null)
                {
                    bug = GameObject.Find("FloatingBugHoldable");
                    if (bug == null)
                    {
                        foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType<GameObject>())
                        {
                            if (obj.name.Contains("Bug"))
                            {
                                bug = obj;
                                break;
                            }
                        }
                    }
                }
                if (bug != null)
                {
                    Vector3 bugPos = bug.transform.position;
                    GTPlayer.Instance.TeleportTo(bugPos, GTPlayer.Instance.transform.rotation);
                    GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.down;
                }
                else
                {
                    Notifications.NotifiLib.SendNotification("[<color=red>ERROR</color>] Doug the Bug not found!");
                }
            }
            else
            {
                foreach (MeshCollider mesh in Resources.FindObjectsOfTypeAll<MeshCollider>())
                {
                    mesh.enabled = true;
                }
            }
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

        public static void Destroywind()
        {
            GameObject Wind = GameObject.Find("Wind");
            if (Wind = null)
            {
                NotifiLib.SendNotification("wind not found");
            }

            if (Wind != null)
            {
                Wind.Destroy();
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
                GorillaNot.instance.rpcErrorMax = int.MaxValue;
                GorillaNot.instance.rpcCallLimit = int.MaxValue;
                GorillaNot.instance.logErrorMax = int.MaxValue;

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

    }
}

    



