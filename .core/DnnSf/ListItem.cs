using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public class ListItem
    {
        public string Text { get; set; }
        public object Value { get; set; }
        public bool Selected { get; set; }
        public string Filter { get; set; }

        public IList<ListItem> Children { get; set; }

        public ListItem()
        {
            Text = "";
            Value = "";
            Children = new List<ListItem>();
        }

        public ListItem(string text)
        {
            Text = text;
            Value = text;
            Children = new List<ListItem>();
        }

        public ListItem(string text, string value)
        {
            Text = text;
            Value = value;
            Children = new List<ListItem>();
        }

        //public ListItem(string text, string value, string filter)
        //{
        //    Text = text;
        //    Value = value;
        //    Filter = filter;
        //}

        public static IList<ListItem> FromEnum<T>()
            where T : struct
        {
            return FromEnum(typeof(T));
        }

        public static IList<ListItem> FromEnum(Type type)
        {
            if (type == null || !type.IsEnum)
                throw new ArgumentException(type.ToString() + " is not an enum");

            Array values = Enum.GetValues(type);
            List<ListItem> items = new List<ListItem>(values.Length);

            foreach (var i in values) {
                items.Add(new ListItem() {
                    Text = Enum.GetName(type, i),
                    Value = i.ToString()
                });
            }

            return items;
        }

    }
}
