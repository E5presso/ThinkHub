using System;
using System.Collections.Generic;

namespace ThinkHub.Models
{
    public partial class User
    {
        public User()
        {
            Access = new HashSet<Access>();
            Authority = new HashSet<Authority>();
            Permission = new HashSet<Permission>();
            Shared = new HashSet<Shared>();
        }

        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string HashSalt { get; set; }
        public DateTime RegistrationDate { get; set; }

        public virtual Profile Profile { get; set; }
        public virtual ICollection<Access> Access { get; set; }
        public virtual ICollection<Authority> Authority { get; set; }
        public virtual ICollection<Permission> Permission { get; set; }
        public virtual ICollection<Shared> Shared { get; set; }
    }
}
