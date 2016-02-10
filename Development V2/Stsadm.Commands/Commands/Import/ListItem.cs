using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.SharePoint;

namespace Stsadm.Commands.Import
{
    public class ListItem
    {
        private Dictionary<String, object> hash = new Dictionary<String, object>();
        private List<FieldBase> fields = new List<FieldBase>();
        private SPList list;
        
        public ListItem(SPList list, XmlNode node)
        {
            this.list = list;

            var selector = node.SelectSingleNode("selector");
            if(selector != null)
                Selector = new ItemSelector(selector, list);

            var fieldNodes = node.SelectNodes("fields/field");
            if (fieldNodes != null)
            {
                foreach (XmlNode fieldNode in fieldNodes)
                {
                    var typeNode = fieldNode.SelectSingleNode("type");
                    if (typeNode == null)
                        throw new ArgumentException("Invalid field. Field must contain type.");
                    
                    var valueNode = fieldNode.SelectSingleNode("value");

                    var field = FieldResolver.Get(typeNode.InnerText, fieldNode);
                    if (field != null)
                        AddField(field, valueNode != null ? valueNode.InnerText : null);
                }
            }
        }

        public void AddField(FieldBase field, object value)
        {
            AddField(field);

            if (hash.ContainsKey(field.Name))
                hash.Add(field.Name, field.GetFieldValue(list, value));
            else
                hash[field.Name] = field.GetFieldValue(list, value);
        }        

        private void AddField(FieldBase field)
        {
            if(!fields.Contains(field))
                fields.Add(field);
        }

        public void Save()
        {
            if (IsNew)
                SetFieldValues(list.Items.Add());
            else
            {
                SPItem item = Selector.Get();
                if(item!=null)
                    SetFieldValues(Selector.Get());
                else SetFieldValues(list.Items.Add());
            }
        }

        private void SetFieldValues(SPItem spItem)
        {
            if (spItem == null)
                return;

            foreach (var field in Fields)
            {
                if (spItem.Fields.ContainsField(field.Name))
                    spItem[field.Name] = this[field.Name];
            }
            spItem.Update();
        }

        public object this[String name]
        {
            get
            {
                return hash.ContainsKey(name) ? hash[name] : null;
            }
        }        

        public bool IsNew
        {
            get { return Selector == null; }
        }

        public ItemSelector Selector
        {
            get; private set;
        }

        public List<FieldBase> Fields
        {
            get { return fields; }
        }
    }
}
