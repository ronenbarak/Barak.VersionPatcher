using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Barak.VersionPatcher.Engine.CSProj;
using File = Barak.VersionPatcher.Engine.CSProj.File;

namespace Barak.VersionPatcher.Engine
{
    public class VersionPatcher
    {
        class ChangeFiles
        {
            public ChangeFiles()
            {
                Projects = new List<CSProject>();
            }

            public string FullPath { get; set; }
            public List<CSProject> Projects { get; private set; }
        }

        private SourceControlProvider m_sourceControlProvider;

        public VersionPatcher(SourceControlProvider sourceControlProvider)
        {
            m_sourceControlProvider = sourceControlProvider;
        }

        private static void AddChangedProjects(Dictionary<CSProject, List<CSProject>> projectToDependents, CSProject changeProject, HashSet<CSProject> changeProjects)
        {
            List<CSProject> dependentProject;
            if (projectToDependents.TryGetValue(changeProject, out dependentProject))
            {
                foreach (var csProject in dependentProject)
                {
                    if (changeProjects.Add(csProject))
                    {
                        AddChangedProjects(projectToDependents, csProject, changeProjects);
                    }
                }
            }
        }

        public void Patch(PatchInfo patchInfo)
        {
            using (ISourceControl sourceControl = GetSourceControl(patchInfo))
            {
                IEnumerable<IRevision> revisions = sourceControl.GetRevisionsUpTo(patchInfo.Revision);
                var changeFiles = revisions.Select(p => new ChangeFiles()
                                                        {
                                                            FullPath =
                                                                System.IO.Path.Combine(patchInfo.FileSystemPath, p.Item),
                                                        }).ToDictionary(p => p.FullPath.ToUpper());

                if (revisions.Count() != 0)
                {
                    var allProjects = GetAllProjects(patchInfo, changeFiles);

                    var changeProjects = GetDirectlyChangedProjects(changeFiles);

                    if (patchInfo.Recursive == true)
                    {
                        AddIndirectChangedProjects(allProjects, changeProjects);
                    }

                    var fileToModify = GetAssemblyInfoFilesToUpdage(changeProjects);

                    UpgradeAssemblyFiles(fileToModify, sourceControl, patchInfo.VersionPart);

                    sourceControl.Commit(patchInfo.Comment);
                }
            }
        }

        private void UpgradeAssemblyFiles(IEnumerable<string> fileToModify,ISourceControl sourceControl,VersionPart versionPart)
        {
            Regex regex = null;
            Regex regexFileVersion = null;
            if (fileToModify.Any())
            {
                regex = new Regex(@"assembly: AssemblyVersion\(""(\d+)\.(\d+)\.(\d+)\.(\d+)""\)",RegexOptions.Compiled);
                regexFileVersion = new Regex(@"assembly: AssemblyFileVersion\(""(\d+)\.(\d+)\.(\d+)\.(\d+)""\)", RegexOptions.Compiled);        
            }
            
            foreach (var fileAssemblySetting in fileToModify)
            {
                var fileText = System.IO.File.ReadAllText(fileAssemblySetting);
                string newFile = fileText;
                var match = regex.Match(newFile);
                bool isCheckout = false;
                newFile = UpdateAssemblyFile(sourceControl, versionPart, match, fileAssemblySetting, newFile, ref isCheckout);
                match = regexFileVersion.Match(newFile);
                newFile = UpdateAssemblyFile(sourceControl, versionPart, match, fileAssemblySetting, newFile, ref isCheckout);

                if (isCheckout)
                {
                    System.IO.File.WriteAllText(fileAssemblySetting, newFile);
                }


            }
        }

        private static string UpdateAssemblyFile(ISourceControl sourceControl, VersionPart versionPart, Match match,
            string fileAssemblySetting, string newFile, ref bool isCheckout)
        {
            if (match.Success)
            {
                if (!isCheckout)
                {
                    sourceControl.Checkout(fileAssemblySetting);
                    isCheckout = true;
                }
                do
                {
                    var group = match.Groups[((int) versionPart) + 1];
                    int value = int.Parse(@group.Value);
                    newFile = newFile.Substring(0, @group.Index) + (value + 1) + newFile.Substring(@group.Index + @group.Length);
                    match = match.NextMatch();
                } while (match.Success);
            }
            return newFile;
        }

