using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ComradeMajor.Database
{
    public class User
    {
        public ulong Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Discriminator { get; set; }

        public User(ulong id, string username, string discriminator)
        {
            Id = id;
            Username = username;
            Discriminator = discriminator;
        }

        virtual public List<UserAction>? Actions { get; set; }
    }
}