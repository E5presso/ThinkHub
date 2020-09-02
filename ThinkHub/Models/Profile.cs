using System;
using System.Collections.Generic;

namespace ThinkHub.Models
{
    public partial class Profile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] Image { get; set; }
        public string Phone { get; set; }
        public DateTime? Birthday { get; set; }

        public virtual User IdNavigation { get; set; }
    }
}
