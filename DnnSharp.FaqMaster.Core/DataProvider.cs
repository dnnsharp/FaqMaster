

using System;
using DotNetNuke;
using System.Data;
using System.Web.UI.WebControls;

using DotNetNuke.Framework;

namespace DnnSharp.FaqMaster.Core
{
    public abstract class DataProvider
    {
        // singleton reference to the instantiated object 
        private static DataProvider objProvider = null;

        // constructor
        static DataProvider()
        {
            CreateProvider();
        }

        // dynamically create provider
        private static void CreateProvider()
        {
            objProvider = (DataProvider)Reflection.CreateObject("data", "DnnSharp.FaqMaster.Core", "");
        }

        // return the provider
        public static DataProvider Instance()
        {
            return objProvider;
        }


        public abstract int UpdateFaq(int faqId, int moduleId, string question, string answer, int viewOrder);
        public abstract IDataReader GetFaqs(int moduleId);
        public abstract IDataReader GetLatestFaq(int moduleId);
        public abstract void DeleteItem(int itemId);

        //public abstract int AddItem(int moduleId, string title, string description, string thumbUrl, string imageUrl, int viewOrder, bool autoGenerateThumb);
        //public abstract void UpdateItem(int itemId, int moduleId, string title, string description, string thumbUrl, string imageUrl, int viewOrder, bool autoGenerateThumb, int imageWidth, int imageHeight, int thumbWidth, int thumbHeight, long lastWriteTime);
        //public abstract void UpdateItemOrder(int itemId, int viewOrder);
        //public abstract IDataReader GetItems(int moduleId);
        //public abstract IDataReader GetItemById(int itemId);
        //public abstract void DeleteItem(int itemId);


    }
}
