using Fiddler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using vapiTraceFiddlerExtension.Properties;

namespace vapiTraceFiddlerExtension
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once RedundantExtendsListEntry
    public class ResponseInspector : Inspector2, IResponseInspector2, IBaseInspector2
	{
		private ResponseUserControl _responseUserControl;
        

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

                if (!SoapEnvelopeParser.TryLoad(value, out var xml, out var error))
                {
                    _responseUserControl.Clear(error);
                    return;
                }

                var bodyNode = SoapEnvelopeParser.GetBodyElement(xml);
                if (bodyNode == null)
                {
                    _responseUserControl.Clear();
                    return;
                }

                var responseNode = SoapEnvelopeParser.GetFirstChildElement(bodyNode);
                if (responseNode == null)
                {
                    _responseUserControl.Clear();
                    return;
                }

                VaultAssembly.TryGetServiceFromNamespace(responseNode.NamespaceURI, out var service);
                var serviceType = VaultAssembly.GetServiceType(service);

                var method = SoapEnvelopeParser.StripResponseSuffix(responseNode.LocalName);
                var methodInfo = serviceType?.GetMethod(method);
                var returnType = methodInfo?.ReturnParameter?.ParameterType;
                var resultNode = SoapEnvelopeParser.GetFirstChildElement(responseNode);

                try
                {
                    if (returnType == null || resultNode == null)
                    {
                        ShowRawResponse(returnType?.FullName ?? responseNode.LocalName, resultNode ?? responseNode, returnType);
                        return;
                    }

                    var deserializationNode = PrepareResultNode(xml, bodyNode, responseNode, resultNode);
                    using (TextReader reader = new StringReader(deserializationNode.OuterXml))
                    {
                        var serializer = new XmlSerializer(returnType, new XmlRootAttribute(deserializationNode.Name));
                        if (returnType.IsArray)
                        {
                            var trees = new List<SerializedTreeNode>();
                            var result = serializer.Deserialize(new SoapTextReader(reader));
                            if (result is IEnumerable enumerable)
                            {
                                foreach (var r in enumerable)
                                {
                                    var itemTypeName = r?.GetType().Name ?? returnType.GetElementType()?.Name ?? "Item";
                                    var treeNode = SerializedTreeNode.CreateTree(r, itemTypeName);
                                    trees.Add(treeNode);
                                }
                            }
                            _responseUserControl.SetData(returnType.FullName, trees, returnType);
                        }
                        else
                        {
                            object result = serializer.Deserialize(new SoapTextReader(reader));
                            var treeNode = SerializedTreeNode.CreateTree(result, returnType.Name);
                            _responseUserControl.SetData(returnType.FullName, treeNode, returnType);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message);
                    ShowRawResponse(
                        returnType?.FullName != null ? $"{returnType.FullName} (raw XML fallback)" : $"{responseNode.LocalName} (raw XML fallback)",
                        resultNode ?? responseNode,
                        returnType);
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
            if (!VaultAssembly.IsVaultDllPresent)
            {
                throw new DllNotFoundException(
                    "Autodesk.Connectivity.WebServices.dll cannot be found. Please install Vault");
            }
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

        private void ShowRawResponse(string header, XmlNode node, Type type)
        {
            var tree = SerializedTreeNode.CreateTree(node, node?.LocalName ?? "Response");
            _responseUserControl.SetData(header, tree, type);
        }

        private static XmlElement PrepareResultNode(XmlDocument xml, XmlElement bodyNode, XmlElement responseNode, XmlElement resultNode)
        {
            var clone = (XmlElement) resultNode.CloneNode(true);
            CopyAttributes(xml, bodyNode, clone);
            CopyAttributes(xml, responseNode, clone);

            var binaryNodes = clone.SelectNodes(".//*[local-name()='Include' and namespace-uri()='http://www.w3.org/2004/08/xop/include']");
            if (binaryNodes != null)
            {
                foreach (System.Xml.XmlNode binaryNode in binaryNodes)
                {
                    var parent = binaryNode.ParentNode;
                    parent?.RemoveAll();
                }
            }

            return clone;
        }

        private static void CopyAttributes(XmlDocument xml, XmlElement source, XmlElement target)
        {
            if (source?.Attributes == null || target?.Attributes == null)
                return;

            foreach (System.Xml.XmlAttribute attribute in source.Attributes)
            {
                if (target.Attributes[attribute.Name] != null)
                    continue;

                var newAttribute = xml.CreateAttribute(attribute.Prefix, attribute.LocalName, attribute.NamespaceURI);
                newAttribute.Value = attribute.Value;
                target.Attributes.Append(newAttribute);
            }
        }
	}
}
