using System;
using CodePlex.SharePointInstaller.Configuration;

namespace CodePlex.SharePointInstaller.Validators
{
    public class SolutionConfigurationValidator : BaseValidator
    {
        private InstallConfiguration configuration;

        public SolutionConfigurationValidator(InstallConfiguration configuration, String id) : base(id)
        {
            this.configuration = configuration;
        }

        protected override ValidationResult Validate()
        {
            if (configuration == null || configuration.Solutions.Count == 0)
                return ValidationResult.Error;
            return ValidationResult.Success;
        }
    }
}
