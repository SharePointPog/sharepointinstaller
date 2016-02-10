using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodePlex.SharePointInstaller.Commands;
using CodePlex.SharePointInstaller.Validators;
using CodePlex.SharePointInstaller.Resources;


namespace CodePlex.SharePointInstaller.Controls
{
    public partial class SystemPrerequisitesValidatorControl : Step
    {
        private bool requireMOSS;
        private bool requireSPS;
        private bool initialized;

        protected ValidationService validationService = new ValidationService();

        public SystemPrerequisitesValidatorControl()
        {
            //this empty constructor is required for the Designer
        }

        public SystemPrerequisitesValidatorControl(Installer form, Step previous) : base(form, previous)
        {
            InitializeComponent();
        }
        
        private void InitializeValidationService()
        {
            validationService.ValidatorSucceed += OnValidatorSucceed;
            validationService.ValidatorFailed += OnValidatorFailed;
            validationService.ValidationFinished += OnValidationFinished;
            validationService.ValidatorSkippped += OnValidatorSkipped;
        }

        protected virtual void OnValidationFinished()
        {
            if (validationService.Errors == 0)
            {
                messageLabel.Text = CommonUIStrings.MessageLabelTextSuccess;
            }
            else
            {
                messageLabel.Text = CommonUIStrings.MessageLabelPreCheckError;
                Form.HandleCompletion();
            }
            SetupButtons();
        }

        private void OnValidatorFailed(BaseValidator validator)
        {
            GetImageLabel(validator).Image = Properties.Resources.CheckFail;
            GetTextLabel(validator).Text = validator.ErrorString;
        }

        private void OnValidatorSucceed(BaseValidator validator)
        {          
            GetImageLabel(validator).Image = Properties.Resources.CheckOk;
            GetTextLabel(validator).Text = validator.SuccessString;
        }

        private void OnValidatorSkipped(BaseValidator validator)
        {
            GetTextLabel(validator).Text = validator.QuestionString;
        }

        private Label GetTextLabel(BaseValidator validator)
        {
            return (Label)table.Controls[String.Format("textLabel{0}", validator.Id)];
        }

        private Label GetImageLabel(BaseValidator validator)
        {
            return (Label)table.Controls[String.Format("imageLabel{0}", validator.Id)];
        }

        private void CreatePrerequisiteValidators()
        {
            table.SuspendLayout();

            CreateValidators();

            table.RowCount++;
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            table.ResumeLayout(false);
            table.PerformLayout();
        }

        public virtual void CreateValidators()
        {

#if SP2010
            var spfValidator = new SPFInstalledValidator(table.RowCount.ToString())
            {
                QuestionString = CommonUIStrings.WssCheckQuestionText2010,
                SuccessString = CommonUIStrings.WssCheckOkText2010,
                ErrorString = CommonUIStrings.WssCheckErrorText2010
            };
            AddValidator(spfValidator);

            if (requireSPS)
            {
                var spsValidator = new SPSInstalledValidator(table.RowCount.ToString())
                {
                    QuestionString = CommonUIStrings.MossCheckQuestionText2010,
                    SuccessString = CommonUIStrings.MossCheckOkText2010,
                    ErrorString = CommonUIStrings.MossCheckErrorText2010
                };
                AddValidator(spsValidator);
            }       
#else
            var wssValidator = new WSSInstalledValidator(table.RowCount.ToString())
                                   {
                                       QuestionString = CommonUIStrings.WssCheckQuestionText,
                                       SuccessString = CommonUIStrings.WssCheckOkText,
                                       ErrorString = CommonUIStrings.WssCheckErrorText
                                   };
            AddValidator(wssValidator);
            
            if (requireMOSS)
            {
                var mossInstalledValidator = new MOSSInstalledValidator(table.RowCount.ToString())
                                                 {
                                                     QuestionString = CommonUIStrings.MossCheckQuestionText,
                                                     SuccessString = CommonUIStrings.MossCheckOkText,
                                                     ErrorString = CommonUIStrings.MossCheckErrorText
                                                 };
                AddValidator(mossInstalledValidator);
            }
#endif



            var adminRightsValidator = new AdminRightsValidator(table.RowCount.ToString())
                                           {
                                               QuestionString = CommonUIStrings.AdminRightsCheckQuestionText,
                                               SuccessString = CommonUIStrings.AdminRightsCheckOkText,
                                               ErrorString = CommonUIStrings.AdminRightsCheckErrorText
                                           };
            AddValidator(adminRightsValidator);


            var administrativeServiceValidator = new AdministrativeServiceValidator(table.RowCount.ToString())
                                                     {
            #if SP2010
                QuestionString = CommonUIStrings.AdminServiceCheckQuestionText2010,
                SuccessString = CommonUIStrings.AdminServiceCheckOkText2010,
                ErrorString = CommonUIStrings.AdminServiceCheckErrorText2010 
            #else
                                                         QuestionString = CommonUIStrings.AdminServiceCheckQuestionText,
                                                         SuccessString = CommonUIStrings.AdminServiceCheckOkText,
                                                         ErrorString = CommonUIStrings.AdminServiceCheckErrorText
            #endif


                                                     };
            AddValidator(administrativeServiceValidator);

            
            var timerServiceValidator = new TimerServiceValidator(table.RowCount.ToString())
                                            {
                                                QuestionString = CommonUIStrings.TimerServiceCheckQuestionText,
                                                SuccessString = CommonUIStrings.TimerServiceCheckOkText,
                                                ErrorString = CommonUIStrings.TimerServiceCheckErrorText
                                            };
            AddValidator(timerServiceValidator);


            var solutionConfigurationValidator = new SolutionConfigurationValidator(Form.Configuration, table.RowCount.ToString())
                                                     {
                                                         QuestionString = "Is there any solution?",
                                                         SuccessString = "Solutions are detected.",
                                                         ErrorString = "Solutions are not detected."
                                                     };
            AddValidator(solutionConfigurationValidator);
        }

