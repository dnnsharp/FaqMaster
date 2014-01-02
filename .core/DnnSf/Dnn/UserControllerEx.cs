using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace DnnSharp.FaqMaster.Core.DnnSf.Dnn
{
    static public class UserControllerEx
    {
        public static UserInfo GetOneUserByProfile(int portalId, string propertyName, string propertyValue)
        {
            // check either a user with same string already exists, in which case append the id
            int totalRecords = 0;
            var users = UserController.GetUsersByProfileProperty(portalId, propertyName, propertyValue, 0, 1, ref totalRecords);
            if (users != null && users.Count > 0)
                return users[0] as UserInfo;

            // also try and get it from host portal, since host users exist on all portals
            if (portalId == -1)
                return null; // already searched host portal

            users = UserController.GetUsersByProfileProperty(-1, propertyName, propertyValue, 0, 1, ref totalRecords);
            if (users != null && users.Count > 0)
                return users[0] as UserInfo;

            // no user found
            return null;
        }
    }

}
