using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace vapiTraceFiddlerExtension
{
    public class InspectorUC : UserControl
    {
        private readonly IContainer components = null;
        private TreeView _requestTree;
        private PictureBox _coLogo;

        public InspectorUC()
        {
            InitializeComponent();
        }

        private void AddElements(TreeNode tNode, XmlNode xNode)
        {
            if (xNode.NodeType == XmlNodeType.Element)
            {
                string name = xNode.Name;
                if (xNode.InnerText == null || xNode.InnerText.Length <= 0 || !xNode.HasChildNodes ? false : xNode.FirstChild.NodeType == XmlNodeType.Text)
                {
                    name = string.Concat(name, " = ", xNode.InnerText);
                }
                TreeNode treeNode = tNode.Nodes.Add(name);
                if (xNode.Attributes != null)
                {
                    foreach (XmlAttribute attribute in xNode.Attributes)
                    {
                        if (attribute.Name.Equals("xmlns"))
                        {
                            continue;
                        }
                        treeNode.Nodes.Add($"{attribute.Name} = {attribute.InnerText}");
                    }
                }
                foreach (XmlNode childNode in xNode.ChildNodes)
                {
                    AddElements(treeNode, childNode);
                }
            }
        }

        public void Clear()
        {
            _requestTree.Nodes.Clear();
        }

        private void coLogo_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.coolorange.com");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InspectorUC));
            this._requestTree = new System.Windows.Forms.TreeView();
            this._coLogo = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this._coLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // _requestTree
            // 
            this._requestTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this._requestTree.Location = new System.Drawing.Point(0, 0);
            this._requestTree.Name = "_requestTree";
            this._requestTree.Size = new System.Drawing.Size(386, 280);
            this._requestTree.TabIndex = 0;
            this._requestTree.Resize += new System.EventHandler(this.InspectorRequest_Resize);
            // 
            // _coLogo
            // 
            this._coLogo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._coLogo.BackColor = System.Drawing.Color.White;
            this._coLogo.Cursor = System.Windows.Forms.Cursors.Hand;
            this._coLogo.Image = ((System.Drawing.Image)(resources.GetObject("_coLogo.Image")));
            this._coLogo.Location = new System.Drawing.Point(330, 8);
            this._coLogo.Name = "_coLogo";
            this._coLogo.Size = new System.Drawing.Size(48, 48);
            this._coLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this._coLogo.TabIndex = 1;
            this._coLogo.TabStop = false;
            this._coLogo.Click += new System.EventHandler(this.coLogo_Click);
            // 
            // InspectorUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._coLogo);
            this.Controls.Add(this._requestTree);
            this.Name = "InspectorUC";
            this.Size = new System.Drawing.Size(386, 280);
            ((System.ComponentModel.ISupportInitialize)(this._coLogo)).EndInit();
            this.ResumeLayout(false);

        }

        private void InspectorRequest_Resize(object sender, EventArgs e)
        {
            //int width = Width - 73;
            //_coLogo.Location = new Point(width, 3);
        }

        public void SetData(string rootText, XmlDocument xml)
        {
            _requestTree.Nodes.Clear();

            XmlNamespaceManager xmlNamespaceManagers = new XmlNamespaceManager(xml.NameTable);
            xmlNamespaceManagers.AddNamespace(xml.FirstChild.Prefix, xml.FirstChild.NamespaceURI);
            XmlNode xmlNodes = xml.DocumentElement?.SelectSingleNode("//s:Body", xmlNamespaceManagers);
            if (xmlNodes != null)
            {
                TreeNode treeNode = _requestTree.Nodes.Add(rootText);
                AddElements(treeNode, xmlNodes.FirstChild);
                treeNode.ExpandAll();
                _requestTree.SelectedNode = treeNode;
            }
        }
    }
}