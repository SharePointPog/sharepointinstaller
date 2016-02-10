using System.Threading;
using CodePlex.SharePointInstaller.Commands.Job;
using CodePlex.SharePointInstaller.Configuration;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Commands.Solution
{
    /// <summary>
    /// The base class of all SharePoint solution related commands.
    /// </summary>
    internal abstract class SolutionCmd : DispatcherCmd
    {
        protected SolutionCmd(SolutionInfo solutionInfo)
        {
            SolutionInfo = solutionInfo;
        }

        public SolutionInfo SolutionInfo
        {
            get; 
            set;
        }

        protected void RemoveSolutionJob(SPSolution solution)
        {
            new RemoveJobCmd(JobCmd.GetSolutionJob(solution)).Execute();
            Thread.Sleep(500);
        }
    }
}