using BepInEx;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace liquidclient.Mods
{
    public class WASDFLY
    { 
        // made by skibidi imudtrust!!!!!!!!!!!!!!
        public static float _flySpeed = 6f; 
        public static bool scaleWithPlayer;
        public static float FlySpeed => _flySpeed * (scaleWithPlayer ? GTPlayer.Instance.scale : 1f);

        public static float startX = -1f;
        public static float startY = -1f;
        public static float subThingy;
        public static float subThingyZ;
        public static Vector3 lastPosition = Vector3.zero;

        private static GameObject leftTarget, rightTarget;

        public static void WASDFly()
        {
            bool guiActive = liquidclient.Menu.OnScreenGUI.IsVisible;
            if (guiActive) 
            { 
                FreezePlayer();
                return; 
            }

            CreateHandTargets();
            
            VRRig rig = GorillaTagger.Instance.offlineVRRig;
            Transform body = GorillaTagger.Instance.bodyCollider.transform;
            Transform head = GorillaTagger.Instance.headCollider.transform;
            Transform parentTransform = GTPlayer.Instance.GetControllerTransform(false).parent;

            if (Mouse.current.rightButton.isPressed)
            {
                Quaternion currentRotation = parentTransform.rotation;
                Vector3 euler = currentRotation.eulerAngles;

                if (startX < 0)
                {
                    startX = euler.y;
                    subThingy = Mouse.current.position.value.x / Screen.width;
                }
                if (startY < 0)
                {
                    startY = euler.x;
                    subThingyZ = Mouse.current.position.value.y / Screen.height;
                }

                float newX = startY - (Mouse.current.position.value.y / Screen.height - subThingyZ) * 360 * 1.33f;
                float newY = startX + (Mouse.current.position.value.x / Screen.width - subThingy) * 360 * 1.33f;

                newX = newX > 180f ? newX - 360f : newX;
                newX = Mathf.Clamp(newX, -90f, 90f);

                parentTransform.rotation = Quaternion.Euler(newX, newY, euler.z);
            }
            else
            {
                startX = -1;
                startY = -1;
            }

            rig.head.rigTarget.transform.rotation = head.rotation;
            
            Vector3 leftHome = body.TransformPoint(new Vector3(-0.28f, -0.52f, -0.05f));
            Vector3 rightHome = body.TransformPoint(new Vector3(0.28f, -0.52f, -0.05f));

            bool isSprinting = UnityInput.Current.GetKey(KeyCode.LeftShift);
            bool isSlowing = UnityInput.Current.GetKey(KeyCode.LeftAlt);
            
            float followStrength = isSprinting ? 0.85f : 0.45f;

            leftTarget.transform.position = Vector3.Lerp(leftTarget.transform.position, leftHome, followStrength);
            rightTarget.transform.position = Vector3.Lerp(rightTarget.transform.position, rightHome, followStrength);

            Quaternion handRot = body.rotation * Quaternion.Euler(90f, 0f, 0f);
            leftTarget.transform.rotation = handRot;
            rightTarget.transform.rotation = handRot;

            rig.leftHand.overrideTarget = leftTarget.transform;
            rig.rightHand.overrideTarget = rightTarget.transform;

            if (rig.mainSkin.gameObject.activeInHierarchy)
            {
                UpdateHandCollision(rig.leftHand.rigTarget.gameObject);
                UpdateHandCollision(rig.rightHand.rigTarget.gameObject);
            }

            float currentSpeed = FlySpeed;
            if (isSprinting) currentSpeed *= 3.5f; 
            if (isSlowing) currentSpeed *= 0.3f;

            HandleFlightMovement(parentTransform, currentSpeed);
        }

        private static void FreezePlayer()
        {
            if (lastPosition != Vector3.zero)
            {
                GorillaTagger.Instance.rigidbody.transform.position = lastPosition;
                GorillaTagger.Instance.rigidbody.velocity = Vector3.zero;
            }
            DisableOverrides();
        }

        private static void UpdateHandCollision(GameObject hand)
        {
            foreach (Collider c in hand.GetComponentsInChildren<Collider>())
            {
                c.enabled = true;
                c.isTrigger = false;
            }
        }

        private static void HandleFlightMovement(Transform pt, float speed)
        {
            bool W = UnityInput.Current.GetKey(KeyCode.W);
            bool S = UnityInput.Current.GetKey(KeyCode.S);
            bool A = UnityInput.Current.GetKey(KeyCode.A);
            bool D = UnityInput.Current.GetKey(KeyCode.D);
            bool Space = UnityInput.Current.GetKey(KeyCode.Space);
            bool Ctrl = UnityInput.Current.GetKey(KeyCode.LeftControl);

            Vector3 moveDir = Vector3.zero;
            if (W) moveDir += pt.forward;
            if (S) moveDir -= pt.forward;
            if (A) moveDir -= pt.right;
            if (D) moveDir += pt.right;
            if (Space) moveDir += Vector3.up;
            if (Ctrl) moveDir += Vector3.down;

            GorillaTagger.Instance.rigidbody.transform.position += moveDir * (Time.deltaTime * speed);
            GorillaTagger.Instance.rigidbody.velocity = Vector3.zero;

            if (moveDir == Vector3.zero && lastPosition != Vector3.zero)
                GorillaTagger.Instance.rigidbody.transform.position = lastPosition;
            else
                lastPosition = GorillaTagger.Instance.rigidbody.transform.position;
        }

        private static void CreateHandTargets()
        {
            if (leftTarget == null)
            {
                leftTarget = new GameObject("Phys_L");
                rightTarget = new GameObject("Phys_R");
                Object.DontDestroyOnLoad(leftTarget);
                Object.DontDestroyOnLoad(rightTarget);
            }
        }

        private static void DisableOverrides()
        {
            if (GorillaTagger.Instance?.offlineVRRig != null)
            {
                GorillaTagger.Instance.offlineVRRig.leftHand.overrideTarget = null;
                GorillaTagger.Instance.offlineVRRig.rightHand.overrideTarget = null;
            }
        }
    }

}
