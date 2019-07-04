using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
    [VRCModInfo("EasyChat", "1.0.1", "Slaynash, Bluscream", "")]
    public class EasyChat : VRCMod
#endif
    {
        private static VRCModInfoAttribute ModInfo;
        private static bool Initialized = false;

        public static event Action<ChatMessage> OnMessage;
        public static event Action<ChatCommand> OnCommand;

        private const string prefSection = "easychat";

        private Rect combinedRect;
        private float screenHeight = 0;
        private GUIStyle chatStyle;
        private GUIStyle chatStyleActive;
        // private GUIStyle textStyle;
        private GUIStyle fieldInputStyle;
        private GUIStyle scrollviewStyle;
        private GUIStyle scrollviewStyleActive;

        private bool allowEnterKey;
        private string chatInputField = "";

        public static List<ChatMessage> messages = new List<ChatMessage>();

        private static System.Diagnostics.Stopwatch messageTimer = new System.Diagnostics.Stopwatch();
        private static float lastMessageTime = 0;

#if UNITY_EDITOR
        public void Start() { OnApplicationStart(); }
#endif

        void OnApplicationStart()
        {
            ModInfo = Attribute.GetCustomAttribute(typeof(EasyChat), typeof(VRCModInfoAttribute)) as VRCModInfoAttribute;
            VRCTools.ModPrefs.RegisterCategory(prefSection, "EasyChat");
            VRCTools.ModPrefs.RegisterPrefInt(prefSection, "maxmessages", 50, "Max Messages");
            VRCTools.ModPrefs.RegisterPrefInt(prefSection, "hideafter", 30, "Hide after (seconds)");
            VRCTools.ModPrefs.RegisterPrefString(prefSection, "cmdprefix", "/", "Command Prefix");
            messageTimer.Start();
        }

        private bool MessagTimerExceeded() {
            return (messageTimer.ElapsedMilliseconds * 0.001f - lastMessageTime) > VRCTools.ModPrefs.GetInt(prefSection, "hideafter");
        }

        public void OnGUI() {
            if (Input.GetKeyDown(KeyCode.Return) || allowEnterKey)
            {
                lastMessageTime = messageTimer.ElapsedMilliseconds * 0.001f;
            }

            if (RoomManager.currentRoom != null && MessagTimerExceeded())
            {
                return;
            }

            if (Event.current.type == EventType.Layout)
            {
                if (screenHeight != Screen.height)
                {
                    RefreshLayout();
                }
                CheckForKeys(Event.current);
            }

            RenderChat();

            if (Event.current.type == EventType.Repaint)
            {
#if !UNITY_EDITOR
                if (!VRCUiManagerUtils.GetVRCUiManager().IsActive() && ChatHotkeyPressed()) {
#else
                if (ChatHotkeyPressed()) {
#endif
                    GUI.FocusControl("chatInputField");
                }
            }
        }

        private void RefreshLayout()
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
            }
            screenHeight = Screen.height;
            combinedRect = new Rect(5, screenHeight - 310 - 5, 460, 310);
        }

        public void RenderChat()
        {
            GUI.Window(0, combinedRect, DrawWindow, "", chatStyle);
            if (!Initialized) {
                Initialized = true;
                var msg = new ChatMessage(content: $"{ModInfo.Name} v{ModInfo.Version} initialized!", timestamp: DateTime.Now); // msg.Color = Color.gray;
                HandleMessage(msg);
                Utils.Log(ModInfo.ToJson());
            }
        }

        private void CheckForKeys(Event _event)
        {
            if (allowEnterKey && (chatInputField.Trim().Length > 0) && EnterPressed(_event)) {
                string message = chatInputField.Trim();

                chatInputField = "";
                allowEnterKey = false;
                GUIUtility.keyboardControl = 0;

                if (message.StartsWith(VRCTools.ModPrefs.GetString(prefSection, "cmdprefix")))
                {
                    Utils.Log("OnCommand " + message);
                    OnCommand?.Invoke(new ChatCommand(message));
                }
                else
                {
                    Utils.Log("OnMessage " + message);
                    object Sender;
                    if (APIUser.CurrentUser is null) {
                        Sender = "You"; 
                    } else { Sender = APIUser.CurrentUser; }
                    var msg = new ChatMessage(content: message, timestamp: DateTime.Now, sender: Sender);
#if UNITY_EDITOR
                    HandleMessage(msg, DateTime.Now, "Me");
#else
                    HandleMessage(msg);
#endif
                    OnMessage?.Invoke(msg);
                }
            } else {
                allowEnterKey = GUI.GetNameOfFocusedControl() == "chatInputField";
            }

            if (allowEnterKey && Event.current.keyCode == KeyCode.Escape)
                GUIUtility.keyboardControl = 0;
        }

        /*public void HandleMessage(string message, DateTime time, APIUser senderApiUser = null, bool sanitize = false)
        {
            var sender = (senderApiUser != null) ? senderApiUser.displayName += ": " : "";
            HandleMessage(new ChatMessage(message, time, sender, sanitize: sanitize));
        }*/

        public static void HandleMessage(ChatMessage message)
        {
            if (!Initialized) return;
            if (message is null || string.IsNullOrEmpty(message.Content)) return;
            lock (messages)
            {
                if (messages.Count > VRCTools.ModPrefs.GetInt(prefSection, "maxmessages"))
                    messages.RemoveAt(0);
            }
            message.String = message.ToMessageString();
            Utils.Log("New Message:" + message.ToJson()); // Utils.JsonConverter.Serialize(
            messages.Add(message);
            lastMessageTime = messageTimer.ElapsedMilliseconds * 0.001f;
        }

        private bool ChatHotkeyPressed()
        {
            return !Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T);
        }

        private bool EnterPressed(Event _event = null)
        {
            if (_event is null) _event = Event.current;
            KeyCode kc = _event.keyCode;
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
                foreach (var message in messages)
                {
                    var textStyle = new GUIStyle(GUI.skin.label);
                    if (message.Color is UnityEngine.Color color) {
                        textStyle.normal.textColor = textStyle.focused.textColor = textStyle.hover.textColor = color;
                    } else {
                        textStyle.normal.textColor = textStyle.focused.textColor = textStyle.hover.textColor = Color.black;
                    }
                    textStyle.wordWrap = true;
                    GUILayout.Label(message.String, textStyle);
                    // Console.WriteLine(message.String);
                }
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
