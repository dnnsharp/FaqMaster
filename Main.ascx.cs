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

namespace avt.FAQMaster
{

    public partial class Main : DotNetNuke.Entities.Modules.PortalModuleBase
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

                FAQMasterController faqCtrl = new FAQMasterController();
                pnlActivate.Visible = !faqCtrl.IsActivated();
                BindFaqs();
            }
        }

        protected string Tokenize(string content)
        {
            if (_MyTokens == null)
                return content;

            //try {
            content = _MyTokens.InvokeMember("ReplaceTokensAll", System.Reflection.BindingFlags.InvokeMethod, null, null, new object[] { content, UserInfo, !DotNetNuke.Common.Globals.IsTabPreview(), this.ModuleConfiguration }).ToString();
            //} catch (Exception ex) {
            //    return ex.Message;
            //}

            return content;
        }

        // using System.Web;
        // using DotNetNuke.Entities.Modules;
        // using System.Reflection;
        protected string Tokenize(string strContent, ModuleInfo modInfo)
        {
            bool bMyTokensInstalled = false;
            MethodInfo methodReplace = null;
            MethodInfo methodReplaceWMod = null;

            // first, determine if MyTokens is installed
            if (HttpRuntime.Cache.Get("avt.MyTokens.Installed") != null) {
                bMyTokensInstalled = Convert.ToBoolean(HttpRuntime.Cache.Get("avt.MyTokens.Installed"));
                if (bMyTokensInstalled == true) {
                    methodReplace = (MethodInfo) HttpRuntime.Cache.Get("avt.MyTokens.MethodReplace");
                    methodReplaceWMod = (MethodInfo) HttpRuntime.Cache.Get("avt.MyTokens.MethodReplaceWMod");
                }
            } else {
                // it's not in cache, let's determine if it's installed
                try {
                    Type myTokensRepl = DotNetNuke.Framework.Reflection.CreateType("avt.MyTokens.MyTokensReplacer");
                    if (myTokensRepl == null)
                        throw new Exception(); // handled in catch

                    bMyTokensInstalled = true;
                    
                    // we now know MyTokens is installed, get ReplaceTokensAll methods

                    methodReplace = myTokensRepl.GetMethod(
                        "ReplaceTokensAll", 
                        BindingFlags.Public | BindingFlags.Static,
                        null,
                         CallingConventions.Any,
                        new Type[] { 
                            typeof(string), 
                            typeof(DotNetNuke.Entities.Users.UserInfo), 
                            typeof(bool) 
                        },
                        null
                    );

                    methodReplaceWMod = myTokensRepl.GetMethod(
                        "ReplaceTokensAll", 
                        BindingFlags.Public | BindingFlags.Static,
                        null,
                         CallingConventions.Any,
                        new Type[] { 
                            typeof(string), 
                            typeof(DotNetNuke.Entities.Users.UserInfo), 
                            typeof(bool),
                            typeof(ModuleInfo)
                        },
                        null
                    );

                    if (methodReplace == null || methodReplaceWMod == null) {
                        // this shouldn't really happen, we know MyTokens is installed
                        throw new Exception();
                    }

                } catch {
                    bMyTokensInstalled = false;
                }

                // cache values so next time the funciton is called the reflection logic is skipped
                HttpRuntime.Cache.Insert("avt.MyTokens.Installed", bMyTokensInstalled);
                if (bMyTokensInstalled) {
                    HttpRuntime.Cache.Insert("avt.MyTokens.MethodReplace", methodReplace);
                    HttpRuntime.Cache.Insert("avt.MyTokens.MethodReplaceWMod", methodReplaceWMod);
                }
            }


            // revert to standard DNN Token Replacement if MyTokens is not installed

            if (!bMyTokensInstalled) {
                DotNetNuke.Services.Tokens.TokenReplace dnnTknRepl = new DotNetNuke.Services.Tokens.TokenReplace();
                dnnTknRepl.AccessingUser = DotNetNuke.Entities.Users.UserController.GetCurrentUserInfo();
                dnnTknRepl.DebugMessages = !DotNetNuke.Common.Globals.IsTabPreview();
                if (modInfo != null)
                    dnnTknRepl.ModuleInfo = modInfo;

                // MyTokens is not installed, execution ends here
                return dnnTknRepl.ReplaceEnvironmentTokens(strContent);
            }

            // we have MyTokens installed, proceed to token replacement
            // Note that we could be using only the second overload and pass null to the ModuleInfo parameter,
            //  but this will break compatibility with integrations made before the second overload was added
            if (modInfo == null) {
                return (string)methodReplace.Invoke(
                    null,
                    new object[] {
                        strContent,
                        DotNetNuke.Entities.Users.UserController.GetCurrentUserInfo(),
                        !DotNetNuke.Common.Globals.IsTabPreview()
                    }
                );
            } else {
                return (string)methodReplaceWMod.Invoke(
                    null,
                    new object[] {
                        strContent,
                        DotNetNuke.Entities.Users.UserController.GetCurrentUserInfo(),
                        !DotNetNuke.Common.Globals.IsTabPreview(),
                        modInfo
                    }
                );
            }
        }

        protected void OnDeleteFaq(object sender, System.EventArgs e)
        {
            int faqId = -1;
            //try
            //{
                faqId = Convert.ToInt32(ddEditFaq.SelectedValue);
                FAQMasterController faqCtrl = new FAQMasterController();
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

            FAQMasterController faqCtrl = new FAQMasterController();
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

            FAQMasterController faqCtrl = new FAQMasterController();
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

            FAQMasterSettings settings = new FAQMasterSettings();
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
                FAQMasterController faqCtrl = new FAQMasterController();
                List<FaqInfo> faqs = faqCtrl.GetFaqs(ModuleId);

                ddEditFaq.Items.Clear();
                ddEditFaq.Items.Add(new ListItem("[ New Faq ]", "-1"));
                foreach (FaqInfo faq in faqs) {
                    ddEditFaq.Items.Add(new ListItem(faq.Question, faq.FaqId.ToString()));
                }


                if (_MyTokens == null) {
                    lblMyTokensQ.InnerHtml = lblMyTokensA.InnerHtml = "can contain MyTokens (get it <a href = 'http://www.avatar-soft.ro/Products/MyTokens/tabid/148/Default.aspx'>here</a>)";
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
            FAQMasterController faqCtrl = new FAQMasterController();
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
            if (!faqCtrl.IsActivated() && r.Next(20) == 1) {
                Writer.WriteStartElement("faq");
                Writer.WriteElementString("id", "-1");
                Writer.WriteElementString("relPath", "#faq-" + ModuleId.ToString() + "--1");
                Writer.WriteElementString("question", "FAQMaster Demo.");
                Writer.WriteElementString("answer", "This copy of FAQMaster is not licensed. Read more about this product on <a href = 'http://www.avatar-soft.ro'>Avatar Software website</a>.");
                Writer.WriteElementString("index", "-1");
                Writer.WriteElementString("opened", "true");
                Writer.WriteEndElement(); // faq
            } else {
                foreach (FaqInfo faq in faqs) {
                    Writer.WriteStartElement("faq");
                    Writer.WriteElementString("id", faq.FaqId.ToString());
                    Writer.WriteElementString("relPath", "#faq-" + ModuleId.ToString() + "-" + faq.FaqId.ToString());
                    Writer.WriteElementString("question", Tokenize(faq.Question, ModuleConfiguration).Replace("\n", "<br />"));
                    Writer.WriteElementString("answer", Tokenize(faq.Answer, ModuleConfiguration).Replace("\n", "<br />"));
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
            FAQMasterSettings settings = new FAQMasterSettings();
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

