<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="InsertComment.aspx.cs" Inherits="CyberBullyBlocker.InsertComment" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>CyberBullyBlocker Insert Comment</title>
    <script type="text/javascript">
          function BeginProcess()
          {
            // Create an iframe.
            var iframe = document.createElement("iframe");
    
            // Point the iframe to the location of
            //  the long running process.
            iframe.src = "LongRunningProcess.aspx";
    
            // Make the iframe invisible.
            iframe.style.display = "none";
    
            // Add the iframe to the DOM.  The process
            //  will begin execution at this point.
            document.body.appendChild(iframe);
          }

          function UpdateProgress(PercentComplete, Message) {
              document.getElementById('Result').innerHTML = PercentComplete + '%: ' + Message;
          }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Label Text="Enter comment below then hit submit to test if it is cyberbullying<br />" runat="server"></asp:Label>
        <asp:TextBox ID="Comment" TextMode="MultiLine" style="width: 800px; height: 600px;" runat="server"></asp:TextBox>
        <br />
        <asp:Button ID="Submit" Text="Submit" runat="server" OnClick="Submit_Click" />
        <asp:Label ID="Submiter" runat="server" />
        <asp:Label ID="Result" runat="server" />
        
    </div>
    </form>
</body>
</html>
