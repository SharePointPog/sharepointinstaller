using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;

namespace Stsadm.Commands.Lists
{
    public class UpdateFieldSchema : SPOperation
    {
        public UpdateFieldSchema()
        {
            var parameters = new SPParamCollection();
            parameters.Add(new SPParam("url", "url", true, null, new SPUrlValidator(), "Please specify the list URL."));
            parameters.Add(new SPParam("filename", "f", true, null, new SPDirectoryExistsAndValidFileNameValidator(), "Please specify the filename."));
            parameters.Add(new SPParam("fieldinternalname", "name", false, null, new SPNonEmptyValidator(), "Please specify the field's internal name to delete."));

            var sb = new StringBuilder();
            sb.Append("\r\n\r\nUpdates field's scheme in a list.\r\n\r\nParameters:");
            sb.Append("\r\n\t-url <list URL>");
            sb.Append("\r\n\t-filename <new schema>");
            sb.Append("\r\n\t-fieldinternalname <field internal name>");
            Init(parameters, sb.ToString());
        }

        public override int Execute(String command, StringDictionary keyValues, out String output)
        {
            output = String.Empty;
            try
            {
                using(var reader = new StreamReader(Params["filename"].Value))
                {
                    var contents = reader.ReadToEnd();

                    var doc = new XmlDocument();
                    doc.LoadXml(contents);

                    using(var site = new SPSite(Params["url"].Value))
                    {
                        using(var web = site.OpenWeb())
                        {
                            var list = web.GetList(Params["url"].Value);
                            if(list.Fields.ContainsField(Params["fieldinternalname"].Value))
                            {
                                var field = list.Fields.GetFieldByInternalName(Params["fieldinternalname"].Value);
                                field.SchemaXml = contents;
                                field.Update();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                output = String.Format("Message: {0}\nException: {1}", e.Message, e);
                return OUTPUT_FAILED;
            }
            return OUTPUT_SUCCESS;
        }

        public override String GetHelpMessage(String command)
        {
            return HelpMessage;
        }
    }
}
