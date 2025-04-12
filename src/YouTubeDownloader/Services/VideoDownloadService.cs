using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YouTubeDownloader.Services
{
    public class VideoDownloadService
    {
        private readonly HttpClient _httpClient;

        public VideoDownloadService()
        {
            _httpClient = new HttpClient();
        }

        public async Task DownloadVideo(string videoUrl, string path)
        {
            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(videoUrl);

            var title = video.Title;
            var duration = video.Duration;

            var filename = MakeValidFileName($"{title}");

            var af = $"{path}\\{filename}.audio.mp4";
            var vf = $"{path}\\{filename}.video.mp4";
            var target = $"{path}\\{filename}.mp4";

            if (File.Exists(target))
            {
                Console.WriteLine($"File already exists: {af}");
                return;
            }

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);

            var a = streamManifest.GetAudioOnlyStreams().Where(s => s.Container == Container.Mp4).GetWithHighestBitrate();
            var v = streamManifest.GetVideoOnlyStreams().Where(s => s.Container == Container.Mp4).Less1080();

            Console.OutputEncoding = System.Text.Encoding.Unicode;  
            
            Console.Write($"{filename}.audio:        ");
            await youtube.Videos.Streams.DownloadAsync(a, af, new Percent());
            Console.Write("\b\b\b   ");
            Console.WriteLine();
            Console.Write($"{filename}.video:        ");
            await youtube.Videos.Streams.DownloadAsync(v, vf, new Percent());
            Console.Write("\b\b\b   ");
            Console.WriteLine();
            Console.WriteLine("Combining video and audio...");
            Console.WriteLine();

            var ffmpegPath = @"c:\Program Files\ffmpeg\ffmpeg.exe";
            await Process.Start(new ProcessStartInfo
            {
                FileName = Path.GetFullPath(ffmpegPath),
                Arguments = $"-i \"{af}\" -i \"{vf}\" -loglevel error -c:v copy -c:a copy \"{target}\""
            }).WaitForExitAsync();

            File.Delete(af);
            File.Delete(vf);
            Console.WriteLine($"Video downloaded ({duration}) and saved as {target}");
        }

        public static string MakeValidFileName(string input)
        {
            string sanitized = Regex.Replace(input.RemoveDiacritics(), "[^0-9a-zA-Z,\\- '’‘]", " ");
            sanitized = Regex.Replace(sanitized, @"\s+", " "); // Replace multiple spaces with a single space

            return sanitized.Length > 255 ? sanitized.Substring(0, 255) : sanitized;
        }
    }

    public class Percent : IProgress<double>
    {
        public void Report(double value)
        {
            var spin = "◟◜◝◞";
            Console.Write("\b\b\b\b\b\b\b");

            var t = (int)Math.Floor((double)DateTime.Now.Millisecond / 1000 * 4);

            Console.Write(((int)(value * 100)).ToString("D3") + "% "+ $"{spin[t]} ");
        }
    }

    public static class Ext
    {
        public static string RemoveDiacritics(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var normalizedString = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static IVideoStreamInfo? Less1080(this IEnumerable<IVideoStreamInfo> streamInfos) =>
            streamInfos.Where(s => s.VideoQuality.MaxHeight <= 1080).OrderByDescending(s => s.VideoQuality).FirstOrDefault();
    }
}