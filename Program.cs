using System.Net;

namespace CloudUpdater
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var config = Config.Load("updater_config.json"); // Ensure the JSON file is present in the output directory
                string localVersionPath = Path.Combine(config.LocalAppPath, config.CurrentVersionFile);
                string remoteVersionUrl = $"ftp://stelios_updater@{config.FtpServer}/{config.RemoteUpdatePath}/{config.CurrentVersionFile}";
                //ftp://stelios_updater@iad1-shared-b8-15.dreamhost.com/cloud_backup_core/updates/version.txt  

                string localVersion = File.Exists(localVersionPath) ? File.ReadAllText(localVersionPath).Trim() : "0.0.0";
                string remoteVersion = DownloadVersionFile(remoteVersionUrl, config.FtpUsername, config.FtpPassword);


                Console.WriteLine($"Local Version: {localVersion}");
                Console.WriteLine($"Remote Version: {remoteVersion}");

                if (string.Compare(remoteVersion, localVersion) > 0)
                {
                    Console.WriteLine($"Update available: {remoteVersion}");
                    DownloadUpdate(config);
                }
                else
                {
                    Console.WriteLine("No updates found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Downloads the version.txt file from the FTP server and returns the version string.
        /// </summary>
        static string DownloadVersionFile(string url, string user, string pass)
        {
            try
            {
                using WebClient client = new WebClient();
                client.Credentials = new NetworkCredential(user, pass);
                return client.DownloadString(url).Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading version file: {ex.Message}");
                return "0.0.0"; // If there's an error, assume no update is available
            }
        }

        /// <summary>
        /// Downloads and applies the update if a new version is found.
        /// </summary>
        static void DownloadUpdate(Config config)
        {
            try
            {
                string updateZip = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cloud_Backup_Core", "update", "temp", "update.zip");
                string updateUrl = $"ftp://stelios_updater@{config.FtpServer}/{config.RemoteUpdatePath}/update.zip";

                //using WebClient client = new WebClient();
                //client.Credentials = new NetworkCredential(config.FtpUsername, config.FtpPassword);
                //client.DownloadFile(updateUrl, updateZip);

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(updateUrl);
                request.Credentials = new NetworkCredential(config.FtpUsername, config.FtpPassword);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                using (Stream ftpStream = request.GetResponse().GetResponseStream())
                using (Stream fileStream = File.Create(updateZip))
                {
                    ftpStream.CopyTo(fileStream);
                }

                Console.WriteLine("Download complete. Extracting update...");

                System.IO.Compression.ZipFile.ExtractToDirectory(updateZip, config.LocalAppPath, true);
                File.Delete(updateZip);

                Console.WriteLine("Update applied. Restarting application...");
                RestartApplication(config);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during update: {ex.Message}");
            }
        }

        /// <summary>
        /// Restarts the main application after applying updates.
        /// </summary>
        static void RestartApplication(Config config)
        {
            try
            {
                var process = System.Diagnostics.Process.GetProcessesByName(
                    Path.GetFileNameWithoutExtension(config.ExecutableName)).FirstOrDefault();

                process?.Kill(); // Kill the old process if running
                System.Threading.Thread.Sleep(2000); // Wait for app to close

                System.Diagnostics.Process.Start(Path.Combine(config.LocalAppPath, config.ExecutableName));
                Console.WriteLine("Application restarted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restarting application: {ex.Message}");
            }
        }
    }
}
