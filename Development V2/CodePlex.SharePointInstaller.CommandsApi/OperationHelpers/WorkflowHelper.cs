using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;

namespace Stsadm.Commands.OperationHelpers
{
    public static  class WorkflowHelper
    {
        public static List<int> GetActiveWorkflowItemId(SPList list)
        {
            List<int> activeWordflowItems = new List<int>();
            foreach (SPListItem item in list.Items)
            {
                bool isRunning = false;
                foreach (SPWorkflow workflow in item.Workflows)
                {
                    if (workflow.InternalState == SPWorkflowState.Running)
                    {
                        isRunning = true;
                    }
                }
                if (isRunning)
                {
                    activeWordflowItems.Add(item.ID);
                }
            }
            return activeWordflowItems;
        }

        public static void StartWorkflows(SPWeb web, SPList list, List<int> items, string associationName)
        {
            SPWorkflowAssociation workflowAssociation;
            foreach (SPWorkflowAssociation association in list.WorkflowAssociations)
                if (association.Name == associationName)
                {
                    workflowAssociation = association;
                    foreach (int itemId in items)
                    {
                        try
                        {
                        SPListItem item = list.GetItemById(itemId);
                        SPWorkflowManager manager = web.Site.WorkflowManager;
                        manager.StartWorkflow(item, workflowAssociation, workflowAssociation.AssociationData, true);
                    }
                        catch (Exception ex)
                        {
                            Exception oops = new Exception("Cann't start workflow", ex);
                            throw oops;
                }
                    }
                }
            web.Update();
        }

        public static void UpdateWorkflowAssociation(SPWeb web, SPList list, string workflowName, string associationName, string tasksListName, string historyListName)
        {
            SPList taskList;
            SPList historyList;

            List<SPWorkflowAssociation> associationsList = new List<SPWorkflowAssociation>();
            foreach (SPWorkflowAssociation association in list.WorkflowAssociations)
                associationsList.Add(association);
            foreach(SPWorkflowAssociation association in associationsList)
                list.RemoveWorkflowAssociation(association);

            foreach (SPWorkflowTemplate template in web.WorkflowTemplates)
                if (template.Name == workflowName)
                {
                    try
                    {
                    historyList = web.Lists[historyListName];
                    taskList = web.Lists[tasksListName];
                    }
                    catch(Exception ex)
                    {
                        Exception ex1 = new Exception("Specific lists can not be found", ex);
                        throw ex1;
                    }
                    SPWorkflowAssociation asso = SPWorkflowAssociation.CreateListAssociation(template,
                                                                                             associationName,
                                                                                             taskList, historyList);
                    asso.AutoStartCreate = true;
                    asso.PermissionsManual = SPBasePermissions.ManageLists;
                    list.AddWorkflowAssociation(asso);
                    list.Update();
                }
            web.Update();
        }
    }
}


