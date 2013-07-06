using System;
using System.Web;
using System.Net;
using System.Web.UI;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using Microsoft.ApplicationBlocks.Data;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using System.Security.Cryptography;


using DotNetNuke;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Host;
using DotNetNuke.Security;
using DotNetNuke.Security.Roles;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Framework;


namespace avt.FAQMaster
{


    public partial class ActivationWnd : PageBase
    {
        public event EventHandler OnActivateSuccess
        {
            add { Events.AddHandler("Activate", value); }
            remove { Events.RemoveHandler("Activate", value); }
        }


        protected void Page_Init(Object Sender, EventArgs args)
        {

            // check that user has rights
            if (!PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName)) {
                Response.Write("Access denied!");
                Response.End();
            }

            AJAX.RegisterScriptManager();
        }

        protected void Page_Load(Object Sender, EventArgs args)
        {
            AJAX.RegisterScriptManager();

            if (!Page.IsPostBack) {
                pnlSuccess.Visible = false;
                lnkPurchase.HRef = avt.FAQMaster.FAQMasterController.BuyLink;
            }
        }

        protected void OnNext(Object Sender, EventArgs args)
        {
            txtRegistrationCode.Text = txtRegistrationCode.Text.Trim();

            AvtRegistrationCode regCode;
            try {
                regCode = new AvtRegistrationCode(txtRegistrationCode.Text);
                //if (!regCode.IsValid(RedirectController.Version)) {
                //    throw new Exception();
                //}
            } catch {
                validateActivation.Text = "The registration code you supplied is invalid.";
                validateActivation.IsValid = false;
                return;
            }

            pnlHosts.Visible = true;

            //trDomains.Visible = true;
            ////ScriptManager.RegisterStartupScript(this, this.GetType(), "init1", "alert('" + regCode.ProductCode + "');", true);

            switch (regCode.VariantCode) {
                case "DOM":
                    FillHosts();
                    break;
                case "3DOM":
                    FillHosts();
                    break;
                case "10DOM":
                    FillHosts();
                    break;
                case "XDOM":
                    FillHosts();
                    break;
                case "SRV":
                    FillDomains();
                    break;
                case "30DAY":
                    FillAll();
                    break;
                default:
                    FillAll();
                    break;
            }

            //btnActivate.Visible = true;
            btnNext.Visible = false;
            ////txtRegistrationCode.ReadOnly = true;
        }

        protected void OnActivate(Object Sender, EventArgs args)
        {
            txtRegistrationCode.Text = txtRegistrationCode.Text.Trim();

            FAQMasterController redirCtrl = new FAQMasterController();
            AvtRegCoreClient regClient = redirCtrl.GetRegCoreClient();

            AvtActivation act;
            try {
                act = regClient.Activate(txtRegistrationCode.Text, FAQMasterController.ProductCode, FAQMasterController.Version, ddHosts.SelectedValue, FAQMasterController.ProductKey);
            } catch (Exception ex) {
                // error
                validateActivation.Text = ex.Message;
                validateActivation.IsValid = false;
                return;
            }

            // activation succesfull
            pnlSuccess.Visible = true;
            pnlActivateForm.Visible = false;
            btnClose.Text = "Close";
            lnkPurchase.Visible = false;

        }



        #region Helpers

        private void FillHosts()
        {
            ddHosts.Items.Clear();

            PortalAliasController paCtrl = new PortalAliasController();
            foreach (DictionaryEntry de in paCtrl.GetPortalAliases()) {
                PortalAliasInfo paInfo = (PortalAliasInfo)de.Value;
                string httpAlias = paInfo.HTTPAlias;
                bool isIP = (Regex.Match(httpAlias, ".*\\d+\\.\\d+\\.\\d+\\.\\d+.*").Length > 0);
                if (isIP) {
                    continue; // this is IP based alias
                }
                // remove port, if exists
                if (httpAlias.LastIndexOf(":") != -1) httpAlias = httpAlias.Substring(0, httpAlias.IndexOf(":"));

                // remove path
                if (httpAlias.LastIndexOf("/") != -1) httpAlias = httpAlias.Substring(0, httpAlias.IndexOf("/"));

                // remove www.
                if (httpAlias.IndexOf("www.") != -1) httpAlias = httpAlias.Substring(httpAlias.IndexOf("www.") + 4);
                if (httpAlias.IndexOf("dev.") != -1) httpAlias = httpAlias.Substring(httpAlias.IndexOf("dev.") + 4);
                if (httpAlias.IndexOf("staging.") != -1) httpAlias = httpAlias.Substring(httpAlias.IndexOf("staging.") + 8);

                if (httpAlias.IndexOf("localhost") == 0) {
                    continue;
                }

                if (ddHosts.Items.FindByText(httpAlias) != null) {
                    continue; // item already exists
                }

                ddHosts.Items.Add(new ListItem(httpAlias, httpAlias));
            }

            if (Request.QueryString["aurl"] != null)
                ddHosts.Items.Add(new ListItem(Request.QueryString["aurl"], Request.QueryString["aurl"]));
        }

