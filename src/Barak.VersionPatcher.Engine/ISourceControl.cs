using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Barak.VersionPatcher.Engine
{
    public interface IRevisionVersion
    {
    }

    public interface ISourceControl : IDisposable
    {
        void Connect(Uri path);
        IRevisionVersion GetRevisionById(string id);
        IRevisionVersion GetRevisionOfItem(IRevisionVersion maxRevisionVersion, string path);
        IEnumerable<IRevision> GetRevisions(IRevisionVersion sourceRevision, IRevisionVersion targetRevision);
        void Checkout(string path);
        void Commit(string comment);
        void Complete();
        void Rollback();
    }

    public interface IRevision
    {
        string Item { get; } 
    }
}