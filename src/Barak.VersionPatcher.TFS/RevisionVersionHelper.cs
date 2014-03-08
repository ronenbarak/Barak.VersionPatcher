using Barak.VersionPatcher.Engine;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Barak.VersionPatcher.TFS
{
    public static class RevisionVersionHelper
    {
        public static VersionSpec GetSpec(this IRevisionVersion revisionVersion)
        {
            return (revisionVersion as TFSRevisionVersion).VersionSpec;
        }
    }
}