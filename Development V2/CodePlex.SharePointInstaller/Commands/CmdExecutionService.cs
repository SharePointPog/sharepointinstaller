using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using CodePlex.SharePointInstaller.Commands.Feature;
using CodePlex.SharePointInstaller.Commands.Job;
using CodePlex.SharePointInstaller.Commands.Solution;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Logging;
using CodePlex.SharePointInstaller.Resources;
using CodePlex.SharePointInstaller.Utils;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Commands
{
    internal class CmdExecutionService : IDisposable
    {
        private IList<Cmd> executeCommands = new List<Cmd>();

        private IList<Cmd> rollbackCommands = new List<Cmd>();

        public virtual void Install(InstallationContext context)
        {
            executeCommands.Add(new AddSolutionCmd(context.SolutionInfo));
            executeCommands.Add(new DeploySolutionCmd(context.WebApps, context.SolutionInfo));
            executeCommands.Add(new WaitForJobCompletionCmd(context.SolutionInfo, CommonUIStrings.WaitForSolutionDeployment));
            executeCommands.Add(new ActivateFeaturesCmd(context));
            executeCommands.Add(new RegisterVersionNumberCmd(context.SolutionInfo));
            executeCommands.Add(new ExecuteCustomCommands(context));

            for (var i = executeCommands.Count - 1; i >= 0; i--)
            {
                rollbackCommands.Add(executeCommands[i]);
            }
        }

        public virtual void Upgrade(InstallationContext context)
        {
            if (IsSolutionRenamed(context.SolutionInfo))
            {
                Log.Info(
                    String.Format(
                        "Unable to upgrade because the solution {0} has been renamed. No action has been performed.",
                        context.SolutionInfo.Id));
                return;
            }
            bool reactivateFeatures = context.SolutionInfo.ReactivateFeatures;
            if (reactivateFeatures)
                executeCommands.Add(new DeactivateFeaturesCmd(context));
            executeCommands.Add(new UpgradeSolutionCmd(context.SolutionInfo));
            executeCommands.Add(new WaitForJobCompletionCmd(context.SolutionInfo, CommonUIStrings.WaitForSolutionUpgrade));
            if (reactivateFeatures)
                executeCommands.Add(new ActivateFeaturesCmd(context));
            executeCommands.Add(new ExecuteCustomCommands(context));
        }

        public virtual void Uninstall(InstallationContext context)
        {
            executeCommands.Add(new DeactivateFeaturesCmd(context));
            executeCommands.Add(new RetractSolutionCmd(context.SolutionInfo));
            executeCommands.Add(new WaitForJobCompletionCmd(context.SolutionInfo, CommonUIStrings.WaitForSolutionRetraction));
            executeCommands.Add(new RemoveSolutionCmd(context.SolutionInfo));
            executeCommands.Add(new UnregisterVersionNumberCmd(context.SolutionInfo));
            executeCommands.Add(new ExecuteCustomCommands(context));
        }

        public virtual void Repair(InstallationContext context)
        {
            executeCommands.Add(new DeactivateFeaturesCmd(context));
            executeCommands.Add(new RetractSolutionCmd(context.SolutionInfo));
            executeCommands.Add(new WaitForJobCompletionCmd(context.SolutionInfo, CommonUIStrings.WaitForSolutionRetraction));
            executeCommands.Add(new RemoveSolutionCmd(context.SolutionInfo));
            executeCommands.Add(new AddSolutionCmd(context.SolutionInfo));
            executeCommands.Add(new DeploySolutionCmd(GetDeployedApplications(context.SolutionInfo), context.SolutionInfo));
            executeCommands.Add(new WaitForJobCompletionCmd(context.SolutionInfo, CommonUIStrings.WaitForSolutionDeployment));
            executeCommands.Add(new ActivateFeaturesCmd(context));
            executeCommands.Add(new ExecuteCustomCommands(context));
        }

        public void Dispose()
        {
            foreach (var cmd in ExecuteCommands)
            {
                var dispatcherCmd = cmd as IMessageDispatcher;
                if (dispatcherCmd != null)
                {
                    dispatcherCmd.DetachAllListeners();
        }
            }
        }

        public void SubscribeMessageListener(IMessageDispatcherListener listener)
        {
            foreach (var cmd in ExecuteCommands)
            {
                var dispatcherCmd = cmd as IMessageDispatcher;
                if (dispatcherCmd != null)
                {
                    dispatcherCmd.AttachListener(listener);
                }
            }
        }

        private bool IsSolutionRenamed(SolutionInfo solutionInfo)
        {
            var solution = SPFarm.Local.Solutions[solutionInfo.Id];
            if (solution == null) 
                return false;

            var solutionFileInfo = new FileInfo(FileUtil.GetRootedPath(solutionInfo.File));
            return !solution.Name.Equals(solutionFileInfo.Name, StringComparison.OrdinalIgnoreCase);
        }

        private Collection<SPWebApplication> GetDeployedApplications(SolutionInfo solutionInfo)
        {
            var solution = SPFarm.Local.Solutions[solutionInfo.Id];
            if (solution.ContainsWebApplicationResource)
            {
                return solution.DeployedWebApplications;
            }
            return null;
        }

        public IList<Cmd> ExecuteCommands
        {
            get
            {
                return executeCommands;
            }
            set
            {
                executeCommands = value;
            }
        }

        public IList<Cmd> RollbackCommands
        {
            get
            {
                return rollbackCommands;
            }
            set
            {
                rollbackCommands = value;
            }
        }
    }
}
