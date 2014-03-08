using Barak.VersionPatcher.Engine;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Barak.VersionPatcher.TFS
{
    public class TFSRevisionVersion : IRevisionVersion
    {
        public VersionSpec VersionSpec { get; private set; }

        public TFSRevisionVersion(VersionSpec versionSpec)
        {
            VersionSpec = versionSpec;
        }
    }
}