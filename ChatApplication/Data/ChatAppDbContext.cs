using ChatApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApplication.Data
{
    public class ChatAppDbContext : DbContext
    {
        public ChatAppDbContext(DbContextOptions options): base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ChatMappings> ChatMappings { get; set; }
        //public DbSet<BlackListedTokens> BlackListedTokens { get;set; }

    }
}
