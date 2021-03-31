using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using WebFormServer.Engine;

namespace WebFormServer
{
    public class Global : HttpApplication
    {
        private ServerEngine serverEngine = null;

        void Application_Start(object sender, EventArgs e)
        {
            this.serverEngine = new ServerEngine();
            serverEngine.Start();

            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
            
}