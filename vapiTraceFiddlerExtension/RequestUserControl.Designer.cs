using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace vapiTraceFiddlerExtension
{
    public partial class RequestUserControl : UserControl
    {
        private readonly IContainer components = null;
        private TreeView _requestTree;
        private TextBox txtCommand;

        private void InitializeComponent()
        {
            this._requestTree = new System.Windows.Forms.TreeView();
            this.txtCommand = new System.Windows.Forms.TextBox();
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.panelSeperator = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // _requestTree
            // 
            this._requestTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._requestTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this._requestTree.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._requestTree.ForeColor = System.Drawing.SystemColors.ControlText;
            this._requestTree.Location = new System.Drawing.Point(0, 127);
            this._requestTree.Margin = new System.Windows.Forms.Padding(4);
            this._requestTree.Name = "_requestTree";
            this._requestTree.Size = new System.Drawing.Size(800, 343);
            this._requestTree.TabIndex = 0;
            // 
            // txtCommand
            // 
            this.txtCommand.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtCommand.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCommand.Location = new System.Drawing.Point(0, 470);
            this.txtCommand.Margin = new System.Windows.Forms.Padding(4);
            this.txtCommand.Name = "txtCommand";
            this.txtCommand.ReadOnly = true;
            this.txtCommand.Size = new System.Drawing.Size(800, 22);
            this.txtCommand.TabIndex = 2;
            this.txtCommand.Text = "$vault.DocumentService.GetFileById($id)";
            // 
            // webBrowser
            // 
            this.webBrowser.AllowWebBrowserDrop = false;
            this.webBrowser.Dock = System.Windows.Forms.DockStyle.Top;
            this.webBrowser.IsWebBrowserContextMenuEnabled = false;
            this.webBrowser.Location = new System.Drawing.Point(0, 0);
            this.webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.ScriptErrorsSuppressed = true;
            this.webBrowser.ScrollBarsEnabled = false;
            this.webBrowser.Size = new System.Drawing.Size(800, 125);
            this.webBrowser.TabIndex = 5;
            this.webBrowser.WebBrowserShortcutsEnabled = false;
            this.webBrowser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.OnWebBrowserDocumentCompleted);
            this.webBrowser.NewWindow += new System.ComponentModel.CancelEventHandler(this.OnWebBrowserNewWindow);
            // 
            // panelSeperator
            // 
            this.panelSeperator.BackColor = System.Drawing.SystemColors.Control;
            this.panelSeperator.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSeperator.Location = new System.Drawing.Point(0, 125);
            this.panelSeperator.Margin = new System.Windows.Forms.Padding(4);
            this.panelSeperator.Name = "panelSeperator";
            this.panelSeperator.Size = new System.Drawing.Size(800, 2);
            this.panelSeperator.TabIndex = 6;
            this.panelSeperator.Visible = false;
            // 
            // RequestUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._requestTree);
            this.Controls.Add(this.panelSeperator);
            this.Controls.Add(this.webBrowser);
            this.Controls.Add(this.txtCommand);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "RequestUserControl";
            this.Size = new System.Drawing.Size(800, 492);
            this.SizeChanged += new System.EventHandler(this.UserControl_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();

            base.Dispose(disposing);
        }

        private WebBrowser webBrowser;
        private Panel panelSeperator;
    }
}