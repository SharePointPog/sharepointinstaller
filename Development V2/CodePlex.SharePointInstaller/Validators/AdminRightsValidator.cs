using System;
using CodePlex.SharePointInstaller.Resources;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Validators
{
    public class AdminRightsValidator : BaseValidator
    {
        public AdminRightsValidator(String id) : base(id)
        {
        }

        protected override ValidationResult Validate()
        {
            try
            {
                if (SPFarm.Local.CurrentUserIsAdministrator())
                {
                    return ValidationResult.Success;
                }
                return ValidationResult.Error;
            }

            catch (NullReferenceException)
            {
                throw new InstallException(CommonUIStrings.InstallExceptionDatabase);
            }

            catch (Exception ex)
            {
                throw new InstallException(ex.Message, ex);
            }
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
