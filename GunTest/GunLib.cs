using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace liquidclient.GunLib
{
    internal class GunLibTEst
    {
        internal static GunLibData ShootLock()
        {
            throw new NotImplementedException();
        }

        public class Config
        {
            public Vector3 PointerScale { get; set; } = new Vector3(0.2f, 0.2f, 0.2f);
            public Color32 PointerColorStart { get; set; } = new Color32(0, 255, 100, 255);
            public Color32 PointerColorEnd { get; set; } = new Color32(0, 200, 255, 255);
            public Color32 TriggeredPointerColorStart { get; set; } = new Color32(255, 100, 50, 255);
            public Color32 TriggeredPointerColorEnd { get; set; } = new Color32(255, 150, 0, 255);
            public float LineWidth { get; set; } = 0.03f;
            public Color32 LineColorStart { get; set; } = new Color32(0, 255, 150, 255);
            public Color32 LineColorEnd { get; set; } = new Color32(0, 180, 255, 255);
            public Color32 TriggeredLineColorStart { get; set; } = new Color32(255, 100, 50, 255);
            public Color32 TriggeredLineColorEnd { get; set; } = new Color32(255, 150, 0, 255);
            public bool EnableAnimations { get; set; } = true;
            public float PulseSpeed { get; set; } = 2f;
            public float PulseAmplitude { get; set; } = 0.04f;
            public bool EnableParticles { get; set; } = true;
            public float ParticleStartSize { get; set; } = 0.1f;
            public float ParticleStartSpeed { get; set; } = 0.5f;
            public int ParticleMaxCount { get; set; } = 100;
            public float ParticleEmissionRate { get; set; } = 20f;
            public bool EnableBoxESP { get; set; } = true;
            public float BoxESPWidth { get; set; } = 1f;
            public float BoxESPHeight { get; set; } = 2f;
            public Color32 BoxESPColor { get; set; } = new Color32(0, 255, 100, 255);
            public Color32 BoxESPOuterColor { get; set; } = new Color32(255, 150, 0, 255);
            public int LineCurve { get; set; } = 150;
            public float WaveFrequency { get; set; } = 5f;
            public float WaveAmplitude { get; set; } = 0.05f;
        }

        public class AthrionGunLibrary : MonoBehaviour
        {
            public static Config GunConfig = new Config();
            public static GameObject spherepointer;
            public static VRRig LockedRigOrPlayerOrwhatever;
            public static Vector3 lr;
            public static ParticleSystem particleSystem;
            private static float waveTimeOffset = 0f;
            private static bool colorIndexInitialized = false;
            private static int ColorIndex = 0;
            private static AnimationMode currentAnimationMode = AnimationMode.DefaultBezier;

            public enum AnimationMode
            {
                None,
                Wave,
                Pulse,
                Zigzag,
                Bouncing,
                Spiral,
                SineWave,
                Helix,
                Sawtooth,
                TriangleWave,
                DefaultBezier
            }

            public static AnimationMode CurrentAnimationMode
            {
                get
                {
                    return currentAnimationMode;
                }
                set
                {
                    currentAnimationMode = value;
                }
            }

            private static Vector3 CalculateBezierPoint(Vector3 start, Vector3 mid, Vector3 end, float t)
            {
                return Mathf.Pow(1f - t, 2f) * start + 2f * (1f - t) * t * mid + Mathf.Pow(t, 2f) * end;
            }

            private static void CurveLineRenderer(LineRenderer lineRenderer, Vector3 start, Vector3 mid, Vector3 end)
            {
                lineRenderer.positionCount = GunConfig.LineCurve;
                waveTimeOffset += Time.deltaTime * 2f;
                
                for (int i = 0; i < GunConfig.LineCurve; i++)
                {
                    float num = (float)i / (float)(GunConfig.LineCurve - 1);
                    Vector3 vector;
                    
                    switch (currentAnimationMode)
                    {
                        case AnimationMode.Wave:
                            vector = Vector3.Lerp(start, end, num);
                            vector.x += Mathf.Sin(num * GunConfig.WaveFrequency + waveTimeOffset) * GunConfig.WaveAmplitude;
                            break;
                        case AnimationMode.Pulse:
                            vector = Vector3.Lerp(start, end, num + Mathf.Sin(Time.time * GunConfig.WaveFrequency) * 0.02f);
                            break;
                        case AnimationMode.Zigzag:
                            vector = Vector3.Lerp(start, end, num);
                            vector.x += (float)((i % 2 == 0) ? 1 : -1) * GunConfig.WaveAmplitude * Mathf.Sin(waveTimeOffset);
                            break;
                        case AnimationMode.Bouncing:
                            vector = Vector3.Lerp(start, end, num);
                            vector.y += Mathf.Abs(Mathf.Sin(num * GunConfig.WaveFrequency + waveTimeOffset) * GunConfig.WaveAmplitude);
                            break;
                        case AnimationMode.Spiral:
                            vector = Vector3.Lerp(start, end, num);
                            vector.x += Mathf.Sin(num * GunConfig.WaveFrequency + waveTimeOffset) * GunConfig.WaveAmplitude;
                            vector.y += Mathf.Cos(num * GunConfig.WaveFrequency + waveTimeOffset) * GunConfig.WaveAmplitude;
                            break;
                        case AnimationMode.SineWave:
                            vector = Vector3.Lerp(start, end, num);
                            vector.z += Mathf.Sin(num * GunConfig.WaveFrequency + waveTimeOffset) * GunConfig.WaveAmplitude;
                            break;
                        case AnimationMode.Helix:
                            vector = Vector3.Lerp(start, end, num);
                            vector.x += Mathf.Sin(6.2831855f * GunConfig.WaveFrequency * num + waveTimeOffset) * GunConfig.WaveAmplitude;
                            vector.z += Mathf.Cos(6.2831855f * GunConfig.WaveFrequency * num + waveTimeOffset) * GunConfig.WaveAmplitude;
                            break;
                        case AnimationMode.Sawtooth:
                            vector = Vector3.Lerp(start, end, num);
                            vector.z += GunConfig.WaveAmplitude * (2f * (num * GunConfig.WaveFrequency - Mathf.Floor(num * GunConfig.WaveFrequency + 0.5f)));
                            break;
                        case AnimationMode.TriangleWave:
                            vector = Vector3.Lerp(start, end, num);
                            vector.y += GunConfig.WaveAmplitude * (2f * Mathf.Abs(2f * (num * GunConfig.WaveFrequency - Mathf.Floor(num * GunConfig.WaveFrequency + 0.5f))) - 1f);
                            break;
                        default:
                            vector = CalculateBezierPoint(start, mid, end, num);
                            break;
                    }
                    
                    lineRenderer.SetPosition(i, vector);
                }
            }

            private static IEnumerator AnimateLineGradient(LineRenderer lineRenderer, Color32 startColor, Color32 endColor)
            {
                while (lineRenderer != null)
                {
                    float t = Mathf.PingPong(Time.time, 1f);
                    lineRenderer.startColor = Color32.Lerp(startColor, endColor, t);
                    lineRenderer.endColor = Color32.Lerp(endColor, startColor, t);
                    yield return null;
                }
            }

            private static IEnumerator StartCurvyLineRenderer(LineRenderer lineRenderer, Vector3 start, Vector3 mid, Vector3 end, Color32 startColor, Color32 endColor)
            {
                while (lineRenderer != null)
                {
                    CurveLineRenderer(lineRenderer, start, mid, end);
                    lineRenderer.startColor = startColor;
                    lineRenderer.endColor = endColor;
                    yield return null;
                }
            }

            private static IEnumerator PulsePointer(GameObject pointer)
            {
                Vector3 originalScale = GunConfig.PointerScale;
                
                while (pointer != null)
                {
                    float pulse = 1f + Mathf.Sin(Time.time * GunConfig.PulseSpeed) * GunConfig.PulseAmplitude;
                    pointer.transform.localScale = originalScale * pulse;
                    yield return null;
                }
            }

            private static void AddPointerParticles(GameObject pointer)
            {
                if (!GunConfig.EnableParticles) return;
    
                particleSystem = pointer.AddComponent<ParticleSystem>();
                ParticleSystem.MainModule main = particleSystem.main;
                main.startColor = new ParticleSystem.MinMaxGradient(GunConfig.PointerColorStart, GunConfig.PointerColorEnd);
                main.startSize = GunConfig.ParticleStartSize;
                main.startSpeed = GunConfig.ParticleStartSpeed;
                main.maxParticles = GunConfig.ParticleMaxCount;
                main.duration = 1f;
                main.loop = true;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
    
                ParticleSystem.EmissionModule emission = particleSystem.emission;
                emission.rateOverTime = GunConfig.ParticleEmissionRate;
    
                ParticleSystem.ShapeModule shape = particleSystem.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.05f;
    
                ParticleSystemRenderer component = particleSystem.GetComponent<ParticleSystemRenderer>();
                component.material = new Material(Shader.Find("Sprites/Default"));
            }

            private static IEnumerator SmoothStopParticles()
            {
                if (particleSystem == null) yield break;
    
                ParticleSystem.EmissionModule emission = particleSystem.emission;
                float startRate = emission.rateOverTime.constant;
    
                for (float t = 0; t < 1f; t += Time.deltaTime)
                {
                    emission.rateOverTime = Mathf.Lerp(startRate, 0f, t);
                    yield return null;
                }
    
                emission.rateOverTime = 0f;
                particleSystem.Stop();
                UnityEngine.Object.Destroy(particleSystem);
            }

            public static void StartVrGun(Action action, bool LockOn)
            {
                if (!ControllerInputPoller.instance.rightGrab) 
                {
                    if (spherepointer != null) CleanupPointer();
                    return;
                }

                RaycastHit raycastHit;
                bool flag = Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, -GorillaTagger.Instance.rightHandTransform.up, out raycastHit, float.MaxValue);
                
                if (spherepointer == null) CreatePointer();
                
                if (flag)
                {
                    if (LockedRigOrPlayerOrwhatever == null)
                    {
                        spherepointer.transform.position = raycastHit.point;
                        spherepointer.GetComponent<Renderer>().material.color = GunConfig.PointerColorStart;
                        
                        if (LockOn && raycastHit.collider.GetComponentInParent<VRRig>() != null)
                        {
                            LockedRigOrPlayerOrwhatever = raycastHit.collider.GetComponentInParent<VRRig>();
                            if (LockedRigOrPlayerOrwhatever == GorillaTagger.Instance.offlineVRRig) LockedRigOrPlayerOrwhatever = null;
                        }
                    }
                    else
                    {
                        spherepointer.transform.position = LockedRigOrPlayerOrwhatever.transform.position;
                    }
                }
                
                HandleLineRendering();
                
                if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f)
                {
                    spherepointer.GetComponent<Renderer>().material.color = GunConfig.TriggeredPointerColorStart;
                    
                    if (LockOn && LockedRigOrPlayerOrwhatever != null)
                    {
                        action.Invoke();
                        if (GunConfig.EnableBoxESP) BoxESP();
                    }
                    else if (!LockOn)
                    {
                        action.Invoke();
                    }
                }
                else if (LockedRigOrPlayerOrwhatever != null)
                {
                    LockedRigOrPlayerOrwhatever = null;
                }
            }

            public static void StartPcGun(Action action, bool LockOn)
            {
                Camera cam = GameObject.Find("Shoulder Camera").activeSelf ? 
                    GameObject.Find("Shoulder Camera").GetComponent<Camera>() : 
                    GorillaTagger.Instance.mainCamera.GetComponent<Camera>();
                
                Ray ray = cam.ScreenPointToRay(UnityInput.Current.mousePosition);
                
                if (!Mouse.current.rightButton.isPressed)
                {
                    if (spherepointer != null)
                    {
                        UnityEngine.Object.Destroy(spherepointer);
                        spherepointer = null;
                        LockedRigOrPlayerOrwhatever = null;
                    }
                    return;
                }

                RaycastHit raycastHit;
                bool flag = Physics.Raycast(ray.origin, ray.direction, out raycastHit, float.PositiveInfinity, -32777);
                
                if (flag && spherepointer == null)
                {
                    spherepointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    spherepointer.AddComponent<Renderer>();
                    spherepointer.transform.localScale = GunConfig.PointerScale;
                    spherepointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    UnityEngine.Object.Destroy(spherepointer.GetComponent<BoxCollider>());
                    UnityEngine.Object.Destroy(spherepointer.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(spherepointer.GetComponent<Collider>());
                    
                    lr = GorillaTagger.Instance.offlineVRRig.rightHandTransform.position;
                    AthrionGunLibrary athrionGunLibrary = spherepointer.AddComponent<AthrionGunLibrary>();
                    athrionGunLibrary.StartCoroutine(PulsePointer(spherepointer));
                    AddPointerParticles(spherepointer);
                }

                if (LockedRigOrPlayerOrwhatever == null)
                {
                    spherepointer.transform.position = raycastHit.point;
                    spherepointer.GetComponent<Renderer>().material.color = GunConfig.PointerColorStart;
                }
                else
                {
                    spherepointer.transform.position = LockedRigOrPlayerOrwhatever.transform.position;
                }

                lr = Vector3.Lerp(lr, (GorillaTagger.Instance.rightHandTransform.position + spherepointer.transform.position) / 2f, Time.deltaTime * 6f);
                
                GameObject lineObject = new GameObject("Line");
                LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
                lineRenderer.startWidth = GunConfig.LineWidth;
                lineRenderer.endWidth = GunConfig.LineWidth;
                
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null) lineRenderer.material = new Material(shader);
                
                AthrionGunLibrary athrionGunLibrary2 = lineObject.AddComponent<AthrionGunLibrary>();
                athrionGunLibrary2.StartCoroutine(StartCurvyLineRenderer(lineRenderer, GorillaTagger.Instance.rightHandTransform.position, lr, spherepointer.transform.position, GunConfig.LineColorStart, GunConfig.LineColorEnd));
                athrionGunLibrary2.StartCoroutine(AnimateLineGradient(lineRenderer, GunConfig.LineColorStart, GunConfig.LineColorEnd));

                if (spherepointer.transform.hasChanged)
                {
                    if (GunConfig.EnableParticles && particleSystem != null && !particleSystem.isPlaying) particleSystem.Play();
                }
                else
                {
                    if (GunConfig.EnableParticles && particleSystem != null) particleSystem.Stop();
                }

                UnityEngine.Object.Destroy(lineRenderer, Time.deltaTime);

                if (Mouse.current.leftButton.isPressed)
                {
                    spherepointer.GetComponent<Renderer>().material.color = GunConfig.TriggeredPointerColorStart;
                    
                    if (LockOn)
                    {
                        if (LockedRigOrPlayerOrwhatever == null)
                        {
                            LockedRigOrPlayerOrwhatever = raycastHit.collider.GetComponentInParent<VRRig>();
                            if (LockedRigOrPlayerOrwhatever == GorillaTagger.Instance.offlineVRRig)
                            {
                                LockedRigOrPlayerOrwhatever = null;
                                return;
                            }
                        }
                        
                        if (LockedRigOrPlayerOrwhatever != null)
                        {
                            spherepointer.transform.position = LockedRigOrPlayerOrwhatever.transform.position;
                            action.Invoke();
                            if (GunConfig.EnableBoxESP) BoxESP();
                        }
                    }
                    else
                    {
                        action.Invoke();
                    }
                }
                else if (LockedRigOrPlayerOrwhatever != null)
                {
                    LockedRigOrPlayerOrwhatever = null;
                }
            }

            private static void CreatePointer()
            {
                spherepointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spherepointer.AddComponent<Renderer>();
                spherepointer.transform.localScale = GunConfig.PointerScale;
                spherepointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                
                UnityEngine.Object.Destroy(spherepointer.GetComponent<BoxCollider>());
                UnityEngine.Object.Destroy(spherepointer.GetComponent<Rigidbody>());
                UnityEngine.Object.Destroy(spherepointer.GetComponent<Collider>());
                
                lr = GorillaTagger.Instance.offlineVRRig.rightHandTransform.position;
                AthrionGunLibrary athrionGunLibrary = spherepointer.AddComponent<AthrionGunLibrary>();
                athrionGunLibrary.StartCoroutine(PulsePointer(spherepointer));
                AddPointerParticles(spherepointer);
            }

            private static void CleanupPointer()
            {
                UnityEngine.Object.Destroy(spherepointer);
                spherepointer = null;
                LockedRigOrPlayerOrwhatever = null;
            }

            private static void HandleLineRendering()
            {
                lr = Vector3.Lerp(lr, (GorillaTagger.Instance.rightHandTransform.position + spherepointer.transform.position) / 2f, Time.deltaTime * 6f);
                
                GameObject lineObject = new GameObject("Line");
                LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
                lineRenderer.startWidth = GunConfig.LineWidth;
                lineRenderer.endWidth = GunConfig.LineWidth;
                
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null) lineRenderer.material = new Material(shader);
                
                AthrionGunLibrary athrionGunLibrary = lineObject.AddComponent<AthrionGunLibrary>();
                athrionGunLibrary.StartCoroutine(StartCurvyLineRenderer(lineRenderer, GorillaTagger.Instance.rightHandTransform.position, lr, spherepointer.transform.position, GunConfig.LineColorStart, GunConfig.LineColorEnd));
                athrionGunLibrary.StartCoroutine(AnimateLineGradient(lineRenderer, GunConfig.LineColorStart, GunConfig.LineColorEnd));
                
                UnityEngine.Object.Destroy(lineRenderer, Time.deltaTime);
            }

            public static void ToggleParticles()
            {
                GunConfig.EnableParticles = !GunConfig.EnableParticles;
                
                if (!GunConfig.EnableParticles && particleSystem != null && spherepointer != null)
                {
                    AthrionGunLibrary athrionGunLibrary = spherepointer.AddComponent<AthrionGunLibrary>();
                    athrionGunLibrary.StartCoroutine(SmoothStopParticles());
                }
            }

            public static void start2guns(Action action, bool lockOn)
            {
                if (IsXRDeviceActive()) StartVrGun(action, lockOn);
                else StartPcGun(action, lockOn);
            }

            public static bool IsXRDeviceActive()
            {
                List<XRDisplaySubsystem> list = new List<XRDisplaySubsystem>();
                SubsystemManager.GetInstances(list);
                
                foreach (XRDisplaySubsystem xrdisplaySubsystem in list)
                {
                    if (xrdisplaySubsystem.running) return true;
                }
                return false;
            }

            public static void ChangePointerColor()
            {
                Color32[] array = new Color32[]
                {
                    new Color32(0, 255, 100, 255),
                    new Color32(0, 200, 255, 255),
                    new Color32(255, 215, 0, 255),
                    new Color32(255, 165, 0, 255),
                    new Color32(128, 0, 128, 255),
                    new Color32(255, 0, 255, 255),
                    new Color32(0, 0, 128, 255),
                    new Color32(255, 69, 0, 255),
                    new Color32(50, 205, 50, 255),
                    new Color32(240, 128, 128, 255),
                    new Color32(173, 216, 230, 255),
                    new Color32(64, 224, 208, 255),
                    new Color32(255, 20, 147, 255),
                    new Color32(123, 104, 238, 255),
                    new Color32(255, 99, 71, 255),
                    new Color32(0, 191, 255, 255),
                    new Color32(255, 140, 0, 255),
                    new Color32(75, 0, 130, 255),
                    new Color32(60, 179, 113, 255),
                    new Color32(244, 164, 96, 255),
                    new Color32(138, 43, 226, 255),
                    new Color32(255, 105, 180, 255),
                    new Color32(255, 250, 205, 255),
                    new Color32(139, 0, 0, 255)
                };

                if (!colorIndexInitialized)
                {
                    ColorIndex = 0;
                    colorIndexInitialized = true;
                }

                ColorIndex = (ColorIndex + 1) % array.Length;
                Color32 color = array[ColorIndex];
                Color32 color2 = Color.Lerp(color, new Color32(255, 255, 255, 255), 0.5f);

                GunConfig.PointerColorStart = color;
                GunConfig.PointerColorEnd = color2;

                if (spherepointer != null) spherepointer.GetComponent<Renderer>().material.color = color;
                if (particleSystem != null)
                {
                    var main = particleSystem.main;
                    main.startColor = new ParticleSystem.MinMaxGradient(color, color2);
                }
            }

            private static IEnumerator AnimateBox(GameObject box, LineRenderer outline)
            {
                float duration = 0.05f;
                float elapsed = 0f;
                
                while (elapsed < duration)
                {
                    float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                    Color color = outline.startColor;
                    color.a = alpha;
                    outline.startColor = color;
                    outline.endColor = color;
                    
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            public static void BoxESP()
            {
                if (!(PhotonNetwork.InRoom || PhotonNetwork.InLobby)) return;
                if (LockedRigOrPlayerOrwhatever == null || LockedRigOrPlayerOrwhatever == GorillaTagger.Instance.offlineVRRig) return;

                GameObject boxObject = new GameObject();
                LineRenderer lineRenderer = boxObject.AddComponent<LineRenderer>();
                
                Vector3 position = LockedRigOrPlayerOrwhatever.transform.position;
                Vector3[] positions = new Vector3[5];
                float boxESPHeight = GunConfig.BoxESPHeight;
                float boxESPWidth = GunConfig.BoxESPWidth;

                positions[0] = position + LockedRigOrPlayerOrwhatever.transform.right * (-boxESPWidth / 2f) + LockedRigOrPlayerOrwhatever.transform.up * (boxESPHeight / 2f);
                positions[1] = position + LockedRigOrPlayerOrwhatever.transform.right * (boxESPWidth / 2f) + LockedRigOrPlayerOrwhatever.transform.up * (boxESPHeight / 2f);
                positions[2] = position + LockedRigOrPlayerOrwhatever.transform.right * (boxESPWidth / 2f) + LockedRigOrPlayerOrwhatever.transform.up * (-boxESPHeight / 2f);
                positions[3] = position + LockedRigOrPlayerOrwhatever.transform.right * (-boxESPWidth / 2f) + LockedRigOrPlayerOrwhatever.transform.up * (-boxESPHeight / 2f);
                positions[4] = positions[0];

                lineRenderer.positionCount = positions.Length;
                lineRenderer.SetPositions(positions);
                lineRenderer.startWidth = 0.007f;
                lineRenderer.endWidth = 0.007f;

                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null) lineRenderer.material = new Material(shader);
                
                lineRenderer.material.renderQueue = 3000;
                lineRenderer.startColor = GunConfig.BoxESPColor;
                lineRenderer.endColor = GunConfig.BoxESPColor;

                LineRenderer lineRenderer2 = new GameObject().AddComponent<LineRenderer>();
                lineRenderer2.transform.parent = boxObject.transform;

                Vector3[] positions2 = new Vector3[5];
                positions2[0] = position + LockedRigOrPlayerOrwhatever.transform.right * (-boxESPWidth / 2f - 0.02f) + LockedRigOrPlayerOrwhatever.transform.up * (boxESPHeight / 2f + 0.02f);
                positions2[1] = position + LockedRigOrPlayerOrwhatever.transform.right * (boxESPWidth / 2f + 0.02f) + LockedRigOrPlayerOrwhatever.transform.up * (boxESPHeight / 2f + 0.02f);
                positions2[2] = position + LockedRigOrPlayerOrwhatever.transform.right * (boxESPWidth / 2f + 0.02f) + LockedRigOrPlayerOrwhatever.transform.up * (-boxESPHeight / 2f - 0.02f);
                positions2[3] = position + LockedRigOrPlayerOrwhatever.transform.right * (-boxESPWidth / 2f - 0.02f) + LockedRigOrPlayerOrwhatever.transform.up * (-boxESPHeight / 2f - 0.02f);
                positions2[4] = positions2[0];

                lineRenderer2.positionCount = positions2.Length;
                lineRenderer2.SetPositions(positions2);
                lineRenderer2.startWidth = 0.01f;
                lineRenderer2.endWidth = 0.01f;

                Shader shader2 = Shader.Find("Sprites/Default");
                if (shader2 != null) lineRenderer2.material = new Material(shader2);
                
                lineRenderer2.material.renderQueue = 3000;
                lineRenderer2.startColor = GunConfig.BoxESPOuterColor;
                lineRenderer2.endColor = GunConfig.BoxESPOuterColor;

                boxObject.transform.position = LockedRigOrPlayerOrwhatever.transform.position;
                boxObject.transform.rotation = LockedRigOrPlayerOrwhatever.transform.rotation;

                AthrionGunLibrary athrionGunLibrary = boxObject.AddComponent<AthrionGunLibrary>();
                athrionGunLibrary.StartCoroutine(AnimateBox(boxObject, lineRenderer2));
                
                UnityEngine.Object.Destroy(boxObject, 0.05f);
            }

            public static void ToggleAnimationMode()
            {
                int length = Enum.GetValues(typeof(AnimationMode)).Length;
                currentAnimationMode = (AnimationMode)(((int)currentAnimationMode + 1) % length);
            }

            public static Vector3 GetPointerPos()
            {
                return spherepointer != null ? spherepointer.transform.position : Vector3.zero;
            }

            internal static void start2guns(Action value)
            {
                throw new NotImplementedException();
            }
            
public static float GunLineWidth = 0.03f;
public static float SphereSize = 0.2f;
public static string currentLineStyle = "Default";

public static void ChangeGunStyle(bool next)
{
    int currentIndex = (int)currentAnimationMode;
    int totalStyles = Enum.GetValues(typeof(AnimationMode)).Length;
    
    if (next)
        currentIndex = (currentIndex + 1) % totalStyles;
    else
        currentIndex = (currentIndex - 1 + totalStyles) % totalStyles;
    
    currentAnimationMode = (AnimationMode)currentIndex;
    currentLineStyle = currentAnimationMode.ToString();
    
    Debug.Log($"Gun style changed to: {currentLineStyle}");
}

public static void ChangeGunLineSize(bool increase)
{
    float step = 0.005f;
    
    if (increase)
        GunLineWidth += step;
    else
        GunLineWidth -= step;
    
    GunLineWidth = Mathf.Clamp(GunLineWidth, 0.001f, 0.1f);
    
    GunConfig.LineWidth = GunLineWidth;
    
    Debug.Log($"Line size changed to: {GunLineWidth}");
}

public static void ChangeGunSphereScale(bool increase)
{
    float step = 0.05f;
    
    if (increase)
        SphereSize += step;
    else
        SphereSize -= step;
    
    SphereSize = Mathf.Clamp(SphereSize, 0.05f, 0.5f);
    
    GunConfig.PointerScale = new Vector3(SphereSize, SphereSize, SphereSize);
    
    if (spherepointer != null)
        spherepointer.transform.localScale = GunConfig.PointerScale;
    
    Debug.Log($"Sphere size changed to: {SphereSize}");
}

public static void ResetGunDefaults()
{
    GunLineWidth = 0.03f;
    SphereSize = 0.2f;
    currentAnimationMode = AnimationMode.DefaultBezier;
    currentLineStyle = currentAnimationMode.ToString();
    
    GunConfig.LineWidth = GunLineWidth;
    GunConfig.PointerScale = new Vector3(SphereSize, SphereSize, SphereSize);
    
    if (spherepointer != null)
        spherepointer.transform.localScale = GunConfig.PointerScale;
    
    Debug.Log("Gun settings reset to defaults");
}

public static void ChangeWaveFrequency(bool increase)
{
    float step = 0.5f;
    
    if (increase)
        GunConfig.WaveFrequency += step;
    else
        GunConfig.WaveFrequency -= step;
    
    GunConfig.WaveFrequency = Mathf.Clamp(GunConfig.WaveFrequency, 1f, 20f);
}

public static void ChangeWaveAmplitude(bool increase)
{
    float step = 0.01f;
    
    if (increase)
        GunConfig.WaveAmplitude += step;
    else
        GunConfig.WaveAmplitude -= step;
    
    GunConfig.WaveAmplitude = Mathf.Clamp(GunConfig.WaveAmplitude, 0f, 0.2f);
}

public static void ToggleParticles2()
{
    GunConfig.EnableParticles = !GunConfig.EnableParticles;
    ToggleParticles();
}

public static void ToggleBoxESP()
{
    GunConfig.EnableBoxESP = !GunConfig.EnableBoxESP;
}
        }

        internal class GunLibData
        {
            internal bool isShooting;
            internal bool isTriggered;
            internal bool isLocked;
        }
    }
}