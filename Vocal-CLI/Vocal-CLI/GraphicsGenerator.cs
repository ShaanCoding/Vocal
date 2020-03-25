using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TagLib;

namespace Vocal_CLI
{
    class GraphicsGenerator
    {
        private const string BACKUP_ALBUM_COVER = @"C:\Users\shaan\Documents\GitHub\Vocal\Release\Test_Assets\Assets\ALBUM_COVER.jpg";
        private const string SEEK_ICON = @"C:\Users\shaan\Documents\GitHub\Vocal\Release\Test_Assets\Assets\nextSong.png";
        private const string OUTPUT_FILE = @"C:\Users\shaan\Documents\GitHub\Vocal\Release\Test_Assets\Assets\Overlay.png";

        public static void GenerateImageOverlay(string currentSong, string nextSong)
        {
            string SONG_NAME = "SONG NAME";
            string ARTIST_NAME = "ARTIST NAME";
            string NEXT_SONG_STRING = "";

            TagLib.File currentSongFile = TagLib.File.Create(currentSong);
            TagLib.File nextSongFile = TagLib.File.Create(nextSong);

            if(currentSongFile.Tag.Title != null)
            {
                SONG_NAME = currentSongFile.Tag.Title;
            }

            if(currentSongFile.Tag.AlbumArtists.Length > 0)
            {
                ARTIST_NAME = currentSongFile.Tag.AlbumArtists[0];
            }

            Image returnImage;
            if(currentSongFile.Tag.Pictures.Length != 0)
            {
                TagLib.IPicture albumPicture = currentSongFile.Tag.Pictures[0]; //filepath is audio file location
                MemoryStream memStream = new MemoryStream(albumPicture.Data.Data); //creates image in memory stream
                returnImage = Image.FromStream(memStream);
            }
            else
            {
                returnImage = null;
            }

            if (nextSongFile.Tag.AlbumArtists.Length > 0)
            {
                NEXT_SONG_STRING = nextSongFile.Tag.AlbumArtists[0] + " - ";
            }
            else
            {
                NEXT_SONG_STRING = "SONG ARTIST - ";
            }

            if (nextSongFile.Tag.Title != null)
            {
                NEXT_SONG_STRING += nextSongFile.Tag.Title;
            }
            else
            {
                NEXT_SONG_STRING += "SONG NAME";
            }

            ImageGenerator(returnImage, SONG_NAME, ARTIST_NAME, NEXT_SONG_STRING);
        }

        private static void ImageGenerator(Image albumCover, string songName, string artistName, string nextSongInfo)
        {
            Bitmap outputImage = new Bitmap(1080, 720);
            using (Graphics g = Graphics.FromImage(outputImage))
            {
                g.InterpolationMode = InterpolationMode.High;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Image albumCoverImage;
                if (albumCover == null)
                {
                     albumCoverImage = Image.FromFile(BACKUP_ALBUM_COVER);
                }
                else
                {
                    albumCoverImage = albumCover;
                }

                Image nextSongImage = Image.FromFile(SEEK_ICON);
                Brush whiteBrush = Brushes.White;
                Brush blackBrush = Brushes.Black;

                g.FillRectangle(whiteBrush, 30, 30, 132, 132);
                g.DrawImage(albumCoverImage, 32, 32, 128, 128);

                Font songNameFont = new Font("Arial", 16, FontStyle.Bold);
                Font songArtistFont = new Font("Arial", 12, FontStyle.Italic);
                Font nextSongFont = new Font("Arial", 12, FontStyle.Regular);

                g.DrawString(songName, songNameFont, whiteBrush, 192, 60);
                g.DrawString(artistName, songArtistFont, whiteBrush, 192, 85);
                g.DrawImage(nextSongImage, 192, 115, 20, 20);
                g.DrawString(nextSongInfo, nextSongFont, whiteBrush, 212, 118);
            }
            outputImage.Save(OUTPUT_FILE, ImageFormat.Png);
        }
    }
}
