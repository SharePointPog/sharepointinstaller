using System;
using System.Text;
using Microsoft.SharePoint;
using System.Xml;
using System.IO;
using Microsoft.SharePoint.Utilities;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;

namespace Stsadm.Commands.Lists
{
    public class ExportListSecurity : SPOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportListSecurity"/> class.
        /// </summary>
        public ExportListSecurity()
        {
            SPParamCollection parameters = new SPParamCollection();
            parameters.Add(new SPParam("url", "url", true, null, new SPUrlValidator(), "Please specify the list view URL to export security from."));
            parameters.Add(new SPParam("outputfile", "file", true, null, new SPDirectoryExistsAndValidFileNameValidator()));
            parameters.Add(new SPParam("quiet", "quiet", false, null, null));
            parameters.Add(new SPParam("scope", "s", true, "Web", new SPRegexValidator("(?i:^Web$|^List$)")));
            parameters.Add(new SPParam("includeitemsecurity", "items"));

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\nExports a list's security settings to an XML file.\r\n\r\nParameters:");
            sb.Append("\r\n\t-url <list view URL or Web URL to export security from>");
            sb.Append("\r\n\t-outputfile <file to output settings to>");
            sb.Append("\r\n\t-scope <Web | List>");
            sb.Append("\r\n\t[-quiet]");
            sb.Append("\r\n\t[-includeitemsecurity]");
            Init(parameters, sb.ToString());
        }

        #region ISPStsadmCommand Members

        /// <summary>
        /// Gets the help message.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public override string GetHelpMessage(string command)
        {
            return HelpMessage;
        }

        /// <summary>
        /// Runs the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="keyValues">The key values.</param>
        /// <param name="output">The output.</param>
        /// <returns></returns>
        public override int Execute(string command, System.Collections.Specialized.StringDictionary keyValues, out string output)
        {
            output = string.Empty;

            string url = Params["url"].Value;
            string scope = Params["scope"].Value.ToLowerInvariant();
            string outputFile = Params["outputfile"].Value;
            bool includeItemSecurity = Params["includeitemsecurity"].UserTypedIn;

            Verbose = !Params["quiet"].UserTypedIn;

            ExportSecurity(outputFile, scope, url, includeItemSecurity);

            return OUTPUT_SUCCESS;
        }

        #endregion

        /// <summary>
        /// Exports the security.
        /// </summary>
        /// <param name="outputFile">The output file.</param>
        /// <param name="scope">The scope.</param>
        /// <param name="url">The URL.</param>
        /// <param name="includeItemSecurity">if set to <c>true</c> [include item security].</param>
        public static void ExportSecurity(string outputFile, string scope, string url, bool includeItemSecurity)
        {
            using (SPSite site = new SPSite(url))
            using (SPWeb web = site.OpenWeb())
            {
                StringBuilder sb = new StringBuilder();

                XmlTextWriter xmlWriter = new XmlTextWriter(new StringWriter(sb));
                xmlWriter.Formatting = Formatting.Indented;

                Log("Start Time: {0}.", DateTime.Now.ToString());


                if (scope == "list")
                {
                    SPList list = Utilities.GetListFromViewUrl(web, url);

                    if (list == null)
                        throw new SPException("List was not found.");
                    ExportSecurity(list, web, xmlWriter, includeItemSecurity);
                }
                else
                {
                    xmlWriter.WriteStartElement("Lists");
                    foreach (SPList list in web.Lists)
                    {
                        ExportSecurity(list, web, xmlWriter, includeItemSecurity);
                    }
                    xmlWriter.WriteEndElement(); //Lists
                }
                xmlWriter.Flush();

                File.WriteAllText(outputFile, sb.ToString());

                Log("Finish Time: {0}.\r\n", DateTime.Now.ToString());

            }
        }

        /// <summary>
        /// Exports the security.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="web">The web.</param>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <param name="includeItemSecurity">if set to <c>true</c> [include item security].</param>
        public static void ExportSecurity(SPList list, SPWeb web, XmlTextWriter xmlWriter, bool includeItemSecurity)
        {
            try
            {
                Log("Progress: Processing list \"{0}\".", list.RootFolder.ServerRelativeUrl);

                xmlWriter.WriteStartElement("List");
                xmlWriter.WriteAttributeString("WriteSecurity", list.WriteSecurity.ToString());
                xmlWriter.WriteAttributeString("ReadSecurity", list.ReadSecurity.ToString());
                xmlWriter.WriteAttributeString("AnonymousPermMask64", ((int)list.AnonymousPermMask64).ToString());
                xmlWriter.WriteAttributeString("AllowEveryoneViewItems", list.AllowEveryoneViewItems.ToString());
                xmlWriter.WriteAttributeString("Url", list.RootFolder.Url);

                // Write the security for the list itself.
                WriteObjectSecurity(list, xmlWriter);

                // Write the security for any folders in the list.
                WriteFolderSecurity(list, xmlWriter);

                // Write the security for any items in the list.
                if (includeItemSecurity)
                    WriteItemSecurity(list, xmlWriter);

                xmlWriter.WriteEndElement(); // List
            }
            finally
            {
                Log("Progress: Finished processing list \"{0}\".", list.RootFolder.ServerRelativeUrl);
            }
        }

        /// <summary>
        /// Writes the folder security.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="xmlWriter">The XML writer.</param>
        public static void WriteFolderSecurity(SPList list, XmlTextWriter xmlWriter)
        {
            foreach (SPListItem folder in list.Folders)
            {
                Log("Progress: Processing folder \"{0}\".", folder.Url);

                xmlWriter.WriteStartElement("Folder");
                xmlWriter.WriteAttributeString("Url", folder.Url);
                WriteObjectSecurity(folder, xmlWriter);
                xmlWriter.WriteEndElement(); // Folder
            }
        }

        /// <summary>
        /// Writes the item security.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="xmlWriter">The XML writer.</param>
        public static void WriteItemSecurity(SPList list, XmlTextWriter xmlWriter)
        {
            foreach (SPListItem item in list.Items)
            {
                Log("Progress: Processing item \"{0}\".", item.ID.ToString());

                xmlWriter.WriteStartElement("Item");
                xmlWriter.WriteAttributeString("Id", item.ID.ToString());
                WriteObjectSecurity(item, xmlWriter);
                xmlWriter.WriteEndElement(); // Item
            }
        }

        /// <summary>
        /// Writes the object security.
        /// </summary>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="xmlWriter">The XML writer.</param>
        public static void WriteObjectSecurity(ISecurableObject sourceObject, XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("HasUniqueRoleAssignments", sourceObject.HasUniqueRoleAssignments.ToString());

            if (!sourceObject.HasUniqueRoleAssignments)
                return;

            //xmlWriter.WriteRaw(sourceObject.RoleAssignments.Xml);

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
                    xmlWriter.WriteEndElement(); //RoleDefinition
                }
                xmlWriter.WriteEndElement(); //RoleDefinitionBindings
                xmlWriter.WriteEndElement(); //RoleAssignment
            }
            xmlWriter.WriteEndElement(); //RoleAssignments
        }

    }
}
