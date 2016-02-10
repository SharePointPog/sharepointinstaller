using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Deployment;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;

namespace Stsadm.Commands.Lists
{
    public class CopyListItem : ImportListItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CopyListItem"/> class.
        /// </summary>
        public CopyListItem()
        {
            SPParamCollection parameters = new SPParamCollection();
            parameters.Add(new SPParam("sourceurl", "sourceurl", true, null, new SPUrlValidator(), "Please specify the list view URL to copy items from."));
            parameters.Add(new SPParam("targeturl", "targeturl", true, null, new SPUrlValidator(), "Please specify the list view URL to copy items to."));
            parameters.Add(new SPParam("id", "id", false, null, new SPNonEmptyValidator()));
            parameters.Add(new SPParam("quiet", "quiet", false, null, null));
            parameters.Add(new SPParam("includeusersecurity", "security", false, null, null));
            parameters.Add(new SPParam("haltonwarning", "warning", false, null, null));
            parameters.Add(new SPParam("haltonfatalerror", "error", false, null, null));
            parameters.Add(new SPParam("nologfile", "nolog", false, null, null));
            parameters.Add(new SPParam("versions", "v", false, SPIncludeVersions.All.ToString(), new SPIntRangeValidator(1, 4), "Please specify the version settings."));
            parameters.Add(new SPParam("updateversions", "updatev", false, SPUpdateVersions.Append.ToString(), new SPIntRangeValidator(1, 3), "Please specify the updateversions setting."));
            parameters.Add(new SPParam("retargetlinks", "retargetlinks", false, null, null));
            parameters.Add(new SPParam("deletesource", "delete", false, null, null));
            parameters.Add(new SPParam("temppath", "temppath", false, null, new SPDirectoryExistsValidator()));
            parameters.Add(new SPParam("includedescendants", "descendants", false, SPIncludeDescendants.All.ToString(), new SPEnumValidator(typeof(SPIncludeDescendants))));
            parameters.Add(new SPParam("excludedependencies", "exdep", false, null, null));
            parameters.Add(new SPParam("nofilecompression", "nofilecompression", false, null, null));
            parameters.Add(new SPParam("suppressafterevents", "sae"));

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\nCopies a list item or items to another list.\r\n\r\nParameters:\r\n\t");
            sb.Append("-sourceurl <list view url to copy items from>\r\n\t");
            sb.Append("-targeturl <list view url to copy items to>\r\n\t");
            sb.Append("[-id <list item ID (separate multiple items with a comma)>]\r\n");
            sb.Append("[-includeusersecurity]\r\n\t");
            sb.Append("[-haltonwarning]\r\n\t");
            sb.Append("[-haltonfatalerror]\r\n\t");
            sb.Append("[-nologfile]\r\n\t");
            sb.Append("[-versions <1-4>\r\n");
            sb.Append("\t\t1 - Last major version for files and list items\r\n");
            sb.Append("\t\t2 - The current version, either the last major or the last minor\r\n");
            sb.Append("\t\t3 - Last major and last minor version for files and list items\r\n");
            sb.Append("\t\t4 - All versions for files and list items (default)]\r\n\t");
            sb.Append("[-updateversions <1-3>\r\n");
            sb.Append("\t\t1 - Add new versions to the current file (default)\r\n");
            sb.Append("\t\t2 - Overwrite the file and all its versions (delete then insert)\r\n");
            sb.Append("\t\t3 - Ignore the file if it exists on the destination]\r\n\t");
            sb.Append("[-quiet]\r\n\t");
            sb.Append("[-retargetlinks (resets links pointing to the source to now point to the target)]\r\n\t");
            sb.Append("[-deletesource]");
            sb.Append("[-temppath <temporary folder path for storing of export files>]\r\n\t");
            sb.Append("[-includedescendants <All | Content | None>]\r\n\t");
            sb.Append("[-excludedependencies (Specifies whether to exclude dependencies from the export package when exporting objects of type SPFile or SPListItem)]");
            sb.Append("[-nofilecompression]\r\n\t");
            sb.Append("[-suppressafterevents (disable the firing of \"After\" events when creating or modifying list items)]\r\n\t");
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

            string sourceUrl = Params["sourceurl"].Value;
            string targetUrl = Params["targeturl"].Value;
            bool compressFile = !Params["nofilecompression"].UserTypedIn;
            bool quiet = Params["quiet"].UserTypedIn;
            bool haltOnWarning = Params["haltonwarning"].UserTypedIn;
            bool haltOnFatalError = Params["haltonfatalerror"].UserTypedIn;
            bool includeusersecurity = Params["includeusersecurity"].UserTypedIn;
            bool excludeDependencies = Params["excludedependencies"].UserTypedIn;
            SPIncludeDescendants includeDescendents = (SPIncludeDescendants)Enum.Parse(typeof(SPIncludeDescendants), Params["includedescendants"].Value, true);
            bool logFile = !Params["nologfile"].UserTypedIn;
            bool deleteSource = Params["deletesource"].UserTypedIn;
            string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            if (Params["temppath"].UserTypedIn)
                directory = Params["temppath"].Value;
            SPIncludeVersions versions = SPIncludeVersions.All;
            if (Params["versions"].UserTypedIn)
                versions = (SPIncludeVersions)Enum.Parse(typeof(SPIncludeVersions), Params["versions"].Value);
            SPUpdateVersions updateVersions = SPUpdateVersions.Append;
            if (Params["updateversions"].UserTypedIn)
                updateVersions = (SPUpdateVersions)Enum.Parse(typeof(SPUpdateVersions), Params["updateversions"].Value);
            bool suppressAfterEvents = Params["suppressafterevents"].UserTypedIn;

            List<int> ids = new List<int>();
            if (Params["id"].UserTypedIn)
            {
                string[] ids2 = Params["id"].Value.Split(',');
                ids = new List<int>(ids2.Length);
                for (int i = 0; i < ids2.Length; i++)
                    if (!string.IsNullOrEmpty(ids2[i]))
                        ids.Add(int.Parse(ids2[i]));
            }
            bool retargetLinks = Params["retargetlinks"].UserTypedIn;

            CopyItem(ids, sourceUrl, targetUrl, retargetLinks, directory, compressFile, excludeDependencies, haltOnFatalError, haltOnWarning, includeusersecurity, logFile, quiet, versions, includeDescendents, updateVersions, deleteSource, suppressAfterEvents);

            return OUTPUT_SUCCESS;
        }

