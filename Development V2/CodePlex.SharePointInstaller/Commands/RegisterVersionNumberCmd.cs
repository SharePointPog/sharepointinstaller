using System;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Resources;

namespace CodePlex.SharePointInstaller.Commands
{
    /// <summary>
    /// Command that registers the version number of a solution.
    /// </summary>
    internal class RegisterVersionNumberCmd : DispatcherCmd
    {
        private SolutionInfo solution;

        public RegisterVersionNumberCmd(SolutionInfo solution)
        {
            this.solution = solution;
        }

        public override String Description
        {
            get
            {
                return CommonUIStrings.RegisterVersionNumberCommand;
            }
        }

        public override bool Execute()
        {
            var oldVersion = InstallationUtility.GetInstalledVersion(solution);
            var newVersion = new Version(solution.Version);

            InstallationUtility.SetInstalledVersion(solution, newVersion);

            DispatchMessage("Solution '{0}' has changed version from '{1}' to '{2}'.", solution.Title, oldVersion, newVersion);
            return true;
        }

        public override bool Rollback()
        {
            new UnregisterVersionNumberCmd(solution).Execute();
            return true;
        }
    }
}