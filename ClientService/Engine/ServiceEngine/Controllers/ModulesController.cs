using DLPSystem.ClientService.ServiceEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLPSystem.ClientService.ServiceEngine.Controllers
{
    public static class ModulesController
    {
        public static List<Process> ExecuteModules(string pathToFolder, List<Module> modules)
        {
            List<Process> processes = new List<Process>();

            foreach(var module in modules)
            {
                processes.Add(Process.Start(Path.Combine(pathToFolder, module.Name)));
            }

            return processes;
        }

        public static void InstallModule(string pathToFolder, Module module)
        {
            File.WriteAllBytes(Path.Combine(pathToFolder, module.Name), module.Data);
        }
    }
}
