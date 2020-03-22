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
            string RTMPURL = YoutubeAPI.RTMPURL;
            string YtPrivateKey = YoutubeAPI.YtPrivateKey;
            string image = @"C:\Users\shaan\Documents\GitHub\Vocal\Release\Test_Assets\images.png";
            string video = @"C:\Users\shaan\Documents\GitHub\Vocal\Release\Test_Assets\sample-video.mp4";
            string audio = @"C:\Users\shaan\Documents\GitHub\Vocal\Release\Test_Assets\Music\lmao.mp3";
            string output = @"C:\Users\shaan\Documents\GitHub\Vocal\Release\john.mp4";
            string outputOverlay = @"C:\Users\shaan\Documents\GitHub\Vocal\Release\johnOverlay.mp4";
            //CombineAudioVideo(video, audio, output);
            //OverlayImageVideo(image, output, outputOverlay);
            RTSPStream(RTMPURL, YtPrivateKey, outputOverlay);
        }

        private void CombineAudioVideo(string video, string audio, string output)
        {
            LaunchCommandLineApp($"-stream_loop -1 -i {video} -i {audio} -shortest -map 0:v:0 -map 1:a:0 -threads 0 -y {output}");
        }

        private void OverlayImageVideo(string image, string video, string output)
        {
            /*
            The scale2ref scales the first input (to the filter) to the size of the second. The input pad indexes 0, and 1 refer to the first and 2nd input to FFmpeg, as that count begins from zero.
            -map 0:a? - the ? tells FFmpeg to map the audio contingently i.e. if present. I have removed the amix since a) filters within a filter complex can't be contingent and b) there's only one input so there's nothing to 'mix
            */
            LaunchCommandLineApp($"-y -i {video} -i {image} -filter_complex \"[1][0]scale2ref[i][m];[m][i]overlay[v]\" -map \"[v]\" -map 0:a? -ac 2 -threads 0 {output}");
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
