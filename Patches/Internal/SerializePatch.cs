using HarmonyLib;
using Photon.Pun;
﻿using System;
using UnityEngine;

namespace liquid.client.Patches.Internal
{
    [HarmonyPatch(typeof(PhotonNetwork), nameof(PhotonNetwork.RunViewUpdate))]
    public class SerializePatch
    {
        /// <summary>
        /// Occurs when a serialization process is initiated.
        /// </summary>
        public static event Action OnSerialize;

        /// <summary>
        /// Delegate that determines whether serialization should be overridden.
        /// </summary>
        public static Func<bool> OverrideSerialization;

        public static bool Prefix()
        {
            if (!PhotonNetwork.InRoom)
                return true;

            try
            {
                OnSerialize?.Invoke();
            } catch (Exception e)
            {
                Debug.LogError($"Error in SerializePatch.OnSerialize: {e}");
            }

            if (OverrideSerialization == null)
                return true;

            try
            {
                return OverrideSerialization();
            } catch (Exception e)
            {
                Debug.LogError($"Error in SerializePatch.OverrideSerialization: {e}");
                return false;
            }
        }
    }
}