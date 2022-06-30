using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace RpiWatcher
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            string tempDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp\RaspiWatcher";
            if (!File.Exists(tempDir)) Directory.CreateDirectory(tempDir);

            RaspiWatcher watcher = new RaspiWatcher(tempDir);
            watcher.OnUpdate += Watcher_OnUpdate;
            watcher.Start();

            // handle stuff
            Thread actionThread = new Thread(HandleActions);
            actionThread.Start();

            Process.GetCurrentProcess().WaitForExit();
        }

        private static void Watcher_OnUpdate(string message, string url)
        {
            Console.WriteLine(
                "\nMessage: " + message +
                "\nLink: " + url + "\n"
            );

            // ex: Stock Alert (US): RPi CM3+ - 1GB RAM, 8GB MMC is In Stock at Adafruit 58 units in stock.

            string title = message.Substring(0, message.IndexOf(':'));
            string description = message.Substring(message.IndexOf(':') + 1);

            new ToastContentBuilder()
                .AddText(title)
                .AddText(description)
                .AddArgument("link", url)
                .Show();
        }

        private static void HandleActions()
        {
            Console.WriteLine("Action Handler started!");

            // Listen to notification activation
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

                string url = args.Get("link");

                Uri uriResult;
                bool isValid = Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (!isValid)
                {
                    MessageBox.Show("Bad Url!\n" + url + "\n\nThis Url will not be opened!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // open in browser
                Process.Start(url);
            };
        }
    }
}