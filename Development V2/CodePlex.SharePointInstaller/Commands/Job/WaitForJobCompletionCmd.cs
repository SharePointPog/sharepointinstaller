using System;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Logging;
using CodePlex.SharePointInstaller.Resources;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Commands.Job
{
    internal class WaitForJobCompletionCmd : DispatcherCmd
    {
        private readonly TimeSpan JobTimeout = TimeSpan.FromMinutes(15);

        private readonly String description;

        private DateTime startTime;

        private bool isAlreadyExecuted;

        private SolutionInfo solutionInfo;

        internal WaitForJobCompletionCmd(SolutionInfo solutionInfo, String description)
        {
            this.solutionInfo = solutionInfo;
            this.description = description;
        }

        public override String Description
        {
            get
            {
                return description;
            }
        }

        public bool IsAlreadyExecuted
        {
            get { return isAlreadyExecuted; }
        }

        public override bool Execute()
        {
            try
            {
                var solution = SPFarm.Local.Solutions[solutionInfo.Id];

                if (!IsAlreadyExecuted)
                {
                    if (!solution.JobExists) 
                        return true;

                    startTime = DateTime.Now;
                    isAlreadyExecuted = true;
                    DispatchMessage("Waiting...");
                }             
                if (solution.JobExists)
                {
                    if (DateTime.Now > startTime.Add(JobTimeout))
                    {
                        DispatchMessage(CommonUIStrings.InstallExceptionTimeout);
                        throw new InstallException(CommonUIStrings.InstallExceptionTimeout);
                    }

                    return false;
                }
                Log.Info(solution.LastOperationDetails);

                var result = solution.LastOperationResult;
                if (result != SPSolutionOperationResult.DeploymentSucceeded &&
                    result != SPSolutionOperationResult.DeploymentWarningsOccurred &&
                    result != SPSolutionOperationResult.RetractionSucceeded &&
                    result != SPSolutionOperationResult.RetractionWarningsOccurred)
                {
                    throw new InstallException(solution.LastOperationDetails);
                }

                return true;
            }

            catch (Exception ex)
            {
                throw new InstallException(ex.Message, ex);
            }
        }

        public override bool Rollback()
        {
            var installedSolution = SPFarm.Local.Solutions[solutionInfo.Id];           
            if (installedSolution != null)
            {
                if (installedSolution.JobExists)
                {
                    if (DateTime.Now > startTime.Add(JobTimeout))
                    {
                        throw new InstallException(CommonUIStrings.InstallExceptionTimeout);
                    }
                    return false;
                }
                Log.Info("Waiting for job completion...");
            }

            return true;
        }
    }
}
