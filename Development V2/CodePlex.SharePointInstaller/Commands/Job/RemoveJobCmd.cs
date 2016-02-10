using System;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Commands.Job
{
    internal class RemoveJobCmd : JobCmd
    {
        public RemoveJobCmd(SPJobDefinition jobDefinition) : base(jobDefinition)
        {
        }

        public override String Description
        {
            get
            {
                return "Removing a job.";
            }
        }

        public override bool Execute()
        {
            if (jobDefinition != null)
            {
                var jobName = jobDefinition.DisplayName;
                jobDefinition.Delete();

                DispatchMessage("Job '{0}' was successfully removed.", jobName);
            }
            return true;
        }
    }
}
