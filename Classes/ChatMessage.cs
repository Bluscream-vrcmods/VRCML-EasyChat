using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VRC.Core;
using UnityEngine;
using Newtonsoft.Json;

namespace EasyChat
{
    public class ChatMessage {
        private static readonly Regex sanitzeRegex = new Regex("<.*>", RegexOptions.Multiline);
        public string Tab { get; set; }
        public bool Visible { get; set; }
        public object Color { get; set; }
        public DateTime TimeStamp { get; set; }
        public string TimeStampString { get {
                var sb = new StringBuilder($"{TimeStamp.Hour.ToString("D2")}:{TimeStamp.Minute.ToString("D2")}");
                if (EasyChat.messages.Count < 1) { return sb.ToString(); }
                var timeDiff = DateTime.Now - EasyChat.messages.Last().TimeStamp;
                Utils.Log("Last Message was", timeDiff.ToJson()); // Todo: Remove
                if (timeDiff.TotalMinutes < 1) {
                    sb.Append($":{TimeStamp.Second.ToString("D2")}");
                }
                if (timeDiff.TotalSeconds < 1) {
                    sb.Append($":{TimeStamp.Millisecond.ToString("D2")}");
                }
                return sb.ToString();
            }
        }
        public string Content { get; set; }
        [JsonIgnore]
        public string SafeContent { get { return Sanitize(Content); } }
        public object Sender { get; set; }
        public string SenderName { get; set; }
        public bool ShouldBeSanitized { get; set; }
        public string String { get; set; }
        public ChatMessageAuthor Author { get; set; }
        // public ChatMessageContent Content { get; set; } // TODO: Implement
        public ChatMessage(string content, bool visible = true, bool sanitize = false, object sender = null) =>
            new ChatMessage(content: content, timestamp: DateTime.Now, visible: visible, sanitize: sanitize, sender: sender);
        public ChatMessage(string content, DateTime timestamp, bool visible = true, bool sanitize = false, object sender = null)
        {
            TimeStamp = timestamp;
            Content = content;
            Sender = sender;
            if (sender is APIUser apiUser) {
                if (!string.IsNullOrEmpty(apiUser.displayName)) SenderName = apiUser.displayName;
            } else if (sender is VRCModLoader.VRCModInfoAttribute modInfo) {
                if (!string.IsNullOrEmpty(modInfo.Name)) SenderName = modInfo.Name;
            } else if (sender is String senderStr) { SenderName = senderStr;
            } else { try { SenderName = sender.ToString(); } catch(Exception) {} }
            Visible = visible;
            ShouldBeSanitized = sanitize;
            // String = ToMessageString(this);
            // Color = color;
        }
        public string ToMessageString() {
            var msg = new StringBuilder();
            msg.Append($"[{TimeStampString}] ");
            if (!string.IsNullOrEmpty(SenderName)) msg.Append(SenderName + ": ");
            if (ShouldBeSanitized) { msg.Append(SafeContent);
            } else { msg.Append(Content); }
            // Console.WriteLine($"Message : {Utils.JsonConverter.Serialize(this)}");
            // Console.WriteLine($"MSG: {Utils.JsonConverter.Serialize(msg)}");
            return msg.ToString();
        }
        public static string Sanitize(string message)
        {
            return sanitzeRegex.Replace(message, "");
        }
    }
}
