using System;
using System.Collections.Generic;

namespace ThinkHub.Models
{
    public partial class Shared
    {
        public Shared()
        {
            Access = new HashSet<Access>();
            Permission = new HashSet<Permission>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public int Type { get; set; }
        public string Path { get; set; }
        public string Code { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Access> Access { get; set; }
        public virtual ICollection<Permission> Permission { get; set; }
    }
}
