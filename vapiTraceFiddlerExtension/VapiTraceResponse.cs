using Fiddler;
using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using vapiTraceFiddlerExtension.Properties;

namespace vapiTraceFiddlerExtension
{
	public class VapiTraceResponse : Inspector2, IResponseInspector2, IBaseInspector2
	{
		private VapiUserControl _responseUserControl;

		public bool bDirty => false;

        public byte[] body
		{
			get => null;
            set
			{
                if (value == null)
                {
                    _responseUserControl.Clear();
					return;
				}

				var str = Encoding.UTF8.GetString(value);
				if (str.IndexOf("<s:Envelope", StringComparison.Ordinal) <= 0)
				{
					_responseUserControl.Clear();
				}
				else
				{
					str = str.Substring(str.IndexOf("<s:Envelope", StringComparison.Ordinal));
					str = string.Concat(str.Substring(0, str.IndexOf("</s:Envelope>", StringComparison.Ordinal)), "</s:Envelope>");
					var xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(str);
					_responseUserControl.SetData("RESPONSE FROM VAULT", xmlDocument);
				}
			}
		}

		public bool bReadOnly
		{
			get;
			set;
		}

		public HTTPResponseHeaders headers
		{
			get;
			set;
		}

		public VapiTraceResponse()
		{
		}

		public override void AddToTab(TabPage o)
		{
			_responseUserControl = new VapiUserControl()
			{
				Dock = DockStyle.Fill
			};
			o.Controls.Add(_responseUserControl);
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