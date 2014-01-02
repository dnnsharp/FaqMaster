using DnnSharp.FaqMaster.Core.DnnSf;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public class SettingsDictionary : Dictionary<string, object>, IXmlSerializable
    {
        public SettingsDictionary()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public SettingsDictionary(Dictionary<string, object> data)
            : base(data, StringComparer.OrdinalIgnoreCase)
        {
        }


        public T GetAs<T>(string name, T defaultValue = default(T))
        {
            if (!ContainsKey(name))
                return defaultValue;

            if (typeof(T) == this[name].GetType())
                return (T)this[name];

            return defaultValue;
        }

        public T GetValue<T>(string name, T defaultValue = default(T))
        {
            if (!ContainsKey(name) || this[name] == null)
                return defaultValue;

            if (typeof(T) == this[name].GetType())
                return (T)this[name];

            // check for expressions
            var val = this[name];
            if (val.GetType() == typeof(Dictionary<string, object>)) {
                var dic = val as IDictionary;
                if (dic.Contains("Expression"))
                    val = dic.Contains("IsExpression") && (bool)dic["IsExpression"] ? dic["Expression"] : dic["Value"];
            }

            if (val == null)
                return defaultValue;

            return ConvertUtils.Cast<T>(val.ToString(), defaultValue, false);
        }

        public IDictionary<string, T> GetDictionary<T>(string name)
        {
            if (!ContainsKey(name))
                return null;

            var data = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            foreach (Dictionary<string, object> item in GetValue(name, new ArrayList())) {
                if (!item.ContainsKey("name")
                    || (item["name"].ToString().Length == 0)
                    || (!item.ContainsKey("value")))
                    continue;

                var field = item["name"].ToString();
                var val = item["value"];
                data[field] = ConvertUtils.Cast<T>(val, default(T));
            }

            return data;
        }

        public ListItem GetListItem(string name)
        {
            if (!ContainsKey(name))
                return null;

            foreach (Dictionary<string, object> hash in GetValue(name, new ArrayList())) {
                var item = new ListItem();
                item.Text = hash.ContainsKey("Text") ? hash["Text"].ToStringOrDefault("") : "";
                item.Value = hash.ContainsKey("Value") ? hash["Value"].ToStringOrDefault("") : "";
                item.Filter = hash.ContainsKey("Filter") ? hash["Filter"].ToStringOrDefault("") : "";
                return item;
            }

            return null;
        }

        public IList<ListItem> GetListItems(string name)
        {
            if (!ContainsKey(name))
                return null;

            var data = new List<ListItem>();
            foreach (Dictionary<string, object> hash in GetValue(name, new ArrayList())) {

                var item = new ListItem();
                item.Text = hash.ContainsKey("Text") ? hash["Text"].ToStringOrDefault("") : "";
                item.Value = hash.ContainsKey("Value") ? hash["Value"].ToStringOrDefault("") : "";
                item.Filter = hash.ContainsKey("Filter") ? hash["Filter"].ToStringOrDefault("") : "";
                data.Add(item);
            }

            return data;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.IsEmptyElement)
                return;

            reader.ReadStartElement();

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement) {

                string key = reader.Name;
                string val = HttpUtility.HtmlDecode(reader.ReadInnerXml());

                this.Add(key.Trim(), val.Trim());

                //reader.ReadEndElement();
            }

            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (var key in this.Keys) {
                if (string.IsNullOrEmpty(key))
                    continue;
                writer.WriteStartElement(key);

                //// if this is an array list, serialize it as JSON
                //// TODO: maybe this should be done for all complex types
                //if (this[key].GetType() == typeof(ArrayList)) {
                //    writer.WriteValue(new JavaScriptSerializer().Serialize(this[key]));
                //    continue;
                //    //var arr = this[key] as ArrayList;
                //    //if (arr.Count == 0) {
                //    //    writer.WriteValue("");
                //    //    return;
                //    //} else {
                //    //    
                //    //    if (arr[0].GetType() == typeof(Dictionary<string, object>)) {
                //    //        for (int i = 0; i < arr.Count; i++) {
                //    //            var item = arr[i] as Dictionary<string, object>;
                //    //            arr[i] = new SettingsDictionary(item);
                //    //        }
                //    //    }
                //    //}
                //}
                writer.WriteValue(this[key]);
                writer.WriteEndElement();
            }
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
    }
}
