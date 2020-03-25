using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TagLib;

namespace Vocal_CLI
{
    class GraphicsGenerator
    {
        //Generate graphics image 128 * 720 p
        //Must have the album icon if not a default one
        //Must have music data info read from meta data
        string ALBUM_COVER = @"C:\Users\shaan\Documents\GitHub\Vocal\Release\Test_Assets\Assets\ALBUM_COVER.jpg";
        string SONG_NAME = "SONG NAME";
        string ARTIST_NAME = "ARTIST NAME";
        string NEXT_SONG_NAME = "NEXT SONG ARTIST - NEXT SONG NAME";

        public GraphicsGenerator()
        {

        }

        public void GetMP3Info(string mp3FileString)
        {
            TagLib.File mp3File = TagLib.File.Create(mp3FileString);
            Console.WriteLine(mp3File.Tag.Title);
            Console.WriteLine(mp3File.Tag.AlbumArtists[0]);
            Console.WriteLine(mp3File.Tag.Album);
        }

        public void GenerateImageOverlay()
        {
            Bitmap outputImage = new Bitmap(1080, 720);
            using (Graphics g = Graphics.FromImage(outputImage))
            {
                g.InterpolationMode = InterpolationMode.High;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Image albumCoverImage = Image.FromFile(@"C:\Users\shaan\Documents\GitHub\Vocal\Release\Test_Assets\Assets\ALBUM_COVER.jpg");
                Image nextSongImage = Image.FromFile(@"C:\Users\shaan\Documents\GitHub\Vocal\Release\Test_Assets\Assets\nextSong.png");
                Brush whiteBrush = Brushes.White;
                Brush blackBrush = Brushes.Black;
                g.FillRectangle(whiteBrush, 30, 30, 132, 132);
                //g.FillRectangle(blackBrush, 32, 32, 128, 128);
                g.DrawImage(albumCoverImage, 32, 32, 128, 128);

                Font songNameFont = new Font("Arial", 16, FontStyle.Bold);
                Font songArtistFont = new Font("Arial", 12, FontStyle.Italic);
                Font nextSongFont = new Font("Arial", 12, FontStyle.Regular);
                //30 px out
                //g.DrawString(topText, memeFont, textBrush, new System.Drawing.Rectangle(0, Convert.ToInt32((outputImage.Height / 100) * 5), outputImage.Width - 20, Convert.ToInt32(MeasureString(topText, memeFont, outputImage.Width - 20).Width)), sf);
                g.DrawString(SONG_NAME, songNameFont, whiteBrush, 192, 60);
                g.DrawString(ARTIST_NAME, songArtistFont, whiteBrush, 192, 85);
                g.DrawImage(nextSongImage, 192, 115, 20, 20);
                g.DrawString(NEXT_SONG_NAME, nextSongFont, whiteBrush, 212, 118);
            }
            outputImage.Save(@"C:\Users\shaan\Documents\GitHub\Vocal\Release\Test_Assets\Assets\Johno.png", ImageFormat.Png);
        }
    }
}
