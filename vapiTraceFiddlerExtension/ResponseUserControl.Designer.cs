using System.ComponentModel;
using System.Windows.Forms;

namespace vapiTraceFiddlerExtension
{
    public partial class ResponseUserControl : UserControl
    {
        private readonly IContainer components = null;
        private TreeView _responseTree;

        private void InitializeComponent()
        {
            this._responseTree = new System.Windows.Forms.TreeView();
            this.panelSeperator = new System.Windows.Forms.Panel();
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // _responseTree
            // 
            this._responseTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._responseTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this._responseTree.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._responseTree.ForeColor = System.Drawing.SystemColors.ControlText;
            this._responseTree.Location = new System.Drawing.Point(0, 102);
            this._responseTree.Margin = new System.Windows.Forms.Padding(4);
            this._responseTree.Name = "_responseTree";
            this._responseTree.Size = new System.Drawing.Size(640, 292);
            this._responseTree.TabIndex = 5;
            // 
            // panelSeperator
            // 
            this.panelSeperator.BackColor = System.Drawing.SystemColors.Control;
            this.panelSeperator.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSeperator.Location = new System.Drawing.Point(0, 125);
            this.panelSeperator.Margin = new System.Windows.Forms.Padding(4);
            this.panelSeperator.Name = "panelSeperator";
            this.panelSeperator.Size = new System.Drawing.Size(640, 2);
            this.panelSeperator.TabIndex = 5;
            this.panelSeperator.Visible = false;
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
            this.webBrowser.TabIndex = 6;
            this.webBrowser.WebBrowserShortcutsEnabled = false;
            this.webBrowser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.OnWebBrowserDocumentCompleted);
            this.webBrowser.NewWindow += new System.ComponentModel.CancelEventHandler(this.OnWebBrowserNewWindow);
            // 
            // ResponseUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._responseTree);
            this.Controls.Add(this.panelSeperator);
            this.Controls.Add(this.webBrowser);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ResponseUserControl";
            this.Size = new System.Drawing.Size(800, 492);
            this.SizeChanged += new System.EventHandler(this.UserControl_SizeChanged);
            this.ResumeLayout(false);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();

            base.Dispose(disposing);
        }
        private Panel panelSeperator;
        private WebBrowser webBrowser;
    }
}