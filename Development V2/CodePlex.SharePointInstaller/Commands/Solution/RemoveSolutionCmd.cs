using System;
using System.Data.SqlClient;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Logging;
using CodePlex.SharePointInstaller.Resources;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Commands.Solution
{
    /// <summary>
    /// Command for removing the SharePoint solution.
    /// </summary>
    internal class RemoveSolutionCmd : SolutionCmd
    {
        public RemoveSolutionCmd(SolutionInfo solutionInfo) : base(solutionInfo)
        {            
        }

        public override String Description
        {
            get
            {
                return CommonUIStrings.RemoveSolutionCmd;
            }
        }

        public override bool Execute()
        {
            try
            {
                DispatchMessage(Environment.NewLine + "Start removing solution '{0}' ({1}).", SolutionInfo.Title, SolutionInfo.FileName);
                Log.Info(Environment.NewLine + CommonUIStrings.LogRemove);

                var solution = SPFarm.Local.Solutions[SolutionInfo.Id];
                if (solution != null)
                {
                    if (!solution.Deployed)
                    {
                        solution.Delete();
                    }
                }

                DispatchMessage("The '{0}' solution was successfully removed from store.", SolutionInfo.Title);
                Log.Info(String.Format("The '{0}' solution was successfully removed from store.", SolutionInfo.Title));
            }

            catch (SqlException ex)
            {
                DispatchErrorMessage("Fail to remove solution '{0}' ({1}).", SolutionInfo.Title, SolutionInfo.FileName);
                throw new InstallException(ex.Message, ex);
            }
            return true;
        }
    }
}
