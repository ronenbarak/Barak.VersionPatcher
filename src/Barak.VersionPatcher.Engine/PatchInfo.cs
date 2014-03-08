using System;

namespace Barak.VersionPatcher.Engine
{
    public enum VersionPart
    {
        Major,
        Minor,
        Build,
        Revision
    }

    public class PatchInfo
    {
        public string ForceVersionControlType { get; set; }
        public Uri SourceControlUrl { get; set; }
        public string VersionControlPath { get; set; }
        public string Revision { get; set; }
        public string FileSystemPath { get; set; }
        public bool Commit { get; set; }
        public string Comment { get; set; }
        public string[] ProjectFiles { get; set; }
        public bool Recursive { get; set; }
        public VersionPart VersionPart { get; set; }
    }
}