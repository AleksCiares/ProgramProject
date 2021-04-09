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
        void Application_Start(object sender, EventArgs e)
        {
            Thread thread = new Thread(() =>
            {
                ServerEngine.Start(@"F:\WorkSpace\Testdlpsystem\Server");
            });
            thread.Start();

            //ServerEngine.Start(@"F:\WorkSpace\Testdlpsystem\Server");

            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        void Application_End(object sender, EventArgs e)
        {
            ServerEngine.Stop();
        }
    }
            
}