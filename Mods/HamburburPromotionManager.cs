using GorillaExtensions;
using HarmonyLib;
using liquidclient.Classes;
using liquidclient.Components;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using static liquidclient.Classes.Tools;
using static Console.ServerData;

namespace liquidclient.Misc;

public class HamburburPromotionManager : Singleton<HamburburPromotionManager>
{
    private bool        hasSetupFeaturedMapVideo;
    private VideoPlayer videoPlayer;
    public Shader      UberShader      { get; private set; }

    private void Start()
    {
        GameObject cameraSpawner = GameObject.Find(
            "Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/SatelliteWardrobe/LCKWallCameraSpawner");

        if (cameraSpawner != null)
            cameraSpawner.Obliterate();

        GameObject fin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fin.transform.localScale = new Vector3(0.8f, 0.9f, 0.0001f);
        fin.transform.position = new Vector3(-64.72f, 12f, -84.72f);
        fin.transform.rotation = Quaternion.Euler(0f, 271.63f, 0f);

        Renderer renderer = fin.GetComponent<Renderer>();
        if (renderer == null)
            return;
        
        Texture2D tex = LoadEmbeddedImage("citty.png");

        if (tex == null)
        {
            Debug.LogError("fin.png failed to load from embedded resources.");
            return;
        }

        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.Apply();

        Shader shader = Shader.Find("Unlit/Texture");

        if (shader == null)
        {
            Debug.LogError("Unlit/Texture shader not found!");
            return;
        }

        Material mat = new Material(shader);
        mat.mainTexture = tex;
        mat.color = Color.white;

        renderer.material = mat;
    }

    private void Update()
    {
        if (GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom") == null)
            return;
        
        if (hasSetupFeaturedMapVideo && !videoPlayer.isPlaying && videoPlayer.gameObject.activeInHierarchy &&
            videoPlayer.enabled)
            videoPlayer.Play();

        if (hasSetupFeaturedMapVideo)
            return;

        GameObject loadingText = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/LoadingText");

        GameObject mapInfoText =
                GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/MapInfo_TMP");

        GameObject featuredMaps =
                GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/ModIOFeaturedMapsDisplay/");

        GameObject displayTextObj =
                GameObject.Find(
                        "Environment Objects/LocalObjects_Prefab/TreeRoom/ModIOFeaturedMapsDisplay/DisplayText");

        if (displayTextObj != null)
            foreach (Transform child in displayTextObj.transform)
                if (child.name.ToLower().EndsWith("tmp"))
                    child.gameObject.SetActive(!child.gameObject.activeSelf);

        if (mapInfoText == null || featuredMaps == null)
            return;

        try
        {
            TextMeshPro featuredMapText = mapInfoText.GetComponent<TextMeshPro>();
            if (featuredMapText != null)
                featuredMapText.text = "<color=black>Liquid.Client ON TOP!</color>";
            
            if (loadingText != null)
                loadingText.Obliterate();

            GameObject featuredMapImage = featuredMaps.transform.Find("FeaturedMapImage")?.gameObject;

            if (featuredMapImage == null)
                return;

            if (featuredMapImage.TryGetComponent(out SpriteRenderer spriteRenderer))
                spriteRenderer.Obliterate();

            MeshFilter mf = featuredMapImage.GetOrAddComponent<MeshFilter>();
            mf.mesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");

            MeshRenderer mr = featuredMapImage.GetOrAddComponent<MeshRenderer>();

            Material videoMat = new(Shader.Find("Unlit/Texture"));
            mr.material = videoMat;

            videoPlayer                 = featuredMapImage.AddComponent<VideoPlayer>();
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            //videoPlayer.url             = "http://localhost:3000/files/hamburger.mp4"; ServerDataEndpoint $"{ServerDataTidal}/data"; // local
            videoPlayer.url             = $"{ServerEndpoint}/files/hamburger.mp4";
 
            RenderTexture rt = new(512, 512, 0);
            videoPlayer.targetTexture = rt;
            mr.material.mainTexture   = rt;

            featuredMapImage.transform.localScale = new Vector3(0.845f, 0.445f, 1f);

            videoPlayer.isLooping = true;
            videoPlayer.Play();

            featuredMapImage.SetActive(true);

            hasSetupFeaturedMapVideo = true;
        }
        catch
        {
            //fine it threw ONE null reference exception without the try block
        }
    }
}

[HarmonyPatch(typeof(NewMapsDisplay), nameof(NewMapsDisplay.UpdateSlideshow))]
public static class NewMapsDisplay_UpdateSlideshow_Patch
{
    private static bool Prefix(NewMapsDisplay __instance)
    {
        if (__instance == null)
            return true;

        return __instance.mapImage != null && __instance.mapImage.gameObject != null;
    }
}