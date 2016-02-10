using System;
using System.Security;
using CodePlex.SharePointInstaller.Resources;
using Microsoft.Win32;

namespace CodePlex.SharePointInstaller.Validators
{
    public class SPSInstalledValidator : BaseValidator
    {
        public const String SPSPath = @"SOFTWARE\Microsoft\Office Server\14.0";

        public SPSInstalledValidator(String id)
            : base(id)
        {
        }

        protected override ValidationResult Validate()
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(SPSPath);
                if (key != null)
                {
                    var version = key.GetValue("BuildVersion") as String;
                    if (version != null)
                    {
                        var buildVersion = new Version(version);
                        if (buildVersion.Major == 14)
                        {
                            return ValidationResult.Success;
                        }
                    }
                }
            }
            catch (SecurityException e)
            {
                throw new InstallException(string.Format(CommonUIStrings.InstallExceptionAccessDenied, SPSPath), e);
            }
            return ValidationResult.Error;
        }
    }
}
