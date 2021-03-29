using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceEngine
{
    public partial class ServiceEngine
    {
        private string pathToModulesConfigFile = null;
        private string serverName = null;
        private int port = 0;

        private void CheckModulesConfigFile()
        {
            if (!File.Exists(this.pathToModulesConfigFile))
            {
                var file = File.Create(pathToModulesConfigFile);
                file.Close();
            }
        }



    }
}
