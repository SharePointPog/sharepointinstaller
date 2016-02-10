using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using CodePlex.SharePointInstaller.Commands.Job;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Logging;
using CodePlex.SharePointInstaller.Resources;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Commands.Solution
{
    internal class DeploySolutionCmd : SolutionCmd
    {
        private readonly Collection<SPWebApplication> applications;

        private WaitForJobCompletionCmd rollbackCmd;

        public DeploySolutionCmd(IList<SPWebApplication> applications, SolutionInfo solutionInfo) : base(solutionInfo)
        {
            if (applications != null)
            {
                this.applications = new Collection<SPWebApplication>();
                foreach (var application in applications)
                {
                    this.applications.Add(application);
                }
            }
            rollbackCmd = new WaitForJobCompletionCmd(solutionInfo, String.Empty);
        }

        public override String Description
        {
            get
            {
                return CommonUIStrings.DeploySolutionCmd;
            }
        }

        public override bool Execute()
        {
            try
            {
                var solution = SPFarm.Local.Solutions[SolutionInfo.Id];
                
                if (solution.JobExists)
                {
                    if (solution.JobStatus == SPRunningJobStatus.Initialized)
                    {
                        throw new InstallException(CommonUIStrings.InstallExceptionDuplicateJob);
                    }

                    RemoveSolutionJob(solution);
                }

                DispatchMessage(Environment.NewLine + "Start deploying solution '{0}' ({1}).", SolutionInfo.Title, SolutionInfo.FileName);
                Log.Info(Environment.NewLine + CommonUIStrings.LogDeployment);
                if (solution.ContainsWebApplicationResource && applications != null && applications.Count > 0)
                {
                    solution.Deploy(JobCmd.GetImmediateJobTime(), true, applications, true);
                }
                else
                {
                    solution.Deploy(JobCmd.GetImmediateJobTime(), true, true);
                }

                DispatchMessage("Finish deploying solution '{0}' ({1}).", SolutionInfo.Title, SolutionInfo.FileName);

                return true;
            }

            catch (SPException ex)
            {
                DispatchErrorMessage("Fail to deploy solution '{0}' ({1}).", SolutionInfo.Title, SolutionInfo.FileName);
                throw new InstallException(ex.Message, ex);
            }

            catch (SqlException ex)
            {
                DispatchErrorMessage("Fail to deploy solution '{0}' ({1}).", SolutionInfo.Title, SolutionInfo.FileName);
                throw new InstallException(ex.Message, ex);
            }
        }

        public override bool Rollback()
        {
            if(!rollbackCmd.IsAlreadyExecuted)
                new RetractSolutionCmd(SolutionInfo).Execute();
            return rollbackCmd.Execute();
        }
    }
}
