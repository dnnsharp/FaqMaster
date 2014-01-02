using Microsoft.ApplicationBlocks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics;

namespace DnnSharp.FaqMaster.Core.DnnSf.Orm.ActiveTable
{
    /// <summary>
    /// This class is public because Entity classes derive from this
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ActiveTableBase<T>
    {
        static Table Table { get; set; }

        // get it once with the static constructor, so all instances can use (read) these
        static ActiveTableBase()
        {
            Table = new Table(typeof(T));
        }

        /// <summary>
        /// This will prevent this class from being used from external assemblies
        /// </summary>
        internal ActiveTableBase()
        {

        }

        public virtual void Save()
        {
            Table.ExecuteUpdate(this);
        }

        public virtual void Delete()
        {
            Table.ExecuteDelete(this);
        }

        public static IList<T> GetAllByProperty(string orderBy = null, int? top = null, params IRestriction[] criteria)
        {
            return Table.GetAllByProperty<T>(orderBy, top, criteria);
        }

        public static T GetOneByProperty(string orderBy = null, params IRestriction[] criteria)
        {
            return Table.GetAllByProperty<T>(orderBy, 1, criteria).FirstOrDefault();
        }

        public static void DeleteAllByProperty(params IRestriction[] criteria)
        {
            Table.DeleteAllByProperty(criteria);
        }
    }
}
