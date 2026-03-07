using System.Collections;
using System.Collections.Generic;
using liquidclient;
using liquidclient.Classes;
using liquidclient.Menu;
using liquidclient.Notifications;
using UnityEngine;
using UnityEngine.UI;
using Button = liquidclient.Classes.Button;

namespace LoadMenu
{
    public class Loader
    {
        private static GameObject gameobject;
        private static GameObject loadingObject;

        public static void Load()
        {
            Debug.Log("inject successfully");
            
            loadingObject = new GameObject("LoadingScreen");
            loadingObject.AddComponent<LoadingScreen>();
            LoadingScreen.Create("LIQUID CLIENT");
            gameobject = new GameObject();
            gameobject.AddComponent<HarmonyPatches>();
            gameobject.AddComponent<NotifiLib>();
            gameobject.AddComponent<Button>();
            gameobject.AddComponent<Main>();
            
            Object.DontDestroyOnLoad(gameobject);
            Object.DontDestroyOnLoad(loadingObject);
            loadingObject.GetComponent<LoadingScreen>().StartCoroutine(
                ((LoadingScreen)loadingObject.GetComponent<LoadingScreen>()).MasterSequence()
            );
        }

        public static void Unload()
        {
            if (gameobject != null)
                Object.Destroy(gameobject);
            if (loadingObject != null)
                Object.Destroy(loadingObject);
        }
    }
}