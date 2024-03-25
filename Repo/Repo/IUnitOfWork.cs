using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceToText_Repo.Models;

namespace VoiceToText_Repo.Repo
{
    public interface IUnitOfWork
    {
        IGenericRepository<User> UserRepostiory { get; }
        IGenericRepository<Message> MessageRepostiory { get; }
        IGenericRepository<Conversation> ConversationRepostiory { get; }

        void SaveChanges();

    }
}
