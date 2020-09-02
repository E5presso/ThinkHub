using System;
using System.Collections.Generic;

namespace ThinkHub.Models
{
    public partial class Access
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SharedId { get; set; }
        public string Path { get; set; }

        public virtual Shared Shared { get; set; }
        public virtual User User { get; set; }
    }
}
