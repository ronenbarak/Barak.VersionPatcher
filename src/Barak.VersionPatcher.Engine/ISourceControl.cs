using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Barak.VersionPatcher.Engine
{
    public interface ISourceControl : IDisposable
    {
        void Connect(Uri path);
        IEnumerable<IRevision> GetRevisionsUpTo(string id);
        void Checkout(string path);
        void Commit(string comment);
    }

    public interface IRevision
    {
        string Item { get; } 
    }
}