using System;
using System.Collections.Specialized;
using System.IO;
using Stsadm.Commands.Lists;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;
using System.Text;
using Microsoft.SharePoint.Deployment;

namespace Stsadm.Commands.Lists
{
    public class DeleteList : SPOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteList"/> class.
        /// </summary>
        public DeleteList()
        {
            SPParamCollection parameters = new SPParamCollection();
            parameters.Add(new SPParam("url", "url", true, null, new SPUrlValidator(), "Please specify the list view URL."));
            parameters.Add(new SPParam("listname", "list", false, null, new SPNonEmptyValidator()));
            parameters.Add(new SPParam("force", "f"));
            parameters.Add(new SPParam("backupdir", "backup", false, null, new SPDirectoryExistsValidator()));

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\nDeletes a list.\r\n\r\nParameters:");
            sb.Append("\r\n\t-url <list view URL or web url>");
            sb.Append("\r\n\t[-listname <list name if url is a web url and not a list view url>]");
            sb.Append("\r\n\t[-force]");
            sb.Append("\r\n\t[-backupdir <directory to backup the list to>]");
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
        public override int Execute(string command, StringDictionary keyValues, out string output)
        {

            try
            {
                output = string.Empty;
                string url = Params["url"].Value.TrimEnd('/');
                bool force = Params["force"].UserTypedIn;
                SPList list = null;
                if (Utilities.EnsureAspx(url, false, false) && !Params["listname"].UserTypedIn)
                    list = Utilities.GetListFromViewUrl(url);
                else if (Params["listname"].UserTypedIn)
                {
                    using (SPSite site = new SPSite(url))
                    using (SPWeb web = site.OpenWeb())
                    {
                        try
                        {
                            list = web.Lists[Params["listname"].Value];
                        }
                        catch (ArgumentException)
                        {
                            throw new SPException("List not found.");
                        }
                    }
                }

                if (list == null)
                    throw new SPException("List not found.");


                if (Params["backupdir"].UserTypedIn)
                {
                    string path = Path.Combine(Params["backupdir"].Value, list.RootFolder.Name.Replace(" ", "_"));
                    int i = 0;
                    while (Directory.Exists(path + i))
                        i++;
                    path += i;
                    Directory.CreateDirectory(path);
                    try
                    {
                        using (SPSite site = new SPSite(url))
                        {
                            ExportList.PerformExport(
                                site.MakeFullUrl(list.DefaultViewUrl),
                                Path.Combine(path, list.RootFolder.Name),
                                true, false, false, true, 0, true, false,
                                false, SPIncludeVersions.All, SPIncludeDescendants.All, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new SPException("Unable to backup list.  List not deleted.", ex);
                    }
                }
                Delete(list, force, url);
            }
            catch (Exception e)
            {
                output = String.Format("Message: {0}\nException: {1}", e.Message, e);
                return OUTPUT_FAILED;
            }
            return OUTPUT_SUCCESS;
        }

        /// <summary>
        /// Validates the specified key values.
        /// </summary>
        /// <param name="keyValues">The key values.</param>
        public override void Validate(StringDictionary keyValues)
        {
            if (Params["url"].UserTypedIn && !Utilities.EnsureAspx(Params["url"].Value, false, false))
                Params["listname"].IsRequired = true;

            base.Validate(keyValues);
        }

        #endregion

        /// <summary>
        /// Deletes the specified list.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="force">if set to <c>true</c> [force].</param>
        /// <param name="url">The URL.</param>
        internal static void Delete(SPList list, bool force, string url)
        {
            if (list == null)
                throw new SPException("List not found.");

            if (!list.AllowDeletion && force)
            {
                list.AllowDeletion = true;
                list.Update();
            }
            else if (!list.AllowDeletion)
                throw new SPException("List cannot be deleted.  Try using the '-force' parameter to force the delete.");

            try
            {
                list.Delete();
            }
            catch (Exception)
            {
                if (force)
                {
                    using (SPSite site = new SPSite(url))
                    {
                        Utilities.RunStsAdmOperation(
                            string.Format(" -o forcedeletelist -url \"{0}\"",
                                          site.MakeFullUrl(list.RootFolder.ServerRelativeUrl)), false);
                    }
                }
            }
        }
    }
}
