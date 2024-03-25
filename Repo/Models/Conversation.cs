using System;
using System.Collections.Generic;

namespace VoiceToText_Repo.Models
{
    public partial class Conversation
    {
        public Conversation()
        {
            Messages = new HashSet<Message>();
        }

        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
    }
}
