using System;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Resources;

namespace CodePlex.SharePointInstaller.Commands
{
    /// <summary>
    /// Command that unregisters the version number of a solution.
    /// </summary>
    internal class UnregisterVersionNumberCmd : DispatcherCmd
    {
        private SolutionInfo solution;

        public UnregisterVersionNumberCmd(SolutionInfo solution)
        {
            this.solution = solution;
        }

        public override String Description
        {
            get
            {
                return CommonUIStrings.UnregisterVersionNumberCommand;
            }
        }

        public override bool Execute()
        {
            InstallationUtility.SetInstalledVersion(solution, null);
            return true;
        }
    }
}