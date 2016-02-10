using System;
using System.Xml;
using Microsoft.SharePoint;

namespace Stsadm.Commands.Import
{
    public abstract class FieldBase
    {
        protected const String FieldNodeName = "field";
        protected const String NameNodeName = "name";
        protected const String ValueNodeName = "value";
        protected const String TypeNodeName = "type";        

        protected FieldBase()
        {
        }

        protected FieldBase(XmlNode xml)
        {
            if(!Validate(xml))
                throw new ArgumentException("Invalid xml. Xml must be <field>...</field> element.", "xml");
            Parse(xml);
        }

        protected virtual void Parse(XmlNode xml)
        {
            var nameNode = xml.SelectSingleNode(NameNodeName);
            if(nameNode == null)
                throw new ArgumentException("Invalid field. Name attribute must exist.");
            Name = nameNode.InnerText;

            var typeNode = xml.SelectSingleNode(TypeNodeName);
            if (typeNode == null)
                throw new ArgumentException("Invalid field. Type attribute must exist.");
            Type = typeNode.InnerText;            
        }

        public abstract object GetFieldValue(SPList list, object value);

        protected virtual bool Validate(XmlNode xml)
        {
            return xml.Name == FieldNodeName;
        }

        public String Name { get; private set; }

        public String Type { get; private set; }        
    }
}
