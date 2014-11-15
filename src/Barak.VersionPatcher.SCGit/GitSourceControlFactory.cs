using System;
using Barak.VersionPatcher.Engine;

namespace Barak.VersionPatcher.SCGit
{
    public class GitSourceControlFactory : ISourceControlFactory
    {
        private string m_fileSystemPath;
        private Uri m_connectionUri;
        private string m_username;
        private string m_password;

        public GitSourceControlFactory(string fileSystemPath,Uri connectionUri,string username,string password )
        {
            m_password = password;
            m_username = username;
            m_connectionUri = connectionUri;
            m_fileSystemPath = fileSystemPath;
        }

        public string Type { get { return "Git"; } }
        
        public bool CanHandle(string path)
        {
            return m_connectionUri.AbsolutePath.EndsWith(".git",StringComparison.OrdinalIgnoreCase);
        }

        public ISourceControl Create(string path)
        {
            return new GitSourceControl(path, m_fileSystemPath,m_username, m_password);
        }
    }
}