using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Barak.VersionPatcher.Engine
{
    public class SourceControlProvider
    {
        private IEnumerable<ISourceControlFactory> m_sourceControlFactories;

        public SourceControlProvider(IEnumerable<ISourceControlFactory> sourceControlFactories)
        {
            m_sourceControlFactories = sourceControlFactories.ToList();
        }

        public ISourceControl GetSourceControlByPath(string path)
        {
            var sourceFactory = m_sourceControlFactories.FirstOrDefault(p => p.CanHandle(path));
            if (sourceFactory != null)
            {
                return sourceFactory.Create(path);
            }

            return null;
        }

        public ISourceControl GetSourceControlByType(string type,string path)
        {
            var sourceFactory =  m_sourceControlFactories.FirstOrDefault(p => p.Type == type);
            if (sourceFactory != null)
            {
                return sourceFactory.Create(path);
            }

            return null;
        }
    }
}
