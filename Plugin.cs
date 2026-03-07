using System;
using System.IO;
using System.Reflection;
using BepInEx;
using static liquidclient.Classes.Tools;
using liquidclient.Classes;
using liquidclient.Misc;
using UnityEngine;
using TMPro;
using liquidclient.Managers;

namespace liquidclient
{
    [System.ComponentModel.Description(PluginInfo.Description)]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class HarmonyPatches : BaseUnityPlugin
    {
        public AssetBundle LiquidBundle; 

        private void Awake()
        {
            GorillaTagger.OnPlayerSpawned(OnPlayerSpawned);

            GameObject obj = new GameObject("HamburburPromotionManager");
            DontDestroyOnLoad(obj);
            obj.AddComponent<HamburburPromotionManager>();
        }
        
        public void OnPlayerSpawned()
        {
            Patches.PatchHandler.PatchAll();
            gameObject.AddComponent<CoroutineManager2>();

            IdkAudioSetup.SetupAudio(() =>
            {
                Debug.Log("Audio ready. Initializing menu audio.");
                MenuAudioHandler.Init();
            });
        }

        /*private void OnGameInitialized()
        {
            Stream bundleStream =
                Assembly.GetExecutingAssembly().GetManifestResourceStream("liquid.client.Resources.hamburbur");

            if (bundleStream == null)
            {
                Debug.LogError("Embedded resource 'hamburbur' not found!");
                return;
            }

            LiquidBundle = AssetBundle.LoadFromStream(bundleStream);
            bundleStream?.Close();

            Debug.Log("AssetBundle loaded successfully!");
        }*/
            

        //void Start() => Console.Console.LoadConsole();
    }
}
