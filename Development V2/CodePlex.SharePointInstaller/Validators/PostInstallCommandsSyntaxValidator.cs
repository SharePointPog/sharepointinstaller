using System;
using CodePlex.SharePointInstaller.CommandsApi;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Resources;

namespace CodePlex.SharePointInstaller.Validators
{
    class PostInstallCommandsSyntaxValidator : BaseValidator
    {
        private SolutionInfo solutionInfo;

        public PostInstallCommandsSyntaxValidator(SolutionInfo solutionInfo, String id) : base(id)
        {
            this.solutionInfo = solutionInfo;
        }

        protected override ValidationResult Validate()
        {
            if (solutionInfo.Id == Guid.Empty)
            {
                throw new InstallException(CommonUIStrings.InstallExceptionConfigurationInvalidId);
            }
            var commands = solutionInfo.CommandsToRun;
            if (commands.Count == 0)
            {
                SuccessString = CommonUIStrings.CommandsCheckSkipQuestionText;
                return ValidationResult.Success;
            }
            foreach (CommandToRun command in commands)
            {
                var commandName = command.Name;
                var commandInstance = Configuration.GetCommandToRun(command);
                if (commandInstance == null)
                {
                    ErrorString = String.Format(CommonUIStrings.CommandsCheckInstantiateErrorText, commandName);
                    return ValidationResult.Error;
                }
                string result = String.Empty;
                var parameters = command.ParseCommandLine();
                if ((parameters == null) || (commandInstance is IPluggableCommand && !((IPluggableCommand)commandInstance).IsSyntaxValid(parameters, false, out result)))
                {
                    ErrorString = String.Format(CommonUIStrings.CommandsCheckSyntaxErrorText, commandName, result);
                    return ValidationResult.Error;
                }
            }
            SuccessString = CommonUIStrings.CommandsCheckSuccessText;
            return ValidationResult.Success;
        }
    }
}
