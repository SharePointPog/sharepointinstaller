using System;
using System.Text;
using Microsoft.SharePoint;
using System.Xml;
using System.IO;
using Microsoft.SharePoint.Utilities;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;

namespace Stsadm.Commands.Webs
{
    public class ExportWebSecurity : SPOperation
    {
        public ExportWebSecurity()
        {
            SPParamCollection parameters = new SPParamCollection();
            parameters.Add(new SPParam("url", "url", true, null, new SPUrlValidator(), "Please specify the web url."));
            parameters.Add(new SPParam("outputfile", "file", true, null, new SPDirectoryExistsAndValidFileNameValidator()));
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\nExports a web security settings to an XML file.\r\n\r\nParameters:");
            sb.Append("\r\n\t-url <web URL or Web URL to export security from>");
            sb.Append("\r\n\t-outputfile <file to output settings to>");
            Init(parameters, sb.ToString());
        }

        #region ISPStsadmCommand Members


        public override string GetHelpMessage(string command)
        {
            return HelpMessage;
        }

        public override int Execute(string command, System.Collections.Specialized.StringDictionary keyValues, out string output)
        {
            output = string.Empty;
            string url = Params["url"].Value;
            string outputFile = Params["outputfile"].Value;
            ExportSecurity(outputFile, url);
            return OUTPUT_SUCCESS;
        }

        #endregion


        public static void ExportSecurity(string outputFile, string url)
        {
            using (SPSite site = new SPSite(url))
            using (SPWeb web = site.OpenWeb())
            {
                StringBuilder sb = new StringBuilder();
                XmlTextWriter xmlWriter = new XmlTextWriter(new StringWriter(sb));
                xmlWriter.Formatting = Formatting.Indented;
                Log("Start Time: {0}.", DateTime.Now.ToString());
                ExportSecurity(web, xmlWriter);
                xmlWriter.Flush();
                File.WriteAllText(outputFile, sb.ToString());
                Log("Finish Time: {0}.\r\n", DateTime.Now.ToString());
            }
        }

        public static void ExportSecurity(SPWeb web, XmlTextWriter xmlWriter)
        {
            try
            {
                Log("Progress: Processing web \"{0}\".", web.Name);
                xmlWriter.WriteStartElement("Web");
                xmlWriter.WriteAttributeString("Name", web.Name);
                WriteObjectSecurity(web, xmlWriter);
                xmlWriter.WriteEndElement(); 
            }
            finally
            {
                Log("Progress: Finished processing list \"{0}\".", web.Url);
            }
        }

        public static void WriteObjectSecurity(ISecurableObject sourceObject, XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("HasUniqueRoleAssignments", sourceObject.HasUniqueRoleAssignments.ToString());
            if (!sourceObject.HasUniqueRoleAssignments)
                return;
            xmlWriter.WriteStartElement("RoleAssignments");
            foreach (SPRoleAssignment ra in sourceObject.RoleAssignments)
            {
                xmlWriter.WriteStartElement("RoleAssignment");
                xmlWriter.WriteAttributeString("Member", ra.Member.Name);
                SPPrincipalType pType = SPPrincipalType.None;
                if (ra.Member is SPUser)
                {
                    pType = SPPrincipalType.User;
                    xmlWriter.WriteAttributeString("LoginName", ((SPUser)ra.Member).LoginName);
                }
                else if (ra.Member is SPGroup)
                {
                    pType = SPPrincipalType.SharePointGroup;
                }
                xmlWriter.WriteAttributeString("PrincipalType", pType.ToString());
                xmlWriter.WriteStartElement("RoleDefinitionBindings");
                foreach (SPRoleDefinition rd in ra.RoleDefinitionBindings)
                {
                    if (rd.Name == "Limited Access")
                        continue;
                    xmlWriter.WriteStartElement("RoleDefinition");
                    xmlWriter.WriteAttributeString("Name", rd.Name);
                    xmlWriter.WriteAttributeString("Description", rd.Description);
                    xmlWriter.WriteAttributeString("Order", rd.Order.ToString());
                    xmlWriter.WriteAttributeString("BasePermissions", rd.BasePermissions.ToString());
                    xmlWriter.WriteEndElement(); 
                }
                xmlWriter.WriteEndElement(); 
                xmlWriter.WriteEndElement(); 
            }
            xmlWriter.WriteEndElement(); 
        }
    }
}
