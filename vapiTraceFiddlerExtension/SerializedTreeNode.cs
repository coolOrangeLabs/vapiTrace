using System.Collections;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Xml;

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
            if (item == null)
                return new SerializedTreeNode { Name = name, Value = "null" };

            if (IsPrimitive(item))
            {
                return new SerializedTreeNode { Name = name, Value = item.ToString() };
            }

            var jss = new JavaScriptSerializer();
            var serialized = jss.Serialize(item);
            var deserialized = jss.DeserializeObject(serialized);

            var root = new SerializedTreeNode { Name = name };
            BuildTree(deserialized, root);
            return root;
        }

        public static SerializedTreeNode CreateTree(XmlNode node, string name = null)
        {
            if (node == null)
                return null;

            var root = new SerializedTreeNode { Name = string.IsNullOrEmpty(name) ? node.LocalName : name };
            PopulateTree(node, root);
            return root;
        }

        private static void BuildTree(object item, SerializedTreeNode node)
        {
            if (item is Dictionary<string, object> dictionary)
            {
                foreach (var kv in dictionary)
                {
                    var keyValueNode = new SerializedTreeNode
                    {
                        Name = kv.Key,
                        Value = GetValueAsString(kv.Value)
                    };
                    node.Children.Add(keyValueNode);
                    BuildTree(kv.Value, keyValueNode);
                }
            }
            else if (item is ArrayList list)
            {
                var index = 0;
                foreach (var value in list)
                {
                    var arrayItem = new SerializedTreeNode
                    {
                        Name = $"[{index}]",
                        Value = IsPrimitive(value) ? GetValueAsString(value) : string.Empty
                    };
                    node.Children.Add(arrayItem);
                    BuildTree(value, arrayItem);
                    index++;
                }
            }
        }

        private static string GetValueAsString(object item)
        {
            if (item == null)
                return "null";

            var t = item.GetType();
            if (t.IsArray)
                return "[]";

            if (item is ArrayList arr)
                return $"[{arr.Count}]";

            if (t.IsGenericType)
                return "{}";

            return item.ToString();
        }

        private static bool IsPrimitive(object item)
        {
            if (item == null)
                return true;

            var t = item.GetType();
            return t.IsPrimitive ||
                   t == typeof(string) ||
                   t == typeof(short) ||
                   t == typeof(int) ||
                   t == typeof(long) ||
                   t == typeof(decimal) ||
                   t == typeof(double) ||
                   t == typeof(System.DateTime) ||
                   t.IsEnum;
        }

        private static void PopulateTree(XmlNode source, SerializedTreeNode target)
        {
            if (source.Attributes != null)
            {
                foreach (XmlAttribute attribute in source.Attributes)
                {
                    if (attribute.Name.StartsWith("xmlns"))
                        continue;

                    target.Children.Add(new SerializedTreeNode
                    {
                        Name = attribute.Name,
                        Value = attribute.Value
                    });
                }
            }

            var hasElementChildren = false;
            foreach (XmlNode child in source.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element)
                    continue;

                hasElementChildren = true;
                var childTree = new SerializedTreeNode { Name = child.LocalName };
                PopulateTree(child, childTree);
                if (childTree.Children.Count == 0 && string.IsNullOrWhiteSpace(childTree.Value))
                    childTree.Value = child.InnerText;

                target.Children.Add(childTree);
            }

            if (!hasElementChildren)
            {
                var value = source.InnerText;
                if (!string.IsNullOrWhiteSpace(value))
                    target.Value = value;
            }
        }
    }
}
