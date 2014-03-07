using Barak.VersionPatcher.Engine;

namespace Barak.VersionPatcher.TFS
{
    public class TfsRevision : IRevision
    {
        public TfsRevision(string path)
        {
            Item = path;
        }
        public string Item { get; private set; }
    }
}