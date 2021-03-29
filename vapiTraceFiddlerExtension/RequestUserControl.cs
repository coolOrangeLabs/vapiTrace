using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace vapiTraceFiddlerExtension
{
    public partial class RequestUserControl
    {
        private const string SoapError = "No valid Autodesk Vault SOAP request";

        public RequestUserControl()
        {
            InitializeComponent();
        }

        public void Clear(string error = null)
        {
            _requestTree.BeginUpdate();
            _requestTree.Nodes.Clear();

            if (string.IsNullOrEmpty(error))
                error = SoapError;

            txtCommand.Text = string.Empty;
            webBrowser.DocumentText = VaultAssembly.Html(error, string.Empty, string.Empty, string.Empty);

            _requestTree.EndUpdate();
            if (_requestTree.Nodes.Count > 0)
                _requestTree.SelectedNode = _requestTree.Nodes[0];
        }

        public void SetData(XmlDocument xml = null, string service = null)
        {
            _requestTree.BeginUpdate();
            _requestTree.Nodes.Clear(); 
            
            txtCommand.Text = string.Empty;
            var h1 = "";

            var documentation = "";
            var command = "";
            var parameterList = new List<string>();

            if (xml != null && service != null)
            {
                var xmlNamespaceManagers = new XmlNamespaceManager(xml.NameTable);
                xmlNamespaceManagers.AddNamespace(xml.FirstChild.Prefix, xml.FirstChild.NamespaceURI);
                var bodyNode = xml.DocumentElement?.SelectSingleNode("//s:Body", xmlNamespaceManagers);
                if (bodyNode != null)
                {
                    var firstNode = _requestTree.Nodes.Add(service);
                    var methodNode = bodyNode.FirstChild;
                    var method = methodNode.Name;
                    AddElements(firstNode, methodNode);
                    _requestTree.ExpandAll();

                    var info = VaultAssembly.GetServiceType(service);
                    var methodInfo = info.GetMethod(method);
                    if (methodInfo != null)
                    {
                        h1 = methodInfo.Name;
                        documentation = methodInfo.GetDocumentation();
                        command = "Request";

                        var parameterInfos = methodInfo.GetParameters();
                        txtCommand.Text = GenerateCommand(service, method, parameterInfos);

                        foreach (var parameterInfo in parameterInfos)
                        {
                            var parameterName = parameterInfo.Name;
                            var parameterTypeName = parameterInfo.ParameterType.FullName;
                            parameterList.Add($"[{parameterTypeName}] ${parameterName}");
                        }
                    }
                }
            }

            if (parameterList.Count > 0)
            {
                var s = "<parameter>";
                foreach (var p in parameterList)
                    s += p + "<br/>";
                s += "</parameter>";

                documentation += s;
            }

            webBrowser.DocumentText = VaultAssembly.Html(h1 + " Method", service, documentation, command);

            _requestTree.EndUpdate();
            if (_requestTree.Nodes.Count > 0)
                _requestTree.SelectedNode = _requestTree.Nodes[0];
        }

        private string GenerateCommand(string service, string method, ParameterInfo[] parameterInfos)
        {
            var parameter = "";
            foreach (var parameterInfo in parameterInfos)
                parameter += "$" + parameterInfo.Name + ", ";
            parameter = parameter.TrimEnd(',', ' ');

            return "$vault." + service + "." + method + "(" + parameter + ")";
        }

        private void AddElements(TreeNode tNode, XmlNode xNode)
        {
            if (xNode.NodeType == XmlNodeType.Element)
            {
                var name = xNode.Name;
                if (xNode.InnerText.Length > 0 && xNode.HasChildNodes && xNode.FirstChild.NodeType == XmlNodeType.Text)
                    name = string.Concat(name, " = ", xNode.InnerText);

                var nextNode = tNode.Nodes.Add(name);
                if (xNode.Attributes != null)
                {
                    foreach (XmlAttribute attribute in xNode.Attributes)
                    {
                        if (attribute.Name.Equals("xmlns"))
                            continue;

                        nextNode.Nodes.Add($"{attribute.Name} = {attribute.InnerText}");
                    }
                }

                foreach (XmlNode childNode in xNode.ChildNodes)
                    AddElements(nextNode, childNode);
            }
        }

        #region WebBrowser
        private void UserControl_SizeChanged(object sender, EventArgs e)
        {
            webBrowser.Refresh();
        }

        private void OnWebBrowserDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser.Document?.Body != null)
            {
                webBrowser.Document.Body.KeyDown -= OnWebBrowserKeyDown;
                webBrowser.Document.Body.KeyDown += OnWebBrowserKeyDown;
                var events = (mshtml.HTMLDocumentEvents2_Event) webBrowser.Document.DomDocument;
                events.onmousewheel -= OnWebBrowserMouseWheel;
                events.onmousewheel += OnWebBrowserMouseWheel;

                var size = new Size(
                    webBrowser.Document.Body.ScrollRectangle.Size.Width,
                    webBrowser.Document.Body.ScrollRectangle.Size.Height);
                size.Height += 6;

                webBrowser.Size = size;
            }
        }

        private void OnWebBrowserNewWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Process.Start("https://www.coolorange.com");
        }

        private void OnWebBrowserKeyDown(object sender, HtmlElementEventArgs e)
        {
            if ((e.KeyPressedCode == 109 && e.CtrlKeyPressed) ||
                (e.KeyPressedCode == 107 && e.CtrlKeyPressed) ||
                (e.CtrlKeyPressed && e.KeyPressedCode == 187) ||
                (e.CtrlKeyPressed && e.KeyPressedCode == 189))
            {
                e.ReturnValue = false;
            }
        }

        private bool OnWebBrowserMouseWheel(mshtml.IHTMLEventObj obj)
        {
            if (obj.ctrlKey)
            {
                obj.cancelBubble = true;
                obj.returnValue = false;
                return false;
            }
            return true;
        }
        #endregion
    }
}