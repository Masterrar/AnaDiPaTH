﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан инструментальным средством
//     В случае повторного создания кода изменения, внесенные в этот файл, будут потеряны.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ViewModel.DescriptionOfTask
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
    using DescriptionOfTask.Tool;
    using Tool.Shape;
	public class DescriptionVM
	{
        private ToolsVM _toolsVM;
        public ToolsVM ToolsVM
        {
            get
            { return _toolsVM; }
            private set
            { _toolsVM = value; }
        }

        private DifImageVM _difImageVM;
        public DifImageVM DifImageVM
        {
            get
            { return _difImageVM; }
            set
            { _difImageVM = value; }
        }
        public DescriptionVM()
        {

        }
        public DescriptionVM(Uri BitMap_Uri)
        {
            DifImageVM = new DifImageVM(BitMap_Uri);
        }
        public void AddLineInTheDI()
        {
            DifImageVM.LineList.Add(ToolsVM.Line(DifImageVM.MidPoint));
            
        }
        public void AddEllInTheDI()
        {
            DifImageVM.EllList.Add(ToolsVM.Ellipse(DifImageVM.MidPoint));
        }

        private System.Windows.Point? mousePos = null;
        //private Canvas canvas; // рабочая область для рисования
        private Shape lastShape = null; // текущая обрабатываемая фигура
        private Window window;
        private Line lastShapeAsLine
        {
            get
            {
                return this.lastShape as Line;
            }
        }
        public MainView()
        {
            InitializeComponent();
            ImageBrush ib = new ImageBrush();
            var current_bimage = new BitmapImage(new Uri("(gray).jpg", UriKind.RelativeOrAbsolute));
            int widthX=current_bimage.PixelWidth;
            int heightY=current_bimage.PixelHeight;
            int Stride;
            byte[] bData = imageToByteArray(new Bitmap("(gray).jpg"),out Stride);
            //System.Drawing.Image asdfa = new Bitmap("./(gray).jpg");
            
            var count_bData=bData.Count();
            //List<MyPoint> points = new List<MyPoint>();
            byte max=255;
            Stack<int> X = new Stack<int>();
            Stack<int> Y = new Stack<int>();
            int ara;
            int c_pixels = 0;
            int y;
            int x;
            int i = -3;
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
            
               
            //ib.ImageSource = current_bimage;
            ////DrawCanvas.Height = ib.ImageSource.Height;
            ////DrawCanvas.Width = ib.ImageSource.Width;
            //DrawCanvas.Background = ib;
            this.Width = 1024;
            this.Height = 768;
            //canvas = DrawCanvas;
            //window = this;

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
                    Line line = new Line();
                    //line.Style = DrawTool.CalculatePolylineStyle(this.CurrentBrush, this.CurrentLineWidth);
                    line.StrokeThickness = 20;
                    
                    line.X1=this.mousePos.Value.X;
                    line.Y1 = this.mousePos.Value.Y;
                    line.Stroke = System.Windows.Media.Brushes.Blue;
                    line.Tag = ShapeTag.None;
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
        
	}
}

