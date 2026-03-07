using System.Collections;
using System.Collections.Generic;
using liquidclient;
using liquidclient.Menu;
using liquidclient.Notifications;
using UnityEngine;
using UnityEngine.UI;

namespace liquidclient
{
    public class LoadingScreen : MonoBehaviour
    {
        private static GameObject canvasObj;
        private static GameObject blurObj;
        private static GameObject textContainer;
        private static readonly List<RectTransform> wordObjects = new();
        
        private static readonly Color LightBlue = new(0.53f, 0.81f, 0.98f);
        private static readonly Color DarkBlue = new(0f, 0.2f, 0.6f);
        
        private static GameObject gameobject;

        public static void Create(string message)
        {
            canvasObj = new GameObject("LiquidClientLoading");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            blurObj = CreateUIElement("BlurBG", canvasObj.transform);
            var bgImage = blurObj.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0);
            SetFullScreenRect(blurObj);

            textContainer = CreateUIElement("TextContainer", canvasObj.transform);
            SetFullScreenRect(textContainer);

            string[] words = message.Split(' ');
            wordObjects.Clear();

            float spacing = 280f;
            float startX = -((words.Length - 1) * spacing) / 2f;

            for (int i = 0; i < words.Length; i++)
            {
                var wordObj = CreateWord(words[i].ToUpper(), i);
                RectTransform rect = wordObj.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(startX + i * spacing, 0);
                wordObjects.Add(rect);
            }

            GameObject.DontDestroyOnLoad(canvasObj);
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }
        
        private static void SetFullScreenRect(GameObject go)
        {
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }

        private static GameObject CreateWord(string text, int index)
        {
            var go = new GameObject("W_" + index);
            go.transform.SetParent(textContainer.transform, false);

            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontStyle = FontStyle.Bold;
            t.fontSize = 75;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true;
            t.text = text;
            
            t.color = new Color(LightBlue.r, LightBlue.g, LightBlue.b, 0);

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 1f);
            outline.effectDistance = new Vector2(2, -2);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(600, 200);

            return go;
        }

        public IEnumerator MasterSequence()
        {
            Image bg = blurObj.GetComponent<Image>();
            
            yield return FadeImage(bg, 0f, 0.8f, 0.7f);

            foreach (var word in wordObjects)
            {
                StartCoroutine(FlyIn(word));
                yield return new WaitForSeconds(0.12f);
            }

            float timer = 0f;
            float duration = 2.5f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.PingPong(timer, 1f);
                foreach (var word in wordObjects)
                {
                    var txt = word.GetComponent<Text>();
                    txt.color = Color.Lerp(LightBlue, DarkBlue, t);
                }
                yield return null;
            }

            foreach (var word in wordObjects)
            {
                StartCoroutine(FlyOut(word));
                yield return new WaitForSeconds(0.08f);
            }

            yield return new WaitForSeconds(0.5f);

            yield return FadeImage(bg, 0.8f, 0f, 1f);

            Destroy(canvasObj);

        }

        private static IEnumerator FadeImage(Image image, float from, float to, float duration)
        {
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                image.color = new Color(0, 0, 0, Mathf.Lerp(from, to, t / duration));
                yield return null;
            }
        }

        private IEnumerator FlyIn(RectTransform r)
        {
            Vector2 target = r.anchoredPosition;
            float angle = Random.Range(0f, Mathf.PI * 2);
            Vector2 start = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 1400f;

            Text t = r.GetComponent<Text>();
            Color color = t.color;
            float duration = 0.85f;

            for (float i = 0; i < duration; i += Time.deltaTime)
            {
                float p = i / duration;
                float ease = 1f - Mathf.Pow(1f - p, 4f);
                r.anchoredPosition = Vector2.LerpUnclamped(start, target, ease);
                r.localScale = Vector3.LerpUnclamped(Vector3.one * 2.5f, Vector3.one, ease);
                t.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(p * 2.5f));
                yield return null;
            }
        }

        private IEnumerator FlyOut(RectTransform r)
        {
            Vector2 start = r.anchoredPosition;
            float angle = Random.Range(0f, Mathf.PI * 2);
            Vector2 end = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 1400f;

            Text t = r.GetComponent<Text>();
            Color color = t.color;
            float duration = 0.75f;

            for (float i = 0; i < duration; i += Time.deltaTime)
            {
                float p = i / duration;
                float ease = p * p * p;
                r.anchoredPosition = Vector2.Lerp(start, end, ease);
                r.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, ease);
                t.color = new Color(color.r, color.g, color.b, 1 - p);
                yield return null;
            }
        }
    }
}