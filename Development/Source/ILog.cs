/******************************************************************/
/*                                                                */
/*                SharePoint Solution Installer                   */
/*                                                                */
/*    Copyright 2007 Lars Fastrup Nielsen. All rights reserved.   */
/*    http://www.fastrup.dk                                       */
/*                                                                */
/*    This program contains the confidential trade secret         */
/*    information of Lars Fastrup Nielsen.  Use, disclosure, or   */
/*    copying without written consent is strictly prohibited.     */
/*                                                                */
/******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace CodePlex.SharePointInstaller
{
  /// <summary>
  /// Interface for loggers. Use the LogManager to get hold of a logger.
  /// </summary>
  public interface ILog
  {
    void Info(object message);

    void Info(object message, Exception t);

    void Warn(object message);

    void Warn(object message, Exception t);

    void Error(object message);

    void Error(object message, Exception t);

    void Fatal(object message);

    void Fatal(object message, Exception t);
  }
}
