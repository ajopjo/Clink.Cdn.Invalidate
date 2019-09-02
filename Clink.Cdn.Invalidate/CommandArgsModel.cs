using System;
using System.Collections.Generic;
using System.Text;

namespace Clink.Cdn.Invalidate
{
    public class CommandArgsModel
    {
        public string NetworkId { get; set; }
        public string[] Paths { get; set; }
        public string Email { get; set; }
        public bool UseProxy { get; set; } = true;
    }
}
