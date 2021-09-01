using System;
using System.Collections.Generic;
using System.Text;

namespace Taller.Common.Models
{
    public class Employed
    {
        public int IdEmployed { get; set; }
        public DateTime InputOutput { get; set; }
        public int Type { get; set; }
        public bool Consolidated { get; set; }
    }
}
