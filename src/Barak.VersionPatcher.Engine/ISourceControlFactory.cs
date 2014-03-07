namespace Barak.VersionPatcher.Engine
{
    public interface ISourceControlFactory
    {
        string Type { get; }
        bool CanHandle(string path);
        ISourceControl Create(string path);
    }
}