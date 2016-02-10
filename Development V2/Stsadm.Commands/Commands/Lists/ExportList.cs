using System;
using System.Collections;
using System.IO;
using System.Text;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Deployment;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;

namespace Stsadm.Commands.Lists
{
    public class ExportList : SPOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportList"/> class.
        /// </summary>
        public ExportList()
        {
            SPParamCollection parameters = new SPParamCollection();
            parameters.Add(new SPParam("url", "url", true, null, new SPUrlValidator(), "Please specify the list view URL."));
            parameters.Add(new SPParam("filename", "f", true, null, new SPNonEmptyValidator(), "Please specify the filename."));
            parameters.Add(new SPParam("overwrite", "over", false, null, null));
            parameters.Add(new SPParam("quiet", "quiet", false, null, null));
            parameters.Add(new SPParam("includeusersecurity", "security", false, null, null));
            parameters.Add(new SPParam("haltonwarning", "warning", false, null, null));
            parameters.Add(new SPParam("haltonfatalerror", "error", false, null, null));
            parameters.Add(new SPParam("nologfile", "nolog", false, null, null));
            parameters.Add(new SPParam("versions", "v", false, SPIncludeVersions.All.ToString(), new SPIntRangeValidator(1, 4), "Please specify the version settings."));
            parameters.Add(new SPParam("cabsize", "csize", false, null, new SPIntRangeValidator(1, 0x400), "Please specify the cab size."));
            parameters.Add(new SPParam("nofilecompression", "nofilecompression", false, null, null));
            parameters.Add(new SPParam("includedescendants", "descendants", false, SPIncludeDescendants.All.ToString(), new SPEnumValidator(typeof(SPIncludeDescendants))));
            parameters.Add(new SPParam("excludedependencies", "exdep", false, null, null));

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\nExports a list.\r\n\r\nParameters:\r\n\t");
            sb.Append("-url <list view url>\r\n\t");
            sb.Append("-filename <export file name>\r\n\t");
            sb.Append("[-overwrite]\r\n\t");
            sb.Append("[-includeusersecurity]\r\n\t");
            sb.Append("[-haltonwarning]\r\n\t");
            sb.Append("[-haltonfatalerror]\r\n\t");
            sb.Append("[-nologfile]\r\n\t");
            sb.Append("[-versions <1-4>\r\n");
            sb.Append("\t\t1 - Last major version for files and list items\r\n");
            sb.Append("\t\t2 - The current version, either the last major or the last minor\r\n");
            sb.Append("\t\t3 - Last major and last minor version for files and list items\r\n");
            sb.Append("\t\t4 - All versions for files and list items (default)]\r\n\t");
            sb.Append("[-cabsize <integer from 1-1024 megabytes> (default: 25)]\r\n\t");
            sb.Append("[-nofilecompression]\r\n\t");
            sb.Append("[-includedescendants <All | Content | None>]\r\n\t");
            sb.Append("[-excludedependencies (Specifies whether to exclude dependencies from the export package when exporting objects of type SPFile or SPListItem)]\r\n\t");
            sb.Append("[-quiet]");
            
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
            bool compressFile = !Params["nofilecompression"].UserTypedIn;
            string filename = Params["filename"].Value;
            bool overwrite = Params["overwrite"].UserTypedIn;
            bool quiet = Params["quiet"].UserTypedIn;
            bool haltOnWarning = Params["haltonwarning"].UserTypedIn;
            bool haltOnFatalError = Params["haltonfatalerror"].UserTypedIn;
            bool includeusersecurity = Params["includeusersecurity"].UserTypedIn;
            bool excludeDependencies = Params["excludedependencies"].UserTypedIn;
            bool logFile = !Params["nologfile"].UserTypedIn;
            int cabSize = 0;

            if (Params["cabsize"].UserTypedIn)
            {
                cabSize = int.Parse(Params["cabsize"].Value);
            }

            SPIncludeVersions versions = SPIncludeVersions.All;
            if (Params["versions"].UserTypedIn)
                versions = (SPIncludeVersions)Enum.Parse(typeof(SPIncludeVersions), Params["versions"].Value);
            SPIncludeDescendants includeDescendents = (SPIncludeDescendants)Enum.Parse(typeof (SPIncludeDescendants), Params["includedescendants"].Value, true);
            PerformExport(url, filename, compressFile, haltOnFatalError, haltOnWarning, includeusersecurity, cabSize, logFile, overwrite, quiet, versions, includeDescendents, excludeDependencies);


            return OUTPUT_SUCCESS;
        }

