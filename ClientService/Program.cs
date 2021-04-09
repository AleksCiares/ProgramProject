using DLPSystem.ClientService.ClientEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace DLPSystem.ClientService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceEngine serviceEngine = new ServiceEngine(@"F:\WorkSpace\Testdlpsystem\Client", "127.0.0.1", 29015);
            serviceEngine.Start();

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new DLPClientService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
