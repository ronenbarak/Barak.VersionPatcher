using Barak.VersionPatcher.Engine;

namespace Barak.VersionPatcher.TFS
{
    public class TfsSourceControlFactory : ISourceControlFactory
    {
        private string m_fileSystemPath;
        public string Type { get { return "TFS"; } }

        public TfsSourceControlFactory(string fileSystemPath)
        {
            m_fileSystemPath = fileSystemPath;
        }

        public bool CanHandle(string path)
        {
            if (path.StartsWith(@"$/"))
            {
                return true;
            }

            return false;
        }

        public ISourceControl Create(string path)
        {
            return new TfsSourceControl(path, m_fileSystemPath);
        }
    }
}