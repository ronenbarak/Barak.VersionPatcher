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
        private VersionSpec m_lastChangeset;

        public TfsSourceControl(string sourceControlRootPath, string fileSystemPath)
        {
            m_fileSystemPath = fileSystemPath;
            m_sourceControlRootPath = sourceControlRootPath;
            m_workspace = new Lazy<Workspace>(() =>
                                              {
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
                var versionControlFile = Path.Combine(m_fileSystemPath, VersionPathcerConsts.VersionControlFile);
                if (System.IO.File.Exists(versionControlFile))
                {
                    Checkout(versionControlFile);
                    System.IO.File.WriteAllText(versionControlFile,
                        string.Format("{0}{1}{2}", DateTime.Now, Environment.NewLine, string.Join(Environment.NewLine, m_pendingChanges)));
                }
                else
                {
                    System.IO.File.WriteAllText(versionControlFile,
                        string.Format("{0}{1}{2}", DateTime.Now, Environment.NewLine, string.Join(Environment.NewLine, m_pendingChanges)));
                    m_workspace.Value.PendAdd(versionControlFile);
                    m_pendingChanges.Add(versionControlFile);
                }

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



        public void Checkout(string path)
        {
            int count = m_workspace.Value.PendEdit(path);
            if (count == 0)
            {
                var relativePath = path.Substring(m_fileSystemPath.Length);
                var sourceControlFilePath = m_sourceControlRootPath + "/" + relativePath.Replace(@"\", "/");
                m_workspace.Value.Get(new[] { sourceControlFilePath }, m_lastChangeset, RecursionType.None, GetOptions.GetAll | GetOptions.Overwrite);

                count = m_workspace.Value.PendEdit(path);
                if (count == 0)
                {
                    throw new Exception(string.Format("Unable to edit file {0}", path));
                }
            }
            m_pendingChanges.Add(path);
        }

        public IEnumerable<IRevision> GetRevisionsUpTo(string id)
        {
            if (string.IsNullOrEmpty(id))
            {

                m_lastChangeset = LatestVersionSpec.Instance;
            }
            else
            {
                m_lastChangeset = new ChangesetVersionSpec(int.Parse(id));   
            }

            VersionSpec sourceVersionSpec = null;

            int? sourceChangeSetId = null;  
            try
            {
                var item = m_versionControlServer.GetItem(
                    System.IO.Path.Combine(m_sourceControlRootPath, VersionPathcerConsts.VersionControlFile),
                    m_lastChangeset,
                    DeletedState.NonDeleted, GetItemsOptions.None);

                sourceVersionSpec = new ChangesetVersionSpec(item.ChangesetId);
                sourceChangeSetId = item.ChangesetId;
            }
            catch (Exception e)
            {
                sourceVersionSpec = new DateVersionSpec(new DateTime(1973, 1, 1)); // The begining of time;
            }

            System.Collections.IEnumerable changesets = m_versionControlServer.QueryHistory(
            m_sourceControlRootPath,
            m_lastChangeset,
            0, 
            RecursionType.Full, 
            null,
            sourceVersionSpec,
            m_lastChangeset,
            int.MaxValue,
            true, 
            false);

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
                if (m_pendingChanges.Count != 0)
                {
                    m_workspace.Value.Undo(m_pendingChanges.ToArray());
                }

                if (m_removeMapping)
                {
                    //m_workspace.Value.DeleteMapping(new WorkingFolder(m_sourceControlRootPath, m_fileSystemPath, WorkingFolderType.Map, RecursionType.Full));
                    m_workspace.Value.Delete();
                }
                m_configurationServer.Disconnect();
            }
        }
    }
}
