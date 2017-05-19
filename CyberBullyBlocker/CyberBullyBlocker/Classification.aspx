<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Classification.aspx.cs" Inherits="CyberBullyBlocker.Classification" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Classifier</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Label ID="Head" runat="server"></asp:Label><br />
        <asp:Label ID="Reminder" runat="server"></asp:Label><br />
        <asp:Label ID="Counter" runat="server"></asp:Label><br />
        <asp:TextBox ID="Comment" runat="server" ReadOnly="true" TextMode="MultiLine" Width="800" Height="400"></asp:TextBox><br />
        <asp:Button ID="Bully" runat="server" Text="Bullying" OnClick="Bully_Click" />
        <asp:Button ID="NotBully" runat="server" Text="Not Bullying" OnClick="NotBully_Click" />
    </div>
    </form>
</body>
</html>
