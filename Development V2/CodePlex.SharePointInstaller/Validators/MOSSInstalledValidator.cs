using System;
using System.Security;
using CodePlex.SharePointInstaller.Resources;
using Microsoft.Win32;

namespace CodePlex.SharePointInstaller.Validators
{
    public class MOSSInstalledValidator : BaseValidator
    {
        public const String MOSSPath = @"SOFTWARE\Microsoft\Office Server\12.0";

        public MOSSInstalledValidator(String id) : base(id)
        {
        }

        protected override ValidationResult Validate()
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(MOSSPath);
                if (key != null)
                {
                    var version = key.GetValue("BuildVersion") as String;
                    if (version != null)
                    {
                        var buildVersion = new Version(version);
                        if (buildVersion.Major == 12)
                        {
                            return ValidationResult.Success;
                        }
                    }
                }
            }
            catch (SecurityException e)
            {
                throw new InstallException(string.Format(CommonUIStrings.InstallExceptionAccessDenied, MOSSPath), e);
            }
            return ValidationResult.Error;
        }
    }
}
