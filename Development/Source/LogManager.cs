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
    public class LogMessage
    {
        public enum Level { Info, Warning, Error, Fatal };
        public Level MessageLevel;
        public string Message;
        public Exception Exception = null;
        public LogMessage(Level level, object message) { this.MessageLevel = level; this.Message = message.ToString(); }
        public LogMessage(Level level, object message, Exception exc) { this.MessageLevel = level; this.Message = message.ToString(); this.Exception = exc; }
    }
    public static class LogManager
    {
        private static readonly MessageLogger defaultLogger = new MessageLogger();

        public static ILog GetLogger()
        {
            return defaultLogger;
        }
        public static List<LogMessage> GetMessages()
        {
            return defaultLogger.Messages;
        }

        // Implementation of LogManager's log (which simply accumulates all messages)
        private class MessageLogger : ILog
        {
            public List<LogMessage> Messages = new List<LogMessage>();
            public void Info(object message)
            {
                Messages.Add(new LogMessage(LogMessage.Level.Info, message));
            }

            public void Info(object message, Exception t)
            {
                Messages.Add(new LogMessage(LogMessage.Level.Info, message, t));
            }

            public void Warn(object message)
            {
                Messages.Add(new LogMessage(LogMessage.Level.Warning, message));
            }

            public void Warn(object message, Exception t)
            {
                Messages.Add(new LogMessage(LogMessage.Level.Warning, message, t));
            }

            public void Error(object message)
            {
                Messages.Add(new LogMessage(LogMessage.Level.Error, message));
            }

            public void Error(object message, Exception t)
            {
                Messages.Add(new LogMessage(LogMessage.Level.Error, message, t));
            }

            public void Fatal(object message)
            {
                Messages.Add(new LogMessage(LogMessage.Level.Fatal, message));
            }

            public void Fatal(object message, Exception t)
            {
                Messages.Add(new LogMessage(LogMessage.Level.Fatal, message));
            }
        }
    }
}
