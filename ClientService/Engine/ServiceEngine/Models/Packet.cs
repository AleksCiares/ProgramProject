using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientService.Model
{
    public class Packet
    {
        public string Task { get; set; }
        public byte[] Data { get; set; }
    }
}
