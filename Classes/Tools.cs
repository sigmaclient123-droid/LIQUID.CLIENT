using System.IO;
using System.Reflection;
using UnityEngine;

namespace liquidclient.Classes
{
    public class Tools
    {
        private AudioSource menuAudioSource;
        
        public static Tools Instance { get; private set; } = new Tools();
        private Tools() 
        {
            InitializeAudioSource();
        }

        public AssetBundle LiquidBundle { get; private set; }
        
        private void InitializeAudioSource()
        {
            if (menuAudioSource != null) return;

            GameObject audioGO = new GameObject("ToolsAudioSource");
            Object.DontDestroyOnLoad(audioGO);
            menuAudioSource = audioGO.AddComponent<AudioSource>();
            menuAudioSource.playOnAwake = false;
            menuAudioSource.spatialBlend = 0f;
        }

        public void PlaySound(AudioClip clip)
        {
            if (clip != null && menuAudioSource != null)
                menuAudioSource.PlayOneShot(clip);
        }

        public static Texture2D LoadEmbeddedImage(string name)
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("liquid.client.Resources." + name);

            if (stream == null) return null;

            byte[] imageData = new byte[stream.Length];
            stream.Read(imageData, 0, imageData.Length);

            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);

            return texture;
        }
    }
    
    public static class GameObjectExtensions
    {
        public static void Obliterate(this GameObject obj) => Object.Destroy(obj);
        public static void Obliterate(this Component comp) => Object.Destroy(comp);
        public static void Obliterate(this GameObject obj, float delay) => Object.Destroy(obj, delay);
        public static void Obliterate(this Component comp, float delay) => Object.Destroy(comp, delay);
    }
}