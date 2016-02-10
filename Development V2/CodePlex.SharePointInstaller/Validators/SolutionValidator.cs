using System;
using CodePlex.SharePointInstaller.Commands;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Resources;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Validators
{
    public class SolutionValidator : BaseValidator
    {
        private SolutionInfo solutionInfo;

        public SPSolution Solution
        {
            get; 
            private set;
        }

        public SolutionValidator(SolutionInfo solutionInfo, String id) : base(id)
        {
            this.solutionInfo = solutionInfo;
        }

        protected override ValidationResult Validate()
        {
            if (solutionInfo.Id == Guid.Empty)
            {
                throw new InstallException(CommonUIStrings.InstallExceptionConfigurationInvalidId);
            }
            try
            {
                Solution = SPFarm.Local.Solutions[solutionInfo.Id];
                if (Solution != null)
                {
                    SuccessString = InstallationUtility.FormatString(solutionInfo, CommonUIStrings.SolutionCheckOkTextInstalled);
                }
                else
                {
                    SuccessString = InstallationUtility.FormatString(solutionInfo, CommonUIStrings.SolutionCheckOkTextNotInstalled);
                }
            }
            catch (NullReferenceException)
            {
                throw new InstallException(CommonUIStrings.InstallExceptionDatabase);
            }
            catch (Exception ex)
            {
                throw new InstallException(ex.Message, ex);
            }

            return ValidationResult.Success;
        }

        protected override bool CanRun
        {
            get
            {
                #if SP2010
                                return new SPFInstalledValidator(String.Empty).RunValidator() == ValidationResult.Success;
                #else
                                return new WSSInstalledValidator(String.Empty).RunValidator() == ValidationResult.Success;
                #endif
            }
        }
    }
}
