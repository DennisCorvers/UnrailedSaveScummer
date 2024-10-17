using SaveScummerLib.Config;

namespace UnrailedSaveScummer.Config
{
    internal class Config : IConfiguration
    {
        private readonly string m_saveFolderLocation = "%LOCALAPPDATA%\\Daedalic Entertainment GmbH\\Unrailed\\GameState\\AllPlayers\\SaveGames";
        private readonly string m_fileExtension = "*.sav";

        public string SaveFolderLocation
        {
            get
            {
                return Environment.ExpandEnvironmentVariables(m_saveFolderLocation);
            }
        }

        public string FileExtension
            => m_fileExtension;

        public Config()
        {

        }
    }
}
