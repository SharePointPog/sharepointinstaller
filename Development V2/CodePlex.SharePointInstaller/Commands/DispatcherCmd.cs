using System.Collections.Generic;
using CodePlex.SharePointInstaller.Logging;

namespace CodePlex.SharePointInstaller.Commands
{
    public abstract class DispatcherCmd : Cmd, IMessageDispatcher
    {
        private readonly List<IMessageDispatcherListener> listeners = new List<IMessageDispatcherListener>();

        public void AttachListener(IMessageDispatcherListener listener)
        {
            if (!listeners.Contains(listener))
                listeners.Add(listener);
        }

        public void DetachListener(IMessageDispatcherListener listener)
        {
            if (listeners.Contains(listener))
                listeners.Remove(listener);
        }

        public void DetachAllListeners()
        {
            listeners.Clear();
        }

        public void DispatchMessage(string message)
        {
            DispatchMessage(message, null);
        }

        public void DispatchMessage(string messageFormat, params object[] args)
        {
            if (listeners.Count == 0)
                return;

            var message = args != null && args.Length > 0
                              ? string.Format(messageFormat, args)
                              : messageFormat;

            foreach (var listener in listeners)
            {
                listener.OnMessageReceived(message);
            }
        }

        public void DispatchErrorMessage(string messageFormat, params object[] args)
        {
            if (listeners.Count == 0)
                return;

            var message = args != null && args.Length > 0
                              ? string.Format(messageFormat, args)
                              : messageFormat;

            foreach (var listener in listeners)
            {
                listener.OnErrorMessageReceived(message);
            }
        }
    }
}