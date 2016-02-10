namespace CodePlex.SharePointInstaller.Logging
{
    public interface IMessageDispatcher
    {
        void AttachListener(IMessageDispatcherListener listener);
        void DetachListener(IMessageDispatcherListener listener);
        void DetachAllListeners();
        void DispatchMessage(string message);
        void DispatchMessage(string messageFormat, params object[] args);
        void DispatchErrorMessage(string messageFormat, params object[] args);
    }
}