using Barak.VersionPatcher.Engine;
using Barak.VersionPatcher.TFS;
using CLAP;

namespace Barak.VersionPatcher.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLiveOptions commandLiveOptions = new CommandLiveOptions();
            Parser.Run<CommandLiveOptions>(args, commandLiveOptions);

            if (commandLiveOptions.PatchInfo != null)
            {
                var scp = new SourceControlProvider(new ISourceControlFactory[] { new TfsSourceControlFactory(commandLiveOptions.PatchInfo.FileSystemPath) });
                var versionPatcher = new Engine.VersionPatcher(scp);
                versionPatcher.Patch(commandLiveOptions.PatchInfo);
            }
        }
    }
}
