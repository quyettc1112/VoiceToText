using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceToText_Repo.Models;

namespace VoiceToText_Repo.Repo
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly VoiceToTextContext _context;

        private IGenericRepository<User> userRepostiory;
        private IGenericRepository<Message> messageRepostiory;
        private IGenericRepository<Conversation> conversationRepostiory;

        public UnitOfWork(VoiceToTextContext context)

        {
            _context = context;
        }


        public IGenericRepository<User> UserRepostiory
        {
            get
            {
                if (userRepostiory == null) userRepostiory = new GenericRepository<User>(_context);
                return userRepostiory;
            }

        }

        public IGenericRepository<Message> MessageRepostiory
        {

            get
            {
                if (messageRepostiory == null) messageRepostiory = new GenericRepository<Message>(_context);
                return messageRepostiory;
            }


        }

        public IGenericRepository<Conversation> ConversationRepostiory
        {
            get
            {
                if (conversationRepostiory == null) conversationRepostiory = new GenericRepository<Conversation>(_context);
                return conversationRepostiory;
            }

        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
