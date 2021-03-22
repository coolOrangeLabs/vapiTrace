using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace vapiTraceFiddlerExtension
{
    public partial class RequestUserControl
    {
        public RequestUserControl()
        {
            InitializeComponent();
        }

        public void SetData(string header, XmlDocument xml = null, string service = null)
        {
            _requestTree.BeginUpdate();
            _requestTree.Nodes.Clear();
            lblHeader.Text = header;

            txtCommand.Text = string.Empty;

            if (xml != null && service != null)
            {
                var xmlNamespaceManagers = new XmlNamespaceManager(xml.NameTable);
                xmlNamespaceManagers.AddNamespace(xml.FirstChild.Prefix, xml.FirstChild.NamespaceURI);
                var bodyNode = xml.DocumentElement?.SelectSingleNode("//s:Body", xmlNamespaceManagers);
                if (bodyNode != null)
                {
                    var firstNode = _requestTree.Nodes.Add(service);
                    AddElements(firstNode, bodyNode.FirstChild, out var method);
                    _requestTree.ExpandAll();
                    _requestTree.SelectedNode = _requestTree.Nodes[0];

                    var info = VaultAssembly.GetServiceType(service);
                    var methodInfo = info.GetMethod(method);
                    var parameterInfos = methodInfo.GetParameters();
                    var parameters = parameterInfos.Select(p => p.Name).ToList();
                    // TODO: get parameter types and display it in the UI
                    // https://stackoverflow.com/questions/15602606/programmatically-get-summary-comments-at-runtime
                    txtCommand.Text = GenerateCommand(service, method, parameters);
                }
            }
            _requestTree.EndUpdate();
        }

        private string GenerateCommand(string service, string method, IEnumerable<string> parameters)
        {
            var arguments = "";
            foreach (var parameter in parameters)
                arguments += "$" + parameter + ", ";
            arguments = arguments.TrimEnd(',', ' ');

            return "$vault." + service + "." + method + "(" + arguments + ")";
        }

        private void AddElements(TreeNode tNode, XmlNode xNode, out string method)
        {
            method = "";
            if (xNode.NodeType == XmlNodeType.Element)
            {
                var name = xNode.Name;
                method = name;
                if (xNode.InnerText.Length > 0 && xNode.HasChildNodes && xNode.FirstChild.NodeType == XmlNodeType.Text)
                    name = string.Concat(name, " = ", xNode.InnerText);

                TreeNode nextNode;
                if ((name.EndsWith("Response") || name.EndsWith("Result")))
                {
                    nextNode = tNode;
                }
                else
                {
                    nextNode = tNode.Nodes.Add(name);
                    if (xNode.Attributes != null)
                    {
                        foreach (XmlAttribute attribute in xNode.Attributes)
                        {
                            if (attribute.Name.Equals("xmlns"))
                                continue;

                            nextNode.Nodes.Add($"{attribute.Name} = {attribute.InnerText}");
                        }
                    }
                }

                foreach (XmlNode childNode in xNode.ChildNodes)
                    AddElements(nextNode, childNode, out _);
            }
        }

        private void LogoClick(object sender, EventArgs e)
        {
            Process.Start("https://www.coolorange.com");
        }
    }
}