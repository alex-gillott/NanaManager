using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace NanaManagerAPI
{
    public static class Extensions
    {
        public static BitmapImage ToBitmapImage( this Bitmap Bitmap, BitmapCacheOption CacheOption ) {
            using MemoryStream memory = new MemoryStream(); //Initializes a MemoryStream, which will be disposed of after usage

            Bitmap.Save( memory, ImageFormat.Png ); //Saves the bitmap data to memory
            memory.Position = 0; //Moves the pointer back to the beginning to read the data
            BitmapImage bitmapImage = new BitmapImage(); //Initializing our new BitmapImage
            bitmapImage.BeginInit(); //Sets the new BitmapImage to being able to have properties changed
            bitmapImage.StreamSource = memory; //Load data from the MemoryStream
            bitmapImage.CacheOption = CacheOption; //Set to OnLoad for fastest. Set to OnDemand for lower resource consumption
            bitmapImage.EndInit(); //Finalises the initialisation for the bitmap image
            return bitmapImage;
        }
    }
}