using System;
using System.Drawing;
using Fiddler;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using vapiTraceFiddlerExtension.Properties;

namespace vapiTraceFiddlerExtension
{
	public class VapiTraceRequest : Inspector2, IRequestInspector2, IBaseInspector2
	{
		private VapiUserControl _requestUserControl;

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
                var error = "No valid Autodesk Vault SOAP request";
                if (value == null)
                {
                    _requestUserControl.Clear(error);
                    return;
                }

				var str = Encoding.UTF8.GetString(value);
				if (str.IndexOf("<s:Envelope", StringComparison.Ordinal) <= 0)
				{
					_requestUserControl.Clear(error);
				}
				else
				{
					str = str.Substring(str.IndexOf("<s:Envelope", StringComparison.Ordinal));
					str = string.Concat(str.Substring(0, str.IndexOf("</s:Envelope>", StringComparison.Ordinal)), "</s:Envelope>");
					var xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(str);
					_requestUserControl.SetData("REQUEST TO VAULT", xmlDocument, _service, $"Vault {_vaultVersion} - Server: {_host} - Database: {_vaultName}");
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
                var headers = value;
                if (headers.Exists("SOAPAction"))
                {
					var action = headers["SOAPAction"];
					_host = headers["Host"];
					if (action.Contains("AutodeskDM/Services") || action.Contains("AutodeskDM/Filestore"))
                    {
						var parts = action.Trim('\\', '"').Split('/');
                        _vaultVersion = parts[parts.Length - 3];
                        _service = parts[parts.Length - 2];
						_vaultName = System.Web.HttpUtility.ParseQueryString(headers.RequestPath).Get("vaultName");
					}
					else
                        _service = null;
				}
			}
		}

		public VapiTraceRequest()
		{
		}

		public override void AddToTab(TabPage o)
		{
			_requestUserControl = new VapiUserControl()
			{
				Dock = DockStyle.Fill
			};
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