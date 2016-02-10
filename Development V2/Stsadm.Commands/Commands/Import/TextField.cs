using System.Xml;
using Microsoft.SharePoint;

namespace Stsadm.Commands.Import
{
    public class TextField : FieldBase
    {
        public TextField()
        {
        }

        public TextField(XmlNode xml) : base(xml)
        {
        }

        public override object GetFieldValue(SPList list, object value)
        {
            return value;
        }
    }
}
