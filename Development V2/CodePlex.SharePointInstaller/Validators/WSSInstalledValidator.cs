using System;
using System.Security;
using Microsoft.Win32;

namespace CodePlex.SharePointInstaller.Validators
{
    public class WSSInstalledValidator : BaseValidator
    {
        public const String WssPath = @"SOFTWARE\Microsoft\Shared Tools\Web Server Extensions\12.0";

        public WSSInstalledValidator(String id) : base(id)
        {
        }

        protected override ValidationResult Validate()
        {        
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(WssPath);
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
                throw new InstallException(String.Format(Resources.CommonUIStrings.InstallExceptionAccessDenied, WssPath), ex);
            }
            return ValidationResult.Error;
        }
    }
}
