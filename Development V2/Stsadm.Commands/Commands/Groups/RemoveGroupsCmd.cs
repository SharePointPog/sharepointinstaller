using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;

namespace Stsadm.Commands.Groups
{
    public class RemoveGroupsCmd : SPOperation
    {
        public const String UrlParam = "url";

        public const String FileNameParam = "filename";
        public const string GroupName = "group";

        public RemoveGroupsCmd()
        {
            var parameters = new SPParamCollection();
            parameters.Add(new SPParam(FileNameParam, FileNameParam, false, null, (SPValidator)null, "Please specify the file name."));
            parameters.Add(new SPParam(GroupName, GroupName, false, null, (SPValidator)null, "Please specify the group name."));
            parameters.Add(new SPParam(UrlParam, UrlParam, true, null, new SPUrlValidator(), "Please specify url to the site."));

            var sb = new StringBuilder();
            sb.Append("\r\n\r\nRemoves group from the site.\r\n\r\nParameters:");
            sb.AppendFormat("\r\n\t-{0}", UrlParam);
            sb.AppendFormat("\r\n\t-{0}", FileNameParam);
            sb.AppendFormat("\r\n\t-{0}", GroupName);
            Init(parameters, sb.ToString());
        }

        public override int Execute(String command, StringDictionary keyValues, out String output)
        {
            output = String.Empty;
            var builder = new StringBuilder();
            try
            {
                if (Params[UrlParam].UserTypedIn)
                {
                    using(var site = new SPSite(Params[UrlParam].Value))
                    {
                        using (var web = site.OpenWeb())
                        {
                            if (Params[FileNameParam].UserTypedIn)
                            {
                                RemoveGroups(web, builder);
                            }
                            else if (Params[GroupName].UserTypedIn)
                            {
                                web.SiteGroups.Remove(Params[GroupName].Value);
                            }
                        }
                        
                        output = builder + "\nCommand successfully executed.";
                        return OUTPUT_SUCCESS;
                    }
                }
            }
            catch (Exception e)
            {
                output = String.Format("{0}\nFailed to execute command. {1}", builder, e);
            }
            return -1;
        }

        public void RemoveGroups(SPWeb web, StringBuilder output)
        {
            if (File.Exists(Params[FileNameParam].Value))
            {
                var document = new XmlDocument();
                document.Load(Params[FileNameParam].Value);

                var buGroups = document.SelectNodes("/Groups/Group");
                if (buGroups != null)
                {
                    foreach (XmlNode node in buGroups)
                        RemoveGroup(node, web, output);
                }
            } 
            else
                throw new FileNotFoundException(Params[FileNameParam].Value);
        }

        private void RemoveGroup(XmlNode groupNode, SPWeb web, StringBuilder output)
        {
            try
            {
                web.SiteGroups.Remove(groupNode.InnerText);
                output.AppendFormat("\nGroup '{0}' removed.", groupNode.InnerText);
            }
            catch (Exception e)
            {
                output.Append(e);
            }
        }

        public override String GetHelpMessage(String command)
        {
            return HelpMessage;
        }
    }
}
