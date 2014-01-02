using DotNetNuke;
using System.Web.UI;
using DotNetNuke.Entities.Modules;
using System.IO;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Diagnostics;
using DotNetNuke.Security;
using System.Web.UI.WebControls;
using System.Xml.Xsl;
using System.Xml;
using System.Text;
using DotNetNuke.Framework;
using System.Web;
using System.Reflection;
using DnnSharp.FaqMaster.Core;
using DnnSharp.FaqMaster.Core.DnnSf;

namespace DnnSharp.FaqMaster
{

    public partial class Main : PortalModuleBase
    {
        protected string lastCode; // used in the repeater to add space between series
        protected Type _MyTokens = null;

        private void Page_Init(object sender, System.EventArgs e)
        {
        }

        private void Page_Load(object sender, System.EventArgs e)
        {
            try {
                _MyTokens = DotNetNuke.Framework.Reflection.CreateType("avt.MyTokens.MyTokensReplacer", true);
                //_MyTokens = BuildManager.GetType("avt.MyTokens.MyTokensReplacer", true);
            } catch (Exception ex) {
                _MyTokens = null;
            }

            if (!Page.IsPostBack) {
                
                // admin panel
                pnlAddFaq.Visible = HasAdminRights();

                pnlActivate.Visible = !App.Instance.IsActivated;
                BindFaqs();
            }
        }

      
        protected void OnDeleteFaq(object sender, System.EventArgs e)
        {
            int faqId = -1;
            //try
            //{
                faqId = Convert.ToInt32(ddEditFaq.SelectedValue);
                FaqMasterController faqCtrl = new FaqMasterController();
                faqCtrl.DeleteItem(ModuleId, faqId);
                
                ResetForm();
                BindFaqs();
                OnChangeEditFaq(null, null);
               
            //}
            //catch { faqId = -1; }

            //if (faqId <= 0)
            //{
            //    ResetForm();
            //    return;
            //}
        }
        protected void OnChangeEditFaq(object sender, System.EventArgs e)
        {
            int faqId = -1;
            try {
                faqId = Convert.ToInt32(ddEditFaq.SelectedValue);
                btnDelete.Visible = true;
            } catch { faqId = -1; }

            if (faqId <= 0) {
                ResetForm();
                btnDelete.Visible = false;
                return;
            }

            FaqMasterController faqCtrl = new FaqMasterController();
            Dictionary<int, FaqInfo> faqs = faqCtrl.GetFaqsDictionary(ModuleId);
            if (faqs.ContainsKey(faqId)) {
                tbFaqAnswer.Text = faqs[faqId].Answer;
                tbFaqQuestion.Text = faqs[faqId].Question;
                if (faqs[faqId].ViewOrder == 2147483647) {
                    tbOrder.Text = "";
                }
                else{
                    tbOrder.Text = faqs[faqId].ViewOrder.ToString();
                }
            }
        }

        private void ResetForm()
        {
            ddEditFaq.SelectedIndex = 0;
            tbFaqAnswer.Text = "";
            tbFaqQuestion.Text = "";
            tbOrder.Text = "";
        }

        protected void OnUpdateFaq(object sender, System.EventArgs e)
        {
            if (!HasAdminRights())
                return;

            FaqMasterController faqCtrl = new FaqMasterController();
            int faqId;
            try {
                faqId = Convert.ToInt32(ddEditFaq.SelectedValue);
                if (faqId <= 0)
                    faqId = -1;
            } catch { faqId = -1; }

            int viewIndex;
            try {
                viewIndex = Convert.ToInt32(tbOrder.Text);
            } catch { viewIndex = int.MaxValue; }
            
            faqCtrl.UpdateFaq(faqId, ModuleId, tbFaqQuestion.Text, tbFaqAnswer.Text, viewIndex);

            // reset form
            ResetForm();

            // rebind faqs
            BindFaqs();
        }

        private void BindFaqs()
        {
            BindAdmin();

            FaqMasterSettings settings = new FaqMasterSettings();
            settings.Load(ModuleId);

            XslCompiledTransform transform = new XslCompiledTransform();
            transform.Load(settings.TemplatePathMain);
            System.IO.StringWriter output = new System.IO.StringWriter();
            transform.Transform(GetFaqsXml(), null, output);
            pnlFaqs.InnerHtml = output.ToString();
        }

