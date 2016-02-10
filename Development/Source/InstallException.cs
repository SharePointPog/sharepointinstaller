/**********************************************************************/
/*                                                                    */
/*                   SharePoint Solution Installer                    */
/*             http://www.codeplex.com/sharepointinstaller            */
/*                                                                    */
/*               (c) Copyright 2007 Lars Fastrup Nielsen.             */
/*                                                                    */
/*  This source is subject to the Microsoft Permissive License.       */
/*  http://www.codeplex.com/sharepointinstaller/Project/License.aspx  */
/*                                                                    */
/**********************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace CodePlex.SharePointInstaller
{
  public class InstallException : ApplicationException
  {
    public InstallException(string message)
      : base(message)
    {
    }

    public InstallException(string message, Exception inner)
      : base(message, inner)
    {
    }
  }
}
