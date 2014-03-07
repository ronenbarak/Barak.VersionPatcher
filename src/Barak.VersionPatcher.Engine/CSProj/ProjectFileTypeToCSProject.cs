using System.Collections.Generic;
using System.Linq;

namespace Barak.VersionPatcher.Engine.CSProj
{
    public class ProjectFileTypeToCSProject
    {
        public static CSProject Convert(string fileName, ProjectFileType projectFileType)
        {
            return new CSProject(fileName, 
                GetFiles(projectFileType)
                , GetReferance(projectFileType)
                );
        }

        private static List<File> GetFiles(ProjectFileType projectFileType)
        {
            var itemGroups = (projectFileType.ItemGroup ?? new ItemGroupType[0]);
            var compiles = itemGroups.SelectMany(p => (p.Compile ?? new CompileType[0]).Select(x => new File(System.IO.Path.GetFileName(x.Include), x.Include)));
            var contents = itemGroups.SelectMany(p => (p.Content ?? new ContentType[0]).Select(x => new File(System.IO.Path.GetFileName(x.Include), x.Include)));
            var embeddedResources = itemGroups.SelectMany(p => (p.EmbeddedResource ?? new EmbeddedResourceType[0]).Select(x => new File(System.IO.Path.GetFileName(x.Include), x.Include)));
            var resources = itemGroups.SelectMany(p => (p.Resource ?? new ResourceType[0]).Select(x => new File(System.IO.Path.GetFileName(x.Include), x.Include)));
            var pages = itemGroups.SelectMany(p => (p.Page ?? new PageType[0]).Select(x => new File(System.IO.Path.GetFileName(x.Include), x.Include)));

            return compiles.Union(contents).Union(embeddedResources).Union(resources).Union(pages).ToList();
        }

        private static List<Referance> GetReferance(ProjectFileType projectFileType)
        {
            var itemGroups = (projectFileType.ItemGroup ?? new ItemGroupType[0]);
            var reference = itemGroups.SelectMany(p => (p.Reference ?? new ReferenceType[0]).Select(x => new Referance(x.Include, x.HintPath)));
            var projectReference = itemGroups.SelectMany(p => (p.ProjectReference ?? new ProjectReferenceType[0]).Select(x => new Referance(x.Name, x.Include)));

            return reference.Union(projectReference).ToList();
        }
    }
}