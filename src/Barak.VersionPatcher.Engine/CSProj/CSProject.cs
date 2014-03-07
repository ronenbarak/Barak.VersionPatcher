using System.Collections.Generic;

namespace Barak.VersionPatcher.Engine.CSProj
{
    public class File
    {
        public string Name { get; private set; }
        public string Path { get; private set; }

        public File(string name,string path)
        {
            Name = name;
            Path = path;
        }
    }

    public class Referance
    {
        public string Name { get; private set; }
        public string Path { get; private set; }

        public Referance(string name,string path)
        {
            Name = name;
            Path = path;
        }
    }
    public class CSProject
    {
        public string Name { get; private set; }
        public IEnumerable<File> Files { get; private set; }
        public IEnumerable<Referance> Referances { get; private set; }
        public string FullPath { get; set; }

        public CSProject(string name,IEnumerable<File> files,IEnumerable<Referance> referances)
        {
            Name = name;
            Files = files;
            Referances = referances;
        }
    }
}
