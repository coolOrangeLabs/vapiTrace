using Fiddler;
using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using vapiTraceFiddlerExtension.Properties;

namespace vapiTraceFiddlerExtension
{
	public class vapiTrace : Inspector2, IResponseInspector2, IBaseInspector2
	{
		private InspectorUC ucResponse = null;

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
				if (str.IndexOf("<s:Envelope") <= 0)
				{
					ucResponse.Clear();
				}
				else
				{
					str = str.Substring(str.IndexOf("<s:Envelope"));
					str = string.Concat(str.Substring(0, str.IndexOf("</s:Envelope>")), "</s:Envelope>");
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(str);
					ucResponse.SetData("Response from Vault server", xmlDocument);
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

		public vapiTrace()
		{
		}

		public override void AddToTab(TabPage o)
		{
			ucResponse = new InspectorUC()
			{
				Dock = DockStyle.Fill
			};
			o.Controls.Add(ucResponse);
			o.Text = "vapiTrace";

			FiddlerApplication.UI.imglSessionIcons.Images.Add(Resources.ICO_logoCO_16x16);
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