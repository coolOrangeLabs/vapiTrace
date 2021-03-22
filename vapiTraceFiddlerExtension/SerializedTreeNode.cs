using System.Collections;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace vapiTraceFiddlerExtension
{
    public class SerializedTreeNode
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public List<SerializedTreeNode> Children { get; set; } = new List<SerializedTreeNode>();

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Value))
                return Name;

            if (string.IsNullOrEmpty(Name))
                return Value;

            return Name + " = " + Value;
        }

        public static SerializedTreeNode CreateTree(object item, string name)
        {
            Dictionary<string, object> dic;
            if (IsPrimitive(item))
            {
                //dic = new Dictionary<string, object> {{name, item}};
                return new SerializedTreeNode { Value = item.ToString() };
            }
            else
            {
                var jss = new JavaScriptSerializer();
                var settings = new JsonSerializerSettings {StringEscapeHandling = StringEscapeHandling.EscapeNonAscii};
                var serialized = JsonConvert.SerializeObject(item, settings);
                dic = jss.Deserialize<Dictionary<string, object>>(serialized);

                var root = new SerializedTreeNode { Name = name };
                BuildTree(dic, root);
                return root;
            }
        }

        private static void BuildTree(object item, SerializedTreeNode node)
        {
            if (item is KeyValuePair<string, object>)
            {
                var kv = (KeyValuePair<string, object>)item;
                var keyValueNode = new SerializedTreeNode
                {
                    Name = kv.Key, 
                    Value = GetValueAsString(kv.Value)
                };
                node.Children.Add(keyValueNode);
                BuildTree(kv.Value, keyValueNode);
            }
            else if (item is ArrayList)
            {
                var list = (ArrayList)item;
                var index = 0;
                foreach (var value in list)
                {
                    var arrayItem = new SerializedTreeNode
                    {
                        Name = $"[{index}]", 
                        Value = IsPrimitive(value) ? value.ToString() : ""
                    };
                    node.Children.Add(arrayItem);
                    BuildTree(value, arrayItem);
                    index++;
                }
            }
            else if (item is Dictionary<string, object>)
            {
                var dictionary = (Dictionary<string, object>)item;
                foreach (var d in dictionary)
                    BuildTree(d, node);
            }
        }

        private static string GetValueAsString(object item)
        {
            if (item == null)
                return "null";
            var t = item.GetType();
            if (t.IsArray)
            {
                return "[]";
            }

            if (item is ArrayList)
            {
                var arr = item as ArrayList;
                return $"[{arr.Count}]";
            }

            if (t.IsGenericType)
            {
                return "{}";
            }

            return item.ToString();
        }

        private static bool IsPrimitive(object item)
        {
            var t = item.GetType();
            return t.IsPrimitive ||
                    t == typeof(System.String) ||
                    t == typeof(System.Int16) ||
                    t == typeof(System.Int32) ||
                    t == typeof(System.Int64) ||
                    t == typeof(System.Decimal) ||
                    t == typeof(System.Double) ||
                    t == typeof(System.DateTime) ||
                    t.IsEnum;
        }
    }
}