using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;
#if MOSS
using Microsoft.SharePoint.Publishing;
#endif
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;

namespace Stsadm.Commands.Lists
{
    public class DeleteListItem : SPOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteListItem"/> class.
        /// </summary>
        public DeleteListItem()
        {
            SPParamCollection parameters = new SPParamCollection();
            parameters.Add(new SPParam("url", "url", true, null, new SPUrlValidator(), "Please specify the list view URL."));
            parameters.Add(new SPParam("id", "id", false, null, new SPNonEmptyValidator()));
            parameters.Add(new SPParam("deletefolders", "df"));

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\nDeletes an item or items from a list.\r\n\r\nParameters:");
            sb.Append("\r\n\t-url <list view URL>");
            sb.Append("\r\n\t[-id <list item ID (separate multiple items with a comma)>]");
            sb.Append("\r\n\t[-deletefolders]");
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
            List<int> ids = new List<int>();
            if (Params["id"].UserTypedIn)
            {
                string[] ids2 = Params["id"].Value.Split(',');
                ids = new List<int>(ids2.Length);
                for (int i = 0; i < ids2.Length; i++)
                    if (!string.IsNullOrEmpty(ids2[i]))
                        ids.Add(int.Parse(ids2[i]));
            }

            DeleteListItems(url, ids, Params["deletefolders"].UserTypedIn);

            return OUTPUT_SUCCESS;
        }

        #endregion

        /// <summary>
        /// Deletes the list items.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="ids">The ids.</param>
        /// <param name="deleteFolders">if set to <c>true</c> [delete folders].</param>
        internal static void DeleteListItems(string url, List<int> ids, bool deleteFolders)
        {
            using (SPSite site = new SPSite(url))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    SPList list = Utilities.GetListFromViewUrl(web, url);

                    if (list == null)
                    {
                        throw new Exception("List not found.");
                    }

                    if (ids.Count == 0)
                    {
                        int index = 0;
                        while (true)
                        {
                            if (list.Items.Count == index)
                                break;

                            SPListItem item = list.Items[index];
                            if (item == null)
                                break;
#if MOSS
                            try
                            {
                                if (item.File != null && PublishingWeb.IsPublishingWeb(web))
                                {
                                    PublishingWeb pubWeb = PublishingWeb.GetPublishingWeb(web);
                                    if (pubWeb != null && pubWeb.DefaultPage.ServerRelativeUrl.ToLower() == item.File.ServerRelativeUrl.ToLower())
                                    {
                                        Console.WriteLine("WARNING: The item that you are trying to delete is the current welcome page and cannot be deleted.");
                                        index++;
                                        continue;
                                    }
                                }
                            }
                            catch (FileLoadException)
                            {
                                // This would occur in a WSS environment where the 'Microsoft.SharePoint.Publishing' assembly does not exist.
                            }
#endif
                            item.Delete();
                        }
                        if (deleteFolders)
                        {
                            while (true)
                            {
                                if (list.Folders == null || list.Folders.Count == 0 || list.Folders[0] == null)
                                    break;

                                list.Folders[0].Delete();
                            }
                        }
                    }
                    else
                    {
                        foreach (int id in ids)
                        {
                            SPListItem item = null;
                            try
                            {
                                item = list.GetItemById(id);
                                if (item == null)
                                    throw new ArgumentException();
#if MOSS
                                try
                                {
                                    if (item.File != null && PublishingWeb.IsPublishingWeb(web))
                                    {
                                        PublishingWeb pubWeb = PublishingWeb.GetPublishingWeb(web);
                                        if (pubWeb != null && pubWeb.DefaultPage.ServerRelativeUrl.ToLower() == item.File.ServerRelativeUrl.ToLower())
                                        {
                                            Console.WriteLine("WARNING: The item that you are trying to delete is the current welcome page and cannot be deleted.");
                                            continue;
                                        }
                                    }
                                }
                                catch (FileLoadException)
                                {
                                    // This would occur in a WSS environment where the 'Microsoft.SharePoint.Publishing' assembly does not exist.
                                }
#endif
                                item.Delete();
                            }
                            catch (ArgumentException)
                            {
                            }
                            if (item == null)
                                Console.WriteLine(string.Format("WARNING: List item not found: {0}.", id));
                        }
                    }
                }
            }
        }
    }
}
