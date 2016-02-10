using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;

using Microsoft.SharePoint;

namespace Stsadm.Commands.Lists
{
    public class SetListContentTypes : SPOperation
    {
         /// <summary>
        /// Initializes a new instance of the <see cref="SetListContentTypes"/> class.
        /// </summary>
        public SetListContentTypes()
        {
            SPParamCollection parameters = new SPParamCollection();
            parameters.Add(new SPParam("url", "url", true, null, new SPUrlValidator(), "Please specify the list view URL."));
            parameters.Add(new SPParam("add", "a", false, null, new SPNonEmptyValidator()));
            parameters.Add(new SPParam("remove", "r", false, null, new SPNonEmptyValidator()));
            parameters.Add(new SPParam("order", "order", false, null, new SPNonEmptyValidator()));
            parameters.Add(new SPParam("allowmanagement", "am", false, null, new SPTrueFalseValidator()));

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\nAdds or removes content types associated with the given list.\r\n\r\nParameters:");
            sb.Append("\r\n\t-url <list view url>");
            sb.Append("\r\n\t[-add <comma separated list of content type names to add to the list>]");
            sb.Append("\r\n\t[-remove <comma separated list of content type names to remove from the list>]");
            sb.Append("\r\n\t[-order <new button order, comma separated, first will be the default>]");
            sb.Append("\r\n\t[-allowmanagement <true | false>]");
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
            Verbose = true;

            string url = Params["url"].Value;
            string[] contentTypesToAdd = new string[] {};
            string[] contentTypesToRemove = new string[] { };
            string[] contentTypeOrder = new string[] { };

            if (Params["add"].UserTypedIn)
                contentTypesToAdd = GetStringArray(Params["add"].Value);
            if (Params["remove"].UserTypedIn)
                contentTypesToRemove = GetStringArray(Params["remove"].Value);
            if (Params["order"].UserTypedIn)
                contentTypeOrder = GetStringArray(Params["order"].Value);

            using (SPSite site = new SPSite(url))
            using (SPWeb web = site.OpenWeb())
            {
                SPList list = Utilities.GetListFromViewUrl(web, url);

                if (list == null)
                    throw new SPException("List was not found.");

                SetContentTypes(site, web, list, contentTypesToAdd, contentTypesToRemove, contentTypeOrder);

                if (Params["allowmanagement"].UserTypedIn)
                {
                    list.ContentTypesEnabled = bool.Parse(Params["allowmanagement"].Value);
                    list.Update();
                }
            }

            return OUTPUT_SUCCESS;
        }

       
        #endregion

        /// <summary>
        /// Gets the string array of values.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private static string[] GetStringArray(string input)
        {
            const string key = "__SETLISTCONTENTTYPES__";
            input = input.Replace(",,", key);
            string[] output = input.Split(',');
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = output[i].Replace(key, ",");
            }
            return output;
        }

        /// <summary>
        /// Sets the content types.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="web">The web.</param>
        /// <param name="list">The list.</param>
        /// <param name="contentTypesToAdd">The content types to add.</param>
        /// <param name="contentTypesToDelete">The content types to delete.</param>
        /// <param name="contentTypeOrder">The content type order.</param>
        public static void SetContentTypes(SPSite site, SPWeb web, SPList list, string[] contentTypesToAdd, string[] contentTypesToDelete, string[] contentTypeOrder)
        {
            foreach (string ct in contentTypesToDelete)
            {
                foreach (SPContentType contentType in list.ContentTypes)
                {
                    if (contentType.Name.ToLowerInvariant() == ct.Trim().ToLowerInvariant())
                    {
                        list.ContentTypes.Delete(contentType.Id);
                        break;
                    }
                }
            }
            foreach (string ct in contentTypesToAdd)
            {
                AddContentTypeToList(site, web, list, ct.Trim());
            }
            ChangeContentTypeOrder(list, contentTypeOrder);
        }

        /// <summary>
        /// Changes the content type order.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="contentTypes">The content types.</param>
        public static void ChangeContentTypeOrder(SPList list, string[] contentTypes)
        {
            if (contentTypes.Length == 0)
                return;

            IList<SPContentType> contentTypeOrder = new List<SPContentType>();
            foreach (string contentTypeName in contentTypes)
            {
                SPContentType contentType = null;
                try
                {
                    contentType = list.ContentTypes[contentTypeName.Trim()];
                }
                catch (ArgumentException)
                {
                    Log("WARNING: Unable to set content type order for '{0}'.  Content type was not found.", EventLogEntryType.Warning,
                        contentTypeName);
                }
                if (contentType != null)
                    contentTypeOrder.Add(contentType);
                else
                    Log("WARNING: Unable to set content type order for '{0}'.  Content type was not found.", EventLogEntryType.Warning,
                        contentTypeName);
            }
            list.RootFolder.UniqueContentTypeOrder = contentTypeOrder;
            list.RootFolder.Update();

        }


        /// <summary>
        /// Adds the content type to list.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="web">The web.</param>
        /// <param name="list">The list.</param>
        /// <param name="contentTypeName">Name of the content type.</param>
        /// <returns></returns>
        public static SPContentType AddContentTypeToList(SPSite site, SPWeb web, SPList list, string contentTypeName)
        {
            SPContentType contentType = null;
            try
            {
                contentType = list.ContentTypes[contentTypeName];
            }
            catch (ArgumentException)
            { }
            if (contentType == null)
            {
                try
                {
                    // Get the content type from the web and add to the list.
                    contentType = web.AvailableContentTypes[contentTypeName];
                }
                catch (ArgumentException)
                {
                    try
                    {
                        contentType = site.RootWeb.AvailableContentTypes[contentTypeName];
                    }
                    catch (ArgumentException)
                    {
                    }
                }
                if (contentType == null)
                    throw new SPException(string.Format("Unable to find content type '{0}'", contentTypeName));
                try
                {
                    return list.ContentTypes.Add(contentType);
                }
                catch (SPException)
                {
                    Log("WARNING: The site content type '{0}' has already been added to this list.", EventLogEntryType.Warning, contentTypeName);
                }
            }
            return null;
        }
    }
}
