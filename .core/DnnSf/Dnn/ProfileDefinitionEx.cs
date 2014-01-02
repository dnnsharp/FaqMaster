using DotNetNuke.Entities.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Dnn
{
    public static class ProfileDefinitionEx
    {
        public static void CreateStringProperty(int portalId, string name, string category = "Preferences", int length = 1024)
        {
            var prop = ProfileController.GetPropertyDefinitionByName(portalId, name);
            if (prop == null) {
                ProfileController.AddPropertyDefinition(new ProfilePropertyDefinition() {
                    PortalId = portalId,
                    PropertyName = name,
                    PropertyCategory = category,
                    DataType = 349, // seems hardcoded to something
                    Length = length,
                    Visibility = DotNetNuke.Entities.Users.UserVisibilityMode.AdminOnly
                });
            }
        }
    }
}
