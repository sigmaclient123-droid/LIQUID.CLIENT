using ExitGames.Client.Photon;
using GorillaGameModes;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace liquid.client.Mods
{
    public class OverPowred
    {
        public static void TagSelf()
        {
            bool rightControllerTriggerButton = ControllerInputPoller.instance.rightControllerTriggerButton;
            if (rightControllerTriggerButton)
            {
                bool flag = !GorillaTagger.Instance.offlineVRRig.mainSkin.material.name.Contains("fected");
                if (flag)
                {
                    foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
                    {
                        bool flag2 = vrrig.mainSkin.material.name.Contains("fected");
                        if (flag2)
                        {
                            GorillaTagger.Instance.offlineVRRig.enabled = false;
                            GorillaTagger.Instance.offlineVRRig.transform.position = vrrig.rightHandTransform.position;
                            GorillaTagger.Instance.myVRRig.transform.position = vrrig.rightHandTransform.position;
                            GameMode.ReportTag(GorillaTagger.Instance.offlineVRRig.OwningNetPlayer);
                            break;
                        }
                    }
                }
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
        }
        
        private static float delay;
        
        public static void UndetectedLagall()
        {
            bool flag = Time.time > delay;
            if (flag)
            {
                for (int i = 0; i < 925; i++)
                {
                    LoadBalancingClient networkingClient = PhotonNetwork.NetworkingClient;
                    byte b = 201;
                    object obj = new object[]
                    {
                        float.NaN,
                        777
                    };
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
                    raiseEventOptions.Receivers = 0;
                    SendOptions sendOptions = default(SendOptions);
                    sendOptions.DeliveryMode = (DeliveryMode)2;
                    sendOptions.Encrypt = true;
                    sendOptions.Reliability = false;
                    networkingClient.OpRaiseEvent(b, obj, raiseEventOptions, sendOptions);
                }
                delay = Time.time + 1f;
            }
        }
    }
}