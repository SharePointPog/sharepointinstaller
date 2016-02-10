using System;
using System.Collections.Specialized;
using System.Text;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Deployment;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;
using System.Collections.Generic;

namespace Stsadm.Commands.Lists
{
    ///<summary>
    ///</summary>
    public class ImportListItem : SPOperation
    {
        protected string m_targetUrl;
        protected string m_sourceUrl;
        protected SPList m_list;
        protected bool m_retargetLinks;
        private StringDictionary m_filesToMove = new StringDictionary();
        private List<string> itemsList = new List<string>();
        private bool allowDublicates;
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportListItem"/> class.
        /// </summary>
        public ImportListItem()
        {
            SPParamCollection parameters = new SPParamCollection();
            parameters.Add(new SPParam("url", "url", true, null, new SPUrlValidator(), "Please specify the list view url to import the items to."));
            parameters.Add(new SPParam("filename", "f", true, null, new SPDirectoryExistsAndValidFileNameValidator(), "Please specify the filename."));
            parameters.Add(new SPParam("quiet", "quiet", false, null, null));
            parameters.Add(new SPParam("includeusersecurity", "security", false, null, null));
            parameters.Add(new SPParam("haltonwarning", "warning", false, null, null));
            parameters.Add(new SPParam("haltonfatalerror", "error", false, null, null));
            parameters.Add(new SPParam("nologfile", "nolog", false, null, null));
            parameters.Add(new SPParam("updateversions", "updatev", false, SPUpdateVersions.Append.ToString(), new SPIntRangeValidator(1, 3), "Please specify the updateversions setting."));
            parameters.Add(new SPParam("nofilecompression", "nofilecompression", false, null, null));
            parameters.Add(new SPParam("retargetlinks", "retargetlinks", false, null, null));
            parameters.Add(new SPParam("sourceurl", "source", false, null, new SPUrlValidator(), "Please specify the URL of the source site (must be on the same farm)."));
            parameters.Add(new SPParam("retainobjectidentity", "retainid", false, null, null));
            parameters.Add(new SPParam("suppressafterevents", "sae"));
            parameters.Add(new SPParam("allowdublicates", "allowdublicates"));

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\nImports a list item or items.\r\n\r\nParameters:\r\n\t");
            sb.Append("-url <list view url to import into>\r\n\t");
            sb.Append("-filename <import file name>\r\n\t");
            sb.Append("[-includeusersecurity]\r\n\t");
            sb.Append("[-haltonwarning]\r\n\t");
            sb.Append("[-haltonfatalerror]\r\n\t");
            sb.Append("[-nologfile]\r\n\t");
            sb.Append("[-updateversions <1-3>\r\n");
            sb.Append("\t\t1 - Add new versions to the current file (default)\r\n");
            sb.Append("\t\t2 - Overwrite the file and all its versions (delete then insert)\r\n");
            sb.Append("\t\t3 - Ignore the file if it exists on the destination]\r\n\t");
            sb.Append("[-nofilecompression]\r\n\t");
            sb.Append("[-quiet]\r\n\t");
            sb.Append("[-retargetlinks (resets links pointing to the source to now point to the target)]\r\n\t");
            sb.Append("[-sourceurl <url to a view of the original list> (use if retargetlinks)]\r\n\t");
            sb.Append("[-suppressafterevents (disable the firing of \"After\" events when creating or modifying list items)]\r\n\t");
            sb.Append("[-retainobjectidentity]");
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
            output = string.Empty;

            string url = Params["url"].Value;
            bool compressFile = !Params["nofilecompression"].UserTypedIn;
            string filename = Params["filename"].Value;
            bool quiet = Params["quiet"].UserTypedIn;
            bool haltOnWarning = Params["haltonwarning"].UserTypedIn;
            bool haltOnFatalError = Params["haltonfatalerror"].UserTypedIn;
            bool includeusersecurity = Params["includeusersecurity"].UserTypedIn;
            bool logFile = !Params["nologfile"].UserTypedIn;
            bool retainObjectIdentity = Params["retainobjectidentity"].UserTypedIn;
            bool suppressAfterEvents = Params["suppressafterevents"].UserTypedIn;
            allowDublicates = Params["allowdublicates"].UserTypedIn;

            m_sourceUrl = Params["sourceurl"].Value;
            m_targetUrl = Params["url"].Value;
            m_retargetLinks = Params["retargetlinks"].UserTypedIn;

            SPUpdateVersions updateVersions = SPUpdateVersions.Append;

            SPImportSettings settings = new SPImportSettings();
            SPImport import = new SPImport(settings);

            if (Params["updateversions"].UserTypedIn)
                updateVersions = (SPUpdateVersions)Enum.Parse(typeof(SPUpdateVersions), Params["updateversions"].Value);
            

            ImportHelper.SetupImportObject(settings, compressFile, filename, haltOnFatalError, haltOnWarning, includeusersecurity, logFile, quiet, updateVersions, retainObjectIdentity, suppressAfterEvents);

            PerformImport(import, settings, logFile, url);


            //if (m_list != null)
            //{
            //    using (SPWeb web = m_list.ParentWeb)
            //    using (SPSite site = web.Site)
            //    {
            //        // If the list is a discussion list then attempt to resolve flattened threads.
            //        SiteCollectionSettings.RepairSiteCollectionImportedFromSubSite.RepairDiscussionList(site, m_list);
            //    }
            //}

            return OUTPUT_SUCCESS;
        }

