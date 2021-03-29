using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace vapiTraceFiddlerExtension
{
    public partial class ResponseUserControl
    {
        private const string SoapError = "No valid Autodesk Vault SOAP response";

        public ResponseUserControl()
        {
            InitializeComponent();
        }

        public void Clear(string error = null)
        {
            _responseTree.BeginUpdate();
            _responseTree.Nodes.Clear();

            if (string.IsNullOrEmpty(error))
                error = SoapError;

            webBrowser.DocumentText = VaultAssembly.Html(error, string.Empty, string.Empty, string.Empty);
            _responseTree.EndUpdate();
        }

        public void SetData(string header, SerializedTreeNode treeNode = null, Type type = null)
        {
            _responseTree.BeginUpdate();
            _responseTree.Nodes.Clear();

            var documentation = "";
            var command = "";

            if (type != null)
                documentation = type.GetDocumentation();

            if (treeNode != null)
            {
                command = "Response";

                var firstNode = _responseTree.Nodes.Add(treeNode.ToString());
                AddElements(firstNode, treeNode);
                firstNode.Expand();
                _responseTree.SelectedNode = _responseTree.Nodes[0];
            }

            webBrowser.DocumentText = VaultAssembly.Html("Response Type", header, documentation, command);
            _responseTree.EndUpdate();
        }

        public void SetData(string header, List<SerializedTreeNode> treeNodes, Type type = null)
        {
            _responseTree.BeginUpdate();
            _responseTree.Nodes.Clear();

            var documentation = "";
            var command = "";
            
            if (type != null)
                documentation = type.GetDocumentation();

            if (treeNodes != null && treeNodes.Count > 0)
            {
                command = "Response";

                foreach (var treeNode in treeNodes)
                {
                    var firstNode = _responseTree.Nodes.Add(treeNode.Name);
                    AddElements(firstNode, treeNode);
                    firstNode.Expand();
                }
                _responseTree.SelectedNode = _responseTree.Nodes[0];
            }

            webBrowser.DocumentText = VaultAssembly.Html("Response Type", header, documentation, command);
            _responseTree.EndUpdate();
        }

        private void AddElements(TreeNode node, SerializedTreeNode tree)
        {
            foreach (var childTree in tree.Children)
            {
                var nextNode = node.Nodes.Add(childTree.ToString());
                AddElements(nextNode, childTree);
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
                var events = (mshtml.HTMLDocumentEvents2_Event)webBrowser.Document.DomDocument;
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