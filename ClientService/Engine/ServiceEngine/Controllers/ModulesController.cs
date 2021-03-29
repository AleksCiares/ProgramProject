using ClientService.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientService.Controllers
{
    public static class ModulesController
    {
        public static List<Process> ExecuteModules(List<Module> modules)
        {
            List<Process> processes = new List<Process>();

            foreach(var module in modules)
            {
                processes.Add(Process.Start(module.Path));
            }

            return processes;
        }
    }
}
