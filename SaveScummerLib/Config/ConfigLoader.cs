namespace SaveScummerLib.Config
{
    public static class ConfigLoader
    {
        // Maybe we turn this into something that actually loads the file from disk sometime...
        public static IConfiguration LoadConfiguration()
        {
            return new Config();
        }

        public static void SaveConfiguration()
        {
            throw new NotImplementedException();
        }
    }
}
