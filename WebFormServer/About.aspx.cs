using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebFormServer.Engine;

namespace WebFormServer
{
    public partial class About : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (ServerEngine.Clients?.Count > 0)
            {
                Type type = ServerEngine.Clients[0].GetType();
                FieldInfo[] fieldInfos = type.GetFields();

                foreach (var client in ServerEngine.Clients)
                {
                    TableRow row = new TableRow();
                    //for (int cellNum = 0; cellNum < fieldInfos.Length; cellNum++)
                    //{
                        TableCell cell = new TableCell();
                        cell.Text = String.Format(
                            $"Host name: {client.HostName}\n" +
                            $"IP: {client.IpHost}\n" +
                            $"DNS: {client.DnsName}");

                        row.Cells.Add(cell);
                    //}
                    Table1.Rows.Add(row);
                }
            }
        }
    }
}