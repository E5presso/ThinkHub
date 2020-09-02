using System;
using System.Collections.Generic;

namespace ThinkHub.Models
{
    public partial class Permission
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SharedId { get; set; }
        public int Create { get; set; }
        public int Read { get; set; }
        public int Write { get; set; }
        public int Delete { get; set; }

        public virtual Shared Shared { get; set; }
        public virtual User User { get; set; }
    }
}