        #endregion

        /// <summary>
        /// Validates the specified key values.
        /// </summary>
        /// <param name="keyValues">The key values.</param>
        //public override void Validate(StringDictionary keyValues)
        //{
        //    if (Params["retargetlinks"].UserTypedIn)
        //        Params["sourceurl"].IsRequired = true;

        //    base.Validate(keyValues);
        //}

        /// <summary>
        /// Performs the import.
        /// </summary>
        /// <param name="import">The import.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="logFile">if set to <c>true</c> [log file].</param>
        /// <param name="url">The URL.</param>
        internal void PerformImport(SPImport import, SPImportSettings settings, bool logFile, string url)
        {
            using (SPSite site = new SPSite(url))
            {
                ImportHelper.ValidateUser(site);

                using (SPWeb web = site.OpenWeb())
                {
                    //import.Settings.WebId = web.ID;
                    m_list = Utilities.GetListFromViewUrl(web, url);
                    if (m_list == null)
                    {
                        throw new Exception("List not found.");
                    }

                    settings.SiteUrl = site.Url;
                    // The following two lines are not really needed because I'm setting the TargetParentUrl for each item.
                    settings.WebUrl = web.Url;
                }
            }
          
            import.Started += new EventHandler<SPDeploymentEventArgs>(OnImportStarted);

            import.ObjectImported += new EventHandler<SPObjectImportedEventArgs>(OnImported);

            import.Completed += new EventHandler<SPDeploymentEventArgs>(OnCompleted);
            try
            {
                import.Run();
            }
            finally
            {
                if (logFile)
                {
                    Console.WriteLine();
                    Console.WriteLine("Log file generated: ");
                    Console.WriteLine("\t{0}", settings.LogFilePath);
                    Console.WriteLine();
                }
            }
        }



