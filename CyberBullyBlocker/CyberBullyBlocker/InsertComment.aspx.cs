using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CyberBullyBlocker
{
    public partial class InsertComment : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Submit_Click(object sender, EventArgs e)
        {
            if(Comment.Text != "")
            {
                Submiter.Text = "<script type='text/javascript'>BeginProcess();</script>";

                Result.Text = "Start Processing";

                Session["Comment"] = Comment.Text;
            }
        }
    }
}