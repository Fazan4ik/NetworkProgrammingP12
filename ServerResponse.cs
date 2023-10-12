using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProgrammingP12
{
    public class ServerResponse
    {
        public String Status { get; set; }
        public IEnumerable<ChatMessage>? Messages { get; set; }
    }
}
