using System;
using System.Collections.Generic;
using System.Text;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;


namespace Stsadm.Commands.Lists
{
    public class ClearList : SPOperation
    {
        public ClearList()
        {
            SPParamCollection parameters = new SPParamCollection();
            parameters.Add(new SPParam("url", "url", true, null, new SPUrlValidator(), "Please specify the site URL."));

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\nClear List\r\n\r\nParameters:");
            sb.Append("\r\n\t-url <site URL>");

            Init(parameters, sb.ToString());
        }
        #region ISPStsadmCommand Members

        public override int Execute(string command, System.Collections.Specialized.StringDictionary keyValues, out string output)
        {
            output = string.Empty;
            string url = Params["url"].Value;

            using (SPSite site = new SPSite(url))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    web.AllowUnsafeUpdates = true;
                    SPList list = Utilities.GetListFromViewUrl(web, url);
                    StringBuilder sbDelete = new StringBuilder();
                    sbDelete.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Batch>");

                    foreach (SPListItem item in list.Items)
                    {
                        sbDelete.Append("<Method>");
                        sbDelete.Append("<SetList Scope=\"Request\">" + list.ID + "</SetList>");
                        sbDelete.Append("<SetVar Name=\"ID\">" + Convert.ToString(item.ID) + "</SetVar>");
                        sbDelete.Append("<SetVar Name=\"Cmd\">Delete</SetVar>");
                        sbDelete.Append("</Method>");
                    }

                    sbDelete.Append("</Batch>");

                    web.ProcessBatchData(sbDelete.ToString());

                    web.AllowUnsafeUpdates = false;
                }
            }


            return OUTPUT_SUCCESS;
        }

        public override string GetHelpMessage(string command)
        {
            return HelpMessage;
        }

        #endregion

    }
}
