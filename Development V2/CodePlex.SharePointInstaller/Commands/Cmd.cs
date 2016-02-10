using System;

namespace CodePlex.SharePointInstaller.Commands
{
    /// <summary>
    /// The base class of all installation commands.
    /// </summary>
    public abstract class Cmd
    {           
        public abstract String Description
        {
            get;
        }

        /// <summary>
        /// The main method to execute the installation command
        /// </summary>
        /// <returns>True if executed successfully; False if shall be retried again and again</returns>
        public virtual bool Execute()
        {
            return true;
        }

        public virtual bool Rollback()
        {
            return true;
        }
    }
}
