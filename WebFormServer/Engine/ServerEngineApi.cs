using DLPSystem.WebFormServer.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebFormServer.Engine.Controllers;

namespace WebFormServer.Engine
{
    public partial class ServerEngine
    {
        public ServerEngine()
        { }

        public void Start()
        {
            this.CheckConfigFiles();
            this.StartListener();
        }

        public List<Client> Clients { get; }


    }
}

