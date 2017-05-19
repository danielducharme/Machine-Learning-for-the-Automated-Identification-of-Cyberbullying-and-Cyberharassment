<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StatusTracker.aspx.cs" Inherits="CyberBullyBlocker.StatusTracker" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Status Tracker</title>
    <meta http-equiv="Refresh" content="60" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <asp:Label ID="Head" runat="server"></asp:Label><br />
    <asp:Label ID="Tracker" runat="server"></asp:Label><br />
     <asp:Button ID="Reset" runat="server" Text="Reset" OnClick="Reset_Click"/> 
    </div>
    </form>
</body>
</html>
