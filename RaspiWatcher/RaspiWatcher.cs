using CodeHollow.FeedReader;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace RpiWatcher
{
    internal class RaspiWatcher
    {
        private string feedUrl = "https://rpilocator.com/feed/";
        private string tempDir = "";
        private string checksumFile = "checksum.txt";

        private Feed feed;
        private Timer updater;
        private string checksum = "";

        public delegate void OnPiDropped(string message, string url);

        public event OnPiDropped OnUpdate;

        public RaspiWatcher(string tempDir, int updateInterval = 5)
        {
            this.tempDir = tempDir;

            // load last checksum if possible
            if (File.Exists(tempDir + Path.DirectorySeparatorChar + this.checksumFile))
            {
                // first line is timestamp, seconf is checksum
                string[] lines = File.ReadAllLines(tempDir + Path.DirectorySeparatorChar + this.checksumFile);

                if (lines.Length < 2)
                {
                    Console.WriteLine("Your checksum file has an invalid format! Removing it...");
                    File.Delete(tempDir + Path.DirectorySeparatorChar + this.checksumFile);
                }
                else
                {
                    string timestamp = lines[0];
                    string check = lines[1];

                    this.checksum = check;

                    Console.WriteLine(
                        "Successfully loaded checksum file:\n" +
                        " - Timestamp: " + timestamp + "\n" +
                        " - Checksum: " + check + "\n" +
                        " > Ok!"
                    );
                }
            }

            this.updater = new Timer((1000 * 60) * updateInterval);
            this.updater.Elapsed += (s, e) => this.DoUpdate().Wait();
            this.updater.AutoReset = true;
        }

        private async Task DoUpdate()
        {
            Console.WriteLine("Updating RSS feed...");

            this.feed = await FeedReader.ReadAsync(this.feedUrl);

            if (this.feed.Items.Count < 1)
            {
                Console.WriteLine(" > Invalid Format. Ignoring update.");
                return;
            }

            // get newest item and build checksum string
            FeedItem updateItem = this.feed.Items[0];

            string itemChecksum = CreateChecksum(updateItem.Title + "|" + updateItem.Link);
            if (this.checksum == itemChecksum)
            {
                Console.WriteLine(" > Checksum matches. Ignoring update.");
                return;
            }

            // update checksum file and send event
            string timestamp = DateTime.Now.ToString();
            File.WriteAllText(
                tempDir + Path.DirectorySeparatorChar + this.checksumFile,
                timestamp + "\n" + itemChecksum
            );

            Console.WriteLine(
                " - Timestamp: " + timestamp + "\n" +
                " - Checksum: " + itemChecksum + "\n" +
                " > Updated! -> Ok!"
            );

            // fire
            this.OnUpdate?.Invoke(updateItem.Title, updateItem.Link);
        }

        private string CreateChecksum(string source)
        {
            string hash;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                hash = BitConverter.ToString(
                  md5.ComputeHash(Encoding.UTF8.GetBytes(source))
                );
            }
            return hash;
        }

        public void Start()
        {
            // update now
            DoUpdate().Wait();

            // finally, activate updater
            this.updater.Enabled = true;
        }
    }
}