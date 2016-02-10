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
    public class RenameGroup : SPOperation
    {
        #region / Param Constants /

        public const String UrlParam = "url";
        public const String FileParam = "filename";
        public const String OldNameParam = "name";
        public const String NewNameParam = "newname";
        public const String Description = "description";

        #endregion

        #region / XPath Constants /

        public const String SelectGroups = "/groups/group";
        public const String SelectOldName = "oldName";
        public const String SelectNewName = "newName";
        public const String SelectDesc = "description";

        #endregion

        public RenameGroup()
        {
            var parameters = new SPParamCollection
                                 {
                                     new SPParam(UrlParam, UrlParam, true, null, new SPUrlValidator(),
                                                 "Please specify url to the site."),
                                     new SPParam(OldNameParam, OldNameParam, false, null, (SPValidator) null,
                                                 "Please specify the group name."),
                                     new SPParam(NewNameParam, NewNameParam, false, null,(SPValidator) null,
                                                 "Please specify the new group name."),
                                     new SPParam(FileParam, FileParam, false, null, new SPDirectoryExistsAndValidFileNameValidator(), "Please specify the filename."),
                                     new SPParam(Description, Description, false, null, null)
                                 };

            var sb = new StringBuilder();
            sb.Append("\r\n\r\nRenames group.\r\n\r\nParameters:");
            sb.AppendFormat("\r\n\t-{0}", UrlParam);
            sb.AppendFormat("\r\n\t-{0}", OldNameParam);
            sb.AppendFormat("\r\n\t-{0}", NewNameParam);
            sb.AppendFormat("\r\n\t-{0}", FileParam);
            sb.AppendFormat("\r\n\t-[{0}]", Description);
            Init(parameters, sb.ToString());
        }

        public override int Execute(String command, StringDictionary keyValues, out String output)
        {
            var successOutput = new StringBuilder();
            var errorOutput = new StringBuilder();

            try
            {
                using (var spSite = new SPSite(Params[UrlParam].Value))
                using (var spWeb = spSite.OpenWeb())
                {
                    if (Params[FileParam].UserTypedIn)
                    {
                        ProcessBatchRenaming(spWeb, successOutput, errorOutput);
                    }
                    else
                    {
                        RenameSpGroup(spWeb, Params[OldNameParam].Value, Params[NewNameParam].Value, Params[Description].Value, successOutput, errorOutput);
                    }

                    if (errorOutput.Length > 0)
                    {
                        output = successOutput
                                    .Append(errorOutput.ToString())
                                    .Append("\r\nSome errors occured during the command execution.").ToString();
                    }
                    else
                    {
                        output = successOutput.Append("\r\nCommand successfully executed.").ToString();
                    }

                    return errorOutput.Length > 0 ? -1 : OUTPUT_SUCCESS;
                }
            }
            catch (Exception e)
            {
                output = String.Format("Failed to execute command. {0}", e.Message);
            }
            return -1;
        }

        private void ProcessBatchRenaming(SPWeb web, StringBuilder successOutput, StringBuilder errorOutput)
        {
            var xmlData = new XmlDocument();
            xmlData.Load(Params[FileParam].Value);

            ValidateData(xmlData);

            foreach (XmlNode groupNode in xmlData.SelectNodes(SelectGroups))
            {
                var oldName = groupNode.SelectSingleNode(SelectOldName).InnerText;
                var newName = groupNode.SelectSingleNode(SelectNewName).InnerText;
                var description = groupNode.SelectSingleNode(SelectDesc) != null ? groupNode.SelectSingleNode(SelectDesc).InnerText : string.Empty;

                RenameSpGroup(web, oldName, newName, description, successOutput, errorOutput);
            }
        }

        private void ValidateData(XmlDocument xmlData)
        {
            var groupNodeCollection = xmlData.SelectNodes(SelectGroups);
            if (groupNodeCollection == null)
                throw new InvalidDataException("Invalid xml document. Xml must start with <groups> tag.");

            int line = 0;
            foreach (XmlNode groupNode in groupNodeCollection)
            {
                line++;

                var nodeOldName = groupNode.SelectSingleNode(SelectOldName);
                var nodeNewName = groupNode.SelectSingleNode(SelectNewName);

                var oldName = nodeOldName != null ? nodeOldName.InnerText.Trim() : string.Empty;
                var newName = nodeNewName != null ? nodeNewName.InnerText.Trim() : string.Empty;

                if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
                    throw new InvalidDataException(string.Format("Xml document contains invalid data. Check line: '{0}'. ", line));
            }
        }

        public void RenameSpGroup(SPWeb web, string oldGroupName, string newGroupName, string description, StringBuilder successOutput, StringBuilder errorOutput)
        {
            oldGroupName = oldGroupName.Trim();
            newGroupName = newGroupName.Trim();

            if (!GroupExists(web, oldGroupName))
                errorOutput.AppendFormat("\r\nGroup with specified name '{0}' doesn't exist", oldGroupName);
            else if (GroupExists(web, newGroupName))
                errorOutput.AppendFormat("\r\nGroup '{0}' can't be renamed to '{1}' because the latter already exists", oldGroupName, newGroupName);
            else
            {
                try
                {
                    var group = GetGroup(web, oldGroupName);
                    if (!string.IsNullOrEmpty(description))
                    {
                        var groupItem = web.SiteUserInfoList.GetItemById(group.ID);
                        groupItem["About Me"] = description;
                        groupItem.Update();
                    }
                    group.Name = newGroupName;
                    group.Update();
                    successOutput.AppendFormat("\r\nGroup '{0}' renamed to '{1}'.", oldGroupName, newGroupName);
                }
                catch (Exception exc)
                {
                    errorOutput.AppendFormat("\r\nFail to renamegroup '{0}' to '{1}'. \r\nException: {2}", oldGroupName, newGroupName, exc);
                }
            }
        }

        private SPGroup GetGroup(SPWeb web, string groupName)
        {
            try
            {
                return web.SiteGroups[groupName];
            }
            catch (Exception)
            {
                return null;
            }
        }

        private bool GroupExists(SPWeb web, string groupName)
        {
            return GetGroup(web, groupName) != null;
        }

        public override String GetHelpMessage(String command)
        {
            return HelpMessage;
        }
    }
}
