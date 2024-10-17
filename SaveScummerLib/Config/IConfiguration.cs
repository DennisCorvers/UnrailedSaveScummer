namespace SaveScummerLib.Config
{
    public interface IConfiguration
    {
        public string SaveFolderLocation { get; }

        public string FileExtension { get; }

        public IReadOnlyCollection<string> IgnoredFiles { get; }
    }
}
