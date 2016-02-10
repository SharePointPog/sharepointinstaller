using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;
using System.Xml;
using System.IO;

namespace Stsadm.Commands.Lists
{
    public class ImportListSecurity : SPOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportListSecurity"/> class.
        /// </summary>
        public ImportListSecurity()
        {
            SPParamCollection parameters = new SPParamCollection();
            parameters.Add(new SPParam("url", "url", true, null, new SPUrlValidator(), "Please specify the web url import security to."));
            parameters.Add(new SPParam("inputfile", "file", true, null, new SPDirectoryExistsAndValidFileNameValidator()));
            parameters.Add(new SPParam("quiet", "quiet", false, null, null));
            parameters.Add(new SPParam("includeitemsecurity", "items"));

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\nImports security settings using data output from gl-exportlistsecurity.\r\n\r\nParameters:");
            sb.Append("\r\n\t-url <url to import security to>");
            sb.Append("\r\n\t-inputfile <file to import settings from>");
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
            Verbose = !Params["quiet"].UserTypedIn;
            string inputFile = Params["inputfile"].Value;
            bool includeItemSecurity = Params["includeitemsecurity"].UserTypedIn;

            ImportSecurity(inputFile, url, includeItemSecurity);
            return OUTPUT_SUCCESS;
        }

        #endregion


