using System;
using System.Collections.Generic;

namespace ThinkHub.Models
{
    public partial class ResourceMap
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int ResourceId { get; set; }

        public virtual Resource Resource { get; set; }
        public virtual Role Role { get; set; }
    }
}