        /// <summary>
        /// Performs the export.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="compressFile">if set to <c>true</c> [compress file].</param>
        /// <param name="haltOnFatalError">if set to <c>true</c> [halt on fatal error].</param>
        /// <param name="haltOnWarning">if set to <c>true</c> [halt on warning].</param>
        /// <param name="includeusersecurity">if set to <c>true</c> [includeusersecurity].</param>
        /// <param name="cabSize">Size of the CAB.</param>
        /// <param name="logFile">if set to <c>true</c> [log file].</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="quiet">if set to <c>true</c> [quiet].</param>
        /// <param name="versions">The versions.</param>
        /// <param name="includeDescendents">The include descendents.</param>
        /// <param name="excludeDependencies">if set to <c>true</c> [exclude dependencies].</param>
        public static void PerformExport(string url, string filename, bool compressFile, bool haltOnFatalError, bool haltOnWarning, bool includeusersecurity, int cabSize, bool logFile, bool overwrite, bool quiet, SPIncludeVersions versions, SPIncludeDescendants includeDescendents, bool excludeDependencies)
        {
            SPExportObject exportObject = new SPExportObject();
            SPExportSettings settings = new SPExportSettings();
            settings.ExcludeDependencies = excludeDependencies;
            SPExport export = new SPExport(settings);


            exportObject.Type = SPDeploymentObjectType.List;
            exportObject.IncludeDescendants = includeDescendents;
            ExportHelper.SetupExportObjects(settings, cabSize, compressFile, filename, haltOnFatalError, haltOnWarning, includeusersecurity, logFile, overwrite, quiet, versions);

            PerformExport(export, exportObject, settings, logFile, quiet, url);
        }

        #endregion

        /// <summary>
        /// Performs the export.
        /// </summary>
        /// <param name="export">The export.</param>
        /// <param name="exportObject">The export object.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="logFile">if set to <c>true</c> [log file].</param>
        /// <param name="quiet">if set to <c>true</c> [quiet].</param>
        /// <param name="url">The URL.</param>
        internal static void PerformExport(SPExport export, SPExportObject exportObject, SPExportSettings settings, bool logFile, bool quiet, string url)
        {
            using (SPSite site = new SPSite(url))
            {
                ImportHelper.ValidateUser(site);

                using (SPWeb web = site.OpenWeb())
                {
                    SPList list = Utilities.GetListFromViewUrl(web, url);

                    if (list == null)
                    {
                        throw new Exception("List not found.");
                    }

                    settings.SiteUrl = web.Url;
                    exportObject.Id = list.ID;
                }
            }

            settings.ExportObjects.Add(exportObject);


            try
            {
                export.Run();
                if (!quiet)
                {
                    ArrayList dataFiles = settings.DataFiles;
                    if (dataFiles != null)
                    {
                        Console.WriteLine();
                        Console.WriteLine("File(s) generated: ");
                        for (int i = 0; i < dataFiles.Count; i++)
                        {
                            Console.WriteLine("\t{0}", Path.Combine(settings.FileLocation, (string)dataFiles[i]));
                            Console.WriteLine();
                        }
                        Console.WriteLine();

                    }
                }
            }
            finally
            {
                if (logFile)
                {
                    Console.WriteLine();
                    Console.WriteLine("Log file generated: ");
                    Console.WriteLine("\t{0}", settings.LogFilePath);
                }
            }
        }

    }
}
