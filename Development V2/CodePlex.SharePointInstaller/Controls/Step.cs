using System;
using CodePlex.SharePointInstaller.Commands;
using CodePlex.SharePointInstaller.Configuration;

namespace CodePlex.SharePointInstaller.Controls
{
    public class Step : InstallerControl
    {
        protected Step previous;

        protected Step next;

        public Step()
        {
            //this empty constructor is required for the Designer
        }

        protected Step(Installer form, Step previous)
            : base(form)
        {
            this.previous = previous;
        }

        public virtual Step GoNext()
        {
            if (next == null)
            {
                next = CreateNextStep(Form.Context);
                next.Initialize(Form.Context);
                next.Execute(Form.Context);
            }
            else
            {
                next.Initialize(Form.Context);
            }
            return next;
        }

        public virtual Step GoPrevious()
        {
            if (previous != null)
                previous.Initialize(Form.Context);
            return previous;
        }

        //those below shall be abstract but it breaks Design View as it dosn't support abstract base class for a control
        public virtual Step SkipAndGoToOtherStep(InstallationContext context)
        {
            throw new NotImplementedException();
        }

        public virtual void Initialize(InstallationContext context)
        {
            throw new NotImplementedException();
        }

        public virtual void Execute(InstallationContext context)
        {
            throw new NotImplementedException();
        }

        public virtual Step CreateNextStep(InstallationContext context)
        {
            throw new NotImplementedException();
        }

        protected InstallConfiguration GetConfiguration(InstallationContext context)
        {
            return context != null ? context.Configuration : Form.Configuration;
        }
    }
}
