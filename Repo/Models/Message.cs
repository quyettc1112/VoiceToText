using System;
using System.Collections.Generic;

namespace VoiceToText_Repo.Models
{
    public partial class Message
    {
        public int MessageId { get; set; }
        public int? ConversationId { get; set; }
        public string Text { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? SenderType { get; set; }
        public string SenderBy { get; set; }

        public virtual Conversation Conversation { get; set; }
    }
}