        private static HashSet<string> GetAssemblyInfoFilesToUpdage(IEnumerable<CSProject> changeProjects)
        {
            HashSet<string> fileToModify = new HashSet<string>();
            foreach (var changeProject in changeProjects)
            {
                File foundFile = changeProject.Files.FirstOrDefault(p => p.Path.ToUpper() == @"PROPERTIES\ASSEMBLYINFO.CS");
                if (foundFile != null)
                {
                    fileToModify.Add(
                        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(changeProject.FullPath), foundFile.Path)).ToUpper());
                }
            }
            return fileToModify;
        }

        private static void AddIndirectChangedProjects(List<CSProject> allProjects, HashSet<CSProject> changeProjects)
        {
            Dictionary<CSProject, List<CSProject>> projectToDependents = allProjects.ToDictionary(p => p,
                project => new List<CSProject>());
            Dictionary<string, CSProject> fullPathToProject = allProjects.ToDictionary(p => p.FullPath);

            foreach (var csProject in allProjects)
            {
                foreach (Referance currDependentReferance in csProject.Referances)
                {
                    if (!string.IsNullOrEmpty(currDependentReferance.Path))
                    {
                        var fullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(csProject.FullPath), currDependentReferance.Path));
                        CSProject currDependentProject;
                        if (fullPathToProject.TryGetValue(fullPath.ToUpper(), out currDependentProject))
                        {
                            List<CSProject> dependetProjects;
                            if (projectToDependents.TryGetValue(currDependentProject, out dependetProjects))
                            {
                                dependetProjects.Add(csProject);
                            }
                        }
                    }
                }
            }

            foreach (var changeProject in changeProjects.ToList())
            {
                AddChangedProjects(projectToDependents, changeProject, changeProjects);
            }
        }

        private static HashSet<CSProject> GetDirectlyChangedProjects(Dictionary<string, ChangeFiles> changeFiles)
        {
            HashSet<CSProject> changeProjects = new HashSet<CSProject>();
            foreach (var changeFilese in changeFiles.Values)
            {
                foreach (var changeProject in changeFilese.Projects)
                {
                    changeProjects.Add(changeProject);
                }
            }
            return changeProjects;
        }

        private static List<CSProject> GetAllProjects(PatchInfo patchInfo, Dictionary<string, ChangeFiles> changeFiles)
        {
            string[] projectPaths = null;
            if (patchInfo.ProjectFiles != null &&
                patchInfo.ProjectFiles.Length != 0)
            {
                projectPaths = patchInfo.ProjectFiles;
            }
            else
            {
                projectPaths = System.IO.Directory.GetFiles(patchInfo.FileSystemPath, "*.csproj", SearchOption.AllDirectories);
            }

            projectPaths = projectPaths.Where(p => System.IO.File.Exists(p)).ToArray();

            List<CSProject> allProjects = new List<CSProject>();
            foreach (var projectPath in projectPaths)
            {
                CSProject csProject = null;
                string projectDir = System.IO.Path.GetDirectoryName(projectPath);
                using (var projectStream = System.IO.File.OpenRead(projectPath))
                {
                    csProject = ProjectFileTypeToCSProject.Convert(System.IO.Path.GetFileName(projectPath), ProjectFileParser.ParseFile(projectStream));
                    csProject.FullPath = projectPath.ToUpper();

                    // Add self if it has been changed.
                    ChangeFiles changeProject;
                    if (changeFiles.TryGetValue(csProject.FullPath, out changeProject))
                    {
                        changeProject.Projects.Add(csProject);
                    }

                    foreach (File currFile in csProject.Files)
                    {
                        ChangeFiles changeFile;
                        if (changeFiles.TryGetValue(Path.GetFullPath(Path.Combine(projectDir, currFile.Path)).ToUpper(),
                            out changeFile))
                        {
                            changeFile.Projects.Add(csProject);
                        }
                    }

                    // Q: What happends When the referance is full and not relative?
                    // A: Hope it wont crash.


                    foreach (Referance currReferance in csProject.Referances)
                    {
                        if (!string.IsNullOrEmpty(currReferance.Path))
                        {
                            ChangeFiles changeFile;
                            if (changeFiles.TryGetValue(Path.GetFullPath(Path.Combine(projectDir, currReferance.Path)).ToUpper(), out changeFile))
                            {
                                changeFile.Projects.Add(csProject);
                            }
                        }
                    }
                }
                allProjects.Add(csProject);
            }
            return allProjects;
        }

        private ISourceControl GetSourceControl(PatchInfo patchInfo)
        {
            var sourceControlProvider = m_sourceControlProvider;
            ISourceControl sourceControl = null;
            if (patchInfo.ForceVersionControlType != null)
            {
                sourceControl = sourceControlProvider.GetSourceControlByType(patchInfo.ForceVersionControlType,
                    patchInfo.VersionControlPath);
            }
            else
            {
                sourceControl = sourceControlProvider.GetSourceControlByPath(patchInfo.VersionControlPath);
            }

            if (sourceControl == null)
            {
                throw new Exception("Source Control not found");
            }

            sourceControl.Connect(patchInfo.SourceControlUrl);
            return sourceControl;
        }
    }
}
