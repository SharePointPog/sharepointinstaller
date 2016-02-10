using System;
using System.Collections.Generic;
using System.Text;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Navigation;
using Microsoft.SharePoint.StsAdmin;

namespace Stsadm.Commands.Links
{
    /// <summary>
    /// This class holds the command which adds a section or link to site QuickLaunch panel.
    /// </summary>
    /// <remarks>
    /// With this command it's possible to add section or link to site QuickLaunch panel.
    /// </remarks>
    public class AddLinkToQuickLaunch : SPOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddLinkToQuickLaunch"/> class.
        /// </summary>
        public AddLinkToQuickLaunch()
        {
            SPParamCollection parameters = new SPParamCollection();
            parameters.Add(new SPParam("url", "url", true, null, new SPUrlValidator(), "Please specify the site URL."));
            parameters.Add(new SPParam("title", "title", true, null, new SPNonEmptyValidator(), "Please specify the link title."));
            parameters.Add(new SPParam("section", "section", false, null, null));
            parameters.Add(new SPParam("force", "force"));
            parameters.Add(new SPParam("sourceurl", "source", false, null, null));
            parameters.Add(new SPParam("position", "pos", false, null, new SPIntRangeValidator(0, int.MaxValue)));

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\nAdds or updates a link (section) in QuickLaunch.\r\n\r\nParameters:");
            sb.Append("\r\n\t-url <site URL>");
            sb.Append("\r\n\t-title <link title>");
            sb.Append("\r\n\t[-sourceurl <link source url>]");
            sb.Append("\r\n\t[-section <section title>] (omit this parameter to create a section)");
            sb.Append("\r\n\t[-position <link position>] (zero-based, omit to insert at the end)");
            sb.Append("\r\n\t[-force] (used to force link(section) overwriting if link(section) with the same title exists)");
            Init(parameters, sb.ToString());
        }    
        #region ISPStsadmCommand Members
        /// <summary>
        /// Runs the specified command.
        /// </summary>
        /// <param name="command">The command name.</param>
        /// <param name="keyValues">The following parameters are expected:<![CDATA[
        ///         -url <site URL>
        ///         -title <link title>
        ///         [-section <link section title>]
        ///         [-sourceurl <link source url>]
        ///         [-position <link position>] (zero-based, omit to insert at the end)
        ///         [-force] (used to force link(section) overwriting if link(section) with the same title exists)]]>
        /// </param>
        /// <param name="output">The output to display.</param>
        /// <returns>The status code of the operation (zero means success).</returns>
        public override int Execute(string command, System.Collections.Specialized.StringDictionary keyValues, out string output)
        {
            output = string.Empty;
            string siteUrl = Params["url"].Value;
            string parentSection = Params["section"].Value;
            string nodeTitle = Params["title"].Value;
            string sourceUrl = Params["sourceurl"].Value;
            bool forceOverwrite = Params["force"].UserTypedIn;
            int? position = null;
            if (Params["position"].UserTypedIn)
            {
                position = Int32.Parse(Params["position"].Value);
            }
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
                        if (forceOverwrite)
                            nodeCollection.Delete(existingNode);
                        else
                        {
                            output =
                                "Link or section of the same name already exists. Please specify \"-force\" parameter to overwrite this link or section.";
                            return 1;
                        }
                    }
                    SPNavigationNode newLink = new SPNavigationNode(nodeTitle, sourceUrl, true);
                    AddNodeToCollection(nodeCollection, newLink, position);
                }
            }
            return OUTPUT_SUCCESS;
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

        static private void AddNodeToCollection(SPNavigationNodeCollection collection, SPNavigationNode node, int? position)
        {
            int pos;
            if (position.HasValue)
            {
                pos = position.Value;
            }
            else
            {
                collection.AddAsLast(node);
                return;
            }
            if (pos == 0)
            {
                collection.AddAsFirst(node);
                return;
            }
            collection.Add(node, collection.Navigation.QuickLaunch[pos - 1]);
        }

        #endregion
    }
}
