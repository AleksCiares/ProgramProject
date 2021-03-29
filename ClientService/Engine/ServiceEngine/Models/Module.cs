using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientService.Model
{
    public class Module
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Version { get; set; }
        public bool AutoStart { get; set; }
    }
}
