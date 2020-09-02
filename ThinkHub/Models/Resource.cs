using System;
using System.Collections.Generic;

namespace ThinkHub.Models
{
    public partial class Resource
    {
        public Resource()
        {
            ResourceMap = new HashSet<ResourceMap>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Target { get; set; }

        public virtual ICollection<ResourceMap> ResourceMap { get; set; }
    }
}
