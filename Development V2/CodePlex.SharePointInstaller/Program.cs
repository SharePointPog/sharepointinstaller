using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace CodePlex.SharePointInstaller
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (!IsRunAsAdmin())           
            {
                Elevate();
                Application.Exit();
            }
            else
            {
                try
                {
                    var form = new Installer {Text = "Setup wizard"};
                    Application.Run(form);
                }
                catch (Exception e)
                {
                    MessageBox.Show(
                        String.Format(
                            "An unhandled exception has occured. Application will now terminate. See the details in application log. {0}Exception: {1}{0}",
                            Environment.NewLine, e.Message), "Unrecoverable Exception Occured", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Logging.Log.Error("Unrecoverable exception occured", e);
                    Application.Exit();
                }
            }
        }

        /// <summary>
        /// The function checks whether the current process is run as administrator.
        /// In other words, it dictates whether the primary access token of the 
        /// process belongs to user account that is a member of the local 
        /// Administrators group and it is elevated.
        /// </summary>
        /// <returns>
        /// Returns true if the primary access token of the process belongs to user 
        /// account that is a member of the local Administrators group and it is 
        /// elevated. Returns false if the token does not.
        /// </returns>
        internal static bool IsRunAsAdmin()
        {
            var id = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// The function checks whether the current process is run as administrator
        /// and elevates privileges, if the user does not accept the elevation the 
        /// program exists.
        /// </summary>
        /// <returns>
        /// Returns true if the the user elevated, false if not.
        /// </returns>
        private static bool Elevate()
        {
            // Launch itself as administrator
            var proc = new ProcessStartInfo
                           {
                               UseShellExecute = true,
                               WorkingDirectory = Environment.CurrentDirectory,
                               FileName = Application.ExecutablePath,
                               Verb = "runas"
                           };
            try
            {
                Process.Start(proc);
                return true;
            }
            catch
            {
                // The user refused to allow privileges elevation.   
                Logging.Log.Error("User did not elevate privileges...");
                return false;
            }

        }
    }
}