using System;

namespace CodePlex.SharePointInstaller
{
    public class InstallException : ApplicationException
    {
        public InstallException(String message) : base(message)
        {
        }

        public InstallException(String message, Exception inner) : base(message, inner)
        {
        }
    }
}
