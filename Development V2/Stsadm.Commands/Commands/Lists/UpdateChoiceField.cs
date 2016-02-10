using System;
using System.Collections.Specialized;
using System.Text;
using System.Xml;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;

namespace Stsadm.Commands.Lists
{
    public class UpdateChoiceField : SPOperation
    {
        public String ListUrlParam = "listurl";

        public String ChoiceFieldNameParam = "field";

        public String DefaultItemParam = "defaultitem";

        public String ChoicesParam = "choices";

        public UpdateChoiceField()
        {
            var parameters = new SPParamCollection
                                 {
                                     new SPParam(ListUrlParam, ListUrlParam, true, null, new SPUrlValidator(),
                                                 "Please specify list name."),
                                     new SPParam(ChoiceFieldNameParam, ChoiceFieldNameParam, true, null,
                                                 (SPValidator) null, "Please specify choice field."),
                                     new SPParam(DefaultItemParam, DefaultItemParam, false, null, (SPValidator) null,
                                                 "Please specify default item."),
                                     new SPParam(ChoicesParam, ChoicesParam, false, null, (SPValidator) null,
                                                 "Please specify choices.")
                                 };

            var sb = new StringBuilder();
            sb.Append("\r\n\r\nChanges default item in choice field.\r\n\r\nParameters:");
            sb.AppendFormat("\r\n\t-{0}", ListUrlParam);
            sb.AppendFormat("\r\n\t-{0}", ChoiceFieldNameParam);
            sb.AppendFormat("\r\n\t-{0}", ChoicesParam);
            Init(parameters, sb.ToString());
        }

        public override int Execute(String command, StringDictionary keyValues, out String output)
        {
            output = String.Empty;
            var messageBuilder = new StringBuilder();
            try
            {
                if (Params[ListUrlParam].UserTypedIn && Params[ChoiceFieldNameParam].UserTypedIn && (Params[DefaultItemParam].UserTypedIn || Params[ChoicesParam].UserTypedIn))
                {
                    using (var site = new SPSite(Params[ListUrlParam].Value))
                    {
                        using (var web = site.OpenWeb())
                        {
                            if (Params[DefaultItemParam].UserTypedIn)
                                ChangeChoiceFieldDefaultItem(web.GetList(Params[ListUrlParam].Value),
                                                             Params[ChoiceFieldNameParam].Value,
                                                             Params[DefaultItemParam].Value);
                            if (Params[ChoicesParam].UserTypedIn)
                                ChangeFieldChoices(web.GetList(Params[ListUrlParam].Value),
                                                             Params[ChoiceFieldNameParam].Value,
                                                             Params[ChoicesParam].Value);
                        }
                    }
                }
                return OUTPUT_SUCCESS;
            }
            catch (Exception e)
            {
                output = String.Format("{0}\nFailed to execute command. {1}", messageBuilder, e);                
            }
            return OUTPUT_FAILED;
        }

        private void ChangeChoiceFieldDefaultItem(SPList list, String fieldName, String defaultItem)
        {
            if (list == null)
                throw new ArgumentException("List is required.", "list");
            if (String.IsNullOrEmpty(fieldName))
                throw new ArgumentException("Name is required.", "fieldName");

            if (list.Fields.ContainsField(fieldName))
            {
                var field = list.Fields.GetFieldByInternalName(fieldName);
                if(!(field is SPFieldChoice))
                    return;

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(field.SchemaXml);
                var defaultNode = xmlDoc.SelectSingleNode("/Field/Default");
                if (defaultNode != null)
                    defaultNode.InnerText = defaultItem;
                else
                {
                    var fieldNode = xmlDoc.SelectSingleNode("/Field");
                    if (fieldNode != null)
                    {
                        var newNode = xmlDoc.CreateElement("Deafult");
                        newNode.InnerText = defaultItem;
                        fieldNode.AppendChild(newNode);
                    }
                }
                field.SchemaXml = xmlDoc.OuterXml;
                field.Update();
            }
        }
        
        private void ChangeFieldChoices(SPList list, String fieldName, String choiceItems)
        {
            if (list == null)
                throw new ArgumentException("List is required.", "list");
            if (String.IsNullOrEmpty(fieldName))
                throw new ArgumentException("Name is required.", "fieldName");
            if (list.Fields.ContainsField(fieldName))
            {
                var field = list.Fields.GetFieldByInternalName(fieldName) as SPFieldChoice;
                if (field == null)
                    return;
                field.Choices.Clear();
                foreach (var choice in choiceItems.Split(new [] {';'},StringSplitOptions.RemoveEmptyEntries))
                {
                    field.Choices.Add(choice);
                }
                field.Update();
            }
        }
        
        public override String GetHelpMessage(String command)
        {
            return HelpMessage;
        }
    }
}
