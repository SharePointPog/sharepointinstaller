using System.Collections.Specialized;

namespace CodePlex.SharePointInstaller.Validators
{
    public interface ISyntaxVerifiable
    {
        bool IsSyntaxValid(StringDictionary keyValues, bool validateUrl, out string message);
    }
}