        /// <summary>
        /// Imports the security.
        /// </summary>
        /// <param name="inputFile">The input file.</param>
        /// <param name="url">The URL.</param>
        /// <param name="includeItemSecurity">if set to <c>true</c> [include item security].</param>
        public static void ImportSecurity(string inputFile, string url, bool includeItemSecurity)
        {
            using (SPSite site = new SPSite(url))
            using (SPWeb web = site.OpenWeb())
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(inputFile);

                Log("Start Time: {0}.", DateTime.Now.ToString());

                foreach (XmlElement listElement in xmlDoc.SelectNodes("//List"))
                {
                    SPList list = null;

                    try
                    {
                        list = web.GetList(web.ServerRelativeUrl.TrimEnd('/') + "/" + listElement.GetAttribute("Url"));
                    }
                    catch (ArgumentException) { }
                    catch (FileNotFoundException) { }

                    if (list == null)
                    {
                        Console.WriteLine("WARNING: List was not found - skipping.");
                        continue;
                    }

                    ImportSecurity(list, web, includeItemSecurity, listElement);

                }
                Log("Finish Time: {0}.\r\n", DateTime.Now.ToString());
            }
        }

        /// <summary>
        /// Imports the security.
        /// </summary>
        /// <param name="targetList">The target list.</param>
        /// <param name="web">The web.</param>
        /// <param name="includeItemSecurity">if set to <c>true</c> [include item security].</param>
        /// <param name="listElement">The list element.</param>
        internal static void ImportSecurity(SPList targetList, SPWeb web, bool includeItemSecurity, XmlElement listElement)
        {
            Log("Progress: Processing list \"{0}\".", targetList.RootFolder.ServerRelativeUrl);

            try
            {
                int writeSecurity = int.Parse(listElement.GetAttribute("WriteSecurity"));
                int readSecurity = int.Parse(listElement.GetAttribute("ReadSecurity"));

                if (writeSecurity != targetList.WriteSecurity)
                    targetList.WriteSecurity = writeSecurity;

                if (readSecurity != targetList.ReadSecurity)
                    targetList.ReadSecurity = readSecurity;

                // Set the security on the list itself.
                SetObjectSecurity(web, targetList, targetList.RootFolder.ServerRelativeUrl, listElement);

                // Set the security on any folders in the list.
                SetFolderSecurity(web, targetList, listElement);

                // Set the security on any items in the list.
                if (includeItemSecurity)
                    SetItemSecurity(web, targetList, listElement);


                if (listElement.HasAttribute("AnonymousPermMask64"))
                {
                    SPBasePermissions anonymousPermMask64 = (SPBasePermissions)int.Parse(listElement.GetAttribute("AnonymousPermMask64"));
                    if (anonymousPermMask64 != targetList.AnonymousPermMask64 && targetList.HasUniqueRoleAssignments)
                        targetList.AnonymousPermMask64 = anonymousPermMask64;
                }

                if (listElement.HasAttribute("AllowEveryoneViewItems"))
                {
                    bool allowEveryoneViewItems = bool.Parse(listElement.GetAttribute("AllowEveryoneViewItems"));
                    if (allowEveryoneViewItems != targetList.AllowEveryoneViewItems)
                        targetList.AllowEveryoneViewItems = allowEveryoneViewItems;
                }

                targetList.Update();

            }
            finally
            {
                Log("Progress: Finished processing list \"{0}\".", targetList.RootFolder.ServerRelativeUrl);
            }
        }

        /// <summary>
        /// Sets the folder security.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="list">The list.</param>
        /// <param name="listElement">The list element.</param>
        private static void SetFolderSecurity(SPWeb web, SPList list, XmlElement listElement)
        {
            foreach (XmlElement folderElement in listElement.SelectNodes("Folder"))
            {
                string folderUrl = folderElement.GetAttribute("Url");
                SPListItem folder = null;
                foreach (SPListItem tempFolder in list.Folders)
                {
                    if (tempFolder.Folder.Url.ToLowerInvariant() == folderUrl.ToLowerInvariant())
                    {
                        folder = tempFolder;
                        break;
                    }
                }
                if (folder == null)
                {
                    Log("Progress: Unable to locate folder '{0}'.", EventLogEntryType.Warning, folderUrl);
                    continue;
                }
                SetObjectSecurity(web, folder, folderUrl, folderElement);
            }
        }

        /// <summary>
        /// Sets the item security.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="list">The list.</param>
        /// <param name="listElement">The list element.</param>
        private static void SetItemSecurity(SPWeb web, SPList list, XmlElement listElement)
        {
            foreach (XmlElement itemElement in listElement.SelectNodes("Item"))
            {
                int itemId = int.Parse(itemElement.GetAttribute("Id"));
                SPListItem item = null;
                try
                {
                    item = list.GetItemById(itemId);
                }
                catch (ArgumentException)
                {
                    // no-op
                }
                if (item == null)
                {
                    Log("Progress: Unable to locate item '{0}'.", EventLogEntryType.Warning, itemId.ToString());
                    continue;
                }
                SetObjectSecurity(web, item, "Item " + itemId, itemElement);
            }
        }

        /// <summary>
        /// Sets the object security.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="targetObject">The target object.</param>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="sourceElement">The source element.</param>
        private static void SetObjectSecurity(SPWeb web, ISecurableObject targetObject, string itemName, XmlElement sourceElement)
        {

            bool hasUniqueRoleAssignments = bool.Parse(sourceElement.GetAttribute("HasUniqueRoleAssignments"));

            if (!hasUniqueRoleAssignments && targetObject.HasUniqueRoleAssignments)
            {
                Log("Progress: Setting target object to inherit permissions from parent for \"{0}\".", itemName);
                targetObject.ResetRoleInheritance();
                return;
            }
            else if (hasUniqueRoleAssignments && !targetObject.HasUniqueRoleAssignments)
            {
                Log("Progress: Breaking target object inheritance from parent for \"{0}\".", itemName);
                targetObject.BreakRoleInheritance(false);
            }
            else if (!hasUniqueRoleAssignments && !targetObject.HasUniqueRoleAssignments)
            {
                Log("Progress: Ignoring \"{0}\".  Target object and source object both inherit from parent.", itemName);
                return; // Both are inheriting so don't change.
            }
            if (hasUniqueRoleAssignments && targetObject.HasUniqueRoleAssignments)
            {
                while (targetObject.RoleAssignments.Count > 0)
                    targetObject.RoleAssignments.Remove(0); // Clear out any existing permissions
            }

            foreach (XmlElement roleAssignmentElement in sourceElement.SelectNodes("RoleAssignments/RoleAssignment"))
            {
                string memberName = roleAssignmentElement.GetAttribute("Member");
                string userName = null;
                if (roleAssignmentElement.HasAttribute("LoginName"))
                    userName = roleAssignmentElement.GetAttribute("LoginName");

                SPRoleAssignment existingRoleAssignment = GetRoleAssignement(web, targetObject, memberName, userName);

                if (existingRoleAssignment != null)
                {
                    if (PermissionHelper.AddRoleDefinitions(web, existingRoleAssignment, roleAssignmentElement))
                    {
                        existingRoleAssignment.Update();

                        Log("Progress: Updated \"{0}\" at target object \"{1}\".", memberName, itemName);
                    }
                }
                else
                {
                    SPPrincipal principal = GetPrincipal(web, memberName, userName);
                    if (principal == null)
                    {
                        Log("Progress: Unable to add Role Assignment for \"{0}\" - Member \"{1}\" not found.", EventLogEntryType.Warning, itemName, memberName);
                        continue;
                    }

                    SPRoleAssignment newRA = new SPRoleAssignment(principal);
                    PermissionHelper.AddRoleDefinitions(web, newRA, roleAssignmentElement);

                    if (newRA.RoleDefinitionBindings.Count == 0)
                    {
                        Log("Progress: Unable to add \"{0}\" to target object \"{1}\" (principals with only \"Limited Access\" cannot be added).", EventLogEntryType.Warning, memberName, itemName);
                        continue;
                    }

                    Log("Progress: Adding new Role Assignment \"{0}\".", newRA.Member.Name);

                    targetObject.RoleAssignments.Add(newRA);

                    existingRoleAssignment = GetRoleAssignement(targetObject, principal);
                    if (existingRoleAssignment == null)
                    {
                        Log("Progress: Unable to add \"{0}\" to target object \"{1}\".", EventLogEntryType.Warning, memberName, itemName);
                    }
                    else
                    {
                        Log("Progress: Added \"{0}\" to target object \"{1}\".", memberName, itemName);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the role assignement.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="securableObject">The securable object.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        private static SPRoleAssignment GetRoleAssignement(SPWeb web, ISecurableObject securableObject, string memberName, string userName)
        {
            SPPrincipal principal = GetPrincipal(web, memberName, userName);
            return GetRoleAssignement(securableObject, principal);
        }

        /// <summary>
        /// Gets the role assignement.
        /// </summary>
        /// <param name="securableObject">The securable object.</param>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        private static SPRoleAssignment GetRoleAssignement(ISecurableObject securableObject, SPPrincipal principal)
        {
            SPRoleAssignment ra = null;
            try
            {
                ra = securableObject.RoleAssignments.GetAssignmentByPrincipal(principal);
            }
            catch (ArgumentException)
            { }
            return ra;
        }

        /// <summary>
        /// Gets the principal.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <returns></returns>
        internal static SPPrincipal GetPrincipal(SPWeb web, string memberName)
        {
            return GetPrincipal(web, memberName, null);
        }

        /// <summary>
        /// Gets the principal.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="loginName">Name of the login.</param>
        /// <returns></returns>
        internal static SPPrincipal GetPrincipal(SPWeb web, string memberName, string loginName)
        {
            foreach (SPPrincipal p in web.SiteUsers)
            {
                if (p.Name.ToLowerInvariant() == memberName.ToLowerInvariant())
                    return p;
            }
            foreach (SPPrincipal p in web.SiteGroups)
            {
                if (p.Name.ToLowerInvariant() == memberName.ToLowerInvariant())
                    return p;
            }

            try
            {
                SPPrincipal principal;
                if (!string.IsNullOrEmpty(loginName) && Microsoft.SharePoint.Utilities.SPUtility.IsLoginValid(web.Site, loginName))
                {
                    // We have a user   
                    Log("Progress: Adding user '{0}' to site.", loginName);
                    principal = web.EnsureUser(loginName);
                }
                else
                {
                    // We have a group   

                    SPGroup groupToAdd = null;
                    try
                    {
                        groupToAdd = web.SiteGroups[memberName];
                    }
                    catch (SPException)
                    {
                    }
                    if (groupToAdd != null)
                    {
                        // The group exists, so get it   
                        principal = groupToAdd;
                    }
                    else
                    {
                        // The group didn't exist so we need to create it:   
                        //  Create it:  
                        Log("Progress: Adding group '{0}' to site.", memberName);
                        web.SiteGroups.Add(memberName, web.Site.Owner, web.Site.Owner, string.Empty);
                        //  Get it:   
                        principal = web.SiteGroups[memberName];
                    }
                }
                return principal;
            }
            catch (Exception ex)
            {
                Log("WARNING: Unable to add member to site: {0}\r\n{1}", memberName, Utilities.FormatException(ex));
            }
            return null;
        }


    }
}
