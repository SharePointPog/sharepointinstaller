using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Logging;
using CodePlex.SharePointInstaller.Resources;
using CodePlex.SharePointInstaller.Wrappers;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.StsAdmin;

namespace CodePlex.SharePointInstaller.Commands
{
    public class ExecuteCustomCommands : DispatcherCmd
    {
        private IContext context;
        private InstallConfiguration configuration;
        private SolutionInfo solution;

        public ExecuteCustomCommands(IContext installationContext)
        {
            context = installationContext;
            configuration = context.Configuration;
            this.solution = context.SolutionInfo;
        }

        public override string Description
        {
            get { return CommonUIStrings.ExecuteCustomCommandDescription; }
        }

        public override bool Execute()
        {
            DispatchMessage(Environment.NewLine + "Start executing custom post-install commands.");
            Log.Info(Environment.NewLine + "Executing custom post-install commands...");

            bool success = true;
            var commandsToRun = solution.CommandsToRun;
            foreach (CommandToRun commandToRun in commandsToRun)
            {
                success &= RunCommand(commandToRun.Name, configuration.GetCommandToRun(commandToRun), commandToRun.Parameters);
            }

            DispatchMessage("Finish executing custom post-install commands." + Environment.NewLine);
            return true; //discarding actual result otherwise it won't progress to the next command
        }

        private bool RunCommand(String commandName, ISPStsadmCommand command, String commandLine)
        {
            if (command == null)
            {
                return true;
            }            
            
            return ExecuteCommands(command, ReplaceTokens(commandLine), commandName);
        }

        public List<String> ReplaceTokens(String commandLine)
        {
            //create a list of commands for each application/site/web with the placeholders replaced
            var commandLines = new List<String>();
            if ((!commandLine.Contains(InstallConfiguration.Placeholders.AppUrl)) &&
                (!commandLine.Contains(InstallConfiguration.Placeholders.SiteUrl) &&
                (!commandLine.Contains(InstallConfiguration.Placeholders.WebUrl))))
            {
                commandLines.Add(commandLine);
                return commandLines;
            }
            if (context.Action == InstallAction.Upgrade && !solution.ReactivateFeatures)
            {
                Log.Error(
                    String.Format(
                        "Unable to launch the post-install command {0} because it contains tokens which could not be resolved during upgrade action without features reactivation",
                        commandLine));
                return commandLines;
            }
            var apps = context.WebApps;
            var k = 0;
            do //using post-condition loops to execute at least once for the case when there're no placeholders
            {
                if (k < apps.Count)
                    commandLine = Regex.Replace(commandLine, InstallConfiguration.Placeholders.AppUrl, apps[k].GetResponseUri(SPUrlZone.Default).ToString());
                var siteCollections = context.SiteCollections;
                var j = 0;
                do
                {
                    var tempSiteCollectionCommand = commandLine;
                    IEntityInfo currentSiteCollection = null;
                    if (j < siteCollections.Count)
                    {
                        currentSiteCollection = siteCollections[j];
                        tempSiteCollectionCommand = Regex.Replace(tempSiteCollectionCommand, InstallConfiguration.Placeholders.SiteUrl,
                                                                  currentSiteCollection.Url);
                    }
                    var sites = context.Sites;
                    var i = 0;
                    do
                    {
                        var tempSiteCommand = tempSiteCollectionCommand;
                        if (i < sites.Count)
                            tempSiteCommand = Regex.Replace(tempSiteCommand, InstallConfiguration.Placeholders.WebUrl, sites[i].Url);
                        else if (sites.Count == 0 && currentSiteCollection != null)
                        {
                            //resolving WebURL for a site collection's root web
                            tempSiteCommand = Regex.Replace(tempSiteCommand,
                                                            InstallConfiguration.Placeholders.WebUrl,
                                                            currentSiteCollection.Url);
                        }

                        if (commandLines.IndexOf(tempSiteCommand) == -1)
                        {
                            commandLines.Add(tempSiteCommand);
                        }
                        i++;
                    } while (i < sites.Count);
                    j++;
                } while (j < siteCollections.Count);
                k++;
            } while (k < apps.Count);
            return commandLines;
        }

        private bool ExecuteCommands(ISPStsadmCommand command, IEnumerable<String> commandLines, String commandName)
        {
            var success = true;
            foreach (var commandline in commandLines)
            {
                try
                {
                    DispatchMessage("Run post-install command {0} {1}.", commandName, commandline);

                    string output;
                    int returnCode = command.Run(commandName, CommandToRun.ParseCommandLine(commandName, commandline), out output);
                    if (returnCode != 0)
                    {
                        DispatchErrorMessage("Fail post-install command '{2}' (parameters: {3}) returned an error (code {0}): {1}", returnCode, output, commandName, commandline);
                        Log.Error(String.Format("Post-install command {2} (parameters: {3}) returned an error (code {0}): {1}", returnCode, output, commandName, commandline));
                        success = false;
                    }
                    else
                    {
                        DispatchMessage("Succesfully executed post-install command {0} {1}.", commandName, commandline);
                        Log.Info(String.Format("Succesfully executed post-install command {0} {1}", commandName,commandline));
                    }
                }
                catch (Exception e)
                {
                    DispatchErrorMessage("Fail post-install command '{0}'.", commandName);
                    Log.Error(String.Format("Post-install command {1} (parameters: {2}) returned an error: {0}", e.Message,commandName, commandline), e);
                    success = false;
                }
            }
            return success;
        }

        public override bool Rollback()
        {
            //no rollback for custom commands
            return true;
        }

    }
}
