using System;
using System.Text;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Navigation;

namespace Stsadm.Commands.Links
{
    /// <summary>
    /// This class holds the command which adds a section or link to site QuickLaunch panel.
    /// </summary>
    /// <remarks>
    /// With this command it's possible to add section or link to site QuickLaunch panel.
    /// </remarks>
    public class RemoveLinkFromQuickLaunch : SPOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddLinkToQuickLaunch"/> class.
        /// </summary>
        public RemoveLinkFromQuickLaunch()
        {
            SPParamCollection parameters = new SPParamCollection();
            parameters.Add(new SPParam("url", "url", true, null, new SPUrlValidator(), "Please specify the site URL."));
            parameters.Add(new SPParam("title", "title", true, null, new SPNonEmptyValidator(), "Please specify the link title."));
            parameters.Add(new SPParam("section", "section", false, null, null));


            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\nAdds or updates a link (section) in QuickLaunch.\r\n\r\nParameters:");
            sb.Append("\r\n\t-url <site URL>");
            sb.Append("\r\n\t-title <link title>");
            sb.Append("\r\n\t[-section <section title>] (omit this parameter to create a section)");

            Init(parameters, sb.ToString());
        }
        #region ISPStsadmCommand Members

        public override int Execute(string command, System.Collections.Specialized.StringDictionary keyValues, out string output)
        {
            try
            {
                output = string.Empty;
                string siteUrl = Params["url"].Value;
                string parentSection = Params["section"].Value;
                string nodeTitle = Params["title"].Value;

                using (SPSite siteCollection = new SPSite(siteUrl))
                {
                    using (SPWeb site = siteCollection.OpenWeb())
                    {
                        SPNavigationNodeCollection nodeCollection = site.Navigation.QuickLaunch;
                        if (!String.IsNullOrEmpty(parentSection))
                        {
                            SPNavigationNode section = GetNodeByTitle(nodeCollection, parentSection);
                            if (section == null)
                            {
                                output = String.Format("The specified section {0} does not exist.", parentSection);
                                return 1;
                            }
                            nodeCollection = section.Children;
                        }

                        SPNavigationNode existingNode = GetNodeByTitle(nodeCollection, nodeTitle);
                        if (existingNode != null)
                        {
                            nodeCollection.Delete(existingNode);
                        }
                    }
                }
                return OUTPUT_SUCCESS;
            }
            catch (Exception e)
            {
                output = String.Format("Failed to execute command. {0}", e.Message);
                return OUTPUT_FAILED;
            }
        }

        /// <summary>
        /// Gets the help message.
        /// </summary>
        /// <param name="command">The command name.</param>
        /// <returns>The help message to display when this command is run with -help switch.</returns>
        public override string GetHelpMessage(string command)
        {
            return HelpMessage;
        }

        #endregion

        #region private Methods

        static private SPNavigationNode GetNodeByTitle(SPNavigationNodeCollection collection, string nodeTitle)
        {
            foreach (SPNavigationNode currentNode in collection)
            {
                if (currentNode.Title == nodeTitle)
                {
                    return currentNode;
                }
            }
            return null;
        }
        #endregion
    }
}
