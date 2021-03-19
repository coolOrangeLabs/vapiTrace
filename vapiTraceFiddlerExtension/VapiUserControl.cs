using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;

namespace vapiTraceFiddlerExtension
{
    public class VapiUserControl : UserControl
    {
        private readonly IContainer components = null;
        private TreeView _requestTree;
        private TextBox txtCommand;
        private Panel panelHeader;
        private Label lblHeader;
        private Panel panelSeperator;
        private PictureBox pictureLogo;

        public VapiUserControl()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VapiUserControl));
            this._requestTree = new System.Windows.Forms.TreeView();
            this.txtCommand = new System.Windows.Forms.TextBox();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblHeader = new System.Windows.Forms.Label();
            this.pictureLogo = new System.Windows.Forms.PictureBox();
            this.panelSeperator = new System.Windows.Forms.Panel();
            this.panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // _requestTree
            // 
            this._requestTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._requestTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this._requestTree.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._requestTree.ForeColor = System.Drawing.SystemColors.ControlText;
            this._requestTree.Location = new System.Drawing.Point(0, 46);
            this._requestTree.Name = "_requestTree";
            this._requestTree.Size = new System.Drawing.Size(600, 332);
            this._requestTree.TabIndex = 0;
            // 
            // txtCommand
            // 
            this.txtCommand.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtCommand.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCommand.Location = new System.Drawing.Point(0, 378);
            this.txtCommand.Name = "txtCommand";
            this.txtCommand.ReadOnly = true;
            this.txtCommand.Size = new System.Drawing.Size(600, 22);
            this.txtCommand.TabIndex = 2;
            this.txtCommand.Text = "$vault.DocumentService.GetFileById($id)";
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.SystemColors.Window;
            this.panelHeader.Controls.Add(this.lblHeader);
            this.panelHeader.Controls.Add(this.pictureLogo);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(600, 42);
            this.panelHeader.TabIndex = 3;
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblHeader.Location = new System.Drawing.Point(11, 13);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(231, 17);
            this.lblHeader.TabIndex = 2;
            this.lblHeader.Text = "No valid Autodesk Vault SOAP request";
            // 
            // pictureLogo
            // 
            this.pictureLogo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureLogo.BackColor = System.Drawing.Color.White;
            this.pictureLogo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureLogo.Image = ((System.Drawing.Image)(resources.GetObject("pictureLogo.Image")));
            this.pictureLogo.Location = new System.Drawing.Point(414, 5);
            this.pictureLogo.Name = "pictureLogo";
            this.pictureLogo.Size = new System.Drawing.Size(179, 32);
            this.pictureLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureLogo.TabIndex = 1;
            this.pictureLogo.TabStop = false;
            this.pictureLogo.Click += new System.EventHandler(this.LogoClick);
            // 
            // panelSeperator
            // 
            this.panelSeperator.BackColor = System.Drawing.SystemColors.Control;
            this.panelSeperator.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSeperator.Location = new System.Drawing.Point(0, 42);
            this.panelSeperator.Name = "panelSeperator";
            this.panelSeperator.Size = new System.Drawing.Size(600, 4);
            this.panelSeperator.TabIndex = 4;
            // 
            // VapiUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._requestTree);
            this.Controls.Add(this.panelSeperator);
            this.Controls.Add(this.txtCommand);
            this.Controls.Add(this.panelHeader);
            this.Name = "VapiUserControl";
            this.Size = new System.Drawing.Size(600, 400);
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();

            base.Dispose(disposing);
        }

        public void Clear(string header = null)
        {
            _requestTree.Nodes.Clear();
            UpdateUserControl(header, string.Empty);
        }

        public void SetData(string rootText, XmlDocument xml, string service = null, string header = null)
        {
            _requestTree.Nodes.Clear();

            var xmlNamespaceManagers = new XmlNamespaceManager(xml.NameTable);
            xmlNamespaceManagers.AddNamespace(xml.FirstChild.Prefix, xml.FirstChild.NamespaceURI);
            var xmlNodes = xml.DocumentElement?.SelectSingleNode("//s:Body", xmlNamespaceManagers);
            if (xmlNodes != null)
            {
                var treeNode = _requestTree.Nodes.Add(rootText);
                TreeNode rootNode;
                if (service != null)
                    rootNode = treeNode.Nodes.Add(service);
                else
                    rootNode = treeNode;

                AddElements(rootNode, xmlNodes.FirstChild, (service == null), out var method, out var parameters);
                treeNode.ExpandAll();
                _requestTree.SelectedNode = treeNode;

                if (service != null)
                    UpdateUserControl(header, GenerateCommand(service, method, parameters));
                else
                    UpdateUserControl(string.Empty, string.Empty);
            }
        }

        private void UpdateUserControl(string header, string command)
        {
            var visible = !string.IsNullOrEmpty(header);
            
            lblHeader.Text = header;
            txtCommand.Text = command;
            txtCommand.Visible = visible;
            panelHeader.Visible = visible;
            panelSeperator.Visible = visible;
        }

        private string GenerateCommand(string service, string method, string[] parameters)
        {
            var arguments = "";
            foreach (var parameter in parameters)
                arguments += "$" + parameter + ", ";
            arguments = arguments.TrimEnd(',', ' ');

            return "$vault." + service + "." + method + "(" + arguments + ")";
        }

        private void AddElements(TreeNode tNode, XmlNode xNode, bool ignoreResponseFrame, out string method, out string[] parameters)
        {
            var arguments = new List<string>();
            method = "";
            if (xNode.NodeType == XmlNodeType.Element)
            {
                var name = xNode.Name;
                method = name;
                if (xNode.InnerText.Length > 0 && xNode.HasChildNodes && xNode.FirstChild.NodeType == XmlNodeType.Text)
                {
                    if (ignoreResponseFrame && name.EndsWith("Result"))
                        name = xNode.InnerText;
                    else
                        name = string.Concat(name, " = ", xNode.InnerText);
                }

                TreeNode nextNode;
                if (ignoreResponseFrame && (name.EndsWith("Response") || name.EndsWith("Result")))
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
                {
                    AddElements(nextNode, childNode, ignoreResponseFrame, out _, out _);
                    arguments.Add(childNode.Name);
                }
            }
            parameters = arguments.ToArray();
        }

        private void LogoClick(object sender, EventArgs e)
        {
            Process.Start("https://www.coolorange.com");
        }
    }
}