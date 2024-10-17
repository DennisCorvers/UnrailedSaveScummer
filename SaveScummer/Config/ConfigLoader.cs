using SaveScummerLib.Config;

namespace UnrailedSaveScummer.Config
{
    internal static class ConfigLoader
    {
        public static IConfiguration LoadConfig()
        {
#if DEBUG
            return new DebugConfig();
#else
            return new Config();
#endif
        }

        // Quick and dirty solution to have debug config without having to deal with config files.
        private class DebugConfig : IConfiguration
        {
            public string SaveFolderLocation => "D:\\Documents\\temp";

            public string FileExtension => "*.txt";

            public IReadOnlyCollection<string> IgnoredFiles => new[] { "filter.txt" };
        }
    }
}
