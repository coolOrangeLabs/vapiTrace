using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace vapiTraceFiddlerExtension
{
    public partial class ResponseUserControl
    {
        public ResponseUserControl()
        {
            InitializeComponent();
        }

        public void SetData(string header, SerializedTreeNode treeNode = null)
        {
            _responseTree.BeginUpdate();
            _responseTree.Nodes.Clear();
            lblHeader.Text = header;
            if (treeNode != null)
            {
                var firstNode = _responseTree.Nodes.Add(treeNode.ToString());
                AddElements(firstNode, treeNode);
                firstNode.Expand();
                _responseTree.SelectedNode = _responseTree.Nodes[0];
            }
            _responseTree.EndUpdate();
        }

        public void SetData(string header, List<SerializedTreeNode> treeNodes)
        {
            _responseTree.BeginUpdate();
            _responseTree.Nodes.Clear();
            lblHeader.Text = header;
            if (treeNodes != null && treeNodes.Count > 0)
            {
                foreach (var treeNode in treeNodes)
                {
                    var firstNode = _responseTree.Nodes.Add(treeNode.Name);
                    AddElements(firstNode, treeNode);
                    firstNode.Expand();
                }
                _responseTree.SelectedNode = _responseTree.Nodes[0];
            }
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

        private void LogoClick(object sender, EventArgs e)
        {
            Process.Start("https://www.coolorange.com");
        }
    }
}