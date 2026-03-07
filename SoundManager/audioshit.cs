using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class IdkAudioSetup
{
    public static string CustomAudioFolderName = "Liquid.Client";
    public static string AudioSubfolder = "Audio";

    private static Dictionary<string, string> audioFiles;

    static IdkAudioSetup()
    {
        ResourceList();
    }

    private static void ResourceList()
    {
        audioFiles = new Dictionary<string, string>
        {
            { "click.wav", "https://files.catbox.moe/5n3mki.wav" },
            { "pop.wav", "https://files.catbox.moe/ucp95l.wav" },
            { "home.wav", "https://files.catbox.moe/z3kzp8.wav" },
            { "eshop.wav", "https://files.catbox.moe/5msges.wav" },
            { "click2.wav", "https://files.catbox.moe/zgfl0c.wav" },
            { "minecraft.wav", "https://files.catbox.moe/4vi29n.wav" },
            { "snapopen.wav", "https://files.catbox.moe/bsytbj.wav" },
            { "water.wav", "https://files.catbox.moe/j24mm1.wav" },
            { "closesound.ogg", "https://files.catbox.moe/gtjydx.ogg" },
            { "notification.wav", "https://files.catbox.moe/da1lqk.wav" },
            { "notification2.wav", "https://files.catbox.moe/7ra3f5.wav" },
            { "openingsound.wav", "https://files.catbox.moe/v9nr7l.wav" },
            { "clicksound.wav", "https://files.catbox.moe/x35kz3.wav" }
        };
    }

    public static void SetupAudio(Action onComplete)
    {
        string gorillaTagPath = GetGorillaTagPath();
        if (string.IsNullOrEmpty(gorillaTagPath))
        {
            Debug.LogError("[IdkAudioSetup] Gorilla Tag folder not found!");
            onComplete?.Invoke();
            return;
        }

        string rootPath = Path.Combine(gorillaTagPath, CustomAudioFolderName);
        string audioPath = Path.Combine(rootPath, AudioSubfolder);

        try
        {
            if (!Directory.Exists(rootPath))
                Directory.CreateDirectory(rootPath);

            if (!Directory.Exists(audioPath))
                Directory.CreateDirectory(audioPath);
        }
        catch (Exception ex)
        {
            Debug.LogError("[IdkAudioSetup] Failed to create directories: " + ex.Message);
            onComplete?.Invoke();
            return;
        }

        CoroutineRunner.Run(DownloadAudioFiles(audioPath, onComplete));
    }

    private static string GetGorillaTagPath()
    {
        string defaultPath = @"C:\Program Files (x86)\Steam\steamapps\common\Gorilla Tag";
        return Directory.Exists(defaultPath) ? defaultPath : null;
    }

    private static IEnumerator DownloadAudioFiles(string audioPath, Action onComplete)
    {
        foreach (var kvp in audioFiles)
        {
            string fileName = kvp.Key;
            string url = kvp.Value;
            string fullPath = Path.Combine(audioPath, fileName);

            if (File.Exists(fullPath))
            {
                Debug.Log("[IdkAudioSetup] Already exists: " + fileName);
                continue;
            }

            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                uwr.SetRequestHeader("User-Agent", "Mozilla/5.0");
                yield return uwr.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                if (uwr.result != UnityWebRequest.Result.Success)
#else
                if (uwr.isNetworkError || uwr.isHttpError)
#endif
                {
                    Debug.LogError("[IdkAudioSetup] Failed to download: " + fileName + " | " + uwr.error);
                    continue;
                }

                try
                {
                    File.WriteAllBytes(fullPath, uwr.downloadHandler.data);
                    Debug.Log("[IdkAudioSetup] Downloaded: " + fileName);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[IdkAudioSetup] Write failed: " + ex.Message);
                }
            }
        }

        Debug.Log("[IdkAudioSetup] All audio files ready!");

        onComplete?.Invoke();
    }

    private class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner instance;

        public static void Run(IEnumerator routine)
        {
            if (instance == null)
            {
                GameObject go = new GameObject("LiquidClient_AudioRunner");
                UnityEngine.Object.DontDestroyOnLoad(go);
                instance = go.AddComponent<CoroutineRunner>();
            }

            instance.StartCoroutine(routine);
        }
    }
}