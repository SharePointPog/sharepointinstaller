using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;

namespace Stsadm.Commands.Groups
{
    public class TransferUsersAcrossGroups : SPOperation
    {
        public const String UrlParam = "url";
        public const string SourceGroupName = "source";
        public const String DestinationGroupName = "destination";


        public TransferUsersAcrossGroups()
        {
            var parameters = new SPParamCollection
                                 {
                                     new SPParam(DestinationGroupName, DestinationGroupName, false, null,
                                                 (SPValidator) null, "Please specify the destination group name."),
                                     new SPParam(SourceGroupName, SourceGroupName, false, null, (SPValidator) null,
                                                 "Please specify the source group name."),
                                     new SPParam(UrlParam, UrlParam, true, null, new SPUrlValidator(),
                                                 "Please specify url to the site.")
                                 };

            var sb = new StringBuilder();
            sb.Append("\r\n\r\nRemoves group from the site.\r\n\r\nParameters:");
            sb.AppendFormat("\r\n\t-{0}", UrlParam);
            sb.AppendFormat("\r\n\t-{0}", DestinationGroupName);
            sb.AppendFormat("\r\n\t-{0}", SourceGroupName);
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
                            if (Params[SourceGroupName].UserTypedIn && Params[DestinationGroupName].UserTypedIn)
                            {
                                TransferUsers(web, builder, Params[SourceGroupName].Value, Params[DestinationGroupName].Value);
                            }
                        }
                        output = builder + "\nCommand successfully executed.";
                        return OUTPUT_SUCCESS;
                    }
                }
            }
            catch (Exception e)
            {
                output = String.Format("Failed to execute command. {0}", e.Message);
            }
            return -1;
        }

        private void TransferUsers(SPWeb web, StringBuilder output,string sourceGroupName,string destinationGroupName)
        {
            try
            {
                    var sourceGroup = GetGroup(web, sourceGroupName);
                    var destinationGroup = GetGroup(web, destinationGroupName);

                    // move all distinct users from one group to another
                    if (sourceGroup != null && destinationGroup != null && sourceGroup.Users.Count > 0)
                    {
                        // move from source group those users are not in desctination group
                        foreach (SPUser spUser in sourceGroup.Users)
                        {
                            if (GetUser(destinationGroup, spUser.LoginName) == null)
                            {
                                try
                                {
                                    // add new user
                                    destinationGroup.AddUser(spUser);
                                    output.AppendLine(string.Format("User {0} moved to {1} group.", spUser.LoginName,
                                                                    destinationGroupName));
                                }
                                catch (Exception ex)
                                {
                                    output.AppendLine(ex.Message);
                                }
                            }
                        }
                    }
                
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

        #region / Helper Methods /

        private SPGroup GetGroup(SPWeb spWeb, string name)
        {
            try
            {
                return spWeb.SiteGroups[name];
            }
            catch (Exception)
            {
                return null;
            }
        }

        private SPUser GetUser(SPGroup group, string loginName)
        {
            try
            {
                return group.Users[loginName];
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion
    }
}
