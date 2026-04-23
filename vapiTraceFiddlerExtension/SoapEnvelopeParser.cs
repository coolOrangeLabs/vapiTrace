using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace vapiTraceFiddlerExtension
{
    internal static class SoapEnvelopeParser
    {
        private static readonly Regex EnvelopeStartRegex =
            new Regex(@"<(?:(?<prefix>[\w\-]+):)?Envelope\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static bool TryLoad(byte[] body, out XmlDocument xml, out string error)
        {
            xml = null;
            error = null;

            if (body == null || body.Length == 0)
            {
                error = "SOAP body is empty.";
                return false;
            }

            return TryLoad(Encoding.UTF8.GetString(body), out xml, out error);
        }

        public static bool TryLoad(string content, out XmlDocument xml, out string error)
        {
            xml = null;
            error = null;

            if (!TryExtractEnvelope(content, out var envelope, out error))
                return false;

            try
            {
                xml = new XmlDocument();
                xml.LoadXml(envelope);
                return true;
            }
            catch (XmlException ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static bool TryExtractEnvelope(string content, out string envelope, out string error)
        {
            envelope = null;
            error = null;

            if (string.IsNullOrWhiteSpace(content))
            {
                error = "SOAP body is empty.";
                return false;
            }

            var startMatch = EnvelopeStartRegex.Match(content);
            if (!startMatch.Success)
            {
                error = "No SOAP envelope was found in the payload.";
                return false;
            }

            var prefix = startMatch.Groups["prefix"].Success ? startMatch.Groups["prefix"].Value : string.Empty;
            var closingExpression = string.IsNullOrEmpty(prefix)
                ? @"</Envelope\s*>"
                : $@"</{Regex.Escape(prefix)}:Envelope\s*>";

            var closingMatch = Regex.Match(
                content.Substring(startMatch.Index),
                closingExpression,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (!closingMatch.Success)
            {
                error = "The SOAP envelope is incomplete.";
                return false;
            }

            var end = startMatch.Index + closingMatch.Index + closingMatch.Length;
            envelope = content.Substring(startMatch.Index, end - startMatch.Index);
            return true;
        }

        public static XmlElement GetBodyElement(XmlDocument xml)
        {
            return xml?.DocumentElement?.SelectSingleNode("/*[local-name()='Envelope']/*[local-name()='Body']") as XmlElement;
        }

        public static XmlElement GetFirstChildElement(XmlNode node)
        {
            if (node == null)
                return null;

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                    return (XmlElement) child;
            }

            return null;
        }

        public static string StripResponseSuffix(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            const string suffix = "Response";
            return name.EndsWith(suffix, StringComparison.Ordinal)
                ? name.Substring(0, name.Length - suffix.Length)
                : name;
        }
    }
}
