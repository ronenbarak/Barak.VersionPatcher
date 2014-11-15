using Barak.VersionPatcher.Engine;

namespace Barak.VersionPatcher.SCGit
{
    public class GitRevision : IRevision
    {
        public GitRevision(string path)
        {
            Item = path;
        }
        public string Item { get; private set; }
    }
}