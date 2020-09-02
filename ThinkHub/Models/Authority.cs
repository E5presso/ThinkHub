using System;
using System.Collections.Generic;

namespace ThinkHub.Models
{
    public partial class Authority
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }

        public virtual Role Role { get; set; }
        public virtual User User { get; set; }
    }
}
