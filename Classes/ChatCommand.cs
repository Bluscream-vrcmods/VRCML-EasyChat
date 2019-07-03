using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRC.Core;

namespace EasyChat
{
    public class ChatCommand
    {
        public string Prefix { get; set; }
        public string Command { get; set; }
        public APIUser Author { get; set; }
        // public string CommandNoPrefix { get; set; }
        public string Message { get; set; }
        public List<string> Arguments { get; set; }
        public ChatCommand(string message)
        {
            Prefix = message.Substring(0, 1);
            Message = message.Substring(1);
            var msg_split = Message.Split(new[] { ' ' }, 2);
            Command = msg_split[0].ToLowerInvariant();
            Arguments = new List<string>();
            if (msg_split.Length > 1) {
                Arguments.AddRange(msg_split[1].Split(new[] { ' ' }));
            }
            if (APIUser.CurrentUser != null) { Author = APIUser.CurrentUser; }
        }
    }
}
