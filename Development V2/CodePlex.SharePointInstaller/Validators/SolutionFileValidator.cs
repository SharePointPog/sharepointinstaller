using System;
using System.IO;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Resources;
using CodePlex.SharePointInstaller.Utils;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Validators
{
    public class SolutionFileValidator : BaseValidator
    {
        private SolutionInfo solutionInfo;

        public SolutionFileValidator(SolutionInfo solutionInfo, String id) : base(id)
        {
            this.solutionInfo = solutionInfo;
        }

        protected override ValidationResult Validate()
        {
            if (!String.IsNullOrEmpty(solutionInfo.File))
            {
                var solutionFileInfo = new FileInfo(FileUtil.GetRootedPath(solutionInfo.File));
                if (!solutionFileInfo.Exists)
                {
                    throw new InstallException(String.Format(CommonUIStrings.InstallExceptionFileNotFound, solutionInfo.File));
                }
            }
            else
            {
                if (SPFarm.Local.Solutions[solutionInfo.Id] != null) //TODO: combine related validators 
                {
                    QuestionString = CommonUIStrings.SolutionFileValidatorQuestionText;
                    solutionInfo.Action = InstallAction.Remove;
                    return ValidationResult.Inconclusive;
                }
                throw new InstallException(CommonUIStrings.InstallExceptionConfigurationNoWsp);
            }

            return ValidationResult.Success;
        }
    }
}
