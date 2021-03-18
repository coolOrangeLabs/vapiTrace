using System;
using Fiddler;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using vapiTraceFiddlerExtension.Properties;

namespace vapiTraceFiddlerExtension
{
	public class vapiTraceRequest : Inspector2, IRequestInspector2, IBaseInspector2
	{
		private InspectorUC _ucRequest;

		public bool bDirty
		{
			get
			{
				return false;
			}
		}

		public byte[] body
		{
			get
			{
				return null;
			}
			set
			{
				string str = Encoding.UTF8.GetString(value);
				if (str.IndexOf("<s:Envelope", StringComparison.Ordinal) <= 0)
				{
					_ucRequest.Clear();
				}
				else
				{
					str = str.Substring(str.IndexOf("<s:Envelope", StringComparison.Ordinal));
					str = string.Concat(str.Substring(0, str.IndexOf("</s:Envelope>", StringComparison.Ordinal)), "</s:Envelope>");
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(str);
					_ucRequest.SetData("Request to Vault server", xmlDocument);
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
			get;
			set;
		}

		public vapiTraceRequest()
		{
		}

		public override void AddToTab(TabPage o)
		{
			_ucRequest = new InspectorUC()
			{
				Dock = DockStyle.Fill
			};
			o.Controls.Add(_ucRequest);
			o.Text = "vapiTrace";
			FiddlerApplication.UI.imglSessionIcons.Images.Add(Resources.ICO_logoCO_16x16.ToBitmap());
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