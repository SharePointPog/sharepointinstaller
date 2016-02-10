using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;

namespace Stsadm.Commands.Import
{
    public class ImportItemCommand : SPOperation
    {
        public ImportItemCommand()
        {
            var parameters = new SPParamCollection();
            parameters.Add(new SPParam("url", "url", true, null, new SPUrlValidator(), "Please specify the list url to import the items to."));
            parameters.Add(new SPParam("filename", "f", true, null, new SPDirectoryExistsAndValidFileNameValidator(), "Please specify the filename."));
                        
            Init(parameters, GetHelpMessage(""));
        }

        public override int Execute(String command, StringDictionary keyValues, out String output)
        {
            output = String.Empty;
            var url = Params["url"].Value;
            var fileName = Params["filename"].Value;

            try
            {
                using(var reader = new StreamReader(fileName))
                {                    
                    using(var site = new SPSite(url))
                    {
                        using(var web = site.OpenWeb())
                        {
                            var list = web.GetList(url);

                            var parser = new Parser(reader.ReadToEnd());
                            parser.Parse(list);

                            foreach(var item in parser.Items)
                                item.Save();
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
            var sb = new StringBuilder();
            sb.Append("\r\n\r\nImports a list item or items.\r\n\r\nParameters:\r\n\t");
            sb.Append("-url <list url to import into>\r\n\t");
            sb.Append("-filename <import file name>");

            return sb.ToString();
        }
    }
}
