using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace DnnSharp.FaqMaster.Core.DnnSf.Dnn
{
    static public class RoleControllerEx
    {
        public static IEnumerable<RoleInfo> GetRoleList(string list)
        {
            var roles = new List<RoleInfo>();
      
            var roleCtl = new RoleController();
            foreach (var roleToken in list.Split(new char[] { '\n', ';', ',' }, System.StringSplitOptions.RemoveEmptyEntries)) {
                int roleId;
                if (int.TryParse(roleToken, out roleId)) {
                    var role = roleCtl.GetRole(roleId, PortalController.GetCurrentPortalSettings().PortalId);
                    if (role != null)
                        roles.Add(role);
                } else {
                    var role = roleCtl.GetRoleByName(PortalController.GetCurrentPortalSettings().PortalId, roleToken);
                    if (role != null)
                        roles.Add(role);
                }
            }

            return roles.Distinct();
        }
    }

}
