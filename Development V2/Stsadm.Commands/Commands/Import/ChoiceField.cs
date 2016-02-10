using System.Xml;

namespace Stsadm.Commands.Import
{
    public class ChoiceField : TextField
    {
        public ChoiceField()
        {
        }

        public ChoiceField(XmlNode xml) : base(xml)
        {
        }
    }
}