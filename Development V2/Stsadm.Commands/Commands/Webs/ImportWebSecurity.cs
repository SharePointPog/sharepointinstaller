using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;
using System.Xml;
using System.IO;

namespace Stsadm.Commands.Webs
{
    public class ImportWebSecurity : SPOperation
    {
        public ImportWebSecurity()
        {
            SPParamCollection parameters = new SPParamCollection();
            parameters.Add(new SPParam("url", "url", true, null, new SPUrlValidator(), "Please specify the web url import security to."));
            parameters.Add(new SPParam("inputfile", "file", true, null, new SPDirectoryExistsAndValidFileNameValidator()));

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\nImports security settings using.\r\n\r\nParameters:");
            sb.Append("\r\n\t-url <url to import security to>");
            sb.Append("\r\n\t-inputfile <file to import settings from>");
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
            string inputFile = Params["inputfile"].Value;
            ImportSecurity(inputFile, url);
            return OUTPUT_SUCCESS;
        }

        #endregion

        public static void ImportSecurity(string inputFile, string url)
        {
            using (SPSite site = new SPSite(url))
            using (SPWeb web = site.OpenWeb())
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(inputFile);
                Log("Start Time: {0}.", DateTime.Now.ToString());
                foreach (XmlElement listElement in xmlDoc.SelectNodes("//Web"))
                {
                    ImportSecurity(web, listElement);
                }
                Log("Finish Time: {0}.\r\n", DateTime.Now.ToString());
            }
        }

        internal static void ImportSecurity(SPWeb web, XmlElement listElement)
        {
            Log("Progress: Processing web \"{0}\".", web.Name);
            try
            {
                SetObjectSecurity(web, listElement);
                web.Update();
            }
            finally
            {
                Log("Progress: Finished processing web \"{0}\".", web.RootFolder.ServerRelativeUrl);
            }
        }

        private static void SetObjectSecurity(SPWeb web, XmlElement sourceElement)
        {

            foreach (XmlElement roleAssignmentElement in sourceElement.SelectNodes("RoleAssignments/RoleAssignment"))
            {
                string memberName = roleAssignmentElement.GetAttribute("Member");
                string userName = null;
                if (roleAssignmentElement.HasAttribute("LoginName"))
                    userName = roleAssignmentElement.GetAttribute("LoginName");

                SPRoleAssignment existingRoleAssignment = GetRoleAssignement(web, web, memberName, userName);

                if (existingRoleAssignment != null)
                {
                    if (PermissionHelper.AddRoleDefinitions(web, existingRoleAssignment, roleAssignmentElement))
                    {
                        existingRoleAssignment.Update();
                        Log("Progress: Updated \"{0}\" at target object \"{1}\".", memberName, web.Name);
                    }
                }
                else
                {
                    SPPrincipal principal = GetPrincipal(web, memberName, userName);
                    if (principal == null)
                    {
                        Log("Progress: Unable to add Role Assignment for \"{0}\" - Member \"{1}\" not found.", EventLogEntryType.Warning, web.Name, memberName);
                        continue;
                    }
                    SPRoleAssignment newRA = new SPRoleAssignment(principal);
                    PermissionHelper.AddRoleDefinitions(web, newRA, roleAssignmentElement);

                    if (newRA.RoleDefinitionBindings.Count == 0)
                    {
                        Log("Progress: Unable to add \"{0}\" to target object \"{1}\" (principals with only \"Limited Access\" cannot be added).", EventLogEntryType.Warning, memberName, web.Name);
                        continue;
                    }
                    Log("Progress: Adding new Role Assignment \"{0}\".", newRA.Member.Name);
                    web.RoleAssignments.Add(newRA);
                    existingRoleAssignment = GetRoleAssignement(web, principal);
                    if (existingRoleAssignment == null)
                    {
                        Log("Progress: Unable to add \"{0}\" to target object \"{1}\".", EventLogEntryType.Warning, memberName, web.Name);
                    }
                    else
                    {
                        Log("Progress: Added \"{0}\" to target object \"{1}\".", memberName, web.Name);
                    }
                }
            }
        }

        

        private static SPRoleAssignment GetRoleAssignement(SPWeb web, ISecurableObject securableObject, string memberName, string userName)
        {
            SPPrincipal principal = GetPrincipal(web, memberName, userName);
            return GetRoleAssignement(securableObject, principal);
        }

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

        internal static SPPrincipal GetPrincipal(SPWeb web, string memberName)
        {
            return GetPrincipal(web, memberName, null);
        }

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
                    Log("Progress: Adding user '{0}' to site.", loginName);
                    principal = web.EnsureUser(loginName);
                }
                else
                {
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
                        principal = groupToAdd;
                    }
                    else
                    {
                        Log("Progress: Adding group '{0}' to site.", memberName);
                        web.SiteGroups.Add(memberName, web.Site.Owner, web.Site.Owner, string.Empty);
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
