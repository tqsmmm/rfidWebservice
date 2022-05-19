using System;

namespace rfidWebservice
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            System.Xml.XmlDocument xmldoc = new System.Xml.XmlDocument();

            try
            {
                string strFilePath = AppDomain.CurrentDomain.BaseDirectory + "/" + "sys.xml";
                xmldoc.Load(strFilePath);
                SysParam.mssql = xmldoc.SelectSingleNode("/root/mssql").InnerText;
                SysParam.oledbMssql = xmldoc.SelectSingleNode("/root/oledbmssql").InnerText;
                SysParam.bxUrl = xmldoc.SelectSingleNode("/root/bxUrl").InnerText;
                SysParam.rfidUrl = xmldoc.SelectSingleNode("/root/rfidUrl").InnerText;
            }
            catch (Exception ex)
            {
                pub.WriteErrLog("读取XML错误：" + ex.Message);
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            string path = Request.Url.LocalPath;

            if (path == "/restMessage")
            {
                Context.RewritePath(path.Replace("/restMessage", "/restMessage.ashx"));
            }

            if (path == "/simERP")
            {
                Context.RewritePath(path.Replace("/simERP", "/simERP.ashx"));
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}