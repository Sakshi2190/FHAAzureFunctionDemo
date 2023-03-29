using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace PreProcessor
{
    class LoadDocument
    {
        internal static XmlDocument XMLInPutDoc = new XmlDocument();
        internal static XmlDocument XMLOutPutDoc = new XmlDocument();
        internal static System.Xml.Schema.XmlSchema bundle_Canonical;
        internal static System.Xml.Schema.XmlSchema bundle_types_Canonical;
        internal static System.Xml.Schema.XmlSchema fhir_xhtml_Canonical;
        private static string ErrorMessage;

        internal static void LoadXML(string Message, string Identifier)
        {
            try
            {
                if (Identifier == "InputMessage")
                {
                    XMLInPutDoc.LoadXml(Message);
                }
                else
                    XMLOutPutDoc.LoadXml(Message);

            }
            catch (XmlException xmlEx)
            {

                ErrorMessage = xmlEx.Message;
            }
        }
        internal static void LoadXSD(string Message, string Identifier)
        {
            try
            {
                XmlTextReader reader = new XmlTextReader(new StringReader(Message));
                if (Identifier == "bundle-Canonical")
                {

                    bundle_Canonical = System.Xml.Schema.XmlSchema.Read(reader, null);

                }
                else if (Identifier == "bundle-types-Canonical")
                {
                    bundle_types_Canonical = System.Xml.Schema.XmlSchema.Read(reader, null);

                }
                else if (Identifier == "fhir-xhtml-Canonical")
                    fhir_xhtml_Canonical = System.Xml.Schema.XmlSchema.Read(reader, null);

            }
            catch(XmlException xmlEx)
            {

                ErrorMessage = xmlEx.Message;
            }
        }
    }
}
