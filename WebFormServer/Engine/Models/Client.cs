using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebFormServer.Engine.Models;

namespace DLPSystem.WebFormServer.Engine.Models
{
    public class Client
    {
        public string Name { get; set; }
        public string IpHost { get; set; }
        public List<Module> Modules { get; set; }
    }
}