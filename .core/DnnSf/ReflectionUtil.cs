using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public static class ReflectionUtil
    {
        public static T CreateInstance<T>(string strDataType)
            where T : class
        {
            if (string.IsNullOrEmpty(strDataType))
                return null;

            Type dataType = Type.GetType(strDataType);
            if (dataType == null) {
                dataType = Type.GetType(strDataType.Substring(0, strDataType.IndexOf(",") + 1) + typeof(T).Assembly.ToString());
            }

            try {
                return Activator.CreateInstance(dataType) as T;
            } catch (Exception ex) {
                throw new Exception("Could not create object of type " + strDataType, ex);
            }
        }
    }
}
