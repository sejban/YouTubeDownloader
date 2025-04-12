using System;
using System.Linq;
using System.Threading.Tasks;
using YouTubeDownloader.Services;
using YoutubeExplode;
using YoutubeExplode.Common;

namespace YouTubeDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var f = args.FirstOrDefault() ?? "";
            var p = args.Skip(1).FirstOrDefault() ?? "h:\\yt\\videos";
            var s = int.Parse(args.Skip(2).FirstOrDefault() ?? "1080");

            Console.WriteLine("Welcome to YouTube Downloader!");
            var videoDownloadService = new VideoDownloadService();

            //var go = false;
            if (f.Contains("playlist"))
            {
                var youtube = new YoutubeClient();

                await foreach (var video in youtube.Playlists.GetVideosAsync(f))
                {
                    // if (video.Url == "")
                    //     go = true;

                    // if (!go) continue;

                    Console.WriteLine($"Downloading {video.Url}");
                    await videoDownloadService.DownloadVideo(video.Url, p, s);
                    await Task.Delay(10000);
                }

                return;
            }


            await videoDownloadService.DownloadVideo(f, p, s);
        }
    }
}