using System;
using System.ComponentModel;
using System.ServiceProcess;
using System.Windows.Forms;
using CodePlex.SharePointInstaller.Logging;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Validators
{
    public class AdministrativeServiceValidator : BaseValidator
    {
        public AdministrativeServiceValidator(String id) : base(id)
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
                
                foreach(var server in SPFarm.Local.Servers)
                {
                    foreach(var service in server.ServiceInstances)
                    {
                        #if SP2010

                        if (service.TypeName == "Microsoft SharePoint Foundation Administration")
                        {
                            try
                            {
                                var serviceController = new ServiceController("SPAdminV4", server.Name);
                                if (serviceController.Status != ServiceControllerStatus.Running)
                                {
                                    Log.Info(
                                        String.Format(
                                            "Microsoft SharePoint Foundation Administration is not running on {0}",
                                            server.Name));
                                    return ValidationResult.Error;
                                }
                            }
                            catch (UnauthorizedAccessException ex)
                            {
                                Log.Error(ex.Message, ex);
                                QuestionString = String.Format("Failed to access Microsoft SharePoint Foundation Administration on {0}.", server.Name);
                                return ValidationResult.Inconclusive;
                            }
                        }   
    
                        #else
                        if(service.TypeName == "Windows SharePoint Services Administration")
                        {
                            try
                            {
                                var serviceController = new ServiceController("SPAdmin", server.Name);
                                if (serviceController.Status != ServiceControllerStatus.Running)
                                {
                                    Log.Info(
                                        String.Format(
                                            "Windows SharePoint Services Administration is not running on {0}",
                                            server.Name));
                                    return ValidationResult.Error;
                                }
                            }
                            catch(UnauthorizedAccessException ex)
                            {
                                Log.Error(ex.Message, ex);
                                QuestionString = String.Format("Failed to access Windows SharePoint Services Administration on {0}.", server.Name);
                                return ValidationResult.Inconclusive;
                            }
                        }
                        #endif
                    }
                }
                
                return ValidationResult.Success;
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
