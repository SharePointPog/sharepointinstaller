using System;
using System.Xml;
using Microsoft.SharePoint;

namespace Stsadm.Commands.Import
{
    class LookupField : FieldBase
    {
        public LookupField()
        {
        }

        public LookupField(XmlNode xml): base(xml)
        {
        }

        protected override void Parse(XmlNode xml)
        {
            base.Parse(xml);

            var lookUpFieldNode = xml.SelectSingleNode("lookupfieldname");
            if (lookUpFieldNode != null)
                LookupFieldName = lookUpFieldNode.InnerText;  
        }

        public override object GetFieldValue(SPList list, object value)
        {
            try
            {
                if(list.Fields.ContainsField(Name))
                {
                    var field = list.Fields.GetFieldByInternalName(Name);
                    if (field is SPFieldLookup)
                    {
                        var lookupList = list.ParentWeb.Lists[new Guid(((SPFieldLookup)field).LookupList)];
                        var lookupField = ((SPFieldLookup)field).LookupField;

                        if (String.IsNullOrEmpty(lookupField))
                            lookupField = LookupFieldName;

                        var lookupFieldType = lookupList.Fields[lookupField].Type.ToString();
                        var query = new SPQuery
                                        {
                                            Query = String.Format(
                                                "<Where><Eq><FieldRef Name='{0}' /><Value Type='{1}'>{2}</Value></Eq></Where>",
                                                lookupField, lookupFieldType, value)
                                        };
                        var items = lookupList.GetItems(query);
                        if (items.Count > 0)
                            return new SPFieldLookupValue(items[0].ID, value.ToString());                            
                    }
                }                
                return null;
            }
            catch { throw new Exception("Can't inport lookup field"); }
        }

        public String LookupFieldName { get; private set; }
    }
}