        /// <summary>
        /// Copies the item.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="sourceUrl">The source URL.</param>
        /// <param name="targetUrl">The target URL.</param>
        /// <param name="retargetLinks">if set to <c>true</c> [retarget links].</param>
        /// <param name="excludeDependencies">if set to <c>true</c> [exclude dependencies].</param>
        /// <param name="includeUserSecurity">if set to <c>true</c> [include user security].</param>
        /// <param name="versions">The versions.</param>
        /// <param name="includeDescendents">The include descendents.</param>
        /// <param name="updateVersions">The update versions.</param>
        /// <param name="suppressAfterEvents">if set to <c>true</c> [suppress after events].</param>
        internal void CopyItem(List<int> ids, string sourceUrl, string targetUrl, bool retargetLinks, bool excludeDependencies, bool includeUserSecurity, SPIncludeVersions versions, SPIncludeDescendants includeDescendents, SPUpdateVersions updateVersions, bool suppressAfterEvents)
        {
            string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            CopyItem(ids, sourceUrl, targetUrl, retargetLinks, directory, true, excludeDependencies, true, false,
                     includeUserSecurity, false, true, versions, includeDescendents, updateVersions, false, suppressAfterEvents);
        }

        /// <summary>
        /// Copies the item.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="sourceUrl">The source URL.</param>
        /// <param name="targetUrl">The target URL.</param>
        /// <param name="retargetLinks">if set to <c>true</c> [retarget links].</param>
        /// <param name="directory">The directory.</param>
        /// <param name="compressFile">if set to <c>true</c> [compress file].</param>
        /// <param name="excludeDependencies">if set to <c>true</c> [exclude dependencies].</param>
        /// <param name="haltOnFatalError">if set to <c>true</c> [halt on fatal error].</param>
        /// <param name="haltOnWarning">if set to <c>true</c> [halt on warning].</param>
        /// <param name="includeusersecurity">if set to <c>true</c> [includeusersecurity].</param>
        /// <param name="logFile">if set to <c>true</c> [log file].</param>
        /// <param name="quiet">if set to <c>true</c> [quiet].</param>
        /// <param name="versions">The versions.</param>
        /// <param name="includeDescendents">The include descendents.</param>
        /// <param name="updateVersions">The update versions.</param>
        /// <param name="deleteSource">if set to <c>true</c> [delete source].</param>
        /// <param name="suppressAfterEvents">if set to <c>true</c> [suppress after events].</param>
        internal void CopyItem(List<int> ids, string sourceUrl, string targetUrl, bool retargetLinks, string directory, bool compressFile, bool excludeDependencies, bool haltOnFatalError, bool haltOnWarning, bool includeusersecurity, bool logFile, bool quiet, SPIncludeVersions versions, SPIncludeDescendants includeDescendents, SPUpdateVersions updateVersions, bool deleteSource, bool suppressAfterEvents)
        {
            m_sourceUrl = sourceUrl;
            m_targetUrl = targetUrl;
            m_retargetLinks = retargetLinks;


            string filename = directory;
            if (compressFile)
                filename = Path.Combine(directory, "temp.cmp");
            
            SPExportSettings exportSettings = new SPExportSettings();
            exportSettings.ExcludeDependencies = excludeDependencies;
            SPExport export = new SPExport(exportSettings);

            ExportHelper.SetupExportObjects(exportSettings, 0, compressFile, filename, haltOnFatalError, haltOnWarning, includeusersecurity, logFile, true, quiet, versions);

            ExportListItem.PerformExport(export, exportSettings, includeDescendents, logFile, quiet, sourceUrl, ids);

            SPImportSettings importSettings = new SPImportSettings();
            SPImport import = new SPImport(importSettings);

            ImportHelper.SetupImportObject(importSettings, compressFile, filename, haltOnFatalError, haltOnWarning, includeusersecurity, logFile, quiet, updateVersions, false, suppressAfterEvents);

            PerformImport(import, importSettings, logFile, targetUrl);

            //if (m_list != null)
            //{
            //    using (SPWeb web = m_list.ParentWeb)
            //    using (SPSite site = web.Site)
            //    {
            //        // If the list is a discussion list then attempt to resolve flattened threads.
            //        SiteCollectionSettings.RepairSiteCollectionImportedFromSubSite.RepairDiscussionList(site, m_list);
            //    }
            //}

            if (!logFile && !deleteSource)
            {
                Directory.Delete(directory, true);
            }
            else if (logFile && !deleteSource)
            {
                foreach (string s in Directory.GetFiles(directory))
                {
                    FileInfo file = new FileInfo(s);
                    if (file.Extension == ".log")
                        continue;
                    file.Delete();
                }
            }

            if (deleteSource)
            {
                DeleteListItem.DeleteListItems(sourceUrl, ids, false);
                Console.WriteLine("Source list item(s) deleted.  You can find the exported list item(s) here: " + directory);
                Console.WriteLine();
            }
        }

        #endregion

    }
}
