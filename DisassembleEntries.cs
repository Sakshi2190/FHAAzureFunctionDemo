
using System;
using System.Xml;
using System.Xml.XPath;

namespace PreProcessor
{
    class DisassembleEntries
    {

        internal static XPathNodeIterator Disassemble()
        {
            try
            {
                XPathDocument xPathDoc = new XPathDocument((new XmlNodeReader(LoadDocument.XMLInPutDoc)));
                XPathNavigator xPathNavigator = xPathDoc.CreateNavigator();
                XPathExpression xPathExpression = xPathNavigator.Compile("/Bundle:Bundle/Bundle:entry");
                XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xPathNavigator.NameTable);
                xmlNamespaceManager.AddNamespace("Bundle", "http://hl7.org/fhir");
                xPathExpression.SetContext(xmlNamespaceManager);
                XPathNodeIterator entries = xPathNavigator.Select(xPathExpression);
                return entries;
            }
            catch
            {
                return null;
            }
        }
    }
}
