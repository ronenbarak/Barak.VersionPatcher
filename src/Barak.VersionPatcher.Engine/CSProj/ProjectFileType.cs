namespace Barak.VersionPatcher.Engine.CSProj
{
    public class BaseElementType
    {
        public object[] Items { get; set; }
        public string Value { get; set; }
    }

    public class ProjectFileType : BaseElementType
    {
        public string ToolsVersion { get; set; }
        public string DefaultTargets { get; set; }
        public string xmlns { get; set; }
        public ImportType[] Import { get; set; }
        public PropertyGroupType[] PropertyGroup { get; set; }
        public ItemGroupType[] ItemGroup { get; set; }
        public ChooseType[] Choose { get; set; }
        public UsingTaskType UsingTask { get; set; }
        public TargetType Target { get; set; }
    }

    public class ImportType : BaseElementType
    {
        public string Project { get; set; }
        public string Condition { get; set; }
    }

    public class PropertyGroupType : BaseElementType
    {
        public ConfigurationType Configuration { get; set; }
        public PlatformType Platform { get; set; }
        public string ProjectGuid { get; set; }
        public string OutputType { get; set; }
        public string AppDesignerFolder { get; set; }
        public string RootNamespace { get; set; }
        public string AssemblyName { get; set; }
        public string TargetFrameworkVersion { get; set; }
        public string FileAlignment { get; set; }
        public string SccProjectName { get; set; }
        public string SccLocalPath { get; set; }
        public string SccAuxPath { get; set; }
        public string SccProvider { get; set; }
        public string TargetFrameworkProfile { get; set; }
        public string Condition { get; set; }
        public string DebugSymbols { get; set; }
        public string DebugType { get; set; }
        public string Optimize { get; set; }
        public string OutputPath { get; set; }
        public string DefineConstants { get; set; }
        public string ErrorReport { get; set; }
        public string WarningLevel { get; set; }
        public string ProjectTypeGuids { get; set; }
        public string PlatformTarget { get; set; }
        public string ApplicationIcon { get; set; }
        public string PostBuildEvent { get; set; }
        public VisualStudioVersionType VisualStudioVersion { get; set; }
        public VSToolsPathType VSToolsPath { get; set; }
        public string ReferencePath { get; set; }
        public string IsCodedUITest { get; set; }
        public string TestProjectType { get; set; }
        public string Prefer32Bit { get; set; }
        public string ProductVersion { get; set; }
        public string SchemaVersion { get; set; }
        public string PublishUrl { get; set; }
        public string Install { get; set; }
        public string InstallFrom { get; set; }
        public string UpdateEnabled { get; set; }
        public string UpdateMode { get; set; }
        public string UpdateInterval { get; set; }
        public string UpdateIntervalUnits { get; set; }
        public string UpdatePeriodically { get; set; }
        public string UpdateRequired { get; set; }
        public string MapFileExtensions { get; set; }
        public string ApplicationRevision { get; set; }
        public string ApplicationVersion { get; set; }
        public string IsWebBootstrapper { get; set; }
        public string UseApplicationTrust { get; set; }
        public string BootstrapperEnabled { get; set; }
        public string StartupObject { get; set; }
        public string Utf8Output { get; set; }
        public string ExpressionBlendVersion { get; set; }
        public string NoWin32Manifest { get; set; }
        public string AllowUnsafeBlocks { get; set; }
    }

    public class ConfigurationType : BaseElementType
    {
        public string Condition { get; set; }
    }

    public class PlatformType : BaseElementType
    {
        public string Condition { get; set; }
    }

    public class ItemGroupType : BaseElementType
    {
        public ReferenceType[] Reference { get; set; }
        public CompileType[] Compile { get; set; }
        public ProjectReferenceType[] ProjectReference { get; set; }
        public ApplicationDefinitionType ApplicationDefinition { get; set; }
        public EmbeddedResourceType[] EmbeddedResource { get; set; }
        public NoneType[] None { get; set; }
        public AppDesignerType AppDesigner { get; set; }
        public PageType[] Page { get; set; }
        public ResourceType[] Resource { get; set; }
        public ContentType[] Content { get; set; }
        public BootstrapperPackageType[] BootstrapperPackage { get; set; }
        public FolderType Folder { get; set; }
        public CodeAnalysisDependentAssemblyPathsType CodeAnalysisDependentAssemblyPaths { get; set; }
        public WCFMetadataType WCFMetadata { get; set; }
    }

    public class ReferenceType : BaseElementType
    {
        public string Include { get; set; }
        public string HintPath { get; set; }
        public string RequiredTargetFramework { get; set; }
        public string SpecificVersion { get; set; }
        public string Private { get; set; }
        public string EmbedInteropTypes { get; set; }
    }

    public class CompileType : BaseElementType
    {
        public string Include { get; set; }
        public string DependentUpon { get; set; }
        public string SubType { get; set; }
        public string AutoGen { get; set; }
        public string DesignTime { get; set; }
        public string DesignTimeSharedInput { get; set; }
        public string Link { get; set; }
    }

    public class ProjectReferenceType : BaseElementType
    {
        public string Include { get; set; }
        public string Project { get; set; }
        public string Name { get; set; }
        public string Private { get; set; }
    }

    public class ApplicationDefinitionType : BaseElementType
    {
        public string Include { get; set; }
        public string Generator { get; set; }
        public string SubType { get; set; }
    }

    public class EmbeddedResourceType : BaseElementType
    {
        public string Include { get; set; }
        public string Generator { get; set; }
        public string LastGenOutput { get; set; }
        public string DependentUpon { get; set; }
        public string SubType { get; set; }
    }

    public class NoneType : BaseElementType
    {
        public string Include { get; set; }
        public string Generator { get; set; }
        public string LastGenOutput { get; set; }
    }

    public class AppDesignerType : BaseElementType
    {
        public string Include { get; set; }
    }

    public class PageType : BaseElementType
    {
        public string Include { get; set; }
        public string Generator { get; set; }
        public string SubType { get; set; }
        public string Link { get; set; }
    }

    public class ResourceType : BaseElementType
    {
        public string Include { get; set; }
    }

    public class VisualStudioVersionType : BaseElementType
    {
        public string Condition { get; set; }
    }

    public class VSToolsPathType : BaseElementType
    {
        public string Condition { get; set; }
    }

    public class ChooseType : BaseElementType
    {
        public WhenType When { get; set; }
        public OtherwiseType Otherwise { get; set; }
    }

    public class WhenType : BaseElementType
    {
        public string Condition { get; set; }
        public ItemGroupType ItemGroup { get; set; }
    }

    public class OtherwiseType : BaseElementType
    {
        public ItemGroupType ItemGroup { get; set; }
    }

    public class ContentType : BaseElementType
    {
        public string Include { get; set; }
        public string Link { get; set; }
        public string CopyToOutputDirectory { get; set; }
    }

    public class BootstrapperPackageType : BaseElementType
    {
        public string Include { get; set; }
        public string Visible { get; set; }
        public string ProductName { get; set; }
        public string Install { get; set; }
    }

    public class FolderType : BaseElementType
    {
        public string Include { get; set; }
    }

    public class CodeAnalysisDependentAssemblyPathsType : BaseElementType
    {
        public string Condition { get; set; }
        public string Include { get; set; }
        public string Visible { get; set; }
    }

    public class WCFMetadataType : BaseElementType
    {
        public string Include { get; set; }
    }

    public class UsingTaskType : BaseElementType
    {
        public string TaskName { get; set; }
        public string AssemblyFile { get; set; }
    }

    public class TargetType : BaseElementType
    {
        public string Name { get; set; }
        //public string Costura.EmbedTask { get; set; }
    }







}
