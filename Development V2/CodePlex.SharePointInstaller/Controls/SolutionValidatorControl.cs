using System;
using CodePlex.SharePointInstaller.Commands;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Resources;
using CodePlex.SharePointInstaller.Validators;

namespace CodePlex.SharePointInstaller.Controls
{
    public class SolutionValidatorControl : SystemPrerequisitesValidatorControl
    {
        private SolutionInfo solutionInfo;
        private bool initialized;

        public SolutionValidatorControl(SolutionInfo solutionInfo, Installer form, Step previous) : base(form, previous)
        {
            this.solutionInfo = solutionInfo;
        }

        public override void CreateValidators()
        {
            var solutionValidatorControl = new SolutionValidator(solutionInfo, table.RowCount.ToString())
                                               {
                                                   QuestionString = InstallationUtility.FormatString(solutionInfo, CommonUIStrings.SolutionCheckQuestionText),
                                                   SuccessString = InstallationUtility.FormatString(solutionInfo, CommonUIStrings.SolutionFileCheckOkText),
                                                   ErrorString = InstallationUtility.FormatString(solutionInfo, CommonUIStrings.SolutionCheckErrorText)
                                               };
            AddValidator(solutionValidatorControl);

            var solutionFileValidator = new SolutionFileValidator(solutionInfo, table.RowCount.ToString())
            {
                QuestionString = CommonUIStrings.SolutionFileCheckQuestionText,
                SuccessString = CommonUIStrings.SolutionFileCheckOkText
            };
            AddValidator(solutionFileValidator);

            var postInstallCommandsSyntaxValidator = new PostInstallCommandsSyntaxValidator(solutionInfo, table.RowCount.ToString())
                                                         {
                                                             QuestionString = CommonUIStrings.CommandsCheckQuestionText
                                                         };
            AddValidator(postInstallCommandsSyntaxValidator);
        }
       
        public override void Initialize(InstallationContext context)
        {
            Title = "Validating Solution";
            SubTitle = InstallationUtility.FormatString(solutionInfo, CommonUIStrings.ControlSubTitleSystemCheck);
            //Form.PrevButton.Enabled = false;
            if (!initialized)
            {
                Form.NextButton.Enabled = false;
            }
            else
            {
                Form.NextButton.Enabled = validationService.Errors == 0;
            }
            Form.SkipButton.Enabled = true;
            TabStop = false;
            if (Form.AbortButton.Focused || Form.SkipButton.Focused)
            {
                Form.NextButton.Focus();
            }
            initialized = true;
        }

        public override Step GoNext()
        {
            if (next is SolutionValidatorControl)
            {
                next = null;
            }
            return base.GoNext();
        }

        public override Step CreateNextStep(InstallationContext context)
        {
            var validator = (SolutionValidator) validationService.Validators[typeof (SolutionValidator)];

            if (validator.Solution == null)
            {
                //we only want the EULA when they are installing
                if (!String.IsNullOrEmpty(GetConfiguration(context).EULA))
                {
                    return new EULAControl(Form, this);
                }

                Form.Context.Action = InstallAction.Install;
                return new DeploymentTargetsControl(Form, this);
            }

            if (solutionInfo.Action != InstallAction.Install) //certain action was explicitly specified in the config
            {
                return new UpgradeControl(Form, this, solutionInfo.Action == InstallAction.Upgrade, solutionInfo.Action == InstallAction.Repair,
                                          solutionInfo.Action == InstallAction.Remove);
            }


            var installedVersion = InstallationUtility.GetInstalledVersion(solutionInfo);
            var newVersion = new Version(solutionInfo.Version);
            if ((installedVersion != null) && (newVersion != installedVersion) && GetConfiguration(context).AllowUpgrade)
            {
                return new UpgradeControl(Form, this, true, false, true);
            }
            return new UpgradeControl(Form, this);
        }

        public override Step SkipAndGoToOtherStep(InstallationContext context)
        {
            context.BeginInstallation();
            context.EndInstallation();
            if (context.SolutionInfo == null)
                next = new CompletionControl(Form, this);
            else
                next = new SolutionValidatorControl(context.SolutionInfo, Form, this);
            next.Initialize(context);
            next.Execute(context);
            return next;
        }
    }
}