        private void FillDomains()
        {
            ddHosts.Items.Clear();

            PortalAliasController paCtrl = new PortalAliasController();
            foreach (DictionaryEntry de in paCtrl.GetPortalAliases()) {
                PortalAliasInfo paInfo = (PortalAliasInfo)de.Value;
                FillIp(paInfo.HTTPAlias);
            }
            if (Request.QueryString["aurl"] != null)
                ddHosts.Items.Add(new ListItem(Request.QueryString["aurl"], Request.QueryString["aurl"]));
        }

        private void FillIp(string httpAlias)
        {
            bool isIP = (Regex.Match(httpAlias, ".*\\d+\\.\\d+\\.\\d+\\.\\d+.*").Length > 0);
            if (!isIP) {
                // translate it to IP
                httpAlias = Regex.Match(httpAlias, ".*\\d+\\.\\d+\\.\\d+\\.\\d+.*").Value;
                try {
                    foreach (IPAddress addr in System.Net.Dns.GetHostEntry(httpAlias).AddressList) {
                        try {
                            FillIp(addr.ToString());
                        } catch { continue; }
                    }
                } catch { }
                return;
            }

            // remove port, if exists
            if (httpAlias.LastIndexOf(":") != -1) httpAlias = httpAlias.Substring(0, httpAlias.IndexOf(":"));

            // remove path
            if (httpAlias.LastIndexOf("/") != -1) httpAlias = httpAlias.Substring(0, httpAlias.IndexOf("/"));

            // remove www.
            if (httpAlias.IndexOf("www.") != -1) httpAlias = httpAlias.Substring(httpAlias.IndexOf("www.") + 4);
            if (httpAlias.IndexOf("dev.") != -1) httpAlias = httpAlias.Substring(httpAlias.IndexOf("dev.") + 4);
            if (httpAlias.IndexOf("staging.") != -1) httpAlias = httpAlias.Substring(httpAlias.IndexOf("staging.") + 8);

            if (httpAlias.IndexOf("127.0.0.1") == 0) {
                return;
            }

            if (ddHosts.Items.FindByText(httpAlias) != null) {
                return; // item already exists
            }

            ddHosts.Items.Add(new ListItem(httpAlias, httpAlias));
        }

        private void FillAll()
        {
            ddHosts.Items.Clear();

            PortalAliasController paCtrl = new PortalAliasController();
            foreach (DictionaryEntry de in paCtrl.GetPortalAliases()) {
                PortalAliasInfo paInfo = (PortalAliasInfo)de.Value;
                string httpAlias = paInfo.HTTPAlias;
                bool isIP = (Regex.Match(httpAlias, ".*\\d+\\.\\d+\\.\\d+\\.\\d+.*").Length > 0);

                // remove port, if exists
                if (httpAlias.LastIndexOf(":") != -1) httpAlias = httpAlias.Substring(0, httpAlias.IndexOf(":"));

                // remove path
                if (httpAlias.LastIndexOf("/") != -1) httpAlias = httpAlias.Substring(0, httpAlias.IndexOf("/"));

                // remove www.
                if (!isIP && httpAlias.IndexOf("www.") != -1) httpAlias = httpAlias.Substring(httpAlias.IndexOf("www.") + 4);
                if (!isIP && httpAlias.IndexOf("dev.") != -1) httpAlias = httpAlias.Substring(httpAlias.IndexOf("dev.") + 4);
                if (!isIP && httpAlias.IndexOf("staging.") != -1) httpAlias = httpAlias.Substring(httpAlias.IndexOf("staging.") + 8);

                if (httpAlias.IndexOf("127.0.0.1") == 0) {
                    continue;
                }

                if (ddHosts.Items.FindByText(httpAlias) != null) {
                    continue; // item already exists
                }

                ddHosts.Items.Add(new ListItem(httpAlias, httpAlias));
            }
        }


        protected void OnCloseSA(object sender, EventArgs e)
        {
            if (Request.QueryString["rurl"] != null)
                Response.Redirect(Server.UrlDecode(Request.QueryString["rurl"]));
            else
                Response.Redirect("~/");
        }

        #endregion

    }
}