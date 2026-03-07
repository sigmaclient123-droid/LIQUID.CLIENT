using BepInEx;
using g3;
using GorillaExtensions;
using GorillaLocomotion;
using HarmonyLib;
using Pathfinding;
using PlayFab.DataModels;
using liquidclient.Classes;
using liquidclient.Notifications;
using liquidclient.Patches;
using liquidclient.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Console;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.VR.InteractionSystem;
using static liquidclient.Menu.Buttons;
using static liquidclient.Settings;
using Object = UnityEngine.Object;

/*
 * Hello, current and future developers!
 * This is ii's Stupid Template, a base mod menu template for Gorilla Tag.
 * 
 * Comments are placed around the code showing you how certain classes work, such as the settings, buttons, and notifications.
 * 
 * If you need help with the template, you may join my Discord server: https://discord.gg/iidk
 * It's full of talented developers that can show you the way and how things work.
 * 
 * If you want to support my, check out my Patreon: https://patreon.com/iiDk
 * Any support is appreciated, and it helps me make more free content for you all!
 * 
 * Thank you, and enjoy the template!
 */

namespace liquidclient.Menu
{
    [HarmonyPatch(typeof(GTPlayer), "LateUpdate")]
    public class Main : MonoBehaviour
    {
        private static bool hasInitGUI = false;
        
        // Constant
        public static void Prefix()
        {
            //ScreenGUI.InitAndUpdate();
            BoardManager.CreateCustomBoards();
            
            try
            {
                GameObject shoulderCam = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
                if (shoulderCam != null)
                {
                    TPC = shoulderCam.GetComponent<Camera>();
                }
            }
            catch { }
            
            if (!hasInitGUI)
            {
                if (GameObject.Find("OnScreenGUIManager") == null)
                {
                    GameObject guiLogic = new GameObject("OnScreenGUIManager");
                    guiLogic.AddComponent<OnScreenGUI>();
                    Object.DontDestroyOnLoad(guiLogic);
                }
                hasInitGUI = true;
                
                ThemeManager.ApplyColors();
            }
                // Initialize Menu
                try
                {
                    bool toOpen = (!rightHanded && ControllerInputPoller.instance.leftControllerSecondaryButton) || (rightHanded && ControllerInputPoller.instance.rightControllerSecondaryButton);
                    bool keyboardOpen = UnityInput.Current.GetKey(keyboardButton);

                    if (menu == null)
                    {
                        if (toOpen || keyboardOpen)
                        {
                            CreateMenu();
                            //RecenterMenu(rightHanded);
                            RecenterMenu(rightHanded, keyboardOpen);
                            
                            MenuAudioHandler.PlayOpeningSound();
                            if (reference == null)
                                CreateReference(rightHanded);
                        }
                    }
                    else
                    {
                        if (toOpen || keyboardOpen)
                            //RecenterMenu(rightHanded);
                            RecenterMenu(rightHanded, keyboardOpen);
                        else
                        {
                            if (TPC != null) TPC.gameObject.SetActive(true);
                            //GameObject.Find("Shoulder Camera").transform.Find("CM vcam1").gameObject.SetActive(true);

                            MenuAudioHandler.PlayClosesound();
                            Rigidbody comp = menu.AddComponent(typeof(Rigidbody)) as Rigidbody;
                            comp.linearVelocity = (rightHanded ? GTPlayer.Instance.LeftHand.velocityTracker : GTPlayer.Instance.RightHand.velocityTracker).GetAverageVelocity(true, 0);

                            UnityEngine.Object.Destroy(menu, 2f);
                            menu = null;

                            UnityEngine.Object.Destroy(reference);
                            reference = null;
                            
                            
                        }
                    }
                }
                catch (Exception exc)
                {
                    Debug.LogError(string.Format("{0} // Error initializing at {1}: {2}", PluginInfo.Name, exc.StackTrace, exc.Message));
                }

                // Constant
                try
                {
                    // Pre-Execution
                    if (fpsObject != null)
                        fpsObject.text = "FPS: " + Mathf.Ceil(1f / Time.unscaledDeltaTime).ToString();

                    // Execute Enabled Mods
                    foreach (ButtonInfo button in buttons
                        .SelectMany(list => list)
                        .Where(button => button.enabled && button.method != null))
                    {
                        try
                        {
                            button.method.Invoke();
                        }
                        catch (Exception exc)
                        {
                            Debug.LogError(string.Format("{0} // Error with mod {1} at {2}: {3}", PluginInfo.Name, button.buttonText, exc.StackTrace, exc.Message));
                        }
                    }
                }
                catch (Exception exc)
                {
                    Debug.LogError(string.Format("{0} // Error with executing mods at {1}: {2}", PluginInfo.Name, exc.StackTrace, exc.Message));
                }
            }


        /*public static void OnLaunch()
        {
            //GameObject ConsoleObject = Console.Console.LoadConsoleImmediately();
            
        }*/


        private static readonly Dictionary<string, GameObject> objectPool = new Dictionary<string, GameObject>();
        
        public static GameObject GetObject(string find)
        {
            if (objectPool.TryGetValue(find, out GameObject go))
                return go;

            GameObject tgo = GameObject.Find(find);
            if (tgo != null)
                objectPool.Add(find, tgo);

            return tgo;
        }

