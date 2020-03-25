using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocal_CLI
{
    class FFMPEGStream
    {
        public FFMPEGStream()
        {

        }

        public static void Temp()
        {
            string RTMPURL = Program.RTMPURL;
            string YtPrivateKey = Program.YtPrivateKey;
            string image = @"C:\Users\shaan\Documents\GitHub\Vocal\Release\Test_Assets\images.png";
            string video = @"C:\Users\shaan\Documents\GitHub\Vocal\Release\Test_Assets\sample-video.mp4";
            string audio = @"C:\Users\shaan\Documents\GitHub\Vocal\Release\Test_Assets\Music\Arkansas_Traveler.mp3";
            string output = @"C:\Users\shaan\Documents\GitHub\Vocal\Release\john.mp4";
            string outputOverlay = @"C:\Users\shaan\Documents\GitHub\Vocal\Release\johnOverlay.mp4";

            RTSPStream(RTMPURL, YtPrivateKey, outputOverlay);
            //AudioVideoOverlayed(image, video, audio, outputOverlay);
        }

        private static void AudioVideoOverlayed(string image, string video, string audio, string output)
        {
            LaunchCommandLineApp($"-stream_loop -1 -i {video} -i {audio} -i {image} -filter_complex \"[0:v]scale=1280:720[v0:v]; [v0:v][2:v] overlay[videoOutput:v]\" -map [\"videoOutput\":v] -map 1:a:0 -shortest -ac 2 -threads 0 -r 24 -y {output}");
        }

        private static void RTSPStream(string RTMPURL, string YtPrivateKey, string input)
        {
            LaunchCommandLineApp($"-re -i {input} -c:v libx264 -b:v 2M -c:a copy -strict -2 -flags +global_header -bsf:a aac_adtstoasc -bufsize 2100k -f flv {RTMPURL}/{YtPrivateKey}");
        }

        private static void LaunchCommandLineApp(string input)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "ffmpeg.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = $"{input}";

            using(Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }
            Console.WriteLine($"FFMPEG Done: {input}");
        }
    }
}
