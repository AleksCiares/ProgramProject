
using System.Collections.Generic;

namespace ClientService
{
    partial class DLPClientService
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private string pathToModulesConfigFile = null;
        private string serverName = null;
        private int serverPort = 0;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // 
            // DLPClientService
            // 
            this.CanHandleSessionChangeEvent = true;
            this.CanShutdown = true;
            this.CanStop = false;
            this.ServiceName = "DLPClientService";


        }

        #endregion

    }
}
