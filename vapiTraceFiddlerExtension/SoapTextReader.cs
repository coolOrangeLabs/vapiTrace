using System.IO;
using System.Xml;

namespace vapiTraceFiddlerExtension
{
    public class SoapTextReader : XmlTextReader
    {
        public SoapTextReader(TextReader reader) : base(reader) { }
        public override string NamespaceURI => "";
        public override string Prefix => "xsd";
    }
}