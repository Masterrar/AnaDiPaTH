using System;
using System.Drawing;
using System.Windows.Media.Imaging;
namespace Model
{
    public class DifImageM
    {
        public DifImageM(Uri Bitmap_Uri)
        {
            this.Bitmap_Uri = Bitmap_Uri;

        }

        private Uri _bitMap_Uri;
        public Uri Bitmap_Uri
        {
            get { return _bitMap_Uri; }
            private set { _bitMap_Uri = value; }
        }


        private Bitmap _bitMap;
        public Bitmap Bitmap
        {
            get
            {
                if (_bitMap == null)
                    _bitMap = new Bitmap(Bitmap_Uri.AbsolutePath);

                return _bitMap;
            }


        }

        private BitmapImage _bitmap_Image;
        public BitmapImage Bitmap_Image
        {
            get
            {
                if (_bitmap_Image == null)
                    _bitmap_Image = new BitmapImage(Bitmap_Uri);
                return _bitmap_Image;
            }
        }
    }
}

