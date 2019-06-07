using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace NIR.Views
{
    using ViewModels;
    public enum DrawToolType : byte
    {
        None = 0,
        Pointer,
        Rectangle,
        Polyline,
        Delete
    }

    public struct MyPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
    /// <summary>
    /// Логика взаимодействия для MainView.xaml
    /// </summary>
    /// 
    public partial class MainView : Window
    {
        public byte[] ImageToByteArray(Bitmap bmp,out int stride )
        {
            var pxf = System.Drawing.Imaging.PixelFormat.Format24bppRgb;

            // Получаем данные картинки.
            var rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
            //Блокируем набор данных изображения в памяти
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);
            stride = bmpData.Stride;
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
        private System.Windows.Point? mousePos = null;
        //private Canvas canvas; // рабочая область для рисования
        private Shape lastShape = null; // текущая обрабатываемая фигура
        private Window window;
#pragma warning disable IDE1006 // Стили именования
        private Line lastShapeAsLine => this.lastShape as Line;
#pragma warning restore IDE1006 // Стили именования

        public MainView()
        {
            InitializeComponent();
            ImageBrush ib = new ImageBrush();
            var current_bimage = new BitmapImage(new Uri("(gray).jpg", UriKind.RelativeOrAbsolute));
            int widthX=current_bimage.PixelWidth;
            int heightY=current_bimage.PixelHeight;
#pragma warning disable IDE0018 // Объявление встроенной переменной
            int Stride;
#pragma warning restore IDE0018 // Объявление встроенной переменной
            byte[] bData = ImageToByteArray(new Bitmap("(gray).jpg"),out Stride);
            //System.Drawing.Image asdfa = new Bitmap("./(gray).jpg");
            
            var count_bData=bData.Count();
            //List<MyPoint> points = new List<MyPoint>();
            byte max=255;
            Stack<int> X = new Stack<int>();
            Stack<int> Y = new Stack<int>();
            int c_pixels = 0;
            int y;
            int x;
            for (y = 0; y < heightY; y++)
            {
                x = 0;
                for (x = 0; x < widthX; x=x+1)
                {
                    var midR = bData[y * Stride + x * 3]; 
                    var midG = bData[y * Stride + x * 3 + 1];
                    var midB = bData[y * Stride + x * 3 + 2];
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
            var midX = X.Sum() / c_pixels;
            var midY = Y.Sum() / c_pixels;
            
               
            ib.ImageSource = current_bimage;
            DrawCanvas.Height = ib.ImageSource.Height;
            DrawCanvas.Width = ib.ImageSource.Width;
            DrawCanvas.Background = ib;

            //canvas = DrawCanvas;
            window = this;

        }
        private void polylineToolMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.mousePos = Mouse.GetPosition(this.DrawCanvas);
        }

        /// <summary>
        /// Перемещение мыши инструмента рисования линии
        /// </summary>
        private void polylineToolMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && this.mousePos.HasValue)
            {
                System.Windows.Point pos = e.GetPosition(this.DrawCanvas);
                if (this.lastShapeAsLine == null)
                {
                    Line line = new Line
                    {
                        //line.Style = DrawTool.CalculatePolylineStyle(this.CurrentBrush, this.CurrentLineWidth);
                        StrokeThickness = 20,

                        X1 = this.mousePos.Value.X,
                        Y1 = this.mousePos.Value.Y,
                        Stroke = System.Windows.Media.Brushes.Blue,
                        Tag = ShapeTag.None
                    };
                    this.lastShape = line;
                    this.DrawCanvas.Children.Add(line);
                    
                }

                this.lastShapeAsLine.X2 = pos.X;
                this.lastShapeAsLine.Y2 = pos.Y;
            }
        }

        /// <summary>
        /// Нажатие кнопки мыши инструмента рисования линии
        /// </summary>
        private void polylineToolMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.mousePos = null;
                if (this.lastShapeAsLine != null)
                {
                    if ((lastShapeAsLine.ActualWidth * lastShapeAsLine.ActualWidth + lastShapeAsLine.ActualHeight * lastShapeAsLine.ActualHeight) <= 4)
                        this.DrawCanvas.Children.Remove(lastShapeAsLine);
                    this.lastShape = null;
                }
            }
        }
        public List<Shape> Selection { get; private set; }
        
        

        
        [FlagsAttribute]
        public enum ShapeTag : byte
        {
            None = 0,
            Select = 1,
            Deleting = 4,
        }
        public DrawToolType ToolType { get; private set; }
        public void SetToolType(DrawToolType type)
        {
            Mouse.RemoveMouseDownHandler(this.DrawCanvas, this.polylineToolMouseDown);
            Mouse.RemoveMouseUpHandler(this, this.polylineToolMouseUp);
            Mouse.RemoveMouseMoveHandler(this, this.polylineToolMouseMove);
            this.DrawCanvas.Cursor = null;
            this.lastShape = null;
            this.ToolType = type;

            Mouse.AddMouseDownHandler(this.DrawCanvas, this.polylineToolMouseDown);
                    Mouse.AddMouseUpHandler(this, this.polylineToolMouseUp);
                    Mouse.AddMouseMoveHandler(this, this.polylineToolMouseMove);
                    this.DrawCanvas.Cursor = Cursors.Cross;
            
        }
        private DrawTool selectedTool;
        private void cmd_Pointer(object sender, ExecutedRoutedEventArgs e)
        {
            this.SetToolType(DrawToolType.Pointer);
            //this.isGradientCB.IsEnabled = true;
            this.updateButtons();
        }
        public static RoutedCommand AddPolyline { get; set; }
        private void cmd_AddPolyline(object sender, RoutedEventArgs e)
        {
            if (this.ToolType == DrawToolType.Polyline)
                this.SetToolType(DrawToolType.Pointer);
            else
            {
                this.SetToolType(DrawToolType.Polyline);
                //this.isGradientCB.IsEnabled = false;
                //this.isGradientCB.IsChecked = false;
                //Brush b = this.getColorPopupBrush();
                //this.setColorPopupBrush(b);
                //this.selectedTool.CurrentBrush = b;
            }

            this.updateButtons();
        }
        private void updateButtons()
        {
            this.DoLine.IsChecked = this.ToolType == DrawToolType.Polyline;
        }

        
        
    }
    namespace NIR.Commands
    {
        public class Commands
        {
            /// <summary>
            /// Загрузить рисунок из файла
            /// </summary>
            public static RoutedCommand Open { get; set; }
            /// <summary>
            /// Сохранить рисунок в файл
            /// </summary>
            public static RoutedCommand Save { get; set; }
            /// <summary>
            /// Трансформировать/переместить/повернуть/выделить фигуру
            /// </summary>
            public static RoutedCommand Pointer { get; set; }
            /// <summary>
            /// Добавить линию
            /// </summary>
            public static RoutedCommand AddPolyline { get; set; }
            /// <summary>
            /// Добавить 4х угольник
            /// </summary>
            public static RoutedCommand AddRectangle { get; set; }
            /// <summary>
            /// Удалить фигуру
            /// </summary>
            public static RoutedCommand Delete { get; set; }

            static Commands()
            {
                Commands.Open = new RoutedCommand("Open", typeof(Commands));
                Commands.Save = new RoutedCommand("Save", typeof(Commands));
                Commands.Pointer = new RoutedCommand("Pointer", typeof(Commands));
                Commands.AddPolyline = new RoutedCommand("AddPolyline", typeof(Commands));
                Commands.AddRectangle = new RoutedCommand("AddRectangle", typeof(Commands));
                Commands.Delete = new RoutedCommand("Delete", typeof(Commands));
            }
        }
    }
}
