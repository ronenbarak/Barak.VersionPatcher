using System;
using System.Globalization;
using Barak.VersionPatcher.Engine;
using CLAP;

namespace Barak.VersionPatcher.Cmd
{
    public enum VersionControl
    {
        Automatic,
        TFS,
        Git,
    }

    public class CommandLineOptions
    {
        public PatchInfo PatchInfo { get; private set; }

        [Verb(IsDefault = true, Description = "Increment the AssemblyVersion attribute if change is detected")]
        public void Patch([Description("version control type ")]
                            [DefaultValue(VersionControl.Automatic)]
                            [Aliases("vc")]
                            VersionControl versionControl,
                            [Description(@"version control path: ($/Barak/Dev/Main)")]
                            [Required]
                            string vcPath,
                            [Description(@"Url for the version control server")]
                            [Required]
                            string vcUrl,
                            [Description(@"revision id, default will be latest")]
                            [DefaultValue(null)]
                            string revision,
                            [Description(@"file system checkout root directory")]
                            [Required]
                            string fsPath,
                            [Description("user name to use with git")]
                            [DefaultValue(null)]
                            string username,
                            [Description("password to use with git")]
                            [DefaultValue(null)]
                            string password,
                            [Description("comment to check-in")]
                            [DefaultValue("VersionPathcer auto increment")]
                            string comment,
                            [Description("recursivly change projects depends on updated projects")]
                            [DefaultValue(true)]
                            bool recursive,
                            [Description("commit changes when done")]
                            [DefaultValue(true)]
                            bool commit,
                            [Description("what part of the version to incress")]
                            [DefaultValue(VersionPart.Build)]
                            VersionPart versionPart,
                            [Description("path to the project(.csproj) file, leave empty to search the folder for all projects")]
                            [DefaultValue(null)]
                            string[] projectFiles)
        {
            PatchInfo = new PatchInfo()
                        {
                            ForceVersionControlType = versionControl == VersionControl.Automatic ? null : versionControl.ToString(),
                            Comment = comment,
                            FileSystemPath = fsPath,
                            Revision = revision,
                            ProjectFiles = projectFiles,
                            SourceControlUrl = new Uri(vcUrl),
                            VersionControlPath = vcPath,
                            Commit = commit,
                            Recursive = recursive,
                            VersionPart = versionPart,
                            Username = username,
                            Passowrd = password,
                        };
        }

        [Error]
        public void OnError(ExceptionContext context)
        {
            Console.WriteLine("Error: " + context.Exception.Message);
            Console.WriteLine();
            Parser.Run<CommandLineOptions>( new string[]{} , this);
            context.ReThrow = false;
        }
        
        [Empty, Help()]
        public void Help(string help)
        {
            // this is an empty handler that prints
            // the automatic help string to the console.

            Console.WriteLine(help);
        }
    }
}