using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodePlex.SharePointInstaller.Commands;
using CodePlex.SharePointInstaller.Configuration;


namespace CodePlex.SharePointInstaller.Controls
{
    public partial class SolutionsSelectorControl : Step
    {       
        private IList<Guid> selectedSolutions = new List<Guid>();

        public SolutionsSelectorControl()
        {
            //this empty constructor is required for the Designer
        }

        public SolutionsSelectorControl(Installer form, Step previous): base(form, previous)
        {
            InitializeComponent();
        }
        
        public override void Initialize(InstallationContext context)
        {
            Title = "Solutions";
            SubTitle = "Select solutions to install.";
        }

        public override void Execute(InstallationContext context)
        {
            foreach (SolutionInfo solutionInfo in Form.Configuration.Solutions)
                checkedList.Items.Add(solutionInfo, true);
        }

        public override Step CreateNextStep(InstallationContext context)
        {
            Form.CreateInstallationContext(selectedSolutions);
            return new SolutionValidatorControl(Form.Context.SolutionInfo, Form, this);
        }
        
        private void OnCheckedListClicked(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Unchecked)
                selectedSolutions.Remove(((SolutionInfo) checkedList.Items[e.Index]).Id);
            else if(e.NewValue == CheckState.Checked)
                selectedSolutions.Add(((SolutionInfo)checkedList.Items[e.Index]).Id);
            Form.NextButton.Enabled = selectedSolutions.Count > 0;
        }
    }
}