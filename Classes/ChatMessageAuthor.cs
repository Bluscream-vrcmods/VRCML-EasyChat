using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRC.Core;

namespace EasyChat
{
    public class ChatMessageAuthor
    {
        public ChatMessageAuthorType Type { get; set; }
        public string Name { get; set; }
        public APIUser APIUser { get; set; }
        public ChatMessageAuthor(ChatMessageAuthorType type, string name, APIUser apiuser) {
            Type = type; Name = name; APIUser = apiuser;
        }
    }
    public enum ChatMessageAuthorType {
        UNKNOWN,
        STRING,
        APIUSER,
        SELF
    }
    public enum Rank
    {
        UNKNOWN,
        VRC_Admin, VRC_Mod, VRC_Supporter,
        VRCT_Admin, VRCT_Mod, VRCT_Supporter, VRC_Legend,
        VRC_Veteran, VRC_Trusted, VRC_Known, VRC_User, VRC_Visitor
    }
    public class ChatMessageAuthorRank
    {
        public Rank Rank { get; set; }
        public ChatMessageAuthorRank(Rank rank) { Rank = rank; }
        public static ChatMessageAuthorRank fromAPIUser(APIUser user)
        {
            if (user.tags.Contains("admin_moderator")) {
                return new ChatMessageAuthorRank(Rank.VRC_Mod);
            } else if (user.tags.Contains("system_trust_legend")) {
                return new ChatMessageAuthorRank(Rank.VRC_Legend);
            } else if (user.tags.Contains("system_trust_veteran")) {
                return new ChatMessageAuthorRank(Rank.VRC_Veteran);
            } else if (user.tags.Contains("system_trust_trusted")) {
                return new ChatMessageAuthorRank(Rank.VRC_Trusted);
            } else if (user.tags.Contains("system_trust_known")) {
                return new ChatMessageAuthorRank(Rank.VRC_Known);
            } else if (user.tags.Contains("system_trust_basic")) {
                return new ChatMessageAuthorRank(Rank.VRC_User);
            }/*else if (user.tags.Contains("system_trust_visitor")) {
                return new ChatMessageAuthorRank(Rank.VRC_Visitor);
            }*/
            return new ChatMessageAuthorRank(Rank.UNKNOWN);
        }
    }
}
