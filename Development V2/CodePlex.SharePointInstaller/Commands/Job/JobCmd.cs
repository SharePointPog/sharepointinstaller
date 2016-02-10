using System;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Commands.Job
{
    internal abstract class JobCmd : DispatcherCmd
    {
        protected SPJobDefinition jobDefinition;

        protected JobCmd(SPJobDefinition jobDefinition)
        {
            this.jobDefinition = jobDefinition;
        }        

        public static SPJobDefinition GetSolutionJob(SPSolution solution)
        {
            var service = SPFarm.Local.TimerService;
            foreach (var definition in service.JobDefinitions)
            {
                if (definition.Title != null && definition.Title.Contains(solution.Name))
                {
                    return definition;
                }
            }
            return null;
        }

        public static DateTime GetImmediateJobTime()
        {
            return DateTime.Now - TimeSpan.FromDays(1); //it should have been done yesterday! :)
        }
    }
}
