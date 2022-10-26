namespace UnitFramework
{
    public static class Version
    {
        private const string GameFrameworkVersionString = "2022.10.01";
        private static Version.IVersionHelper s_VersionHelper;

        /// <summary>获取游戏框架版本号。</summary>
        public static string GameFrameworkVersion
        {
            get
            {
                return "2022.10.01";
            }
        }

        /// <summary>获取游戏版本号。</summary>
        public static string GameVersion
        {
            get
            {
                return Version.s_VersionHelper == null ? string.Empty : Version.s_VersionHelper.GameVersion;
            }
        }

        /// <summary>获取内部游戏版本号。</summary>
        public static int InternalGameVersion
        {
            get
            {
                return Version.s_VersionHelper == null ? 0 : Version.s_VersionHelper.InternalGameVersion;
            }
        }

        /// <summary>设置版本号辅助器。</summary>
        /// <param name="versionHelper">要设置的版本号辅助器。</param>
        public static void SetVersionHelper(Version.IVersionHelper versionHelper)
        {
            Version.s_VersionHelper = versionHelper;
        }

        /// <summary>版本号辅助器接口。</summary>
        public interface IVersionHelper
        {
            /// <summary>获取游戏版本号。</summary>
            string GameVersion { get; }

            /// <summary>获取内部游戏版本号。</summary>
            int InternalGameVersion { get; }
        }
    }
}