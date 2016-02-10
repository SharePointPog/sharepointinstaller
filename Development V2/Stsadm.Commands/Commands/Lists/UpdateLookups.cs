using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;

namespace Stsadm.Commands.Lists
{
    public delegate void ExecuteSiteCommand(SPSite site, SPWeb web, SPWeb referencedWeb);
    /// <summary>
    /// This class holds the command used to update a lookup field with a guid retrieved by list's name
    /// </summary>
    /// <remarks>
    /// It's a utility command necessary for deployment of inter-related lists functionality
    /// </remarks>
    public class UpdateLookups : SPOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteListField"/> class.
        /// </summary>
        public UpdateLookups()
        {
            var parameters = new SPParamCollection();
            parameters.Add(new SPParam("listurl", "lurl", true, null, new SPUrlValidator(), "Please specify the list URL."));
            parameters.Add(new SPParam("weburl", "wurl", false, null, new SPUrlValidator(), "Please specify the web URL."));

            var sb = new StringBuilder();
            sb.Append("\r\n\r\nUpdates a lookup field with a guid retrieved by list's name.\r\n\r\nParameters:");
            sb.Append("\r\n\t-weburl <URl to a referenced web>");
            sb.Append("\r\n\t-listurl <URL of a list to process>");
            Init(parameters, sb.ToString());
        }

        #region ISPStsadmCommand Members

        /// <summary>
        /// Gets the help message.
        /// </summary>
        /// <param name="command">The command name.</param>
        /// <returns>The help message to display when this command is run with -help switch.</returns>
        public override string GetHelpMessage(string command)
        {
            return HelpMessage;
        }

        /// <summary>
        /// Runs the specified command.
        /// </summary>
        /// <param name="command">The command name.</param>
        /// <param name="keyValues">The following parameters are expected:<![CDATA[
        ///         -listurl <URL of a list to process>]]>
        /// </param>
        /// <param name="output">The output to display.</param>
        /// <returns>The status code of the operation (zero means success).</returns>
        public override int Execute(string command, StringDictionary keyValues, out string output)
        {
            var result = OUTPUT_SUCCESS;
            var builder = new StringBuilder();

            Do(delegate(SPSite site, SPWeb web, SPWeb referencedWeb)
                   {
                       SPList list;
                       try
                       {
                           list = Utilities.GetListFromViewUrl(Params["listurl"].Value);
                           foreach (SPField field in list.Fields)
                           {
                               var lookupField = field as SPFieldLookup;
                               if (lookupField == null)
                                   continue;

                               var doc = new XmlDocument();
                               doc.LoadXml(field.SchemaXml);
                               var customAttribute = doc.DocumentElement.Attributes["_ListName"];
                               if (customAttribute == null || String.IsNullOrEmpty(customAttribute.Value))
                                   continue;

                               try
                               {
                                   var referencedList = referencedWeb.Lists[customAttribute.Value];
                                   doc.DocumentElement.Attributes["List"].Value = referencedList.ID.ToString();
                                   doc.DocumentElement.Attributes.Remove(customAttribute);

                                   web.AllowUnsafeUpdates = true;
                                   lookupField.SchemaXml = doc.OuterXml;
                                   lookupField.LookupList = referencedList.ID.ToString();
                                   lookupField.LookupWebId = referencedWeb.ID;
                                   lookupField.Update();
                                   web.AllowUnsafeUpdates = false;

                                   builder.AppendFormat("Successfully updated field {0} with reference to list {1} ({2}){3}", lookupField.Title, customAttribute.Value, referencedList.ID, Environment.NewLine);
                               }
                               catch (ArgumentException)
                               {
                                   builder.AppendFormat("Referenced list {0} doesn't exist.", customAttribute.Value);
                                   result = 1;
                               }
                           }
                       }
                       catch (FileNotFoundException)
                       {
                           builder.AppendFormat("No list under this URL: {0}.", Params["listurl"].Value);
                           result = 1;
                       }                       
                   });

            output = builder.Length == 0 ? "No lookup fields require update." : builder.ToString();
            return result;
        }

        private void Do(ExecuteSiteCommand command)
        {
             using(var site = new SPSite(Params["listurl"].Value))
             {
                 using(var web = site.OpenWeb())
                 {
                     SPWeb referencedWeb = null;
                     try
                     {
                         referencedWeb = Params["weburl"].UserTypedIn ? site.OpenWeb(new Uri(Params["weburl"].Value).AbsolutePath) : web;
                         if (command != null)
                             command(site, web, referencedWeb);
                     }
                     finally
                     {
                         if (referencedWeb != null && referencedWeb.ID != web.ID)
                             referencedWeb.Dispose();
                     }
                 }
             }
        }


        #endregion
    }
}
