<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="WebFormServer.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %>.</h2>
    <h3>Your application description page.</h3>
    <p>Use this area to provide additional information.</p>
    <asp:Panel ID="Panel1" runat="Server"
      Height="300px" Width="400px"
      BackColor="#808080" ScrollBars="Auto">

      <asp:Table ID="Table1" runat="Server"></asp:Table>  

    </asp:Panel>
</asp:Content>
