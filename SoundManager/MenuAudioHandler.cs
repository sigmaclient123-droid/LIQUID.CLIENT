using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class MenuAudioHandler
{
    public static AudioSource MenuAudioSource;

    public static AudioClip ClickSound;
    public static AudioClip OpenSound;
    public static AudioClip OpenSound2;
    public static AudioClip OpenSound3;
    public static AudioClip Click2Sound;
    public static AudioClip SwichSound;
    public static AudioClip WaterSound;
    public static AudioClip closesound;
    public static AudioClip notificationsound;
    public static AudioClip opensound2;
    public static AudioClip notificationsound2;
    public static AudioClip Click3Sound;

    private static readonly string AudioPath = @"C:\Program Files (x86)\Steam\steamapps\common\Gorilla Tag\Liquid.Client\Audio\";
    private static readonly string ResourcePath = @"C:\Program Files (x86)\Steam\steamapps\common\Gorilla Tag\Liquid.Client\Resources\";

    private static Dictionary<string, AudioClip> LoadedClips = new Dictionary<string, AudioClip>();
    private static bool isLoading = false;

    public static bool ResourcesReady { get; private set; } = false;

    public static void Init()
    {
        if (MenuAudioSource == null)
        {
            GameObject go = new GameObject("MenuAudioSource");
            UnityEngine.Object.DontDestroyOnLoad(go);
            MenuAudioSource = go.AddComponent<AudioSource>();
            MenuAudioSource.spatialBlend = 0f;
            MenuAudioSource.playOnAwake = false;
            
        }

        if (!isLoading)
        {
            isLoading = true;
            CoroutineRunner.Run(LoadAllAudio());
        }
    }

    private static void EnsureInit()
    {
        if (MenuAudioSource == null)
            Init();
    }

    public static Texture2D LoadTextureFromFile(string fileName)
    {
        string path = Path.Combine(ResourcePath, fileName);

        if (!File.Exists(path))
        {
            Debug.LogError("[MenuAudioHandler] Texture file not found: " + path);
            return null;
        }

        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            ImageConversion.LoadImage(tex, bytes);
            return tex;
        }
        catch (Exception ex)
        {
            Debug.LogError("[MenuAudioHandler] Failed to load texture: " + ex.Message);
            return null;
        }
    }

    private static IEnumerator LoadAllAudio()
    {
        if (!Directory.Exists(AudioPath))
        {
            Debug.LogError("[MenuAudioHandler] Audio folder not found: " + AudioPath);
            isLoading = false;
            yield break;
        }

        string[] files = Directory.GetFiles(AudioPath, "*.*", SearchOption.TopDirectoryOnly);
        foreach (string file in files)
        {
            if (!file.EndsWith(".wav") && !file.EndsWith(".mp3") && !file.EndsWith(".ogg"))
                continue;

            string clipName = Path.GetFileNameWithoutExtension(file);

            if (LoadedClips.ContainsKey(clipName))
                continue;

            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip("file:///" + file, AudioType.UNKNOWN))
            {
                yield return uwr.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                if (uwr.result != UnityWebRequest.Result.Success)
#else
                if (uwr.isNetworkError || uwr.isHttpError)
#endif
                {
                    Debug.LogError("[MenuAudioHandler] Failed to load audio: " + file);
                    continue;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
                LoadedClips.Add(clipName, clip);

                // Assign to correct fields
                switch (clipName.ToLower())
                {
                    case "click": ClickSound = clip; break;
                    case "click2": Click2Sound = clip; break;
                    case "water": OpenSound = clip; break;
                    case "open2": OpenSound2 = clip; break;
                    case "home": OpenSound3 = clip; break;
                    //case "water": WaterSound = clip; break;
                    case "closesound": closesound = clip; break;
                    case "notification": notificationsound = clip; break;
                    case "notification2": notificationsound2 = clip; break;
                    case "openingsound": opensound2 = clip; break;
                    case "clicksound": Click3Sound = clip; break;
                    //clicksound
                }
            }
        }

        ResourcesReady = true;
        isLoading = false;
        Debug.Log("[MenuAudioHandler] Audio resources loaded!");
    }

    public static void PlayClick()       { EnsureInit(); if (ClickSound != null) MenuAudioSource.PlayOneShot(ClickSound); }
    public static void PlayClick2()      { EnsureInit(); if (Click2Sound != null) MenuAudioSource.PlayOneShot(Click2Sound); }
    public static void PlayOpen()        { EnsureInit(); if (OpenSound != null) MenuAudioSource.PlayOneShot(OpenSound); }
    public static void PlayOpen2()       { EnsureInit(); if (OpenSound2 != null) MenuAudioSource.PlayOneShot(OpenSound2); }
    public static void PlayOpen3()       { EnsureInit(); if (OpenSound3 != null) MenuAudioSource.PlayOneShot(OpenSound3); }
    public static void PlaySwich()       { EnsureInit(); if (SwichSound != null) MenuAudioSource.PlayOneShot(SwichSound); }
    public static void PlayClosesound()   { EnsureInit(); if (closesound != null) MenuAudioSource.PlayOneShot(closesound); }
    public static void PlayNotificationSound()   { EnsureInit(); if (notificationsound != null) MenuAudioSource.PlayOneShot(notificationsound); }
    public static void PlayWater()   { EnsureInit(); if (WaterSound != null) MenuAudioSource.PlayOneShot(WaterSound); }
    public static void PlayNotificationSound2()   { EnsureInit(); if (notificationsound2 != null) MenuAudioSource.PlayOneShot(notificationsound2); }
    public static void PlayOpeningSound()   { EnsureInit(); if (opensound2 != null) MenuAudioSource.PlayOneShot(opensound2); }
    public static void PlayClick3Sound()   { EnsureInit(); if (Click3Sound != null) MenuAudioSource.PlayOneShot(Click3Sound); }

    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner instance;

        public static void Run(IEnumerator routine)
        {
            if (instance == null)
            {
                GameObject go = new GameObject("CoroutineRunner");
                UnityEngine.Object.DontDestroyOnLoad(go);
                instance = go.AddComponent<CoroutineRunner>();
            }
            instance.StartCoroutine(routine);
        }
    }
}