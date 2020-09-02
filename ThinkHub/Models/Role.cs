using System;
using System.Collections.Generic;

namespace ThinkHub.Models
{
    public partial class Role
    {
        public Role()
        {
            Authority = new HashSet<Authority>();
            ResourceMap = new HashSet<ResourceMap>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Authority> Authority { get; set; }
        public virtual ICollection<ResourceMap> ResourceMap { get; set; }
    }
}
