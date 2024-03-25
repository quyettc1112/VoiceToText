using System;
using System.Collections.Generic;

namespace VoiceToText_Repo.Models
{
    public partial class User
    {
        public User()
        {
            Conversations = new HashSet<Conversation>();
        }

        public int UserId { get; set; }
        public string Username { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual ICollection<Conversation> Conversations { get; set; }
    }
}
