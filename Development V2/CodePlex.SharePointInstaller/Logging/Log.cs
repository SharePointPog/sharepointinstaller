using System;
using System.Collections.Generic;

namespace CodePlex.SharePointInstaller.Logging
{
    public class Log
    {
        private static readonly IList<String> records = new List<String>();

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Installer));
        
        public static void Info(String message)
        {
            records.Add(message);
            log.Info(message);
        }

        public static void InfoLine(String message)
        {
            InfoLine(message, null);
        }

        public static void InfoLine(String messageFormat, params object[] args)
        {
            var message = args != null && args.Length > 0 ? string.Format(messageFormat, args) : messageFormat;

            records.Add(String.Format("\r\n{0}\r\n", message));
            log.Info(String.Format("\r\n{0}\r\n", message));
        }

        public static void Info(String message, Exception e)
        {
            records.Add(message);
            records.Add(e.ToString());
            log.Info(message, e);
        }        

        public static void Debug(String message)
        {
            records.Add(message);
            log.Debug(message);
        }

        public static void Debug(String message, Exception e)
        {
            records.Add(message);
            records.Add(e.ToString());
            log.Debug(message);
        }

        public static void Error(String message)
        {
            records.Add(message);
            log.Error(message);
        }

        public static void ErrorFormat(Exception e, String message, params object[] args)
        {
            records.Add(String.Format(message, args));
            records.Add(e.ToString());
            log.Error(String.Format(message, args), e);            
        }

        public static void Error(String message, Exception e)
        {
            records.Add(message);
            records.Add(e.ToString());
            log.Error(message, e);
        }

        public static void Error(Exception e)
        {
            log.Error(e.Message, e);
        }


        public static void WriteTrace(string msg)
        {
            log.Debug(msg);
        }

        public static IList<String> Records
        {
            get
            {
                return records;
            }
        }
    }
}