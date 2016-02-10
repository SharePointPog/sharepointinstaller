using System;
using System.Collections.Generic;
using System.Text;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Navigation;
using Microsoft.SharePoint.StsAdmin;
using Stsadm.Commands.OperationHelpers;

namespace Stsadm.Commands.Workflows
{
    public class RestartActiveWorkflows : SPOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestartActiveWorkflows"/> class.
        /// </summary>
        public RestartActiveWorkflows()
        {
            SPParamCollection parameters = new SPParamCollection();
            parameters.Add(new SPParam("url", "url", true, null, new SPUrlValidator(), "Please specify the site URL."));
            parameters.Add(new SPParam("workflowname", "workflowname", true, null, new SPNonEmptyValidator(), "Please specify the workflow Name."));
            parameters.Add(new SPParam("associationname", "associationname",true, null, new SPNonEmptyValidator(), "Please specify the association Name."));
            parameters.Add(new SPParam("newassociationname", "newassociationname", false, null, null));
            parameters.Add(new SPParam("taskslistname", "taskslistname", true, null, new SPNonEmptyValidator(), "Please specify the tasks list Name."));
            parameters.Add(new SPParam("historylistname", "historylistname", true, null, new SPNonEmptyValidator(), "Please specify the history list Name."));

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\nRestart all active workflows after changing workflow association\r\n\r\nParameters:");
            sb.Append("\r\n\t-url <site URL>");
            sb.Append("\r\n\t-workflowname <workflowname>");
            sb.Append("\r\n\t-associationname <associationname> (name of workflow association)");
            sb.Append("\r\n\t[-newassociationname <newassociationname>] (name of new workflow association)");
            sb.Append("\r\n\t-taskslistname <taskslistname> (name of tasks list)");
            sb.Append("\r\n\t-historylistname <historylistname> (name of history list)");
           
            Init(parameters, sb.ToString());
        }
        #region ISPStsadmCommand Members
        public override int Execute(string command, System.Collections.Specialized.StringDictionary keyValues, out string output)
        {
            output = String.Empty;
            var messageBuilder = new StringBuilder();
            try
            {
            string url = Params["url"].Value;
            string workflowName = Params["workflowname"].Value;
            string associationName = Params["associationname"].Value;
            string taskslistname = Params["taskslistname"].Value;
            string historylistname = Params["historylistname"].Value;

            using (SPSite site = new SPSite(url))
            {
                using (SPWeb web = site.OpenWeb())
                {
                        SPList list = Utilities.GetListFromViewUrl(web, url);
                        if (list == null)
                        {
                            output = String.Format("List - {0} can't be found", url);
                            return OUTPUT_FAILED;
                        }
                    List<int> activeWorkflowItems = WorkflowHelper.GetActiveWorkflowItemId(list);
                    WorkflowHelper.UpdateWorkflowAssociation(web, list, workflowName, associationName, taskslistname, historylistname);
                    WorkflowHelper.StartWorkflows(web, list, activeWorkflowItems, associationName);
                }
            }
            return OUTPUT_SUCCESS;
        }
            catch (Exception e)
            {
                output = String.Format("{0}\nFailed to execute command. {1}", messageBuilder, e);
            }
            return OUTPUT_FAILED;

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

    }
}