        // Functions
        public static void CreateMenu()
        {

            // Menu Holder
            menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(menu.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(menu.GetComponent<BoxCollider>());
            UnityEngine.Object.Destroy(menu.GetComponent<Renderer>());
            menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.3825f);

            // Menu Background
            menuBackground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(menuBackground.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(menuBackground.GetComponent<BoxCollider>());
            menuBackground.transform.parent = menu.transform;
            menuBackground.transform.rotation = Quaternion.identity;
            menuBackground.transform.localScale = menuSize;
            menuBackground.GetComponent<Renderer>().material.color = backgroundColor.colors[0].color;
            menuBackground.transform.position = new Vector3(0.05f, 0f, 0f);
            RoundObj(menuBackground, 0.03f);

            // menu outline!

            GameObject MenuOutline = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(MenuOutline.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(MenuOutline.GetComponent<BoxCollider>());
            MenuOutline.transform.parent = menu.transform;
            MenuOutline.transform.rotation = Quaternion.identity;
            MenuOutline.transform.localScale = new Vector3(0.05f, 1.1f, 1.1f);
            MenuOutline.transform.position = new Vector3(0.05f, 0f, 0f);
            MenuOutline.GetComponent<Renderer>().material.color = ColorLib.DarkBlue;
            //MenuOutline.GetComponent<Renderer>().material.color = ColorLib.SkyBlue;
            RoundObj(MenuOutline, 0.03f);

            // Canvas
            canvasObject = new GameObject();
            canvasObject.transform.parent = menu.transform;
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasScaler.dynamicPixelsPerUnit = 1000f; // higher cause it looks better btw

            // Title and FPS
            Text text = new GameObject
            {
                transform =
                    {
                        parent = canvasObject.transform
                    }
            }.AddComponent<Text>();
            text.font = currentFont;
            text.text = PluginInfo.Name;
            text.fontSize = 1;
            text.color = textColors[0];
            text.supportRichText = true;
            text.fontStyle = FontStyle.Italic;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            RectTransform component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(0.28f, 0.05f);
            component.position = new Vector3(0.06f, 0f, 0.185f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            if (fpsCounter)
            {
                fpsObject = new GameObject
                {
                    transform =
                        {
                            parent = canvasObject.transform
                        }
                }.AddComponent<Text>();
                fpsObject.font = currentFont;
                fpsObject.text = currentCategory + "|" + pageNumber + "FPS: " + Mathf.Ceil(1f / Time.unscaledDeltaTime).ToString();
                fpsObject.color = textColors[0];
                fpsObject.fontSize = 1;
                fpsObject.supportRichText = true;
                fpsObject.fontStyle = FontStyle.Italic;
                fpsObject.alignment = TextAnchor.MiddleCenter;
                fpsObject.horizontalOverflow = HorizontalWrapMode.Overflow;
                fpsObject.resizeTextForBestFit = true;
                fpsObject.resizeTextMinSize = 0;
                RectTransform component2 = fpsObject.GetComponent<RectTransform>();
                component2.localPosition = Vector3.zero;
                component2.sizeDelta = new Vector2(0.28f, 0.02f);
                component2.position = new Vector3(0.06f, 0f, 0.140f);
                component2.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }

            // Buttons
            // Disconnect
            if (disconnectButton)
            {
                GameObject disconnectbutton = GameObject.CreatePrimitive(PrimitiveType.Cube);
                if (!UnityInput.Current.GetKey(KeyCode.Q))
                {
                    disconnectbutton.layer = 2;
                }
                UnityEngine.Object.Destroy(disconnectbutton.GetComponent<Rigidbody>());
                disconnectbutton.GetComponent<BoxCollider>().isTrigger = true;
                disconnectbutton.transform.parent = menu.transform;
                disconnectbutton.transform.rotation = Quaternion.identity;
                disconnectbutton.transform.localScale = new Vector3(0.09f, 0.9f, 0.08f);
                disconnectbutton.transform.localPosition = new Vector3(0.56f, 0f, 0.62f);
                disconnectbutton.GetComponent<Renderer>().material.color = buttonColors[0].colors[0].color;
                disconnectbutton.AddComponent<Classes.Button>().relatedText = "Disconnect";
                RoundObj(disconnectbutton, 0.03f);

                GameObject disconnectbuttonOutline = GameObject.CreatePrimitive(PrimitiveType.Cube);
                if (!UnityInput.Current.GetKey(keyboardButton))
                {
                    disconnectbutton.layer = 2;
                }
                UnityEngine.Object.Destroy(disconnectbuttonOutline.GetComponent<Rigidbody>());
                disconnectbuttonOutline.GetComponent<BoxCollider>().isTrigger = true;
                disconnectbuttonOutline.transform.parent = menu.transform;
                disconnectbuttonOutline.transform.rotation = Quaternion.identity;
                disconnectbuttonOutline.transform.localScale = new Vector3(0.089f, 0.91f, 0.091f);
                disconnectbuttonOutline.transform.localPosition = new Vector3(0.56f, 0f, 0.62f);
                disconnectbuttonOutline.GetComponent<Renderer>().material.color = ColorLib.DarkBlue;
                RoundObj(disconnectbuttonOutline, 0.03f);


                Text discontext = new GameObject
                {
                    transform =
            {
                parent = canvasObject.transform
            }
                }.AddComponent<Text>();
                discontext.text = "Disconect";
                discontext.font = currentFont;
                discontext.fontSize = 1;
                discontext.color = textColors[0];
                discontext.alignment = TextAnchor.MiddleCenter;
                discontext.resizeTextForBestFit = true;
                discontext.resizeTextMinSize = 0;

                RectTransform rectt = discontext.GetComponent<RectTransform>();
                rectt.localPosition = Vector3.zero;
                rectt.sizeDelta = new Vector2(0.1f, 0.03f);
                rectt.localPosition = new Vector3(0.064f, 0f, 0.238f);
                rectt.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }

            // Home Button

            /*if (homeButton)
            {
                GameObject homebutton = GameObject.CreatePrimitive(PrimitiveType.Cube);
                if (!UnityInput.Current.GetKey(KeyCode.Q))
                {
                    homebutton.layer = 2;
                }
                UnityEngine.Object.Destroy(homebutton.GetComponent<Rigidbody>());
                homebutton.GetComponent<BoxCollider>().isTrigger = true;
                homebutton.transform.parent = menu.transform;
                homebutton.transform.rotation = Quaternion.identity;
                homebutton.transform.localScale = new Vector3(0.09f, 0.250f, 0.08f);
                homebutton.transform.localPosition = new Vector3(0.56f, 0f, -0.433f);
                homebutton.GetComponent<Renderer>().material.color = buttonColors[0].colors[0].color;
                homebutton.AddComponent<Classes.Button>().relatedText = "Home";
                RoundObj(homebutton);


                Text hometext = new GameObject
                {
                    transform =
            {
                parent = canvasObject.transform
            }
                }.AddComponent<Text>();
                hometext.text = "⌂";
                hometext.font = currentFont;
                hometext.fontSize = 1;
                hometext.color = textColors[0];
                hometext.alignment = TextAnchor.MiddleCenter;
                hometext.resizeTextForBestFit = true;
                hometext.resizeTextMinSize = 0;

                RectTransform rectt = hometext.GetComponent<RectTransform>();
                rectt.localPosition = Vector3.zero;
                rectt.sizeDelta = new Vector2(0.1f, 0.03f);
                rectt.localPosition = new Vector3(0.065f, 0f, -0.157f);
                rectt.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }*/
            
            if (homeButton && currentCategory != 0)
            {
                GameObject homebutton = GameObject.CreatePrimitive(PrimitiveType.Cube);
                if (!UnityInput.Current.GetKey(KeyCode.Q))
                {
                    homebutton.layer = 2;
                }
                UnityEngine.Object.Destroy(homebutton.GetComponent<Rigidbody>());
                homebutton.GetComponent<BoxCollider>().isTrigger = true;
                homebutton.transform.parent = menu.transform;
                homebutton.transform.rotation = Quaternion.identity;
                homebutton.transform.localScale = new Vector3(0.09f, 0.250f, 0.08f);
                homebutton.transform.localPosition = new Vector3(0.56f, 0f, -0.433f);
                homebutton.GetComponent<Renderer>().material.color = buttonColors[0].colors[0].color;
                homebutton.AddComponent<Classes.Button>().relatedText = "Home";
                RoundObj(homebutton);

                Text hometext = new GameObject
                {
                    transform =
                    {
                        parent = canvasObject.transform
                    }
                }.AddComponent<Text>();
                hometext.text = "⌂";
                hometext.font = currentFont;
                hometext.fontSize = 1;
                hometext.color = textColors[0];
                hometext.alignment = TextAnchor.MiddleCenter;
                hometext.resizeTextForBestFit = true;
                hometext.resizeTextMinSize = 0;

                RectTransform rectt = hometext.GetComponent<RectTransform>();
                rectt.localPosition = Vector3.zero;
                rectt.sizeDelta = new Vector2(0.1f, 0.03f);
                rectt.localPosition = new Vector3(0.065f, 0f, -0.157f);
                rectt.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }

            if (settingsButton)
            {
                GameObject settingsbutton = GameObject.CreatePrimitive(PrimitiveType.Cube);
                if (!UnityInput.Current.GetKey(KeyCode.Q))
                {
                    settingsbutton.layer = 2;
                }
                UnityEngine.Object.Destroy(settingsbutton.GetComponent<Rigidbody>());
                settingsbutton.GetComponent<BoxCollider>().isTrigger = true;
                settingsbutton.transform.parent = menu.transform;
                settingsbutton.transform.rotation = Quaternion.identity;
                settingsbutton.transform.localScale = new Vector3(0.1f, 0.1f, 0.08f);
                settingsbutton.transform.localPosition = new Vector3(0.56f, -0.600f, -0.500f);
                settingsbutton.GetComponent<Renderer>().material.color = backgroundColor.colors[0].color;
                settingsbutton.AddComponent<Classes.Button>().relatedText = "Settings";
                RoundObj(settingsbutton);


                GameObject settingsbuttonOutline = GameObject.CreatePrimitive(PrimitiveType.Cube);
                if (!UnityInput.Current.GetKey(keyboardButton))
                {
                    settingsbutton.layer = 2;
                }
                UnityEngine.Object.Destroy(settingsbuttonOutline.GetComponent<Rigidbody>());
                settingsbuttonOutline.GetComponent<BoxCollider>().isTrigger = true;
                settingsbuttonOutline.transform.parent = menu.transform;
                settingsbuttonOutline.transform.rotation = Quaternion.identity;
                settingsbuttonOutline.transform.localScale = new Vector3(0.056f, 0.12f, 0.09f);
                settingsbuttonOutline.transform.localPosition = new Vector3(0.56f, -0.6025f, -0.502f);
                settingsbuttonOutline.GetComponent<Renderer>().material.color = ColorLib.DarkBlue;
                RoundObj(settingsbuttonOutline);

                Text settingstext = new GameObject
                {
                    transform =
            {
                parent = canvasObject.transform
            }
                }.AddComponent<Text>();
                settingstext.text = "⋮";
                settingstext.font = currentFont;
                settingstext.fontSize = 1;
                settingstext.color = ColorLib.White;
                settingstext.alignment = TextAnchor.MiddleCenter;
                settingstext.resizeTextForBestFit = true;
                settingstext.resizeTextMinSize = 0;

                RectTransform rectt = settingstext.GetComponent<RectTransform>();
                rectt.localPosition = Vector3.zero;
                rectt.sizeDelta = new Vector2(0.05f, 0.020f);
                rectt.localPosition = new Vector3(0.064f, -0.180f, -0.190f);
                rectt.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }


            // Page Buttons

            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (!UnityInput.Current.GetKey(KeyCode.Q))
            {
                gameObject.layer = 2;
            }
            UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.GetComponent<BoxCollider>().isTrigger = true;
            gameObject.transform.parent = menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(0.06f, 0.250f, 0.08f);
            gameObject.transform.localPosition = new Vector3(0.56f, 0.295f, -0.433f);
            gameObject.GetComponent<Renderer>().material.color = buttonColors[0].colors[0].color;
            gameObject.AddComponent<Classes.Button>().relatedText = "PreviousPage";
            RoundObj(gameObject);


            text = new GameObject
            {
                transform =
        {
            parent = canvasObject.transform
        }
            }.AddComponent<Text>();
            text.font = currentFont;
            text.text = "<";
            text.fontSize = 1;
            text.color = textColors[0];
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(0.2f, 0.03f);
            component.localPosition = new Vector3(0.06f, 0.090f, -0.160f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (!UnityInput.Current.GetKey(KeyCode.Q))
            {
                gameObject.layer = 2;
            }
            UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.GetComponent<BoxCollider>().isTrigger = true;
            gameObject.transform.parent = menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(0.06f, 0.250f, 0.08f);
            gameObject.transform.localPosition = new Vector3(0.56f, -0.295f, -0.433f);
            gameObject.GetComponent<Renderer>().material.color = buttonColors[0].colors[0].color;
            gameObject.AddComponent<Classes.Button>().relatedText = "NextPage";
            RoundObj(gameObject);


            text = new GameObject
            {
                transform =
        {
            parent = canvasObject.transform
        }
            }.AddComponent<Text>();
            text.font = currentFont;
            text.text = ">";
            text.fontSize = 1;
            text.color = textColors[0];
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(0.2f, 0.03f);
            component.localPosition = new Vector3(0.06f, -0.090f, -0.160f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            List<ButtonInfo> listToDraw = (currentCategory == 4) ? GetActiveMods() : buttons[currentCategory].ToList();
            ButtonInfo[] activeButtons = listToDraw.Skip(pageNumber * buttonsPerPage).Take(buttonsPerPage).ToArray();
            for (int i = 0; i < activeButtons.Length; i++)
                CreateButton(i * 0.1f, activeButtons[i]);
        }

        public static void CreateButton(float offset, ButtonInfo method)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (!UnityInput.Current.GetKey(KeyCode.Q))
            {
                gameObject.layer = 2;
            }
            UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.GetComponent<BoxCollider>().isTrigger = true;
            gameObject.transform.parent = menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(0.09f, 0.8f, 0.08f);
            gameObject.transform.localPosition = new Vector3(0.56f, 0f, 0.28f - offset);
            gameObject.GetComponent<Renderer>().material.color = buttonColors[method.enabled ? 1 : 0].colors[0].color;
            gameObject.AddComponent<Classes.Button>().relatedText = method.buttonText;
            RoundObj(gameObject);



            Text text = new GameObject
            {
                transform =
                {
                    parent = canvasObject.transform
                }
            }.AddComponent<Text>();
            if (method.overlapText != null)
                text.text = method.overlapText;
            text.font = currentFont;
            text.text = method.buttonText;
            text.supportRichText = true;
            text.fontSize = 1;
            if (method.enabled)
            {
                text.color = textColors[1];
            }
            else
            {
                text.color = textColors[0];
            }
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Italic;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            RectTransform component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(.2f, .03f);
            component.localPosition = new Vector3(.064f, 0, 0.111f - offset / 2.7f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }


        public static void RecreateMenu()
        {
            if (menu != null)
            {
                UnityEngine.Object.Destroy(menu);
                menu = null;

                CreateMenu();
                RecenterMenu(rightHanded, UnityInput.Current.GetKey(keyboardButton));
                //RecenterMenu(rightHanded);
            }
        }

        /*public static void RecenterMenu(bool isRightHanded)
        {
            if (!isRightHanded)
            {
                menu.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                menu.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
            }
            else
            {
                menu.transform.position = GorillaTagger.Instance.rightHandTransform.position;

                Vector3 rotation = GorillaTagger.Instance.rightHandTransform.rotation.eulerAngles;
                rotation += new Vector3(0f, 0f, 180f);

                menu.transform.rotation = Quaternion.Euler(rotation);
            }
        }*/
        
        public static void RecenterMenu(bool isRightHanded, bool isKeyboardCondition)
        {
            if (!isKeyboardCondition)
            {
                if (!isRightHanded)
                {
                    menu.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                    menu.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                }
                else
                {
                    menu.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    Vector3 rotation = GorillaTagger.Instance.rightHandTransform.rotation.eulerAngles;
                    rotation += new Vector3(0f, 0f, 180f);
                    menu.transform.rotation = Quaternion.Euler(rotation);
                }
            }
            else
            {
                try
                {
                    TPC = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera").GetComponent<Camera>();
                }
                catch { }

                GameObject.Find("Shoulder Camera").transform.Find("CM vcam1").gameObject.SetActive(false);

                if (TPC != null)
                {
                    TPC.transform.position = new Vector3(-999f, -999f, -999f);
                    TPC.transform.rotation = Quaternion.identity;
                    GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bg.transform.localScale = new Vector3(10f, 10f, 0.01f);
                    bg.transform.transform.position = TPC.transform.position + TPC.transform.forward;
                    Color realcolor = backgroundColor.GetCurrentColor();
                    bg.GetComponent<Renderer>().material.color = new Color32((byte)(realcolor.r * 50), (byte)(realcolor.g * 50), (byte)(realcolor.b * 50), 255);
                    UnityEngine.Object.Destroy(bg, 0.05f);
                    menu.transform.parent = TPC.transform;
                    menu.transform.position = TPC.transform.position + (TPC.transform.forward * 0.52f) + (TPC.transform.up * -0.02f);
                    menu.transform.rotation = TPC.transform.rotation * Quaternion.Euler(-90f, 90f, 0f);

                    if (reference != null)
                    {
                        if (Mouse.current.leftButton.isPressed)
                        {
                            Ray ray = TPC.ScreenPointToRay(Mouse.current.position.ReadValue());
                            bool hitButton = Physics.Raycast(ray, out RaycastHit hit, 100);
                            if (hitButton)
                            {
                                Classes.Button collide = hit.transform.gameObject.GetComponent<Classes.Button>();
                                collide?.OnTriggerEnter(buttonCollider);
                            }
                        }
                        else
                            reference.transform.position = new Vector3(999f, -999f, -999f);
                    }
                }
            }
        }

        public static void CreateReference(bool isRightHanded)
        {
            reference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            reference.transform.parent = isRightHanded ? GorillaTagger.Instance.leftHandTransform : GorillaTagger.Instance.rightHandTransform;
            reference.GetComponent<Renderer>().material.color = backgroundColor.colors[0].color;
            reference.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            reference.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            buttonCollider = reference.GetComponent<SphereCollider>();

            ColorChanger colorChanger = reference.AddComponent<ColorChanger>();
            colorChanger.colors = backgroundColor;
        }

        public static void GreenModMenuPreset()
        {
            backgroundColor = new ExtGradient
            {
                colors = ExtGradient.GetSolidGradient(ColorLib.DarkSeaGreen)
            };

            buttonColors = new ExtGradient[]
            {
                  new ExtGradient { colors = ExtGradient.GetSolidGradient(ColorLib.DarkGreen) }, // Disabled
                  new ExtGradient { colors = ExtGradient.GetSolidGradient(ColorLib.Green) } // Enabled
            };

            textColors = new Color[]
            {
               ColorLib.White,   // Disabled
               ColorLib.White       // Enabled
            };
        }
        
        public static void RedModMenuPreset()
        {

            backgroundColor = new ExtGradient
            {
                colors = ExtGradient.GetSolidGradient(ColorLib.Red)
            };

            buttonColors = new ExtGradient[]
            {
        new ExtGradient { colors = ExtGradient.GetSolidGradient(ColorLib.DarkRed) },
        new ExtGradient {colors = ExtGradient.GetSolidGradient(ColorLib.WineRed) }
            };

            textColors = new Color[]
            {
        ColorLib.White,
        ColorLib.White
            };
        }
        
        public static void MainPreset()
        {

            backgroundColor = new ExtGradient
            {
                colors = ExtGradient.GetSolidGradient(ColorLib.PurpleWithTinyAmountsofblue)
            };

            // or
            // Theme.background = new ExtGradient { ... };
            // CustomThemes.currentBackground = new ExtGradient { ... };
            // MenuPatches.bgGradient = new ExtGradient { ... };

            buttonColors = new ExtGradient[]
            {
        new ExtGradient { colors = ExtGradient.GetSolidGradient(ColorLib.DarkRoyalPurple) },
        new ExtGradient { colors = ExtGradient.GetSolidGradient(ColorLib.VeryDarkPurple) }
            };

            textColors = new Color[]
            {
        ColorLib.White,
        ColorLib.White
            };
        }

        public static void Toggle(string buttonText)
        {
            int lastPage = ((buttons[currentCategory].Length + buttonsPerPage - 1) / buttonsPerPage) - 1;
            
            if (buttonText == "Disconnect")
            {
                if (PhotonNetwork.InRoom)
                {
                    PhotonNetwork.Disconnect();
                }
                return;
            }

            if (buttonText == "Home")
            {
                Main.currentCategory = 0;
            }
            
            if (buttonText == "Settings")
            {
                Main.currentCategory = 1;
            }
            
            if (buttonText == "PreviousPage")
            {
                pageNumber--;
                if (pageNumber < 0)
                    pageNumber = lastPage;
            }
            else
            {
                if (buttonText == "NextPage")
                {
                    pageNumber++;
                    if (pageNumber > lastPage)
                        pageNumber = 0;
                }
                else
                {
                    ButtonInfo target = GetIndex(buttonText);
                    if (target != null)
                    {
                        if (target.isTogglable)
                        {
                            target.enabled = !target.enabled;
                            if (target.enabled)
                            {
                                NotifiLib.SendNotification("<color=grey>[</color><color=blue>ENABLED</color><color=grey>]</color> " + target.toolTip);
                                if (target.enableMethod != null)
                                    try { target.enableMethod.Invoke(); } catch { }
                            }
                            else
                            {
                                NotifiLib.SendNotification("<color=grey>[</color><color=blue>DISABLED</color><color=grey>]</color> " + target.toolTip);
                                if (target.disableMethod != null)
                                    try { target.disableMethod.Invoke(); } catch { }
                            }
                        }
                        else
                        {
                            NotifiLib.SendNotification("<color=grey>[</color><color=blue>SUCCESS</color><color=grey>]</color> " + target.toolTip);
                            if (target.method != null)
                                try { target.method.Invoke(); } catch { }
                        }

                    }
                }
            }
            RecreateMenu();
        }

        private static readonly Dictionary<string, (int Category, int Index)> cacheGetIndex = new Dictionary<string, (int Category, int Index)>(); // Looping through 800 elements is not a light task :/
        public static ButtonInfo GetIndex(string buttonText)
        {
            if (buttonText == null)
                return null;

            if (cacheGetIndex.ContainsKey(buttonText))
            {
                var CacheData = cacheGetIndex[buttonText];
                try
                {
                    if (buttons[CacheData.Category][CacheData.Index].buttonText == buttonText)
                        return buttons[CacheData.Category][CacheData.Index];
                }
                catch { cacheGetIndex.Remove(buttonText); }
            }

            int categoryIndex = 0;
            foreach (ButtonInfo[] buttons in buttons)
            {
                int buttonIndex = 0;
                foreach (ButtonInfo button in buttons)
                {
                    if (button.buttonText == buttonText)
                    {
                        try
                        {
                            cacheGetIndex.Add(buttonText, (categoryIndex, buttonIndex));
                        }
                        catch
                        {
                            if (cacheGetIndex.ContainsKey(buttonText))
                                cacheGetIndex.Remove(buttonText);
                        }

                        return button;
                    }
                    buttonIndex++;
                }
                categoryIndex++;
            }

            return null;
        }

        public static Vector3 RandomVector3(float range = 1f) =>
            new Vector3(UnityEngine.Random.Range(-range, range),
                        UnityEngine.Random.Range(-range, range),
                        UnityEngine.Random.Range(-range, range));

        public static Quaternion RandomQuaternion(float range = 360f) =>
            Quaternion.Euler(UnityEngine.Random.Range(0f, range),
                        UnityEngine.Random.Range(0f, range),
                        UnityEngine.Random.Range(0f, range));

        public static Color RandomColor(byte range = 255, byte alpha = 255) =>
            new Color32((byte)UnityEngine.Random.Range(0, range),
                        (byte)UnityEngine.Random.Range(0, range),
                        (byte)UnityEngine.Random.Range(0, range),
                        alpha);

        public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueLeftHand()
        {
            Quaternion rot = GorillaTagger.Instance.leftHandTransform.rotation * GTPlayer.Instance.LeftHand.handRotOffset;
            return (GorillaTagger.Instance.leftHandTransform.position + GorillaTagger.Instance.leftHandTransform.rotation * GTPlayer.Instance.LeftHand.handOffset, rot, rot * Vector3.up, rot * Vector3.forward, rot * Vector3.right);
        }

        

        public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueRightHand()
        {
            Quaternion rot = GorillaTagger.Instance.rightHandTransform.rotation * GTPlayer.Instance.RightHand.handRotOffset;
            return (GorillaTagger.Instance.rightHandTransform.position + GorillaTagger.Instance.rightHandTransform.rotation * GTPlayer.Instance.RightHand.handOffset, rot, rot * Vector3.up, rot * Vector3.forward, rot * Vector3.right);
        }

        public static void WorldScale(GameObject obj, Vector3 targetWorldScale)
        {
            Vector3 parentScale = obj.transform.parent.lossyScale;
            obj.transform.localScale = new Vector3(
                targetWorldScale.x / parentScale.x,
                targetWorldScale.y / parentScale.y,
                targetWorldScale.z / parentScale.z
            );
        }

        public static void FixStickyColliders(GameObject platform)
        {
            Vector3[] localPositions = new Vector3[]
            {
                new Vector3(0, 1f, 0),
                new Vector3(0, -1f, 0),
                new Vector3(1f, 0, 0),
                new Vector3(-1f, 0, 0),
                new Vector3(0, 0, 1f),
                new Vector3(0, 0, -1f)
            };
            Quaternion[] localRotations = new Quaternion[]
            {
                Quaternion.Euler(90, 0, 0),
                Quaternion.Euler(-90, 0, 0),
                Quaternion.Euler(0, -90, 0),
                Quaternion.Euler(0, 90, 0),
                Quaternion.identity,
                Quaternion.Euler(0, 180, 0)
            };
            for (int i = 0; i < localPositions.Length; i++)
            {
                GameObject side = GameObject.CreatePrimitive(PrimitiveType.Cube);
                try
                {
                    if (platform.GetComponent<GorillaSurfaceOverride>() != null)
                    {
                        side.AddComponent<GorillaSurfaceOverride>().overrideIndex = platform.GetComponent<GorillaSurfaceOverride>().overrideIndex;
                    }
                }
                catch { }
                float size = 0.025f;
                side.transform.SetParent(platform.transform);
                side.transform.position = localPositions[i] * (size / 2);
                side.transform.rotation = localRotations[i];
                WorldScale(side, new Vector3(size, size, 0.01f));
                side.GetComponent<Renderer>().enabled = false;
            }
        }

        private static int? noInvisLayerMask;
        public static int NoInvisLayerMask()
        {
            noInvisLayerMask ??= ~(
                1 << LayerMask.NameToLayer("TransparentFX") |
                1 << LayerMask.NameToLayer("Ignore Raycast") |
                1 << LayerMask.NameToLayer("Zone") |
                1 << LayerMask.NameToLayer("Gorilla Trigger") |
                1 << LayerMask.NameToLayer("Gorilla Boundary") |
                1 << LayerMask.NameToLayer("GorillaCosmetics") |
                1 << LayerMask.NameToLayer("GorillaParticle"));

            return noInvisLayerMask ?? GTPlayer.Instance.locomotionEnabledLayers;
        }

        public static bool gunLocked;
        public static VRRig lockTarget;

        public static (RaycastHit Ray, GameObject NewPointer) RenderGun(int? overrideLayerMask = null)
        {
            Transform GunTransform = GorillaTagger.Instance.rightHandTransform;

            Vector3 StartPosition = GunTransform.position;
            Vector3 Direction = GunTransform.forward;

            Physics.Raycast(StartPosition + Direction / 4f, Direction, out var Ray, 512f, overrideLayerMask ?? NoInvisLayerMask());
            Vector3 EndPosition = gunLocked ? lockTarget.transform.position : Ray.point;

            if (EndPosition == Vector3.zero)
                EndPosition = StartPosition + Direction * 512f;

            if (GunPointer == null)
                GunPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            GunPointer.SetActive(true);
            GunPointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            GunPointer.transform.position = EndPosition;

            Renderer PointerRenderer = GunPointer.GetComponent<Renderer>();
            PointerRenderer.material.shader = Shader.Find("GUI/Text Shader");
            PointerRenderer.material.color = gunLocked || ControllerInputPoller.TriggerFloat(XRNode.RightHand) > 0.5f ? buttonColors[1].GetCurrentColor() : buttonColors[0].GetCurrentColor();

            UnityEngine.Object.Destroy(GunPointer.GetComponent<Collider>());

            if (GunLine == null)
            {
                GameObject line = new GameObject("iiMenu_GunLine");
                GunLine = line.AddComponent<LineRenderer>();
            }

            GunLine.gameObject.SetActive(true);
            GunLine.material.shader = Shader.Find("GUI/Text Shader");
            GunLine.startColor = backgroundColor.GetCurrentColor();
            GunLine.endColor = backgroundColor.GetCurrentColor(0.5f);
            GunLine.startWidth = 0.025f;
            GunLine.endWidth = 0.025f;
            GunLine.positionCount = 2;
            GunLine.useWorldSpace = true;

            GunLine.SetPosition(0, StartPosition);
            GunLine.SetPosition(1, EndPosition);

            return (Ray, GunPointer);
        }

        public static void RoundObj(GameObject toRound, float bevel = 0.025f, string sides = "both")
        {
            Renderer ToRoundRenderer = toRound.GetComponent<Renderer>();

            GameObject BaseA = GameObject.CreatePrimitive(PrimitiveType.Cube);
            BaseA.GetComponent<Renderer>().enabled = ToRoundRenderer.enabled;
            UnityEngine.Object.Destroy(BaseA.GetComponent<Collider>());
            BaseA.transform.parent = menu.transform;
            BaseA.transform.rotation = Quaternion.identity;
            BaseA.transform.localPosition = toRound.transform.localPosition;
            BaseA.transform.localScale = toRound.transform.localScale + new Vector3(0f, bevel * -2.55f, 0f);

            GameObject BaseB = GameObject.CreatePrimitive(PrimitiveType.Cube);
            BaseB.GetComponent<Renderer>().enabled = ToRoundRenderer.enabled;
            UnityEngine.Object.Destroy(BaseB.GetComponent<Collider>());
            BaseB.transform.parent = menu.transform;
            BaseB.transform.rotation = Quaternion.identity;
            BaseB.transform.localPosition = toRound.transform.localPosition;
            BaseB.transform.localScale = toRound.transform.localScale + new Vector3(0f, 0f, -bevel * 2f);

            List<GameObject> ToChange = new List<GameObject> { BaseA, BaseB };

            if (sides == "both" || sides == "bottom" || sides == "left")
            {
                GameObject RoundCornerD = CreateRoundedCorner(toRound, ToRoundRenderer, bevel,
                    new Vector3(0f, -(toRound.transform.localScale.y / 2f) + (bevel * 1.275f), -(toRound.transform.localScale.z / 2f) + bevel));

                GameObject RoundCornerB = CreateRoundedCorner(toRound, ToRoundRenderer, bevel,
                    new Vector3(0f, -(toRound.transform.localScale.y / 2f) + (bevel * 1.275f), (toRound.transform.localScale.z / 2f) - bevel));

                ToChange.Add(RoundCornerD);
                ToChange.Add(RoundCornerB);
            }

            if (sides == "both" || sides == "top" || sides == "right")
            {
                GameObject RoundCornerC = CreateRoundedCorner(toRound, ToRoundRenderer, bevel,
                    new Vector3(0f, (toRound.transform.localScale.y / 2f) - (bevel * 1.275f), -(toRound.transform.localScale.z / 2f) + bevel));

                GameObject RoundCornerA = CreateRoundedCorner(toRound, ToRoundRenderer, bevel,
                    new Vector3(0f, (toRound.transform.localScale.y / 2f) - (bevel * 1.275f), (toRound.transform.localScale.z / 2f) - bevel));

                ToChange.Add(RoundCornerC);
                ToChange.Add(RoundCornerA);
            }

            foreach (GameObject Changed in ToChange)
            {
                ColorRoundObj TargetChanger = Changed.AddComponent<ColorRoundObj>();
                TargetChanger.targetRenderer = ToRoundRenderer;
                TargetChanger.Start();
            }

            ToRoundRenderer.enabled = false;
        }



        private static GameObject CreateRoundedCorner(GameObject parent, Renderer renderer, float bevel, Vector3 localPositionOffset)
        {

            GameObject corner = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            corner.GetComponent<Renderer>().enabled = renderer.enabled;
            UnityEngine.Object.Destroy(corner.GetComponent<Collider>());

            corner.transform.parent = menu.transform;
            corner.transform.rotation = Quaternion.identity * Quaternion.Euler(0f, 0f, 90f);
            corner.transform.localPosition = parent.transform.localPosition + localPositionOffset;
            corner.transform.localScale = new Vector3(bevel * 2.55f, parent.transform.localScale.x / 2f, bevel * 2f);

            return corner;
        }



        public static void RoundLeftObj(GameObject toRound)
        {
            float Bevel = 0.1f;

            Renderer ToRoundRenderer = toRound.GetComponent<Renderer>();
            GameObject BaseA = GameObject.CreatePrimitive(PrimitiveType.Cube);
            BaseA.GetComponent<Renderer>().enabled = ToRoundRenderer.enabled;
            UnityEngine.Object.Destroy(BaseA.GetComponent<Collider>());

            BaseA.transform.parent = menu.transform;
            BaseA.transform.rotation = Quaternion.identity;
            BaseA.transform.localPosition = toRound.transform.localPosition;
            BaseA.transform.localScale = toRound.transform.localScale;

            GameObject BaseB = GameObject.CreatePrimitive(PrimitiveType.Cube);
            BaseB.GetComponent<Renderer>().enabled = ToRoundRenderer.enabled;
            UnityEngine.Object.Destroy(BaseB.GetComponent<Collider>());

            BaseB.transform.parent = menu.transform;
            BaseB.transform.rotation = Quaternion.identity;
            BaseB.transform.localPosition = toRound.transform.localPosition;
            BaseB.transform.localScale = toRound.transform.localScale + new Vector3(0f, 0f, -Bevel * 2f);

            GameObject RoundCornerA = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            RoundCornerA.GetComponent<Renderer>().enabled = ToRoundRenderer.enabled;
            UnityEngine.Object.Destroy(RoundCornerA.GetComponent<Collider>());

            RoundCornerA.transform.parent = menu.transform;
            RoundCornerA.transform.rotation = Quaternion.identity * Quaternion.Euler(0f, 0f, 90f);

            RoundCornerA.transform.localPosition = toRound.transform.localPosition + new Vector3(0f, (toRound.transform.localScale.y / 2f) - (Bevel * 1.275f), -(toRound.transform.localScale.z / 2f) + Bevel);
            RoundCornerA.transform.localScale = new Vector3(Bevel * 2.55f, toRound.transform.localScale.x / 2f, Bevel * 2f);

            GameObject RoundCornerB = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            RoundCornerB.GetComponent<Renderer>().enabled = ToRoundRenderer.enabled;
            UnityEngine.Object.Destroy(RoundCornerB.GetComponent<Collider>());

            RoundCornerB.transform.parent = menu.transform;
            RoundCornerB.transform.rotation = Quaternion.identity * Quaternion.Euler(0f, 0f, 90f);

            RoundCornerB.transform.localPosition = toRound.transform.localPosition + new Vector3(0f, -(toRound.transform.localScale.y / 2f) + (Bevel * 1.275f), -(toRound.transform.localScale.z / 2f) + Bevel);
            RoundCornerB.transform.localScale = new Vector3(Bevel * 2.55f, toRound.transform.localScale.x / 2f, Bevel * 2f);

            GameObject[] ToChange = new GameObject[]
            {
        BaseA,
        BaseB,
        RoundCornerA,
        RoundCornerB
            };

            foreach (GameObject Changed in ToChange)
            {
                ColorRoundObj TargetChanger = Changed.AddComponent<ColorRoundObj>();
                TargetChanger.targetRenderer = ToRoundRenderer;

                TargetChanger.Start();
            }

            ToRoundRenderer.enabled = false;
        }
        
        public static readonly Dictionary<Type, object[]> typePool = new Dictionary<Type, object[]>();
        private static readonly Dictionary<Type, float> receiveTypeDelay = new Dictionary<Type, float>();
        
        public static T[] GetAllType<T>(float decayTime = 5f) where T : Object
        {
            Type type = typeof(T);

            float lastReceivedTime = receiveTypeDelay.GetValueOrDefault(type, -1f);

            if (Time.time > lastReceivedTime)
            {
                typePool.Remove(type);
                receiveTypeDelay[type] = Time.time + decayTime;
            }

            if (!typePool.ContainsKey(type))
                typePool.Add(type, FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None));

            return (T[])typePool[type];
        }

        private static float randomIndex;
        private static float randomDecayTime;


        // Variables
        // Important
        // Objects
        public static GameObject menu;
        public static GameObject menuBackground;
        public static GameObject reference;
        public static GameObject canvasObject;
        public static GameObject ColorRoundObj;

        public static SphereCollider buttonCollider;
        public static Camera TPC;
        public static Text fpsObject;

        private static GameObject GunPointer;
        private static LineRenderer GunLine;
        
        public static string CosmeticsOwned;

        // Data
        public static int pageNumber = 0;
        public static int _currentCategory;
        public static int currentCategory
        {
            get => _currentCategory;
            set
            {
                _currentCategory = value;
                pageNumber = 0;
            }
        }
    }
}