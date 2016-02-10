using System;
using System.ComponentModel;
using System.ServiceProcess;
using CodePlex.SharePointInstaller.Logging;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Validators
{
    public class TimerServiceValidator : BaseValidator
    {
        public TimerServiceValidator(String id) : base(id)
        {
        }

        protected override ValidationResult Validate()
        {
            try
            {
                if (SPFarm.Local == null)
                {
                    ErrorString = "Insufficient rights to access configuration database.";
                    return ValidationResult.Error;
                }

                foreach (var server in SPFarm.Local.Servers)
                {
                    foreach (var service in server.ServiceInstances)
                    {
                        #if SP2010
                        //this is spf 2010
                        if (service.TypeName == "Microsoft SharePoint Foundation Timer")
                        {
                            try
                            {
                                var serviceController = new ServiceController("SPTimerV4", server.Name);
                                if (serviceController.Status != ServiceControllerStatus.Running)
                                {
                                    Log.Info(String.Format("Microsoft SharePoint Foundation Timer is not running on {0}",
                                                           server.Name));
                                    return ValidationResult.Error;
                                }

                            }
                            catch (UnauthorizedAccessException ex)
                            {
                                Log.Error(ex.Message, ex);
                                QuestionString = String.Format("Failed to access Microsoft SharePoint Foundation Timer on {0}.", server.Name);
                                return ValidationResult.Inconclusive;
                            }
                        }
                        #else
                        //this is wss 3.0
                        if (service.TypeName == "Windows SharePoint Services Timer")
                        {
                            try
                            {
                                var serviceController = new ServiceController("SPTimerV3", server.Name);
                                if (serviceController.Status != ServiceControllerStatus.Running)
                                {
                                    Log.Info(String.Format("Windows SharePoint Services Timer is not running on {0}",
                                                           server.Name));
                                    return ValidationResult.Error;
                                }

                            }
                            catch (UnauthorizedAccessException ex)
                            {
                                Log.Error(ex.Message, ex);
                                QuestionString = String.Format("Failed to access Windows SharePoint Services Timer on {0}.", server.Name);
                                return ValidationResult.Inconclusive;
                            }
                        }
            
                        #endif
                    }
                }

                return ValidationResult.Success;
                
                //
                // LFN 2009-06-21: Do not restart the time service anymore. First it does
                // not always work with Windows Server 2008 where it seems a local 
                // admin may not necessarily be allowed to start and stop the service.
                // Secondly, the timer service has become more stable with WSS SP1 and SP2.
                //
                /*TimeSpan timeout = new TimeSpan(0, 0, 60);
                ServiceController sc = new ServiceController("SPTimerV3");
                if (sc.Status == ServiceControllerStatus.Running)
                {
                  sc.Stop();
                  sc.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                }

                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, timeout);

                return SystemCheckResult.Success;*/
            }
            catch (System.ServiceProcess.TimeoutException ex)
            {
                Log.Error(ex.Message, ex);
            }
            catch (Win32Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex.Message, ex);
            }

            return ValidationResult.Inconclusive; 
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
