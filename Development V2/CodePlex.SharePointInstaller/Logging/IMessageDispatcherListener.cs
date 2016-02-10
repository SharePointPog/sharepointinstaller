namespace CodePlex.SharePointInstaller.Logging
{
    public interface IMessageDispatcherListener
    {
        void OnErrorMessageReceived(string message);
        void OnMessageReceived(string message);
    }
}