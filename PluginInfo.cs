namespace liquidclient
{
    public class PluginInfo
    {
        public const string GUID = "Liquid.Client";
        public const string Name = "Liquid.Client";
        public const string Description = "liquid on top creds to imundtrust and cdev";

        public const bool BetaBuild = false;

#if BETA
        public const string Version = "1.2.8 Beta Testing";
#else
        public const string Version = "1.2.7";
#endif

        public static string BaseDirectory = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gorilla Tag\\Liquid.Client";
    }
}                                  
