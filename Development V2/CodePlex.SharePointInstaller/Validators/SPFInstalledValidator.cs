using System;
using System.Security;
using Microsoft.Win32;

namespace CodePlex.SharePointInstaller.Validators
{
    public class SPFInstalledValidator : BaseValidator
    {
        public const String SpfPath = @"SOFTWARE\Microsoft\Shared Tools\Web Server Extensions\14.0";

        public SPFInstalledValidator(String id) : base(id)
        {
        }

        protected override ValidationResult Validate()
        {        
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(SpfPath);
                if (key != null)
                {
                    var value = key.GetValue("SharePoint");
                    if (value != null && value.Equals("Installed"))
                    {
                        return ValidationResult.Success;
                    }
                }
            }
            catch (SecurityException ex)
            {
                throw new InstallException(String.Format(Resources.CommonUIStrings.InstallExceptionAccessDenied, SpfPath), ex);
            }
            return ValidationResult.Error;
        }
    }
}
