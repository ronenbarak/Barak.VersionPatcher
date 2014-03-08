using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Barak.VersionPatcher.Engine;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Common.Internal;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Barak.VersionPatcher.TFS
{
    public class TfsSourceControl : ISourceControl
    {
        private VersionControlServer m_versionControlServer;
        private TfsConfigurationServer m_configurationServer;
        private string m_sourceControlRootPath;
        private List<string> m_pendingChanges = new List<string>();
        private bool m_removeMapping = false;
        private string m_fileSystemPath;

        private bool m_disposed = false;
        private Lazy<Workspace> m_workspace;
        
        private bool m_shouldUndo = true;
        
        public TfsSourceControl(string sourceControlRootPath, string fileSystemPath)
        {
            m_fileSystemPath = fileSystemPath;
            m_sourceControlRootPath = sourceControlRootPath;
            m_workspace = new Lazy<Workspace>(() =>
                                              {
                                                  m_shouldUndo = true;
                                                  Workspace workspace = m_versionControlServer.TryGetWorkspace(m_fileSystemPath);

                                                  
                                                  if (workspace != null)
                                                  {
                                                      Guid lastGuid;
                                                      if (workspace.Name.StartsWith("VersionPatcher") && 
                                                          workspace.Name.Length > "VersionPatcher".Length && 
                                                          Guid.TryParse(workspace.Name.Substring("VersionPatcher".Length),out lastGuid))
                                                      {
                                                          m_removeMapping = true;
                                                      }
                                                  }
                                                  else 
                                                  {
                                                      workspace = m_versionControlServer.CreateWorkspace("VersionPatcher" + Guid.NewGuid());
                                                      var workingFolder = new WorkingFolder(m_sourceControlRootPath, m_fileSystemPath, WorkingFolderType.Map,RecursionType.Full);
                                                      m_removeMapping = true;
                                                      workspace.CreateMapping(workingFolder);
                                                      workspace.Get(LatestVersionSpec.Instance, GetOptions.Preview);
                                                  }

                                                  return workspace;
                                              });
        }

        public void Commit(string comment)
        {
            if (m_pendingChanges.Count != 0)
            {
                var filesToCheckin = m_workspace.Value.GetPendingChanges(m_pendingChanges.ToArray());
                try
                {
                    m_workspace.Value.CheckIn(filesToCheckin, comment);
                    m_pendingChanges.Clear();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to checkin: " + e.Message);
                    Conflict[] conf = m_workspace.Value.QueryConflicts(new string[0], true);
                    foreach (var conflict in conf)
                    {
                        Console.WriteLine("Conflict : " + conflict.GetDetailedMessage(true));
                    }
                    throw;
                }
            }
        }

        public void Complete()
        {
            m_shouldUndo = false;
        }

        public void Rollback()
        {
            if (m_removeMapping || (m_shouldUndo && m_pendingChanges.Count != 0))
            {
                m_workspace.Value.Undo(m_pendingChanges.ToArray());
            }

            if (m_removeMapping)
            {
                m_workspace.Value.Delete();
            }
        }

        private string GetWithoutTralingSlash(string url)
        {
            if (url.EndsWith(@"/"))
            {
                return url.Substring(0, url.Length - 1);
            }
            return url;
        }

        public void Connect(Uri tfsUri)
        {
            var projectCollectionName = tfsUri.Segments[tfsUri.Segments.Length - 1];
            var tfsConnection = tfsUri.AbsoluteUri.Substring(0, tfsUri.AbsoluteUri.Length - projectCollectionName.Length);

            m_configurationServer = TfsConfigurationServerFactory.GetConfigurationServer(new Uri(tfsConnection));
            //m_configurationServer.Connect(ConnectOptions.IncludeServices);

            CatalogNode configurationServerNode = m_configurationServer.CatalogNode;

            // Query the children of the configuration server node for all of the team project collection nodes
            ReadOnlyCollection<CatalogNode> tpcNodes = configurationServerNode.QueryChildren(
                    new Guid[] { CatalogResourceTypes.ProjectCollection },
                    false,
                    CatalogQueryOptions.None);

            foreach (CatalogNode tpcNode in tpcNodes)
            {
                if (tpcNode.Resource.DisplayName.ToUpper() == GetWithoutTralingSlash(projectCollectionName).ToUpper())
                {
                    Guid tpcId = new Guid(tpcNode.Resource.Properties["InstanceId"]);
                    TfsTeamProjectCollection tpc = m_configurationServer.GetTeamProjectCollection(tpcId);                
                    m_versionControlServer = tpc.GetService<VersionControlServer>();   
                    break;
                }
            }
            if (m_versionControlServer == null)
            {
                throw new Exception(string.Format("Project collection name '{0}' not found",projectCollectionName));
            }
        }

        public IRevisionVersion GetRevisionById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new TFSRevisionVersion(VersionSpec.Latest);   
            }
            else
            {
                return new TFSRevisionVersion(new ChangesetVersionSpec(id));   
            }
        }

        public IRevisionVersion GetRevisionOfItem(IRevisionVersion maxRevisionVersion, string path)
        {
            try
            {
                var item = m_versionControlServer.GetItem(
                    System.IO.Path.Combine(m_sourceControlRootPath, path),
                    maxRevisionVersion.GetSpec(),
                    DeletedState.NonDeleted, GetItemsOptions.None);

                return new TFSRevisionVersion(new ChangesetVersionSpec(item.ChangesetId));
            }
            catch (Exception e)
            {
                return new TFSRevisionVersion(new DateVersionSpec(new DateTime(1973, 1, 1))); ; // The begining of time;
            }
        }

        public void Checkout(string path)
        {
            m_shouldUndo = true;
            int count = m_workspace.Value.PendEdit(path);
            if (count == 0)
            {
                var relativePath = path.Substring(m_fileSystemPath.Length);
                var sourceControlFilePath = m_sourceControlRootPath + "/" + relativePath.Replace(@"\", "/");
                var getStatus = m_workspace.Value.Get(new[] { sourceControlFilePath }, VersionSpec.Latest, RecursionType.None, GetOptions.GetAll | GetOptions.Overwrite);
                if (getStatus.NumUpdated == 0)
                {
                    count = m_workspace.Value.PendAdd(path);   
                }
                else
                {
                    count = m_workspace.Value.PendEdit(path);   
                }
                if (count == 0)
                {
                    throw new Exception(string.Format("Unable to edit file {0}", path));
                }
            }
            m_pendingChanges.Add(path);
        }

        public IEnumerable<IRevision> GetRevisions(IRevisionVersion sourceRevision, IRevisionVersion targetRevision)
        {
            System.Collections.IEnumerable changesets = m_versionControlServer.QueryHistory(
            m_sourceControlRootPath,
            targetRevision.GetSpec(),
            0, 
            RecursionType.Full, 
            null,
            sourceRevision.GetSpec(),
            targetRevision.GetSpec(),
            int.MaxValue,
            true, 
            false);

            int? sourceChangeSetId = null;
            if (sourceRevision.GetSpec() is ChangesetVersionSpec)
            {
                sourceChangeSetId = (sourceRevision.GetSpec() as ChangesetVersionSpec).ChangesetId;
            }

            HashSet<string> items = new HashSet<string>();
            foreach (var changeset in changesets.OfType<Changeset>())
            {
                if (!sourceChangeSetId.HasValue || sourceChangeSetId.Value != changeset.ChangesetId)
                {
                    foreach (Change change in changeset.Changes)
                    {
                        if (change.Item.ServerItem.StartsWith(m_sourceControlRootPath,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            var item = change.Item.ServerItem.Substring(m_sourceControlRootPath.Length);
                            items.Add(item);
                        }
                    }
                }
            }
            return items.Select(p=> new TfsRevision(MakeFileSystemType(p))).ToList();
        }

        private string MakeFileSystemType(string revitionPath)
        {
            var fileSystemNice = revitionPath.Replace(@"/", @"\");
            if (fileSystemNice.StartsWith(@"\"))
            {
                return fileSystemNice.Substring(1);
            }
            return fileSystemNice;
        }

        public void Dispose()
        {
            if (!m_disposed)
            {
                m_disposed = true;
                Rollback();
                if (m_configurationServer != null)
                {
                    m_configurationServer.Disconnect();
                }
            }
        }
    }
}
