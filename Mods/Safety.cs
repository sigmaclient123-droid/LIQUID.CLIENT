using System;
using System.Collections.Generic;
using GorillaLocomotion;
using liquidclient.Classes;
using System.Linq;
using liquidclient.Notifications;
using UnityEngine;
using UnityEngine.XR;
using static liquidclient.Classes.RigManager;
using static liquidclient.Menu.Main;
using Object = UnityEngine.Object;

namespace liquidclient.Mods
{
    public class Safety
    {
        public static VRRig reportRig;
        public static void AntiReport(System.Action<VRRig, Vector3> onReport)
        {
            if (!NetworkSystem.Instance.InRoom) return;

            if (reportRig != null)
            {
                onReport?.Invoke(reportRig, reportRig.transform.position);
                reportRig = null;
                return;
            }

            foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (line.linePlayer != NetworkSystem.Instance.LocalPlayer) continue;
                Transform report = line.reportButton.gameObject.transform;

                foreach (var vrrig in from vrrig in VRRigCache.m_activeRigs where !vrrig.isLocal let D1 = Vector3.Distance(vrrig.rightHandTransform.position, report.position) let D2 = Vector3.Distance(vrrig.leftHandTransform.position, report.position) where D1 < 0.35f || D2 < 0.35f select vrrig)
                    onReport?.Invoke(vrrig, report.transform.position);
            }
        }
        
        private static float lastCacheClearedTime;
        public static void AutoClearCache()
        {
            if (Time.time > lastCacheClearedTime)
            {
                lastCacheClearedTime = Time.time + 60f;
                GC.Collect();
            }
        }

        public static float antiReportDelay;
        public static void AntiReportDisconnect()
        {
            AntiReport((vrrig, position) =>
            {
                NetworkSystem.Instance.ReturnToSinglePlayer();

                if (!(Time.time > antiReportDelay)) return;
                antiReportDelay = Time.time + 1f;
                NotifiLib.SendNotification("<color=grey>[</color><color=purple>ANTI-REPORT</color><color=grey>]</color> " + GetPlayerFromVRRig(vrrig).NickName + " attempted to report you, you have been disconnected.");
            });
        }

        public static void Clearnotis()
        {
            Notifications.NotifiLib.ClearAllNotifications();
        }
        
        public static readonly Dictionary<(long, float), GameObject> auraPool = new Dictionary<(long, float), GameObject>();
        public static void VisualizeAura(Vector3 position, float range, Color color, long? indexId = null, float alpha = 0.25f)
        {
            long index = indexId ?? BitPackUtils.PackWorldPosForNetwork(position);
            var key = (index, range);

            if (!auraPool.TryGetValue(key, out GameObject visualizeGO))
            {
                visualizeGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Object.Destroy(visualizeGO.GetComponent<Collider>());

                auraPool.Add(key, visualizeGO);
            }

            visualizeGO.SetActive(true);

            visualizeGO.transform.position = position;
            visualizeGO.transform.localScale = new Vector3(range, range, range);

            Renderer auraRenderer = visualizeGO.GetComponent<Renderer>();

            Color clr = color;
            clr.a = alpha;
            auraRenderer.material.shader = Shader.Find("GUI/Text Shader");
            auraRenderer.material.color = clr;
        }
        
        public static bool antiMute;
        
        public static int antiReportRangeIndex;
        public static float threshold = 0.35f;
        
        public static void VisualizeAntiReport()
        {
            foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (line.linePlayer != NetworkSystem.Instance.LocalPlayer) continue;
                Transform report = line.reportButton.gameObject.transform;

                VisualizeAura(report.position, threshold, Color.red);

                if (antiMute)
                    VisualizeAura(line.muteButton.gameObject.transform.position, threshold, Color.red);
            }
        }

        public static void AntiReportJoinRand()
        {
            AntiReport((vrrig, position) =>
            {
                //Mods.Safety.AntiReportJoinRand();

                if (!(Time.time > antiReportDelay)) return;
                antiReportDelay = Time.time + 1f;
                NotifiLib.SendNotification("<color=grey>[</color><color=purple>ANTI-REPORT</color><color=grey>]</color> " + GetPlayerFromVRRig(vrrig).NickName + " attempted to report you, you have been disconnected.");
            });
        }
    }
}
