using System;
using CodePlex.SharePointInstaller.Configuration;

namespace CodePlex.SharePointInstaller.Validators
{
    public abstract class BaseValidator
    {
        private InstallConfiguration configuration;

        public String Id
        {
            get; 
            private set;
        }

        protected BaseValidator(String id)
        {
            Id = id;
        }

        public ValidationResult RunValidator()
        {
            if(CanRun)
            {
                return Validate();
            }
            return ValidationResult.Inconclusive;
        }

        protected abstract ValidationResult Validate();

        protected virtual bool CanRun
        {
            get
            {
                return true;
            }
        }

        public String QuestionString
        {
            get; 
            set;
        }

        public String SuccessString
        {
            get; 
            set;
        }

        public String ErrorString
        {
            get; 
            set;
        }

        public InstallConfiguration Configuration
        {
            get
            {
                if (configuration == null)
                    configuration = InstallConfiguration.GetConfiguration();
                return configuration;
            }
        }
    }
}