        /// <summary>
        /// Called when [completed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Microsoft.SharePoint.Deployment.SPDeploymentEventArgs"/> instance containing the event data.</param>
        private void OnCompleted(object sender, SPDeploymentEventArgs e)
        {
            foreach (string source in m_filesToMove.Keys)
            {
                foreach (SPListItem item in m_list.Items)
                {
                    if (item.Name == source)
                    {
                        // We found the new item so if there's a file associated with it then do the move.
                        // We currently can't handle items without files as there is no built in move operation for
                        // these items.
                        if (item.File != null)
                        {
                            item.File.MoveTo(m_filesToMove[source], true);
                        }
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// Called when [import started].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Microsoft.SharePoint.Deployment.SPDeploymentEventArgs"/> instance containing the event data.</param>
        private void OnImportStarted(object sender, SPDeploymentEventArgs e)
        {
            if (m_list.BaseType != SPBaseType.DocumentLibrary)
                return;
            foreach (SPImportObject io in e.RootObjects)
            {
                // This would appear to be causing a bug but it's actually an attempt at doing my best to resolve a bug with
                // the deployment API.  The problem is that if I don't set everything to be targeted to the root folder then
                // the items could end up just about anywhere.  This is because there's some sort of bug with the deployment
                // API which is resulting in the import placing items in seemingly random locations - I believe that it's
                // picking up one location and then sticking with that for all items rather than re-evaluating the location
                // for each item.  Example - if you have a page layout with a preview image in "_catalogs/masterpages/en-us/preview images/"
                // the import will put the layout page and the preview image in the preview images folder if I remove the
                // line below.  Try and set the TargetParentUrl for each item individually and it will simply put everything
                // in the "_catalogs/masterpages/" folder and not in the individual folders.  So, rather than let the import
                // place things "wherever" I decided to force it all to the root so that it's easy to find and move post import.
                io.TargetParentUrl = m_list.RootFolder.ServerRelativeUrl;


                // Attempt to determine what the final path to the item should be and store it for later moving (attempts to
                // solve the problem above for files).
                using (SPWeb web = m_list.ParentWeb)
                {
                    string targetRelativePath = io.TargetParentUrl.Substring(web.ServerRelativeUrl.Length).TrimStart('/');
                    string sourcePath = io.SourceUrl.TrimStart('/');
                    // Keep triming path items off the source path until we get a relative path that reflects our targets relative path.
                    while (true)
                    {
                        if (sourcePath.StartsWith(targetRelativePath))
                            break;
                        else
                        {
                            if (sourcePath.Contains("/"))
                                sourcePath = sourcePath.Substring(sourcePath.IndexOf('/') + 1);
                            else
                                break;
                        }
                    }
                    if (io.Type == SPDeploymentObjectType.ListItem &&
                        sourcePath.Substring(0, sourcePath.LastIndexOf('/') + 1) != targetRelativePath)
                    {
                        // The sources relative path does not match what we have so flag it for moving.
                        m_filesToMove.Add(io.SourceUrl.Substring(io.SourceUrl.LastIndexOf('/') + 1), sourcePath);
                    }
                }
            }
        }

        /// <summary>
        /// Called when [imported].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="Microsoft.SharePoint.Deployment.SPObjectImportedEventArgs"/> instance containing the event data.</param>
        private void OnImported(object sender, SPObjectImportedEventArgs eventArgs)
        {
            if (eventArgs.Type != SPDeploymentObjectType.ListItem)
                return;
            if (!allowDublicates && (eventArgs.SourceUrl != eventArgs.TargetUrl))
            {
                SPListItem itemToRemove = m_list.GetItemByUniqueId(eventArgs.TargetId);
                m_list.Items.DeleteItemById(itemToRemove.ID);
            }
            SPImport import = sender as SPImport;
            if (import == null)
                return;

            string sourceUrl = eventArgs.SourceUrl; // This is not fully qualified so we need the user specified url for the site.

            if (!m_retargetLinks)
                return;

            try
            {
                using (SPSite targetSite = new SPSite(m_targetUrl))
                using (SPSite sourceSite = new SPSite(m_sourceUrl))
                using (SPWeb sourceWeb = sourceSite.OpenWeb(sourceUrl, false))
                {
                    string targetUrl = targetSite.MakeFullUrl(eventArgs.TargetUrl);
                    SPListItem li = sourceWeb.GetListItem(sourceUrl);
                    if (li.FileSystemObjectType == SPFileSystemObjectType.Folder)
                        return;

                    int count = li.BackwardLinks.Count;
                    for (int i = count - 1; i >= 0; i--)
                    {
                        SPLink link = li.BackwardLinks[i];
                        using (SPWeb rweb = sourceSite.OpenWeb(link.ServerRelativeUrl, false))
                        {
                            object o = rweb.GetObject(link.ServerRelativeUrl);
                            if (o is SPFile)
                            {
                                SPFile f = o as SPFile;
                                f.ReplaceLink(eventArgs.SourceUrl, targetUrl);
                            }
                            if (o is SPListItem)
                            {
                                SPListItem l = o as SPListItem;
                                l.ReplaceLink(eventArgs.SourceUrl, targetUrl);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Links could not be retargeted for " + eventArgs.SourceUrl + "\r\n" + ex.Message);
            }
        } 


    }
}
