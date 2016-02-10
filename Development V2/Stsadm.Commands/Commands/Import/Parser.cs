using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.SharePoint;

namespace Stsadm.Commands.Import
{
    public class Parser
    {
        private XmlDocument xmlDocument;
        private List<ListItem> items = new List<ListItem>();

        public Parser(String xml)
        {
            xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
        }

        public Parser(XmlDocument xmlDocument)
        {
            this.xmlDocument = xmlDocument;
        }

        public void Parse(SPList list)
        {
            var itemNodes = xmlDocument.SelectNodes("items/item");
            if(itemNodes == null)
                throw new ArgumentException("Invalid xml document. Xml must start with <items> tag.");

            if (itemNodes != null)
            {
                foreach (XmlNode itemNode in itemNodes)
                    items.Add(new ListItem(list, itemNode));
            }
        }

        public IEnumerable<ListItem> Items
        {
            get
            {
                return items;
            }
        }
    }
}
