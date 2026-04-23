using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace vapiTraceFiddlerExtension
{
    public partial class RequestUserControl
    {
        private const string SoapError = "No valid Autodesk Vault SOAP request";
        private static readonly Color CommandDefaultColor = Color.FromArgb(31, 31, 31);
        private static readonly Color PowerShellVariableColor = Color.FromArgb(0, 102, 204);
        private static readonly Color MemberNameColor = Color.FromArgb(163, 21, 21);
        private static readonly Color StringLiteralColor = Color.FromArgb(163, 21, 21);
        private static readonly Color NumberLiteralColor = Color.FromArgb(9, 134, 88);
        private static readonly Color TypeLiteralColor = Color.FromArgb(43, 145, 175);
        private static readonly Color KeywordLiteralColor = Color.FromArgb(0, 0, 255);
        private static readonly Regex VariableRegex = new Regex(@"\$\w+", RegexOptions.Compiled);
        private static readonly Regex MemberRegex = new Regex(@"(?<=\.)[A-Za-z_]\w*", RegexOptions.Compiled);
        private static readonly Regex StringLiteralRegex = new Regex(@"'([^']|'')*'", RegexOptions.Compiled);
        private static readonly Regex NumberLiteralRegex = new Regex(@"(?<![\w'])-?\d+(\.\d+)?(?![\w'])", RegexOptions.Compiled);
        private static readonly Regex TypeLiteralRegex = new Regex(@"\[[A-Za-z_][\w\.]*\]", RegexOptions.Compiled);
        private static readonly Regex KeywordLiteralRegex = new Regex(@"\$(true|false|null)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public RequestUserControl()
        {
            InitializeComponent();
            SetCommandText(string.Empty);
        }

        public void Clear(string error = null)
        {
            _requestTree.BeginUpdate();
            _requestTree.Nodes.Clear();

            if (string.IsNullOrEmpty(error))
                error = SoapError;

            SetCommandText(string.Empty);
            webBrowser.DocumentText = VaultAssembly.Html(error, string.Empty, string.Empty, string.Empty);

            _requestTree.EndUpdate();
            if (_requestTree.Nodes.Count > 0)
                _requestTree.SelectedNode = _requestTree.Nodes[0];
        }

        public void SetData(XmlDocument xml = null, string service = null)
        {
            _requestTree.BeginUpdate();
            _requestTree.Nodes.Clear(); 
            
            SetCommandText(string.Empty);
            var h1 = "";
            var serviceHeader = service;

            var documentation = "";
            var command = "";
            var parameterList = new List<string>();

            if (xml != null)
            {
                var bodyNode = SoapEnvelopeParser.GetBodyElement(xml);
                if (bodyNode != null)
                {
                    var methodNode = SoapEnvelopeParser.GetFirstChildElement(bodyNode);
                    if (methodNode != null)
                    {
                        var serviceName = service;
                        if (string.IsNullOrEmpty(serviceName))
                            VaultAssembly.TryGetServiceFromNamespace(methodNode.NamespaceURI, out serviceName);
                        serviceHeader = serviceName;

                        var firstNode = _requestTree.Nodes.Add(serviceName ?? methodNode.LocalName);
                        var method = methodNode.LocalName;
                        AddElements(firstNode, methodNode);
                        _requestTree.ExpandAll();

                        var info = VaultAssembly.GetServiceType(serviceName);
                        var methodInfo = info?.GetMethod(method);
                        if (methodInfo != null)
                        {
                            h1 = methodInfo.Name;
                            documentation = methodInfo.GetDocumentation();
                            command = "Request";

                            var parameterInfos = methodInfo.GetParameters();
                            SetCommandText(GenerateCommandScript(serviceName, method, methodNode, parameterInfos));

                            foreach (var parameterInfo in parameterInfos)
                            {
                                var parameterName = parameterInfo.Name;
                                var parameterTypeName = parameterInfo.ParameterType.FullName;
                                parameterList.Add($"[{parameterTypeName}] ${parameterName}");
                            }
                        }
                    }
                }
            }

            if (parameterList.Count > 0)
            {
                var s = "<parameter>";
                foreach (var p in parameterList)
                    s += p + "<br/>";
                s += "</parameter>";

                documentation += s;
            }

            webBrowser.DocumentText = VaultAssembly.Html(h1 + " Method", serviceHeader, documentation, command);

            _requestTree.EndUpdate();
            if (_requestTree.Nodes.Count > 0)
                _requestTree.SelectedNode = _requestTree.Nodes[0];
        }

        private string GenerateCommandScript(string service, string method, XmlElement methodNode, ParameterInfo[] parameterInfos)
        {
            var parameter = "";
            var assignments = new List<string>();
            foreach (var parameterInfo in parameterInfos)
            {
                parameter += "$" + parameterInfo.Name + ", ";
                var parameterNode = FindParameterNode(methodNode, parameterInfo.Name);
                var parameterValue = ConvertXmlToPowerShell(parameterNode, parameterInfo.ParameterType, 0);
                assignments.Add($"${parameterInfo.Name} = {parameterValue}");
            }
            parameter = parameter.TrimEnd(',', ' ');

            var command = "$vault." + service + "." + method + "(" + parameter + ")";
            if (assignments.Count == 0)
                return command;

            return string.Join(Environment.NewLine, assignments) + Environment.NewLine + Environment.NewLine + command;
        }

        private void SetCommandText(string commandText)
        {
            txtCommand.Text = commandText ?? string.Empty;
            btnCopyCommand.Enabled = !string.IsNullOrWhiteSpace(txtCommand.Text);
            ColorizeCommandText();
        }

        private void ColorizeCommandText()
        {
            if (txtCommand.TextLength == 0)
                return;

            var selectionStart = txtCommand.SelectionStart;
            var selectionLength = txtCommand.SelectionLength;

            txtCommand.SelectAll();
            txtCommand.SelectionColor = CommandDefaultColor;

            ApplyCommandColor(VariableRegex, PowerShellVariableColor);
            ApplyCommandColor(MemberRegex, MemberNameColor);
            ApplyCommandColor(TypeLiteralRegex, TypeLiteralColor);
            ApplyCommandColor(NumberLiteralRegex, NumberLiteralColor);
            ApplyCommandColor(StringLiteralRegex, StringLiteralColor);
            ApplyCommandColor(KeywordLiteralRegex, KeywordLiteralColor);

            txtCommand.Select(selectionStart, selectionLength);
            if (selectionLength == 0)
                txtCommand.SelectionColor = CommandDefaultColor;
        }

        private void ApplyCommandColor(Regex regex, Color color)
        {
            foreach (Match match in regex.Matches(txtCommand.Text))
            {
                txtCommand.Select(match.Index, match.Length);
                txtCommand.SelectionColor = color;
            }
        }

        private XmlElement FindParameterNode(XmlElement methodNode, string parameterName)
        {
            if (methodNode == null || string.IsNullOrEmpty(parameterName))
                return null;

            foreach (XmlNode child in methodNode.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element)
                    continue;

                if (string.Equals(child.LocalName, parameterName, StringComparison.OrdinalIgnoreCase))
                    return (XmlElement) child;
            }

            return null;
        }

        private string ConvertXmlToPowerShell(XmlElement element, Type targetType, int indentLevel)
        {
            targetType = UnwrapNullableType(targetType);

            if (element == null || IsNilElement(element))
                return "$null";

            if (targetType == typeof(byte[]))
                return $"[Convert]::FromBase64String({QuotePowerShellString(element.InnerText)})";

            if (IsArrayType(targetType))
                return ConvertArrayToPowerShell(element, targetType.GetElementType(), indentLevel);

            if (IsScalarType(targetType))
                return ConvertScalarToPowerShell(element.InnerText, targetType);

            var childElements = GetChildElements(element);
            if (childElements.Count == 0)
            {
                if (targetType == typeof(string))
                    return QuotePowerShellString(element.InnerText);

                return string.IsNullOrWhiteSpace(element.InnerText)
                    ? "$null"
                    : ConvertScalarToPowerShell(element.InnerText, targetType);
            }

            return ConvertObjectToPowerShell(element, targetType, indentLevel);
        }

        private string ConvertArrayToPowerShell(XmlElement element, Type elementType, int indentLevel)
        {
            var childElements = GetChildElements(element);
            if (childElements.Count == 0)
                return "@()";

            var items = new List<string>();
            foreach (var childElement in childElements)
                items.Add(ConvertXmlToPowerShell(childElement, elementType, indentLevel + 1));

            if (items.Count == 1 && !items[0].Contains(Environment.NewLine))
                return $"@({items[0]})";

            return WrapCollection("@(", ")", items, indentLevel, ",");
        }

        private string ConvertObjectToPowerShell(XmlElement element, Type targetType, int indentLevel)
        {
            var entries = new List<string>();
            foreach (var childElement in GetChildElements(element))
            {
                var childType = ResolveChildType(targetType, childElement);
                var childValue = ConvertXmlToPowerShell(childElement, childType, indentLevel + 1);
                entries.Add($"{QuotePowerShellString(childElement.LocalName)} = {childValue}");
            }

            if (entries.Count == 0)
                return "@{}";

            return WrapCollection("@{", "}", entries, indentLevel, string.Empty);
        }

        private string WrapCollection(string opening, string closing, List<string> items, int indentLevel, string separator)
        {
            var builder = new StringBuilder();
            var indent = new string(' ', indentLevel * 4);
            var childIndent = new string(' ', (indentLevel + 1) * 4);

            builder.Append(opening);
            builder.AppendLine();

            for (var i = 0; i < items.Count; i++)
            {
                builder.Append(IndentText(items[i], childIndent));
                if (i < items.Count - 1 && !string.IsNullOrEmpty(separator))
                    builder.Append(separator);
                builder.AppendLine();
            }

            builder.Append(indent);
            builder.Append(closing);
            return builder.ToString();
        }

        private string IndentText(string value, string indent)
        {
            if (string.IsNullOrEmpty(value))
                return indent;

            var lines = value.Replace("\r\n", "\n").Split('\n');
            var builder = new StringBuilder();
            for (var i = 0; i < lines.Length; i++)
            {
                if (i > 0)
                    builder.AppendLine();
                builder.Append(indent);
                builder.Append(lines[i]);
            }
            return builder.ToString();
        }

        private Type ResolveChildType(Type parentType, XmlElement childElement)
        {
            if (childElement == null)
                return null;

            parentType = UnwrapNullableType(parentType);
            if (parentType == null)
                return null;

            if (parentType.IsArray)
                return parentType.GetElementType();

            const BindingFlags memberFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;

            var property = parentType.GetProperty(childElement.LocalName, memberFlags);
            if (property != null)
                return property.PropertyType;

            var field = parentType.GetField(childElement.LocalName, memberFlags);
            if (field != null)
                return field.FieldType;

            return null;
        }

        private List<XmlElement> GetChildElements(XmlElement element)
        {
            var result = new List<XmlElement>();
            if (element == null)
                return result;

            foreach (XmlNode child in element.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                    result.Add((XmlElement) child);
            }

            return result;
        }

        private string ConvertScalarToPowerShell(string value, Type targetType)
        {
            targetType = UnwrapNullableType(targetType);
            value = value ?? string.Empty;

            if (targetType == null || targetType == typeof(string) || targetType == typeof(char) || targetType == typeof(Guid))
                return QuotePowerShellString(value);

            if (targetType == typeof(bool))
                return value.Equals("true", StringComparison.OrdinalIgnoreCase) || value == "1" ? "$true" : "$false";

            if (targetType == typeof(DateTime))
                return $"[datetime]{QuotePowerShellString(value)}";

            if (targetType.IsEnum)
                return QuotePowerShellString(value);

            if (targetType == typeof(decimal))
                return $"[decimal]{QuotePowerShellString(value)}";

            if (targetType == typeof(float))
                return $"[float]{QuotePowerShellString(value)}";

            if (targetType == typeof(double))
                return $"[double]{QuotePowerShellString(value)}";

            if (targetType == typeof(long))
                return $"[long]{value}";

            if (targetType == typeof(ulong))
                return $"[ulong]{value}";

            if (targetType == typeof(int))
                return $"[int]{value}";

            if (targetType == typeof(uint))
                return $"[uint32]{value}";

            if (targetType == typeof(short))
                return $"[short]{value}";

            if (targetType == typeof(ushort))
                return $"[ushort]{value}";

            if (targetType == typeof(byte))
                return $"[byte]{value}";

            if (targetType == typeof(sbyte))
                return $"[sbyte]{value}";

            return QuotePowerShellString(value);
        }

        private bool IsScalarType(Type targetType)
        {
            targetType = UnwrapNullableType(targetType);
            if (targetType == null)
                return true;

            return targetType.IsPrimitive ||
                   targetType.IsEnum ||
                   targetType == typeof(string) ||
                   targetType == typeof(decimal) ||
                   targetType == typeof(DateTime) ||
                   targetType == typeof(Guid);
        }

        private bool IsArrayType(Type targetType)
        {
            targetType = UnwrapNullableType(targetType);
            return targetType != null && targetType.IsArray;
        }

        private Type UnwrapNullableType(Type targetType)
        {
            return targetType != null && targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? Nullable.GetUnderlyingType(targetType)
                : targetType;
        }

        private bool IsNilElement(XmlElement element)
        {
            if (element?.Attributes == null)
                return false;

            foreach (XmlAttribute attribute in element.Attributes)
            {
                if (attribute.LocalName == "nil" &&
                    attribute.NamespaceURI == "http://www.w3.org/2001/XMLSchema-instance" &&
                    attribute.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private string QuotePowerShellString(string value)
        {
            return "'" + (value ?? string.Empty).Replace("'", "''") + "'";
        }

        private void AddElements(TreeNode tNode, XmlNode xNode)
        {
            if (xNode.NodeType == XmlNodeType.Element)
            {
                var name = xNode.Name;
                if (xNode.InnerText.Length > 0 && xNode.HasChildNodes && xNode.FirstChild.NodeType == XmlNodeType.Text)
                    name = string.Concat(name, " = ", xNode.InnerText);

                var nextNode = tNode.Nodes.Add(name);
                if (xNode.Attributes != null)
                {
                    foreach (XmlAttribute attribute in xNode.Attributes)
                    {
                        if (attribute.Name.Equals("xmlns"))
                            continue;

                        nextNode.Nodes.Add($"{attribute.Name} = {attribute.InnerText}");
                    }
                }

                foreach (XmlNode childNode in xNode.ChildNodes)
                    AddElements(nextNode, childNode);
            }
        }

        private void OnCopyCommandClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCommand.Text))
                return;

            Clipboard.SetText(txtCommand.Text);
        }

        #region WebBrowser
        private void UserControl_SizeChanged(object sender, EventArgs e)
        {
            webBrowser.Refresh();
        }

        private void OnWebBrowserDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser.Document?.Body != null)
            {
                webBrowser.Document.Body.KeyDown -= OnWebBrowserKeyDown;
                webBrowser.Document.Body.KeyDown += OnWebBrowserKeyDown;
                var events = (mshtml.HTMLDocumentEvents2_Event) webBrowser.Document.DomDocument;
                events.onmousewheel -= OnWebBrowserMouseWheel;
                events.onmousewheel += OnWebBrowserMouseWheel;

                var size = new Size(
                    webBrowser.Document.Body.ScrollRectangle.Size.Width,
                    webBrowser.Document.Body.ScrollRectangle.Size.Height);
                size.Height += 6;

                webBrowser.Size = size;
            }
        }

        private void OnWebBrowserNewWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Process.Start("https://www.coolorange.com");
        }

        private void OnWebBrowserKeyDown(object sender, HtmlElementEventArgs e)
        {
            if ((e.KeyPressedCode == 109 && e.CtrlKeyPressed) ||
                (e.KeyPressedCode == 107 && e.CtrlKeyPressed) ||
                (e.CtrlKeyPressed && e.KeyPressedCode == 187) ||
                (e.CtrlKeyPressed && e.KeyPressedCode == 189))
            {
                e.ReturnValue = false;
            }
        }

        private bool OnWebBrowserMouseWheel(mshtml.IHTMLEventObj obj)
        {
            if (obj.ctrlKey)
            {
                obj.cancelBubble = true;
                obj.returnValue = false;
                return false;
            }
            return true;
        }
        #endregion
    }
}
