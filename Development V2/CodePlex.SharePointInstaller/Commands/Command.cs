using System;

namespace CodePlex.SharePointInstaller.Commands
{
    /// <summary>
    /// The base class of all installation commands.
    /// </summary>
    internal abstract class Command
    {
        private readonly InstallProcessControl parent;

        protected Command(InstallProcessControl parent)
        {
            this.parent = parent;
        }

        protected InstallProcessControl Parent
        {
            get
            {
                return parent;
            }
        }

        protected InstallProcessControl.MessageCollector Log
        {
            get
            {
                return InstallProcessControl.Log;
            }
        }
        
        public abstract String Description
        {
            get;
        }

        protected internal virtual bool Execute()
        {
            return true;
        }

        protected internal virtual bool Rollback()
        {
            return true;
        }
    }
}
