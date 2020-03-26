using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Vocal
{
    class FFMPEGStream
    {
        private static string IMAGE_OVERLAY = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)?.Replace("file:\\", "") + "\\Assets\\Overlay.png";
        private static string VIDEO_SOURCE = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)?.Replace("file:\\", "") + "\\InputAssets\\videoSource.gif";
        private static string AUDIO_FOLDER = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)?.Replace("file:\\", "") + "\\InputAssets\\Music";

        private static string RTMPURL = "";
        private static string YOUTUBE_PRIVATE_KEY = "";

        public static void Start()
        {


            RTMPURL = Program.RTMPURL;
            YOUTUBE_PRIVATE_KEY = Program.YtPrivateKey;

            string currentSongURL = GetRandomSong();
            string nextSongURL = GetRandomSong();


            for (int i = 0; i < 3; i++)
            {
                //END GOAL IS TO STREAM ALL
                GraphicsGenerator.GenerateImageOverlay(currentSongURL, nextSongURL);
                AudioVideoOverlayed(IMAGE_OVERLAY, VIDEO_SOURCE, currentSongURL);
                //RTSPStream(RTMPURL, YOUTUBE_PRIVATE_KEY, outputOverlay);

                //At end we need to bump up the queue
                currentSongURL = nextSongURL;
                nextSongURL = GetRandomSong();
                //DEBUG
                Console.WriteLine("EXPORTED: SONG");
            }
        }

        private static string GetRandomSong()
        {
            Random rand = new Random();
            string[] soundFiles = Directory.GetFiles(AUDIO_FOLDER, "*.mp3");
            string randomSong = soundFiles[rand.Next(0, soundFiles.Length)];
            return randomSong;
        }

        private static void AudioVideoOverlayed(string image, string video, string audio)
        {
            LaunchCommandLineApp($"-stream_loop -1 -i {video} -i {audio} -i {image} -filter_complex \"[0:v]scale=1280:720[v0:v]; [v0:v][2:v] overlay[videoOutput:v]\" -map [\"videoOutput\":v] -map 1:a:0 -shortest -ac 2 -threads 0 -r 24 -vcodec libx264 -preset ultrafast -f flv \"{RTMPURL}/{YOUTUBE_PRIVATE_KEY}\"");
        }

        private static void LaunchCommandLineApp(string input)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "ffmpeg.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = $"{input}";

            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }
            Console.WriteLine($"SONG Done: {input}");
        }
    }
}
