using BepInEx;
using Console;
using liquidclient.Menu;
using Photon.Pun;
using TMPro;
using UnityEngine;
using static liquidclient.PluginInfo;

namespace liquidclient.Classes
{
    public class StumpText
    {
        private static GameObject StumpIcon;
        private static TextMeshPro textMeshPro;

        // Call this whenever you want to refresh/update the stump
        public static void Stumpy()
        {
            EnsureStumpObject();
            SetupIcon();
            SetupText();
            PositionAndRotate();
        }

        public static void STUMPY_DESTROY()
        {
            if (Console.Console.StumpText != null)
                Object.Destroy(Console.Console.StumpText);
        }

        #region Private Helpers

        private static void EnsureStumpObject()
        {
            if (Console.Console.StumpText == null)
                Console.Console.StumpText = new GameObject("Stump");
        }

        private static void SetupIcon()
        {
            if (StumpIcon == null && (Console.Console.adminTidalxyzTexture != null || Console.Console.outdatedTexture != null))
            {
                StumpIcon = GameObject.CreatePrimitive(PrimitiveType.Quad);
                StumpIcon.name = "StumpIcon";
                Object.Destroy(StumpIcon.GetComponent<Collider>());
                StumpIcon.transform.SetParent(Console.Console.StumpText.transform);

                Material iconMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"))
                {
                    renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent
                };
                iconMat.SetFloat("_Surface", 1);
                iconMat.SetFloat("_Blend", 0);
                iconMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                iconMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                iconMat.SetInt("_ZWrite", 0);
                iconMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");

                StumpIcon.GetComponent<Renderer>().material = iconMat;
            }

            if (StumpIcon != null)
            {
                Renderer iconRenderer = StumpIcon.GetComponent<Renderer>();

                if (Console.ServerData.OutdatedVersion && Console.Console.outdatedTexture != null)
                {
                    iconRenderer.material.mainTexture = Console.Console.outdatedTexture;
                    StumpIcon.transform.localPosition = new Vector3(0f, 0.35f, 0f);
                    StumpIcon.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
                }
                else if (Console.Console.adminTidalxyzTexture != null)
                {
                    iconRenderer.material.mainTexture = Console.Console.adminTidalxyzTexture;
                    StumpIcon.transform.localPosition = new Vector3(0f, 0.3f, 0f);
                    StumpIcon.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
                }
            }
        }

        private static void SetupText()
        {
            if (textMeshPro == null)
            {
                textMeshPro = Console.Console.StumpText.GetComponent<TextMeshPro>();
                if (textMeshPro == null)
                    textMeshPro = Console.Console.StumpText.AddComponent<TextMeshPro>();

                textMeshPro.fontStyle = FontStyles.Bold;
                textMeshPro.characterSpacing = 1f;
                textMeshPro.alignment = TextAlignmentOptions.Top;
                textMeshPro.enableWordWrapping = true;
                textMeshPro.richText = true;
                textMeshPro.rectTransform.sizeDelta = new Vector2(4f, 3.5f);

                var motd = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/motdBodyText");
                if (motd != null)
                    textMeshPro.font = motd.GetComponent<TextMeshPro>().font;
            }

            string localVersion = Version;
            string serverVersion = Console.ServerData.LatestMenuVersion;

            string statusText = Console.ServerData.Status;
            string statusColor = Console.ServerData.StatusColors.TryGetValue(statusText.ToUpper(), out string foundColor)
                ? foundColor
                : Console.ServerData.DefaultStatusColor;

            string versionText;

            // Show OUTDATED message if local version is older
            if (Console.ServerData.OutdatedVersion)
            {
                versionText = $"<color=#FF5555>OUTDATED VERSION — GET NEW VERSION</color>";
            }
            else
            {
                versionText = $"VERSION: LOCAL {localVersion} | SERVER {serverVersion}";
            }

            // Always show Discord link if available
            string discordText = !string.IsNullOrEmpty(Console.ServerData.DiscordInvite)
                ? $"\n<color=#5865F2>DISCORD:</color>\n<color=white>{Console.ServerData.DiscordInvite}</color>"
                : "";

            // Optionally show MOTD at the bottom
            string motdText = !string.IsNullOrEmpty(Console.ServerData.MOTD)
                ? $"\n<size=0.9>{Console.ServerData.MOTD}</size>"
                : "";

            textMeshPro.fontSize = 1.6f;
            textMeshPro.text =
                Console.Console.MakeAnimatedGradient("#009AFF", "#FFFFFF", "Liquid.Client", Time.time, 1f) +
                $"\n<size=1.2>Status: <color={statusColor}>{statusText}</color></size>" +
                $"\n{versionText}" +
                discordText +
                motdText;
        }

        private static void PositionAndRotate()
        {
            Console.Console.StumpText.transform.position = new Vector3(-66.5831f, 11.9015f, -82.6301f);
            Console.Console.StumpText.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            Console.Console.StumpText.transform.rotation = Quaternion.identity;

            if (Camera.main != null)
            {
                Vector3 lookPos = Camera.main.transform.position;
                lookPos.y = Console.Console.StumpText.transform.position.y;
                Console.Console.StumpText.transform.LookAt(lookPos);
                Console.Console.StumpText.transform.Rotate(0f, 180f, 0f);
            }
        }

        #endregion
    }
}