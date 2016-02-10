using System;
using System.Data.SqlClient;
using CodePlex.SharePointInstaller.Commands.Job;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Logging;
using CodePlex.SharePointInstaller.Resources;
using CodePlex.SharePointInstaller.Utils;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Commands.Solution
{
    internal class UpgradeSolutionCmd : SolutionCmd
    {
        public UpgradeSolutionCmd(SolutionInfo solutionInfo) : base(solutionInfo)
        {
        }

        public override String Description
        {
            get
            {
                return CommonUIStrings.UpgradeSolutionCmd;
            }
        }

        public override bool Execute()
        {
            try
            {
                if (String.IsNullOrEmpty(SolutionInfo.File))
                {
                    throw new InstallException(CommonUIStrings.InstallExceptionConfigurationNoWsp);
                }

                var solution = SPFarm.Local.Solutions[SolutionInfo.Id];

                //
                // Remove existing job, if any. 
                //
                if (solution.JobExists)
                {
                    RemoveSolutionJob(solution);
                }

                DispatchMessage(Environment.NewLine + "Start upgrading solution '{0}' ({1}).", SolutionInfo.Title, SolutionInfo.FileName);
                Log.Info(Environment.NewLine + CommonUIStrings.LogUpgrade);

                solution.Upgrade(FileUtil.GetRootedPath(SolutionInfo.File), JobCmd.GetImmediateJobTime());

                DispatchMessage("Finish upgrading solution '{0}' ({1}).", SolutionInfo.Title, SolutionInfo.FileName);
                return true;
            }
            catch (SqlException ex)
            {
                DispatchErrorMessage("Fail to upgrade solution '{0}' ({1}).", SolutionInfo.Title, SolutionInfo.FileName);
                throw new InstallException(ex.Message, ex);
            }
        }
    }
}
