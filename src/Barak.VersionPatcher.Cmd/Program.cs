using Barak.VersionPatcher.Engine;
using Barak.VersionPatcher.Git;
using Barak.VersionPatcher.TFS;
using CLAP;

namespace Barak.VersionPatcher.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLineOptions commandLineOptions = new CommandLineOptions();
            Parser.Run<CommandLineOptions>(args, commandLineOptions);

            if (commandLineOptions.PatchInfo != null)
            {
                var scp = new SourceControlProvider(new ISourceControlFactory[]
                {
                    new TfsSourceControlFactory(commandLineOptions.PatchInfo.FileSystemPath),
                    new GitSourceControlFactory(commandLineOptions.PatchInfo.FileSystemPath,commandLineOptions.PatchInfo.SourceControlUrl,commandLineOptions.PatchInfo.Username,commandLineOptions.PatchInfo.Passowrd)
                });
                var versionPatcher = new Engine.VersionPatcher(scp);
                versionPatcher.Patch(commandLineOptions.PatchInfo);
            }
        }
    }
}
