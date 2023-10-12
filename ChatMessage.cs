using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProgrammingP12
{
    public class ChatMessage
    {
        public String Login { get; set; }
        public String Text { get; set; }
        public DateTime Moment { get; set; }
        public override string ToString()
        {
            return $"{Login}: {Text}";
        }
    }
}
