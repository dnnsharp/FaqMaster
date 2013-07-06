using System.IO;
using System.Runtime.Serialization.Formatters;
using DotNetNuke;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Collections.Generic;
using System;
using DotNetNuke.Entities.Modules;

namespace avt.FAQMaster
{
    public partial class Settings : DotNetNuke.Entities.Modules.ModuleSettingsBase
    {

        void Page_Init(object sender, EventArgs e)
        {
            //PopulateFolders("", PortalSettings.HomeDirectoryMapPath, 0);
        }

        public override void LoadSettings()
        {
            if (!Page.IsPostBack) {
                
                // load templates
                foreach (string dir in Directory.GetDirectories(Server.MapPath("~/DesktopModules/avt.FAQMaster/templates/"))) {
                    ddTemplates.Items.Add(new ListItem(Path.GetFileName(dir), Path.GetFileName(dir)));
                }

                FAQMasterSettings settings = new FAQMasterSettings();
                settings.Load(ModuleId);

                if (ModuleSettings.ContainsKey("Template") && ddTemplates.Items.FindByValue(ModuleSettings["Template"].ToString()) != null) {
                    ddTemplates.Items.FindByValue(ModuleSettings["Template"].ToString()).Selected = true;
                }

                // load AJAX switch
                if (ModuleSettings.ContainsKey("UseAjax")) {
                    try {
                        cbUseAjax.Checked = Convert.ToBoolean(ModuleSettings["UseAjax"]);
                    } catch {
                        cbUseAjax.Checked = false;
                    }
                }

                // load jQuery switch
                cbUseOwnjQuery.Checked = settings.UseOwnjQuery;

                // load show effect
                if (ModuleSettings.ContainsKey("ShowEffect") && ddEffectsShow.Items.FindByValue(ModuleSettings["ShowEffect"].ToString()) != null) {
                    ddEffectsShow.Items.FindByValue(ModuleSettings["ShowEffect"].ToString()).Selected = true;
                }

                // load hide effect
                if (ModuleSettings.ContainsKey("HideEffect") && ddEffectsHide.Items.FindByValue(ModuleSettings["HideEffect"].ToString()) != null) {
                    ddEffectsHide.Items.FindByValue(ModuleSettings["HideEffect"].ToString()).Selected = true;
                }
            }
        }


        public override void UpdateSettings()
        {
            ModuleController modCtrl = new ModuleController();

            // save template
            modCtrl.UpdateModuleSetting(ModuleId, "Template", ddTemplates.SelectedValue);

            // save ajax switch
            modCtrl.UpdateModuleSetting(ModuleId, "UseAjax", cbUseAjax.Checked.ToString());

            // save jQuery switch
            modCtrl.UpdateModuleSetting(ModuleId, "UseOwnjQuery", cbUseOwnjQuery.Checked.ToString());

            // save show effect
            modCtrl.UpdateModuleSetting(ModuleId, "ShowEffect", ddEffectsShow.SelectedValue);

            // save hide effect
            modCtrl.UpdateModuleSetting(ModuleId, "HideEffect", ddEffectsHide.SelectedValue);
        }

    }

}


