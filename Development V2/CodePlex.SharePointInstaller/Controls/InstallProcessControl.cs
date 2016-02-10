using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CodePlex.SharePointInstaller.Commands;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Controls;
using CodePlex.SharePointInstaller.Resources;
using CodePlex.SharePointInstaller.Logging;

namespace CodePlex.SharePointInstaller.Controls
{
    public partial class InstallProcessControl : Step, IMessageDispatcherListener
    {
        private Timer timer = new Timer();

        private int nextCommand;

        private bool requestCancel;

        private int errors;

        private int rollbackErrors;

        private bool isCompleted;

        private InstallationContext context;

        private BackgroundWorker worker = new BackgroundWorker(); 

        private CmdExecutionService executionService = new CmdExecutionService();
        
        public InstallProcessControl(Installer form, Step previous) : base(form, previous)
        {
            InitializeComponent();            
            Application.ApplicationExit += OnParentFormClosed;            
        }

        private void StartExecution(Cmd cmd)
        {
            worker.DoWork += Execute;
            worker.RunWorkerCompleted += ExecutionCompleted;
            worker.RunWorkerAsync(cmd);            
        }

        private void StartRollback(Cmd cmd)
        {
            worker.DoWork += Rollback;
            worker.RunWorkerCompleted += RollbackCompleted;
            worker.RunWorkerAsync(cmd);
        }
        
        protected void RollbackCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            worker.DoWork -= Rollback;
            worker.RunWorkerCompleted -= RollbackCompleted;

            var result = e.Result as Result;
            if (result != null)
            {
                if(result.Success)
                {
                    nextCommand++;
                    progressBar.PerformStep();
                }
                timer.Start();
            }
        }

        protected void ExecutionCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            worker.DoWork -= Execute;
            worker.RunWorkerCompleted -= ExecutionCompleted;

