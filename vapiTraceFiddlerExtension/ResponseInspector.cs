using Fiddler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using vapiTraceFiddlerExtension.Properties;

namespace vapiTraceFiddlerExtension
{
	public class ResponseInspector : Inspector2, IResponseInspector2, IBaseInspector2
	{
		private ResponseUserControl _responseUserControl;
        private const string SoapError = "No valid Autodesk Vault SOAP response";

        public bool bDirty => false;

        public byte[] body
        {
            get => null;
            set
            {
                if (value == null)
                {
                    _responseUserControl.SetData(SoapError);
                    return;
                }

                var s = Encoding.UTF8.GetString(value);
                if (s.IndexOf("<s:Envelope", StringComparison.Ordinal) <= 0)
                {
                    _responseUserControl.SetData(SoapError);
                    return;
                }

                s = s.Substring(s.IndexOf("<s:Envelope", StringComparison.Ordinal));
                s = string.Concat(s.Substring(0, s.IndexOf("</s:Envelope>", StringComparison.Ordinal)),
                    "</s:Envelope>");

                var xml = new XmlDocument();
                xml.LoadXml(s);

                var xmlNamespaceManagers = new XmlNamespaceManager(xml.NameTable);
                xmlNamespaceManagers.AddNamespace(xml.FirstChild.Prefix, xml.FirstChild.NamespaceURI);
                xmlNamespaceManagers.AddNamespace("xop", "http://www.w3.org/2004/08/xop/include");

                var bodyNode = xml.DocumentElement?.SelectSingleNode("//s:Body", xmlNamespaceManagers);
                if (bodyNode == null)
                {
                    _responseUserControl.SetData(SoapError);
                    return;
                }

                var responseNode = bodyNode.FirstChild;
                var ns = responseNode.Attributes?["xmlns"];

                string start;
                if (ns != null && ns.InnerText.Contains("AutodeskDM/Services/"))
                    start = "AutodeskDM/Services/";
                else if (ns != null && ns.InnerText.Contains("AutodeskDM/Filestore/"))
                    start = "AutodeskDM/Filestore/";
                else
                {
                    _responseUserControl.SetData(SoapError);
                    return;
                }

                string service;
                var part = ns.InnerText.Substring(ns.InnerText.IndexOf(start, StringComparison.Ordinal) + start.Length);
                if (part.IndexOf('/') < 0)
                    service = part;
                else
                    service = part.Substring(0, part.IndexOf('/'));
                var serviceType = VaultAssembly.GetServiceType(service);

                var resultNode = responseNode.FirstChild;
                if (resultNode == null)
                {
                    _responseUserControl.SetData("Response from Vault: Empty");
                    return;
                }

                if (bodyNode.Attributes != null)
                {
                    foreach (XmlAttribute attribute in bodyNode.Attributes)
                    {
                        var newAttribute = xml.CreateAttribute(attribute.Name);
                        newAttribute.Value = attribute.Value;
                        resultNode.Attributes?.Append(newAttribute);
                    }
                }

                var binaryNodes = xml.DocumentElement?.SelectNodes("//xop:Include", xmlNamespaceManagers);
                if (binaryNodes != null)
                {
                    foreach (XmlNode binaryNode in binaryNodes)
                    {
                        var parent = binaryNode.ParentNode;
                        parent?.RemoveAll();
                    }
                }

                var method = resultNode.Name.Remove(resultNode.Name.LastIndexOf("Result", StringComparison.Ordinal));
                var methodInfo = serviceType.GetMethod(method);
                if (methodInfo == null || methodInfo.ReturnParameter == null)
                {
                    _responseUserControl.SetData(SoapError);
                    return;
                }

                var parameterType = methodInfo.ReturnParameter.ParameterType;

                try
                {
                    using (TextReader reader = new StringReader(resultNode.OuterXml))
                    {
                        var serializer = new XmlSerializer(parameterType, new XmlRootAttribute(resultNode.Name));
                        if (parameterType.IsArray)
                        {
                            var trees = new List<SerializedTreeNode>();
                            var result = serializer.Deserialize(new SoapTextReader(reader));
                            if (result is IEnumerable)
                            {
                                foreach (var r in (IEnumerable)result)
                                {
                                    var treeNode = SerializedTreeNode.CreateTree(r, r.GetType().Name);
                                    trees.Add(treeNode);
                                }
                            }
                            _responseUserControl.SetData("Response from Vault: " + parameterType.Name, trees);
                        }
                        else
                        {
                            object result = serializer.Deserialize(new SoapTextReader(reader));
                            var treeNode = SerializedTreeNode.CreateTree(result, parameterType.Name);
                            _responseUserControl.SetData("Response from Vault: " + parameterType.Name, treeNode);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message);
                    _responseUserControl.SetData(ex.Message);
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

		public ResponseInspector()
        {
        }

		public override void AddToTab(TabPage o)
        {
            _responseUserControl = new ResponseUserControl {Dock = DockStyle.Fill};
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