        private void BindAdmin()
        {
            // populate faqs edit dropdown
            if (HasAdminRights()) {
                FaqMasterController faqCtrl = new FaqMasterController();
                List<FaqInfo> faqs = faqCtrl.GetFaqs(ModuleId);

                ddEditFaq.Items.Clear();
                ddEditFaq.Items.Add(new System.Web.UI.WebControls.ListItem("[ New Faq ]", "-1"));
                foreach (FaqInfo faq in faqs) {
                    ddEditFaq.Items.Add(new System.Web.UI.WebControls.ListItem(faq.Question, faq.FaqId.ToString()));
                }


                if (_MyTokens == null) {
                    lblMyTokensQ.InnerHtml = lblMyTokensA.InnerHtml = "can contain MyTokens (get it <a href = 'http://www.dnnsharp.com/dnn/modules/my-custom-tokens'>here</a>)";
                    lblMyTokensQ.Attributes["style"] += "color: #dd7777;";
                    lblMyTokensA.Attributes["style"] += "color: #dd7777;";
                } else {
                    lblMyTokensQ.InnerHtml = lblMyTokensA.InnerHtml = "can contain MyTokens (installed)";
                    lblMyTokensQ.Attributes["style"] += "color: #7777dd;";
                    lblMyTokensA.Attributes["style"] += "color: #7777dd;";
                }

                // setup validation group
                tbFaqQuestion.ValidationGroup = "vldFaqMaster" + ModuleId;
                tbFaqAnswer.ValidationGroup = "vldFaqMaster" + ModuleId;
                btnSave.ValidationGroup = "vldFaqMaster" + ModuleId;
            }
        }

        private XmlDocument GetFaqsXml()
        {
            FaqMasterController faqCtrl = new FaqMasterController();
            List<FaqInfo> faqs = faqCtrl.GetFaqs(ModuleId);

            int activeFaq = -1;
            try {
                activeFaq = Convert.ToInt32(Request.Params["faqid"]);
            } catch { activeFaq = -1; }

            StringBuilder strXML = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            XmlWriter Writer = XmlWriter.Create(strXML, settings);

            Writer.WriteStartElement("root");

            Writer.WriteElementString("rootElementId", pnlFaqs.ClientID.ToString());
            Writer.WriteElementString("moduleId", ModuleId.ToString());

            Writer.WriteStartElement("faqs");


            Random r = new Random();
            if (App.Instance.IsActivated) {
                foreach (FaqInfo faq in faqs) {
                    Writer.WriteStartElement("faq");
                    Writer.WriteElementString("id", faq.FaqId.ToString());
                    Writer.WriteElementString("relPath", "#faq-" + ModuleId.ToString() + "-" + faq.FaqId.ToString());
                    Writer.WriteElementString("question", TokenUtil.Tokenize(faq.Question, ModuleConfiguration, UserInfo, false, true).Replace("\n", "<br />"));
                    Writer.WriteElementString("answer", TokenUtil.Tokenize(faq.Answer, ModuleConfiguration, UserInfo, false, true).Replace("\n", "<br />"));
                    Writer.WriteElementString("index", faq.ViewOrder.ToString());
                    Writer.WriteElementString("opened", activeFaq == faq.FaqId ? "true" : "false");
                    Writer.WriteEndElement(); // faq
                }
            }

            Writer.WriteEndElement(); // faqs
            Writer.WriteEndElement(); // root
            Writer.Close();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(strXML.ToString());
            return doc;
        }

        private bool HasAdminRights()
        {
            if (PortalSettings.UserMode == DotNetNuke.Entities.Portals.PortalSettings.Mode.Edit &&
                    ((ModuleId != -1 && PortalSecurity.HasNecessaryPermission(SecurityAccessLevel.Edit, PortalSettings, ModuleConfiguration)) ||
                    (PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName)))) {
                return true;
            }

            return false;
        }

        protected override void OnPreRender(EventArgs e)
        {
            FaqMasterSettings settings = new FaqMasterSettings();
            settings.Load(ModuleId);

            // add includes
            if (settings.UseOwnjQuery) {
                Page.ClientScript.RegisterClientScriptInclude("avt_jQuery_1_3_2_FAQM", TemplateSourceDirectory + "/js/jquery-1.3.2.js");
            } else {
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "avt_jQuery_1_3_2_FAQM", "var avt_jQuery_1_3_2_FAQM = jQuery;", true);
            }

            //Response.Write(ModuleId);
            CDefault defaultPage = (CDefault)Page;
            defaultPage.AddStyleSheet("faqTpl" + settings.Template,  TemplateSourceDirectory + "/templates/" + settings.Template + "/styles.css");

            base.OnPreRender(e);
        }
    }

}

