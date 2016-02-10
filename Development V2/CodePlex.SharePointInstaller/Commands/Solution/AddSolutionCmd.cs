using System;
using System.Security;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Logging;
using CodePlex.SharePointInstaller.Resources;
using CodePlex.SharePointInstaller.Utils;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Commands.Solution
{
    /// <summary>
    /// Command for adding the SharePoint solution.
    /// </summary>
    internal class AddSolutionCmd : SolutionCmd
    {
        internal AddSolutionCmd(SolutionInfo solutionInfo) : base(solutionInfo)
        {
        }

        public override String Description
        {
            get
            {
                return CommonUIStrings.AddSolutionCmd;
            }
        }

        public override bool Execute()
        {
            if (String.IsNullOrEmpty(SolutionInfo.File))
            {
                throw new InstallException(CommonUIStrings.InstallExceptionConfigurationNoWsp);
            }

            try
            {
                DispatchMessage(Environment.NewLine + "Start adding solution '{0}' ({1}).", SolutionInfo.Title, SolutionInfo.FileName);
                Log.Info(Environment.NewLine + CommonUIStrings.LogAdd);
                SPFarm.Local.Solutions.Add(FileUtil.GetRootedPath(SolutionInfo.File));

                DispatchMessage("The '{0}' solution was successfully added to store.", SolutionInfo.Title);
                Log.Info(String.Format("The '{0}' solution was successfully added to store.", SolutionInfo.Title));
                return true;
            }
            catch (SecurityException ex)
            {
                DispatchErrorMessage("Fail to add '{0}' solution ({1}).", SolutionInfo.Title, SolutionInfo.FileName);

                var message = CommonUIStrings.AddSolutionCmdAccessError;
                if (Environment.OSVersion.Version >= new Version("6.0"))
                    message += " " + CommonUIStrings.AddSolutionCmdAccessErrorWinServer2008Solution;
                else
                    message += " " + CommonUIStrings.AddSolutionCmdAccessErrorWinServer2003Solution;
                throw new InstallException(message, ex);
            }
            catch (Exception ex)
            {
                DispatchErrorMessage("Fail to add '{0}' solution ({1}).", SolutionInfo.Title, SolutionInfo.FileName);
                throw new InstallException(ex.Message, ex);
            }
            }

        public override bool Rollback()
        {
            return new RemoveSolutionCmd(SolutionInfo).Execute(); 
        }
    }
}