        protected void AddValidator(BaseValidator validator)
        {
            var row = table.RowCount;

            var img = new Label
                          {
                              Dock = DockStyle.Fill,
                              Image = Properties.Resources.CheckWait,
                              Location = new System.Drawing.Point(3, 0),
                              Name = ("imageLabel" + row),
                              Size = new System.Drawing.Size(24, 20)
                          };

            var text = new Label
                           {
                               AutoSize = true,
                               Dock = DockStyle.Fill,
                               Location = new System.Drawing.Point(33, 0),
                               Name = ("textLabel" + row)
                           };

            text.AutoSize = true;
            text.Text = validator.QuestionString;
            text.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            table.Controls.Add(img, 0, row);
            table.Controls.Add(text, 1, row);
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize, 20F));
            table.RowCount++;

            validationService.Add(validator);
        }
              
        protected virtual void CreateNewStepControls()
        {
            
        }
        
        public bool RequireMOSS
        {
            get
            {
                return requireMOSS;
            }
            set
            {
                requireMOSS = value;
            }
        }

        public bool RequireSPS
        {
            get
            {
                return requireSPS;
            }
            set
            {
                requireSPS = value;
            }
        }

        public bool RequireSearchSKU
        {
            get; 
            set;
        }

        public override void Initialize(InstallationContext context)
        {
            Title = CommonUIStrings.ControlTitleSystemCheck;
            SubTitle = InstallationUtility.ReplaceSolutionTitle(CommonUIStrings.ControlSubTitleSystemCheck, context != null ? GetConfiguration(context).InstallationName: Form.Configuration.InstallationName);
            RequireMOSS = context != null ? GetConfiguration(context).RequireMoss : false;
            RequireSPS = context != null ? context.Configuration.RequireSPS : false;
            RequireSearchSKU = false;
            TabStop = false;
            Form.SkipButton.Enabled = false;
            if (!initialized)
            {
                Form.NextButton.Enabled = false;
            }
            else
            {
                SetupButtons();
            }
            initialized = true;
        }

        public override void Execute(InstallationContext context)
        {            
            InitializeValidationService();
            CreatePrerequisiteValidators();

            validationService.Run();
        }

        public override Step CreateNextStep(InstallationContext context)
        {
            if(validationService.Errors == 0)
            {
                if (Form.Configuration.Solutions.Count == 1)
                {
                    Form.CreateInstallationContext(new List<Guid> {Form.Configuration.Solutions[0].Id});
                    return new SolutionValidatorControl(Form.Context.SolutionInfo, Form, this);
                }
                return new SolutionsSelectorControl(Form, this);
            }
            return null;
        }

        private void SetupButtons() //TODO: move buttons setup to the base class from all the controls
        {
            Form.NextButton.Enabled = validationService.Errors == 0;
            if (Form.AbortButton.Focused || Form.SkipButton.Focused)
            {
                Form.NextButton.Focus();
            }
        }
    }
}