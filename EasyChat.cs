using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !UNITY_EDITOR
using VRC.Core;
using VRCModLoader;
using VRCTools;
#endif

namespace EasyChat
{
#if UNITY_EDITOR
    public class EasyChat : MonoBehaviour
#else
    [VRCModInfo("EasyChat", "1.0", "Slaynash")]
    public class EasyChat : VRCMod
#endif
    {
        public static int maxMessagesCounts = 40;
        public static float hideAfterSeconds = 10;

        public static event Action<string> OnMessage;
        public static event Action<string> OnCommand;

        private Rect combinedRect;
        private float screenHeight = 0;
        private GUIStyle chatStyle;
        private GUIStyle chatStyleActive;
        private GUIStyle textStyle;
        private GUIStyle fieldInputStyle;
        private GUIStyle scrollviewStyle;
        private GUIStyle scrollviewStyleActive;

        private bool allowEnterKey;
        private string chatInputField = "";

        private List<string> messages = new List<string>();

        private System.Diagnostics.Stopwatch messageTimer = new System.Diagnostics.Stopwatch();
        private float lastMessageTime = 0;

#if UNITY_EDITOR
        public void Start()
        {
            OnApplicationStart();
        }
#endif

        void OnApplicationStart()
        {
            messageTimer.Start();
        }

        public void OnGUI()
        {
            if (messageTimer.ElapsedMilliseconds * 0.001f - lastMessageTime > hideAfterSeconds)
                return;
            if (Event.current.type == EventType.Layout)
            {
                if (screenHeight != Screen.height)
                {
                    if (screenHeight == 0)
                    {
                        // Init
                        chatStyle = new GUIStyle(GUI.skin.box);
                        chatStyle.normal.background = CreateTexture(0.1f, 0.9f, 1f, 0.2f);
                        chatStyleActive = new GUIStyle(GUI.skin.box);
                        chatStyleActive.normal.background = CreateTexture(0.1f, 0.9f, 1f, 0.6f);

                        scrollviewStyle = new GUIStyle(GUI.skin.scrollView);
                        scrollviewStyle.normal.background = CreateTexture(0.1f, 0.9f, 1f, 0.2f);
                        scrollviewStyleActive = new GUIStyle(GUI.skin.scrollView);
                        scrollviewStyleActive.normal.background = CreateTexture(0.1f, 0.9f, 1f, 0.6f);

                        fieldInputStyle = new GUIStyle(GUI.skin.textField);
                        fieldInputStyle.normal.background = null;
                        fieldInputStyle.focused.background = null;
                        fieldInputStyle.hover.background = null;
                        fieldInputStyle.padding.top = 1;
                        fieldInputStyle.normal.textColor = Color.black;
                        fieldInputStyle.focused.textColor = Color.black;
                        fieldInputStyle.hover.textColor = Color.black;

                        textStyle = new GUIStyle(GUI.skin.label);
                        textStyle.normal.textColor = Color.black;
                        textStyle.focused.textColor = Color.black;
                        textStyle.hover.textColor = Color.black;
                        textStyle.wordWrap = true;
                    }
                    screenHeight = Screen.height;
                    combinedRect = new Rect(5, screenHeight - 310 - 5, 460, 310);
                }

                GUI.Window(0, combinedRect, DrawWindow, "", chatStyle);

                if (allowEnterKey && (chatInputField.Trim().Length > 0) && IsPressingEnter())
                {
                    string message = chatInputField.Trim();

                    chatInputField = "";
                    allowEnterKey = false;
                    GUIUtility.keyboardControl = 0;

                    if (message.StartsWith("/"))
                    {
                        Debug.Log("OnCommand " + message);
                        OnCommand?.Invoke(message.Substring(1));
                    }
                    else
                    {
                        Debug.Log("OnMessage " + message);
#if UNITY_EDITOR
                        HandleMessage(message, DateTime.Now, "Me");
#else
                        HandleMessage(message, DateTime.Now, APIUser.CurrentUser);
#endif
                        OnMessage?.Invoke(message);
                    }
                }
                else
                    allowEnterKey = GUI.GetNameOfFocusedControl() == "chatInputField";

                if (allowEnterKey && Event.current.keyCode == KeyCode.Escape)
                    GUIUtility.keyboardControl = 0;
            }

            if (Event.current.type == EventType.Repaint)
            {
#if !UNITY_EDITOR
                if (!VRCUiManagerUtils.GetVRCUiManager().IsActive() && !Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T))
#else
                if (Input.GetKeyDown(KeyCode.T))
#endif
                    GUI.FocusControl("chatInputField");
            }
        }

#if UNITY_EDITOR
        public void HandleMessage(string message, DateTime time, string sender)
        {
#else
        public void HandleMessage(string message, DateTime time, APIUser senderApiUser)
        {
            string sender = senderApiUser?.displayName;
#endif
            if (sender != null)
                sender += ":";
            else
                sender = "";
            lock (messages)
            {
                if (messages.Count > maxMessagesCounts)
                    messages.RemoveAt(0);
            }
            messages.Add($"[{time.Hour.ToString("D2")}:{time.Minute.ToString("D2")}] {sender} {message}");
            lastMessageTime = messageTimer.ElapsedMilliseconds * 0.001f;
        }







        private bool IsPressingEnter()
        {
            KeyCode kc = Event.current.keyCode;
            return kc == KeyCode.Return || kc == KeyCode.KeypadEnter;
        }

        private Texture2D CreateTexture(float r, float g, float b, float a)
        {
            Texture2D texture = new Texture2D(4, 4, TextureFormat.ARGB32, false);
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    texture.SetPixel(i, j, new Color(r, g, b, a));
            texture.Apply();
            return texture;
        }

        private void DrawWindow(int id)
        {
            bool focused = GUI.GetNameOfFocusedControl() == "chatInputField";

            GUILayout.BeginScrollView(Vector2.zero, focused ? scrollviewStyleActive : scrollviewStyle, GUILayout.Width(450), GUILayout.Height(265));
            lock (messages)
            {
                foreach (string message in messages)
                    GUILayout.Label(message, textStyle);
            }
            GUILayout.EndScrollView();

            GUILayout.Space(4f);

            GUILayout.BeginVertical(focused ? chatStyleActive : chatStyle, GUILayout.Width(450), GUILayout.Height(25));
            GUI.SetNextControlName("chatInputField");
            chatInputField = GUILayout.TextField(chatInputField, fieldInputStyle, GUILayout.Width(440));
            GUILayout.EndVertical();
        }
    }

}
