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
        public UnityEngine.Color Color { get; set; }
        public DateTime TimeStamp { get; set; }
        public string TimeStampString { get { return $"{TimeStamp.Hour.ToString("D2")}:{TimeStamp.Minute.ToString("D2")}:{TimeStamp.Second.ToString("D2")}";} }
        public string Content { get; set; }
        [JsonIgnore]
        public string SafeContent { get { return Sanitize(Content); } }
        public APIUser APIUser { get; set; }
        public string Sender { get; set; }
        public bool ShouldBeSanitized { get; private set; }
        public string String { get; set; }
        public ChatMessage(string content, string sender = "", APIUser apiuser = null, bool visible = true, bool sanitize = false) => new ChatMessage(content, DateTime.Now, sender, apiuser, visible, sanitize);
        public ChatMessage(string content, DateTime timestamp, string sender = "", APIUser apiuser = null, bool visible = true, bool sanitize = false)
        {
            TimeStamp = timestamp;
            Content = content;
            if (string.IsNullOrEmpty(sender))
            {
                if (apiuser != null) Sender = apiuser.displayName;
            } else Sender = sender;
            APIUser = apiuser;
            Visible = visible;
            ShouldBeSanitized = sanitize;
            String = ToString();
        }
        public new string ToString()
        {
            var msg = new StringBuilder();
            msg.Append($"[{this.TimeStampString}] ");
            if (!string.IsNullOrEmpty(this.Sender)) msg.Append(this.Sender + ": ");
            if (this.ShouldBeSanitized) msg.Append(this.SafeContent);
            else msg.Append(this.Content);
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
