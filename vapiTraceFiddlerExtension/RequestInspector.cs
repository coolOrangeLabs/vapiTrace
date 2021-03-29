using System;
using System.Drawing;
using Fiddler;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using vapiTraceFiddlerExtension.Properties;

namespace vapiTraceFiddlerExtension
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once RedundantExtendsListEntry
	public class RequestInspector : Inspector2, IRequestInspector2, IBaseInspector2
	{
		private RequestUserControl _requestUserControl;

        private string _vaultVersion;
		private string _service;
        private string _host;
        private string _vaultName;

		public bool bDirty => false;

        public byte[] body
		{
			get => null;
            set
            {
                if (value == null)
                {
                    _requestUserControl.Clear();
                    return;
                }

				var s = Encoding.UTF8.GetString(value);
				if (s.IndexOf("<s:Envelope", StringComparison.Ordinal) <= 0)
				{
					_requestUserControl.Clear();
				}
				else
				{
					s = s.Substring(s.IndexOf("<s:Envelope", StringComparison.Ordinal));
					s = string.Concat(s.Substring(0, s.IndexOf("</s:Envelope>", StringComparison.Ordinal)), "</s:Envelope>");
					var xml = new XmlDocument();
					xml.LoadXml(s);

                    //var t = $"Database: {_vaultName}, Server: {_host} (Vault {_vaultVersion})";
					_requestUserControl.SetData(xml, _service);
				}
			}
		}

		public bool bReadOnly
		{
			get;
			set;
		}

		public HTTPRequestHeaders headers
		{
            get => null;
            set
            {
                if (value.Exists("SOAPAction"))
                {
					var action = value["SOAPAction"];
					_host = value["Host"];
					if (action.Contains("AutodeskDM/Services") || action.Contains("AutodeskDM/Filestore"))
                    {
						var parts = action.Trim('\\', '"').Split('/');
                        _vaultVersion = parts[parts.Length - 3];
                        _service = parts[parts.Length - 2];
						_vaultName = System.Web.HttpUtility.ParseQueryString(value.RequestPath).Get("vaultName");
					}
					else
                        _service = null;
				}
			}
		}

		public RequestInspector()
        {
            if (!VaultAssembly.IsVaultDllPresent)
            {
                throw new DllNotFoundException(
					"Autodesk.Connectivity.WebServices.dll cannot be found. Please install Vault");
            }
        }

		public override void AddToTab(TabPage o)
		{
			_requestUserControl = new RequestUserControl { Dock = DockStyle.Fill };
			o.Controls.Add(_requestUserControl);
			o.Text = @"vapiTrace";

            var icon = new Icon(Resources.coolorange, 16, 16).ToBitmap();
			FiddlerApplication.UI.imglSessionIcons.Images.Add(icon);
			o.ImageIndex = FiddlerApplication.UI.imglSessionIcons.Images.Count - 1;
			FiddlerApplication.UI.tabsViews.TabPages.Add(o);
		}

		public void Clear()
		{
		}

		public override int GetOrder()
		{
			return 0;
		}
	}
}