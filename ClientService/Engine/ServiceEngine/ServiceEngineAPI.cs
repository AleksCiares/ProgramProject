using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceEngine
{
    public partial class ServiceEngine
    {
        ServiceEngine(string path, string serverName, int port)
        {
            this.pathToModulesConfigFile = path;
            this.serverName = serverName;
            this.port = port;
        }

        public void Start()
        {
            //make async
            CheckModulesConfigFile();
        }
    }
}
