using System.Drawing;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ViewModel.DescriptionOfTask
{
    using Model;
    using Tool.Shape;
    
	public class DifImageVM
	{
        private DifImageM _difImageM;

        public DifImageVM(Uri Bitmap_Uri)
        {
            this._difImageM = new DifImageM(Bitmap_Uri);

        }

        
        public Uri Bitmap_Uri
        {
            get { return _difImageM.Bitmap_Uri; }
           
        }

        
        
        public Bitmap Bitmap
        {
            get
            {
                return _difImageM.Bitmap;
            }
        }
        public BitmapImage BMP_Img
        {
            get{return _difImageM.Bitmap_Image;}
        }

        private int _stride;
        public int Stride
        {
            get { return _stride; }
            private set { _stride = value; }
        }

        private Point _midPoint;
        public Point MidPoint
        {
            get
            {
                if (_midPoint == null)
                {
                    _midPoint = getMidPoint();
                }
                return _midPoint;
                
            }
        }
        private Point getMidPoint()
        {
            int widthX=BMP_Img.PixelWidth;
            int heightY=BMP_Img.PixelHeight;
                     byte max=255;
            Stack<int> X = new Stack<int>();
            Stack<int> Y = new Stack<int>();

            int c_pixels = 0;
            int y;
            int x;

            for (y = 0; y < heightY; y++)
            {
                x = 0;
                for (x = 0; x < widthX; x = x + 1)
                {
                    var midR = BitmapAsBytes[y * Stride + x * 3];
                    var midG = BitmapAsBytes[y * Stride + x * 3 + 1];
                    var midB = BitmapAsBytes[y * Stride + x * 3 + 2];
                    var mid = (30 * midR + 59 * midG + 11 * midB) / 100;
                    //Console.WriteLine(y + " " + x + " " + mid + bData[y * x] + bData[y * x + 1] +  bData[y * x + 2]);
                    if (mid == max)
                    {


                        Y.Push(y);
                        X.Push(x);
                        c_pixels++;
                    }
                }

            }
            return new Point( X.Sum() / c_pixels,Y.Sum() / c_pixels);
            
        }
        private byte[] _bitmapAsBytes;
        public byte[] BitmapAsBytes
        {
            get
            {
                if (_bitmapAsBytes == null)
                {
                    _bitmapAsBytes = imageToByteArray(this.Bitmap);
                }
                return _bitmapAsBytes;
            }
        }
#pragma warning disable IDE1006 // Стили именования
        private byte[] imageToByteArray(Bitmap bmp)
#pragma warning restore IDE1006 // Стили именования
        {
            var pxf = System.Drawing.Imaging.PixelFormat.Format24bppRgb;

            // Получаем данные картинки.
            var rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
            //Блокируем набор данных изображения в памяти
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);
            Stride = bmpData.Stride;
            // Получаем адрес первой линии.
            IntPtr ptr = bmpData.Scan0;

            // Задаём массив из Byte и помещаем в него надор данных.
            // int numBytes = bmp.Width * bmp.Height * 3; 
            //На 3 умножаем - поскольку RGB цвет кодируется 3-мя байтами
            //Либо используем вместо Width - Stride
            int numBytes = bmpData.Stride * bmp.Height;
            int widthBytes = bmpData.Stride;
            byte[] rgbValues = new byte[numBytes];

            // Копируем значения в массив.
            Marshal.Copy(ptr, rgbValues, 0, numBytes);

            return rgbValues;
        }

        
        public BitmapImage Bitmap_Image
        {
            get
            {
                return _difImageM.Bitmap_Image;
            }
        }

        private List<LineVM> _lineList = new List<LineVM>();
        private List<EllipseVM> _ellList = new List<EllipseVM>();

        public List<LineVM> LineList
        {
            get
            { return _lineList; }
            private set
            { _lineList = value; }

        }
        public List<EllipseVM> EllList
        {
            get
            { return _ellList; }
            private set
            { _ellList = value; }
        }
	}
}

