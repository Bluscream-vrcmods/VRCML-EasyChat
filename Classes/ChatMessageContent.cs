using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using VRC.Core;

namespace EasyChat
{
    class ChatMessageContent
    {
        public string[] Texts { get; set; }
        public UnityEngine.UI.Image[] Images { get; set; }
        public APIUser[] Mentions { get; set; }
        public ChatMessageContent(string[] texts, Image[] images, APIUser[] mentions) {
            Texts = texts; Images = images; Mentions = mentions;
        }
    }
}