            var result = e.Result as Result;
            if (result != null)
            {
                if (result.Success)
                {
                    nextCommand++;
                    progressBar.PerformStep();

                    if (nextCommand < executionService.ExecuteCommands.Count)
                    {
                        descriptionLabel.Text = executionService.ExecuteCommands[nextCommand].Description;
                    }
                }
                if(!result.ExceptionThrown)
                    timer.Start();
                else
                    InitiateRollback();
            }            
            else
                InitiateRollback();
        }

        protected void Rollback(object sender, DoWorkEventArgs e)
        {
            var cmd = e.Argument as Cmd;
            if(cmd != null)
            {
                try
                {
                    e.Result = new Result(cmd.Rollback(), false);
                }
                catch(Exception ex)
                {
                    e.Result = new Result(false, true);                
                    Log.Error(CommonUIStrings.LogError);
                    Log.Error(ex.Message, ex);

                    rollbackErrors++;
                    nextCommand++;
                    Invoke((MethodInvoker)(()=>progressBar.PerformStep()));
                }
            }
        }

        protected void Execute(object sender, DoWorkEventArgs e)
        {
            var cmd = e.Argument as Cmd;
            if (cmd != null)
            {                
                try
                {
                    e.Result = new Result(cmd.Execute(), false);
                }
                catch (Exception ex)
                {
                    e.Result = new Result(false, true);
                    Log.Error(CommonUIStrings.LogError);
                    Log.Error(ex.Message, ex);

                    errors++;
                    Invoke((MethodInvoker)(()=>
                       {
                           errorPictureBox.Visible = true;
                           errorDetailsTextBox.Visible = true;
                           errorDetailsTextBox.Text = ex.Message + Environment.NewLine + "See the log for more details";
                           descriptionLabel.Text = CommonUIStrings.DescriptionLabelTextErrorsDetected;
                       }));                    
                }
            }
        }       
           
        protected void OnParentFormClosed(object sender, EventArgs e)
        {
            if (executionService != null)
                executionService.Dispose();
        }

        private void OnBeginProcess(Object myObject, EventArgs myEventArgs)
        {
            timer.Stop();

            if (requestCancel)
            {
                descriptionLabel.Text = CommonUIStrings.DescriptionLabelTextOperationCanceled;
                InitiateRollback();
            }
            else if (nextCommand < executionService.ExecuteCommands.Count)
                StartExecution(executionService.ExecuteCommands[nextCommand]);
            else
            {
                descriptionLabel.Text = CommonUIStrings.DescriptionLabelTextSuccess;
                HandleCompletion();
            }
        }        

        private void OnRollbackProcess(Object myObject, EventArgs myEventArgs)
        {
            timer.Stop();

            if (nextCommand < executionService.RollbackCommands.Count)
            {
                StartRollback(executionService.RollbackCommands[nextCommand]);
            }
            else
            {
                if (rollbackErrors == 0)
                    descriptionLabel.Text = CommonUIStrings.DescriptionLabelTextRollbackSuccess;
                else
                    descriptionLabel.Text = String.Format(CommonUIStrings.DescriptionLabelTextRollbackError, rollbackErrors);

                HandleCompletion();
            }
        }
      
        protected internal override void CancelInstallation()
        {
            if (isCompleted)
                base.CancelInstallation();
            else
            {
                requestCancel = true;
                Form.AbortButton.Enabled = false;
            }
        }
          
        private void HandleCompletion()
        {
            isCompleted = true;
            Form.NextButton.Enabled = true;
            Form.AbortButton.Enabled = true;
            Form.NextButton.Focus();

            context.EndInstallation();
            LogInstallationResults();

            if (executionService != null)
                executionService.Dispose();
        }

        public override void Initialize(InstallationContext context)
        {
            errorPictureBox.Visible = false;
            errorDetailsTextBox.Visible = false;

            TabStop = false;
            Form.NextButton.Enabled = true;
            Form.SkipButton.Enabled = false;
        }

        public override void Execute(InstallationContext context)
        {
            nextCommand = 0;
            this.context = context;
            this.context.BeginInstallation();
            installDetailsTextBox.Text = string.Empty;

            switch (context.Action)
            {
                case InstallAction.Install:
                    WriteInfoLine(CommonUIStrings.InstallBegin, context.SolutionInfo, DateTime.Now);
                    Log.InfoLine(CommonUIStrings.InstallBegin, context.SolutionInfo, DateTime.Now);
                    Title = CommonUIStrings.InstallTitle;
                    SubTitle = InstallationUtility.FormatString(context.SolutionInfo, CommonUIStrings.InstallSubTitle);
                    executionService.Install(context);
                    break;

                case InstallAction.Upgrade:
                    WriteInfoLine(CommonUIStrings.UpgradeBegin, context.SolutionInfo, DateTime.Now);
                    Log.InfoLine(CommonUIStrings.UpgradeBegin, context.SolutionInfo, DateTime.Now);
                    Title = CommonUIStrings.UpgradeTitle;
                    SubTitle = InstallationUtility.FormatString(context.SolutionInfo, CommonUIStrings.UpgradeSubTitle);
                    executionService.Upgrade(context);
                    break;

                case InstallAction.Repair:
                    WriteInfoLine(CommonUIStrings.RepairBegin, context.SolutionInfo, DateTime.Now);
                    Log.InfoLine(CommonUIStrings.RepairBegin, context.SolutionInfo, DateTime.Now);
                    Title = CommonUIStrings.RepairTitle;
                    SubTitle = InstallationUtility.FormatString(context.SolutionInfo, CommonUIStrings.RepairSubTitle);
                    executionService.Repair(context);
                    break;

                case InstallAction.Remove:
                    WriteInfoLine(CommonUIStrings.UninstallBegin, context.SolutionInfo, DateTime.Now);
                    Log.InfoLine(CommonUIStrings.UninstallBegin, context.SolutionInfo, DateTime.Now);
                    Title = CommonUIStrings.UninstallTitle;
                    SubTitle = InstallationUtility.FormatString(context.SolutionInfo, CommonUIStrings.UninstallSubTitle);
                    executionService.Uninstall(context);
                    break;
            }

            // subscribe form to listen the messages from commands
            executionService.SubscribeMessageListener(this);

            progressBar.Maximum = executionService.ExecuteCommands.Count;
            descriptionLabel.Text = executionService.ExecuteCommands.Count > 0 ? executionService.ExecuteCommands[0].Description : String.Empty;

            timer.Interval = 1000;
            timer.Tick += OnBeginProcess;
            timer.Start();

            Form.NextButton.Enabled = false;
        }

        public override Step CreateNextStep(InstallationContext context)
        {
            if (context.SolutionInfo != null)
            {
                return new SolutionValidatorControl(context.SolutionInfo, Form, this);
            }
            return new CompletionControl(Form, this);
        }

        private void LogInstallationResults()
        {
            switch (Form.Context.Action)
            {
                case InstallAction.Install:
                    WriteInfoLine(errors == 0 ? CommonUIStrings.InstallSuccess : CommonUIStrings.InstallError, DateTime.Now);
                    Log.InfoLine(errors == 0 ? CommonUIStrings.InstallSuccess : CommonUIStrings.InstallError, DateTime.Now);
                    break;

                case InstallAction.Upgrade:
                    WriteInfoLine(errors == 0 ? CommonUIStrings.UpgradeSuccess : CommonUIStrings.UpgradeError, DateTime.Now);
                    Log.InfoLine(errors == 0 ? CommonUIStrings.UpgradeSuccess : CommonUIStrings.UpgradeError, DateTime.Now);
                    break;

                case InstallAction.Repair:
                    WriteInfoLine(errors == 0 ? CommonUIStrings.RepairSuccess : CommonUIStrings.RepairError, DateTime.Now);
                    Log.InfoLine(errors == 0 ? CommonUIStrings.RepairSuccess : CommonUIStrings.RepairError, DateTime.Now);
                    break;

                case InstallAction.Remove:
                    WriteInfoLine(errors == 0 ? CommonUIStrings.UninstallSuccess : CommonUIStrings.UninstallError, DateTime.Now);
                    Log.InfoLine(errors == 0 ? CommonUIStrings.UninstallSuccess : CommonUIStrings.UninstallError, DateTime.Now);
                    break;
            }
        }

        private void InitiateRollback()
        {
            Form.AbortButton.Enabled = false;

            progressBar.Maximum = executionService.RollbackCommands.Count;
            progressBar.Value = executionService.RollbackCommands.Count;
            nextCommand = 0;
            rollbackErrors = 0;
            progressBar.Step = -1;
          
            timer = new Timer {Interval = 1000};
            timer.Tick += OnRollbackProcess;
            timer.Start();
    }

        private void HighlightText(string text, Color color)
        {
            int endIndex = text.Trim().Length;
            int startIndex = -1;
            
            if (endIndex > 0)
                startIndex = installDetailsTextBox.Find(text.Trim(), RichTextBoxFinds.NoHighlight);
            
            if (startIndex > -1)
            {
                installDetailsTextBox.Select(startIndex, endIndex);
                installDetailsTextBox.SelectionColor = color;
            }
        }

        private void ScrollText()
        {
            installDetailsTextBox.SelectionStart = installDetailsTextBox.TextLength;
            installDetailsTextBox.ScrollToCaret();
        }

        private void WriteInfoLine(string messageFormat, params object[] args)
        {
            var message = args != null && args.Length > 0 ? string.Format(messageFormat, args) : messageFormat;

            installDetailsTextBox.AppendText(Environment.NewLine + message + Environment.NewLine);
            HighlightText(message, Color.BlueViolet);
            ScrollText();
        }

        void IMessageDispatcherListener.OnErrorMessageReceived(string message)
        {
            Invoke((MethodInvoker)(() =>
            {
                installDetailsTextBox.AppendText(message + Environment.NewLine);
                HighlightText(message, Color.Red);
                ScrollText();
            }));
        }

        void IMessageDispatcherListener.OnMessageReceived(string message)
        {
            Invoke((MethodInvoker)(() =>
                    {
                        installDetailsTextBox.AppendText(message + Environment.NewLine);
                        ScrollText();
                    }));
        }
    }

    public class Result
    {
        private bool success;

        private bool exceptionThrown;

        public Result()
        {
        }

        public Result(bool success, bool exceptionThrown)
        {
            this.success = success;
            this.exceptionThrown = exceptionThrown;
        }

        public bool Success
        {
            get { return success; }
            set { success = value; }
        }

        public bool ExceptionThrown
        {
            get { return exceptionThrown; }
            set { exceptionThrown = value; }
        }
    }
}