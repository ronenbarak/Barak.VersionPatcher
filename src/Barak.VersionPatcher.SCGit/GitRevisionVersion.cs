using Barak.VersionPatcher.Engine;
using LibGit2Sharp;

namespace Barak.VersionPatcher.SCGit
{
    public static class RevisionVersionHelper
    {
        public static Commit GetCommit(this IRevisionVersion revisionVersion)
        {
            return (revisionVersion as GitRevisionVersion).Commit;
        }
    }

    public class GitRevisionVersion : IRevisionVersion
    {
        public Commit Commit { get; private set; }

        public GitRevisionVersion(Commit commit)
        {
            Commit = commit;
        }
    }
}