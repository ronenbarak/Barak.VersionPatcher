using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Barak.VersionPatcher.Engine;
using LibGit2Sharp;

namespace Barak.VersionPatcher.SCGit
{
    public class GitSourceControl : ISourceControl
    {
        private readonly string m_branchPath;
        private readonly string m_fileSystemPath;
        private Repository m_repository;
        private Branch m_branch;
        private string m_bareWorkDirPath;

        public GitSourceControl(string branchPath, string fileSystemPath,string userName,string password)
        {
            m_password = password;
            m_userName = userName;
            m_branchPath = branchPath;
            m_fileSystemPath = fileSystemPath;
            if (!m_fileSystemPath.EndsWith(new string(Path.DirectorySeparatorChar,1)))
            {
                m_fileSystemPath = m_fileSystemPath + Path.DirectorySeparatorChar;
            }
        }

        public void Dispose()
        {
            Rollback();
        }

        public void Connect(Uri path)
        {
            // Try to see if ther is already a git repository inside the filesystem path
            if (Repository.IsValid(m_fileSystemPath))
            {
                m_repository = new Repository(m_fileSystemPath, new RepositoryOptions());
            }
            else
            {
                var tempFolder = Guid.NewGuid().ToString("N");

                m_bareWorkDirPath = System.IO.Path.Combine(m_fileSystemPath, tempFolder);
                var ret = Repository.Clone(path.AbsoluteUri, m_bareWorkDirPath, new CloneOptions()
                {
                    CredentialsProvider = (url, fromUrl, types) => new UsernamePasswordCredentials()
                    {
                        Password = m_password,
                        Username = m_userName,
                    },
                    Checkout = false,
                    IsBare = true,
                });

                m_repository = new Repository(m_bareWorkDirPath, new RepositoryOptions());
            }

            m_branch = m_repository.Branches[m_branchPath];
            if (m_branch == null)
            {
                throw new Exception("Branch not found: " + m_branchPath);
            }
        }

        public IRevisionVersion GetRevisionById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new GitRevisionVersion(m_branch.Commits.First()); // Latest
            }
            else
            {
                return new GitRevisionVersion(m_branch.Commits.First(p => p.Sha == id));
            }
        }

        public IRevisionVersion GetRevisionOfItem(IRevisionVersion maxRevisionVersion, string path)
        {
            var relativePath = path.Substring(m_fileSystemPath.Length);

            var fileInGit = maxRevisionVersion.GetCommit()[relativePath];
            if (fileInGit == null)
            {
                return new GitRevisionVersion(m_branch.Commits.Last());// BegginingOfTime;
            }
            
            string currentSha = fileInGit.Target.Sha;

            bool maxCommitFound = false;
            var prevCommit = maxRevisionVersion.GetCommit();
            foreach (Commit c in m_branch.Commits)
            {
                if (!maxCommitFound)
                {
                    if (c == maxRevisionVersion.GetCommit())
                    {
                        maxCommitFound = true;
                    }
                }

                if (maxCommitFound)
                {
                    var fileInCommit = c[relativePath];
                    if (fileInCommit != null)
                    {
                        // If file with given name was found, check its SHA
                        if (fileInCommit.Target.Sha != currentSha)
                        {
                            return new GitRevisionVersion(prevCommit); // This is when the file has been last changed
                        }
                    }
                    else
                    {
                        return new GitRevisionVersion(prevCommit);
                    }
                    prevCommit = c;
                }
            }

            return maxRevisionVersion;
        }

        public IEnumerable<IRevision> GetRevisions(IRevisionVersion sourceRevision, IRevisionVersion targetRevision)
        {
            HashSet<string> paths = new HashSet<string>(new StringIgnoreCaseEqualityComarer());

            var changes = m_repository.Diff.Compare<TreeChanges>(sourceRevision.GetCommit().Tree, targetRevision.GetCommit().Tree);

            foreach (var changed in changes.Added.Union(changes.Copied).Union(changes.Deleted).Union(changes.Modified).Union(changes.Renamed).Union(changes.TypeChanged))
            {
                if (string.IsNullOrEmpty(changed.Path))
                {
                    paths.Add(changed.Path);
                }

                if (!string.IsNullOrEmpty(changed.OldPath))
                {
                    paths.Add(changed.OldPath);
                }
            }
            
            // Get all changed files between this
            return paths.Select(p => new GitRevision(p)).ToList();
        }

        private List<string> m_checkoutFiles = new List<string>();
        private bool m_isCompleted;
        private string m_userName;
        private string m_password;

        public void Checkout(string path)
        {
            m_checkoutFiles.Add(path);

            if (m_bareWorkDirPath == null)
            {
            }
        }

        public void Commit(string comment)
        {
            if (m_checkoutFiles.Count != 0)
            {
                if (m_bareWorkDirPath == null)
                {
                    foreach (var path in m_checkoutFiles)
                    {
                        var relativePath = path.Substring(m_fileSystemPath.Length);

                        m_repository.Index.Stage(relativePath);
                    }

                    if (m_repository.Index.RetrieveStatus().IsDirty)
                    {
                        m_repository.Commit(comment);
                    }
                    m_isCompleted = true;
                }
                else
                {
                    TreeDefinition td = TreeDefinition.From(m_branch.Commits.First());

                    foreach (var filePath in m_checkoutFiles)
                    {
                        var contentBytes = System.IO.File.ReadAllBytes(filePath);
                        MemoryStream ms = new MemoryStream(contentBytes);
                        Blob newBlob = m_repository.ObjectDatabase.CreateBlob(ms);

                        
                        var relativePath = filePath.Substring(m_fileSystemPath.Length);

                        var oldFile = td[relativePath];
                        if (oldFile != null)
                        {
                            td.Remove(relativePath);
                        }
                        td.Add(relativePath, newBlob, Mode.NonExecutableFile);
                    }

                    Tree tree = m_repository.ObjectDatabase.CreateTree(td);

                    Signature committer = new Signature("VersionPatcher", "VersionPathcer@BarakEx.com", DateTime.Now);
                    Signature author = committer;


                    // Create binary stream from the text
                    Commit commit = m_repository.ObjectDatabase.CreateCommit(
                        committer,
                        author, comment,
                        tree,
                        new[]{m_branch.Commits.First()}, false);

                    // Update the HEAD reference to point to the latest commit
                    m_repository.Refs.UpdateTarget(m_repository.Refs.Head, commit.Id);

                    m_isCompleted = true;
                    Push();
                }
            }
        }

        public void Push()
        {
            var remote = m_repository.Network.Remotes["origin"];
            var options = new PushOptions();
            options.CredentialsProvider = (url, fromUrl, types) => new UsernamePasswordCredentials()
            {
                Username = m_userName,
                Password = m_password,
            };
            var pushRefSpec = m_branchPath;
            m_repository.Network.Push(remote, pushRefSpec,options);
        }

        public void Complete()
        {
            m_isCompleted = true;
        }

        public void Rollback()
        {
            if (m_bareWorkDirPath != null)
            {                
                m_repository.Dispose();
                try
                {
                    System.IO.Directory.Delete(m_bareWorkDirPath,true);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Exception while deleting rep = " + exception.ToString());
                }

                m_bareWorkDirPath = null;
                m_repository = null;
            }
            else if (m_repository != null)
            {
                if (!m_isCompleted)
                {
                    // rollback all checkout files
                    foreach (var checkoutFile in m_checkoutFiles)
                    {
                        m_repository.Index.Unstage(checkoutFile);      
                    }
                }
                m_repository.Dispose();
                m_repository = null;
            }
        }
    }
}
