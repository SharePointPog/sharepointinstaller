using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;

namespace Stsadm.Commands.Import
{
    public class ItemSelector
    {
        private SPList list;
        private const String FieldPattern = "<Eq><FieldRef Name='{0}' /><Value Type='{1}'>{2}</Value></Eq>";

        public ItemSelector(XmlNode xml, SPList list)
        {
            this.list = list;

            if(xml.Name != "selector")
                throw new ArgumentException("Invalid xml. Selector must start with <selector> element.");

            var idNode = xml.SelectSingleNode("id");
            if (idNode != null)
                Id = int.Parse(idNode.InnerText);

            Fields = new List<QueryField>();
            var queryFieldsNode = xml.SelectSingleNode("queryfields");
            if(queryFieldsNode != null)
            {
                var queryFields = queryFieldsNode.SelectNodes("field");
                if (queryFields != null)
                {
                    foreach (XmlNode queryField in queryFields)
                    {
                        var nameNode = queryField.SelectSingleNode("name");
                        if(nameNode == null)
                            throw new ArgumentException("Invalid query field. Name required.");

                        var typeNode = queryField.SelectSingleNode("type");
                        if(typeNode == null)
                            throw new ArgumentException("Invalid query field. Type required.");

                        var valueNode = queryField.SelectSingleNode("value");
                        Fields.Add(new QueryField(nameNode.InnerText, typeNode.InnerText, valueNode != null ? valueNode.InnerText : ""));
                    }
                }
            }
        }

        public SPListItem Get()
        {
            SPListItem item = null;
            if(Id.HasValue)
            {
                try
                {
                    item = list.GetItemById(Id.Value);
                }
                catch
                {
                }
            }
            else
            {
                var items = list.GetItems(CreateQuery());
                if (items.Count > 0)
                    item = items[0];
            }
            return item;
        }

        public SPQuery CreateQuery()
        {
            return new SPQuery
                       {
                           Query = String.Format("<Where>{0}</Where>", CreateFieldQuery(0, Fields.Count - 1))
                       };
        }

        private String CreateFieldQuery(int left, int right)
        {
            if ((right - left) >= 2)
                return String.Format("<And>{0}{1}</And>", CreateFieldQuery(left, (left + right)/2),
                                     CreateFieldQuery((left + right)/2 + 1, right));

            if ((right - left) == 1)
                return String.Format("<And>{0}{1}</And>",
                                     String.Format(FieldPattern, Fields[left].Name, Fields[left].Type,
                                                   Fields[left].Value),
                                     String.Format(FieldPattern, Fields[right].Name, Fields[right].Type,
                                                   Fields[right].Value));

            return String.Format(FieldPattern, Fields[left].Name, Fields[left].Type, Fields[left].Value);
        }

        private int? Id { get; set; }
        private List<QueryField> Fields { get; set; }
    }

    public class QueryField
    {
        public QueryField(String name, String type, String value)
        {
            Name = name;
            Type = type;
            Value = value;
        }

        public QueryField()
        {
        }

        public String Type { get; set; }
        public String Name { get; set; }
        public String Value { get; set; }
    }
}
