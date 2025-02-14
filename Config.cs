using Newtonsoft.Json;

namespace CloudUpdater
{
    public class Config
    {
        public string FtpServer { get; set; }
        public string FtpUsername { get; set; }
        public string FtpPassword { get; set; }
        public string RemoteUpdatePath { get; set; }
        public string LocalAppPath { get; set; }
        public string CurrentVersionFile { get; set; }
        public string ExecutableName { get; set; }
        public string LocalUpdatePath { get; set; }

        public static Config Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Configuration file not found: {filePath}");
            }

            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<Config>(json);
        }
    }
}
