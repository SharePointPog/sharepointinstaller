using System;
using System.Data.SqlClient;
using CodePlex.SharePointInstaller.Commands.Job;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Logging;
using CodePlex.SharePointInstaller.Resources;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Commands.Solution
{
    internal class RetractSolutionCmd : SolutionCmd
    {
        public RetractSolutionCmd(SolutionInfo solutionInfo) : base(solutionInfo)
        {
        }

        public override String Description
        {
            get
            {
                return CommonUIStrings.RetractSolutionCmd;
            }
        }

        public override bool Execute()
        {
            
            
            try
            {
                var solution = SPFarm.Local.Solutions[SolutionInfo.Id];

                if (solution.JobExists)
                {
                    RemoveSolutionJob(solution);
                }

                if (solution.Deployed)
                {
                    DispatchMessage(Environment.NewLine + "Start retracting solution '{0}' ({1}).", SolutionInfo.Title, SolutionInfo.FileName);
                    Log.Info(Environment.NewLine + CommonUIStrings.LogRetract);

                    if (solution.ContainsWebApplicationResource)
                    {
                        var applications = solution.DeployedWebApplications;
                        foreach (var webApplication in applications)
                        {
                            DispatchMessage("Retract from '{0}' web application.", webApplication.Name);
                        }

                        solution.Retract(JobCmd.GetImmediateJobTime(), applications);
                    }
                    else
                    {
                        solution.Retract(JobCmd.GetImmediateJobTime());
                    }

                    DispatchMessage("Finish retracting solution '{0}' ({1}).", SolutionInfo.Title, SolutionInfo.FileName);
                }
                return true;
            }
            catch (Exception ex)
            {
                DispatchErrorMessage("Some errors occurs during the retracting solution '{0}' ({1}).", SolutionInfo.Title, SolutionInfo.FileName);
                throw new InstallException(ex.Message, ex);
            }
        }
    }
}
