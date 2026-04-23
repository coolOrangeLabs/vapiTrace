using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace vapiTraceFiddlerExtension
{
    public partial class RequestUserControl : UserControl
    {
        private readonly IContainer components = null;
        private TreeView _requestTree;
        private RichTextBox txtCommand;

        private void InitializeComponent()
        {
            this._requestTree = new System.Windows.Forms.TreeView();
            this.commandSplitter = new System.Windows.Forms.Splitter();
            this.commandPanel = new System.Windows.Forms.Panel();
            this.txtCommand = new System.Windows.Forms.RichTextBox();
            this.commandButtonPanel = new System.Windows.Forms.Panel();
            this.btnCopyCommand = new System.Windows.Forms.Button();
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.panelSeperator = new System.Windows.Forms.Panel();
            this.commandPanel.SuspendLayout();
            this.commandButtonPanel.SuspendLayout();
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
            this._requestTree.Size = new System.Drawing.Size(800, 199);
            this._requestTree.TabIndex = 0;
            // 
            // commandSplitter
            // 
            this.commandSplitter.BackColor = System.Drawing.SystemColors.ControlDark;
            this.commandSplitter.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.commandSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.commandSplitter.Location = new System.Drawing.Point(0, 326);
            this.commandSplitter.MinExtra = 120;
            this.commandSplitter.MinSize = 80;
            this.commandSplitter.Name = "commandSplitter";
            this.commandSplitter.Size = new System.Drawing.Size(800, 6);
            this.commandSplitter.TabIndex = 7;
            this.commandSplitter.TabStop = false;
            // 
            // commandPanel
            // 
            this.commandPanel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.commandPanel.Controls.Add(this.txtCommand);
            this.commandPanel.Controls.Add(this.commandButtonPanel);
            this.commandPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.commandPanel.Location = new System.Drawing.Point(0, 332);
            this.commandPanel.Margin = new System.Windows.Forms.Padding(0);
            this.commandPanel.MinimumSize = new System.Drawing.Size(0, 80);
            this.commandPanel.Name = "commandPanel";
            this.commandPanel.Padding = new System.Windows.Forms.Padding(8);
            this.commandPanel.Size = new System.Drawing.Size(800, 160);
            this.commandPanel.TabIndex = 2;
            // 
            // txtCommand
            // 
            this.txtCommand.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtCommand.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCommand.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtCommand.DetectUrls = false;
            this.txtCommand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCommand.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCommand.Location = new System.Drawing.Point(8, 8);
            this.txtCommand.Margin = new System.Windows.Forms.Padding(0);
            this.txtCommand.Name = "txtCommand";
            this.txtCommand.ReadOnly = true;
            this.txtCommand.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
            this.txtCommand.ShortcutsEnabled = true;
            this.txtCommand.Size = new System.Drawing.Size(676, 144);
            this.txtCommand.TabIndex = 0;
            this.txtCommand.Text = "$vault.DocumentService.GetFileById($id)";
            this.txtCommand.WordWrap = false;
            // 
            // commandButtonPanel
            // 
            this.commandButtonPanel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.commandButtonPanel.Controls.Add(this.btnCopyCommand);
            this.commandButtonPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.commandButtonPanel.Location = new System.Drawing.Point(684, 8);
            this.commandButtonPanel.Margin = new System.Windows.Forms.Padding(0);
            this.commandButtonPanel.Name = "commandButtonPanel";
            this.commandButtonPanel.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.commandButtonPanel.Size = new System.Drawing.Size(108, 144);
            this.commandButtonPanel.TabIndex = 1;
            // 
            // btnCopyCommand
            // 
            this.btnCopyCommand.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnCopyCommand.Location = new System.Drawing.Point(8, 0);
            this.btnCopyCommand.Margin = new System.Windows.Forms.Padding(4);
            this.btnCopyCommand.Name = "btnCopyCommand";
            this.btnCopyCommand.Size = new System.Drawing.Size(100, 32);
            this.btnCopyCommand.TabIndex = 1;
            this.btnCopyCommand.Text = "Copy Code";
            this.btnCopyCommand.UseVisualStyleBackColor = true;
            this.btnCopyCommand.Click += new System.EventHandler(this.OnCopyCommandClick);
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
            this.Controls.Add(this.commandSplitter);
            this.Controls.Add(this.commandPanel);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "RequestUserControl";
            this.Size = new System.Drawing.Size(800, 492);
            this.SizeChanged += new System.EventHandler(this.UserControl_SizeChanged);
            this.commandPanel.ResumeLayout(false);
            this.commandButtonPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();

            base.Dispose(disposing);
        }

        private WebBrowser webBrowser;
        private Panel panelSeperator;
        private Splitter commandSplitter;
        private Panel commandPanel;
        private Panel commandButtonPanel;
        private Button btnCopyCommand;
    }
}
