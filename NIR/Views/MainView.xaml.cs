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
    using global::NIR.ShapesExtension;
    using global::NIR.Tools;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using ViewModel.DescriptionOfTask;
    using ViewModels;
    using Brush = System.Windows.Media.Brush;
    using Point = System.Windows.Point;

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
        private DifImageVM difImage { get; set; }
        /// <summary>
        /// Событие возникает когда выделюятся новые фигуры/пропадает выделение
        /// </summary>
        public event EventHandler SelectionChange;

        
        private System.Windows.Point? mousePos = null;
        //private Canvas DrawCanvas; // рабочая область для рисования
        private Shape lastShape = null; // текущая обрабатываемая фигура
        private Window window;
#pragma warning disable IDE1006 // Стили именования
        private Line lastShapeAsLine => this.lastShape as Line;

#pragma warning restore IDE1006 // Стили именования
        private System.Windows.Media.Brush CurrentBrush = null;

        private ItemsControl dotsControl; // точки трансформации
        private DrawToolDots dots; // источник данных точек трансформации
        private DrawToolDot selectedDot = null;
        private Border canvasBorder; // рамки рабочей области рисования (для позиционирования)

        public double CurrentLineWidth
        {
            get; set;
        } = 5;
        public MainView()
        {
            InitializeComponent();
            ImageBrush ib = new ImageBrush();
            difImage = new DifImageVM(new Uri("C:\\_Projects\\AnaDiPaTH\\NIR\\Resources\\(gray).JPG"));
            
            ib.ImageSource = difImage.Bitmap_Image;
            var h = DrawCanvas.Height;
            var w = DrawCanvas.Width;

            resizeIfSoLarge(out h,out w, ib.ImageSource.Height, ib.ImageSource.Width);


            DrawCanvas.Height = h;
            DrawCanvas.Width = w;
            DrawCanvas.Background = ib;

            //DrawCanvas = DrawCanvas;
            window = this;

            this.dotsControl = (ItemsControl)XamlReader.Parse(MainView.DotsItemsControlXaml);
            this.dots = new DrawToolDots();
            this.dotsControl.Tag = this.dots;
            this.dotsControl.ItemsSource = this.dots.Dots;

            this.DrawCanvas.Children.Add(this.dotsControl);
            this.Selection = new List<Shape>();

            this.SelectionChange += selectedTool_SelectionChange;
            this.updateButtons();
            this.canvasBorder = this.border;
        }
        void selectedTool_SelectionChange(object sender, EventArgs e)
        {
            this.updateButtons();
            if (this.Selection.Count == 1)
            {
                Shape s = this.Selection.First();
                Brush b = this.CurrentBrush;
                
            }
        }
        private void resizeIfSoLarge(   out double resultHeight , out double resultWidth,
                                        double currentHeight    , double currentWidth)
        {
            var h = currentHeight;
            var w = currentWidth;
            //while (h > 1050.0 || w > 1300.0)
            //{
            //    h /= 1.5;
            //    w /= 1.5;
            //}
            resultHeight = h;
            resultWidth = w;
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
                //Берем координаты мыши
                System.Windows.Point pos = e.GetPosition(this.DrawCanvas);
                // Если не над чем работать, создаем над чем работать.
                if (this.lastShapeAsLine == null)
                {
                    Line line = new Line
                    {
                        Style = DrawTool.CalculatePolylineStyle(this.CurrentBrush, 5),
                        //StrokeThickness = 5,

                        X1 = pos.X,
                        Y1 = pos.Y,
                        X2 = difImage.MidPoint.X + (difImage.MidPoint.X - this.mousePos.Value.X),
                        Y2 = difImage.MidPoint.Y + (difImage.MidPoint.Y - this.mousePos.Value.Y),

                        Stroke = System.Windows.Media.Brushes.Blue,
                        Tag = ShapeTag.None
                    };
                    this.lastShape = line;
                    this.DrawCanvas.Children.Add(line);
                    
                }
                // Производим работу, а тобишь присваиваем координаты точка линии, чтобы она двигалась по кругу.
                this.lastShapeAsLine.X1 = pos.X;
                this.lastShapeAsLine.Y1 = pos.Y;

                this.lastShapeAsLine.X2 = difImage.MidPoint.X + (difImage.MidPoint.X - pos.X);
                this.lastShapeAsLine.Y2 = difImage.MidPoint.Y + (difImage.MidPoint.Y - pos.Y);
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
            switch (this.ToolType)
            {
                case DrawToolType.Pointer:
                    Mouse.RemoveMouseDownHandler(this.window, this.toolMouseDown);
                    Mouse.RemoveMouseUpHandler(this.window, this.toolMouseUp);
                    Mouse.RemoveMouseMoveHandler(this.window, this.toolMouseMove);
                    Keyboard.RemoveKeyDownHandler(this.window, this.toolKeyDown);
                    Keyboard.RemoveKeyUpHandler(this.window, this.toolKeyUp);
                    Mouse.RemoveMouseDownHandler(this.dotsControl, this.dotsControl_MouseDown);
                    Mouse.RemoveMouseUpHandler(this.dotsControl, this.dotsControl_MouseUp);
                    Mouse.RemoveMouseMoveHandler(this.dotsControl, this.dotsControl_MouseMove);
                    this.selectShapes(null);
                    break;
                case DrawToolType.Polyline:
                    Mouse.RemoveMouseDownHandler(this.DrawCanvas, this.polylineToolMouseDown);
                    Mouse.RemoveMouseUpHandler(this.window, this.polylineToolMouseUp);
                    Mouse.RemoveMouseMoveHandler(this.window, this.polylineToolMouseMove);
                    this.DrawCanvas.Cursor = null;
                    this.lastShape = null;
                    break;
            }
            this.ToolType = type;

            switch (this.ToolType)
            {
                case DrawToolType.Pointer:
                    Mouse.AddMouseDownHandler(this.dotsControl, this.dotsControl_MouseDown);
                    Mouse.AddMouseUpHandler(this.dotsControl, this.dotsControl_MouseUp);
                    Mouse.AddMouseDownHandler(this.window, this.toolMouseDown);
                    Mouse.AddMouseUpHandler(this.window, this.toolMouseUp);
                    Mouse.AddMouseMoveHandler(this.window, this.toolMouseMove);
                    Mouse.AddMouseMoveHandler(this.dotsControl, this.dotsControl_MouseMove);
                    Keyboard.AddKeyDownHandler(this.window, this.toolKeyDown);
                    Keyboard.AddKeyUpHandler(this.window, this.toolKeyUp);
                    break;

                case DrawToolType.Polyline:
                    Mouse.AddMouseDownHandler(this.DrawCanvas, this.polylineToolMouseDown);
                    Mouse.AddMouseUpHandler(this.window, this.polylineToolMouseUp);
                    Mouse.AddMouseMoveHandler(this.window, this.polylineToolMouseMove);
                    this.DrawCanvas.Cursor = Cursors.Cross;
                    break;
            }

            }
        private DrawTool selectedTool;
        private void cmd_Pointer(object sender, RoutedEventArgs e)
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
     
                
                //this.CurrentBrush = b;
            }

            this.updateButtons();
        }
        private void updateButtons()
        {
            this.DoLine.IsChecked = this.ToolType == DrawToolType.Polyline;
        }
        /// <summary>
        /// Удаляет выбранные  фигуры
        /// </summary>
        public void DeleteSelected()
        {
            foreach (var s in this.Selection)
                this.DrawCanvas.Children.Remove(s);

            this.selectShapes(null);
        }

        /// <summary>
        /// Возращает элемент над которым находится мышь
        /// </summary>
        private UIElement GetCanvasHoveredElement()
        {
            var elems = this.DrawCanvas.Children.OfType<UIElement>().Where(e => e.Visibility == Visibility.Visible && e.IsMouseOver);
            return elems.DefaultIfEmpty(null).First();
        }

        /// <summary>
        /// Нажатие кнопки мыши инструмента удаления
        /// </summary>
        private void deleteToolMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (this.lastShape != null && this.lastShape.IsMouseOver)
                {
                    this.DrawCanvas.Children.Remove(this.lastShape);
                }
                else
                {
                    var s = this.GetCanvasHoveredElement() as Shape;
                    if (s != null)
                    {
                        this.DrawCanvas.Children.Remove(s);
                    }
                }
                this.lastShape = null;
            }
        }

        /// <summary>
        /// Движение кнопки мыши инструмента удаления
        /// </summary>
        private void deleteToolMouseMove(object sender, MouseEventArgs e)
        {
            if (this.lastShape != null)
            {
                if (!this.lastShape.IsMouseOver)
                {
                    this.lastShape.SetDeletingStyle(false);
                    this.lastShape = null;

                    if (!this.DrawCanvas.IsMouseOver)
                        return;
                }
                else
                    return;
            }
            if (this.DrawCanvas.IsMouseOver)
            {
                var hovershape = this.GetCanvasHoveredElement() as Shape;
                if (hovershape != null)
                    hovershape.SetDeletingStyle(true);

                this.lastShape = hovershape;
            }
        }
        /// <summary>
        /// Выделение одной фигуры
        /// </summary>
        private void selectShape(Shape s)
        {
            this.Selection.ClearShapes();
            if (s != null)
            {
                this.Selection.AddShape(s);
                if (s is Path)
                {
                    Style style = s.Style;
                    var sett = style.Setters.OfType<Setter>().Where(ss => ss.Property == Path.FillProperty);
                    if (sett != null && sett.Count() > 0)
                        this.CurrentBrush = (sett.First().Value as Brush);
                }
                else if (s is Polyline)
                {
                    Style style = s.Style;
                    var sett = style.Setters.OfType<Setter>().Where(ss => ss.Property == Polyline.StrokeProperty);
                    if (sett != null && sett.Count() > 0)
                        this.CurrentBrush = (sett.First().Value as Brush);

                    sett = style.Setters.OfType<Setter>().Where(ss => ss.Property == Polyline.StrokeThicknessProperty);
                    if (sett != null && sett.Count() > 0)
                        this.CurrentLineWidth = (double)sett.First().Value;
                }
            }
            this.dots.SetSource(s);
            if (this.SelectionChange != null)
                this.SelectionChange(this, new EventArgs());
        }
        /// <summary>
        /// Нажатие кнопки мыши на точке трансформации
        /// </summary>
        private void dotsControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var ht = VisualTreeHelper.HitTest(this.dotsControl, Mouse.GetPosition(this.dotsControl));
            if (ht != null)
            {
                this.selectedDot = (ht.VisualHit as System.Windows.Shapes.Rectangle).Tag as DrawToolDot;
                if (this.selectedDot.Parent.Source is Path && this.selectedDot.RectPoint == RectPoints.Center)
                {
                    Mouse.OverrideCursor = Cursors.ScrollWE;
                    return;
                }

            }
            Mouse.OverrideCursor = null;
        }
        /// <summary>
        /// Нажатие кнопки Shift
        /// </summary>
        private void toolKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.LeftShift || e.Key == Key.RightShift) && !e.IsRepeat)
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    this.mousePos = Mouse.GetPosition(this.window);
                    this.border.Width = 0;
                    this.border.Height = 0;

                    this.border.SetValue(Canvas.LeftProperty, this.mousePos.Value.X);
                    this.border.SetValue(Canvas.TopProperty, this.mousePos.Value.Y);

                    this.border.Visibility = Visibility.Visible;
                    this.dotsControl.Visibility = Visibility.Hidden;

                    this.selectShapes(null);
                }
            }
        }

        /// <summary>
        /// Отпускание кнопки Shift
        /// </summary>
        private void toolKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.LeftShift || e.Key == Key.RightShift) && !e.IsRepeat)
            {
                this.border.Visibility = Visibility.Hidden;
                this.mousePos = null;
            }
        }

        /// <summary>
        /// Нажатие кнопки мыши инструмента трансформации
        /// </summary>
        private void toolMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.mousePos = Mouse.GetPosition(this.window);
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) // надатие мыши при нажатой кнопке shift
                {
                    this.border.SetValue(Canvas.LeftProperty, this.mousePos.Value.X);
                    this.border.SetValue(Canvas.TopProperty, this.mousePos.Value.Y);
                    this.border.Width = 0;
                    this.border.Height = 0;

                    this.border.Visibility = Visibility.Visible;

                    this.selectShapes(null);
                    Mouse.OverrideCursor = null;
                }
                else if (e.ClickCount > 1 && this.Selection.Count == 1) // двойной щелчок по линии
                {
                    Polyline line = this.Selection.First() as Polyline;
                    if (line != null)
                    {
                        Point topleft = e.GetPosition(this.DrawCanvas);
                        topleft.Offset(-2, -2); // немного расширяем область для более комфортного нажатия на линию
                        Point bottomright = new Point(topleft.X + 5, topleft.Y + 5);
                        for (int i = line.Points.Count - 1; i >= 1; i--)
                        {
                            if (ShapesHelper.CheckLineLineIntersection(topleft, bottomright, line.Points[i - 1], line.Points[i]))
                            {
                                line.Points.Insert(i, topleft);
                                this.dots.SetSource(line);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    var s = this.GetCanvasHoveredElement();
                    if (s != null) // выделение 1 фигуры
                    {
                        if (s != this.dotsControl) // Если не точка транфсормации
                        {
                            if (!this.Selection.Contains(s))
                                this.selectShape(s as Shape);
                            this.selectedDot = null;
                            Mouse.OverrideCursor = Cursors.SizeAll;
                        }
                    }
                    else // Щелчок по пустой области
                    {
                        this.selectShapes(null);
                        Mouse.OverrideCursor = Cursors.ScrollAll;
                    }
                }
            }
        }

        /// <summary>
        /// Перемещение кнопки мыши инструмента трансформации
        /// </summary>
        private void toolMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && this.mousePos != null)
            {
                Point lastPos = Mouse.GetPosition(this.window);
                if (lastPos == this.mousePos.Value)
                    return;

                if (this.border.Visibility == Visibility.Visible) // Выделение
                {
                    this.toolSelection(lastPos);
                }
                else if (this.selectedDot != null) // Трансформация
                {
                    if (this.selectedDot.Parent.Source is Polyline) // перемещеине узлов линий
                    {
                        this.toolPolylineTransform(lastPos);
                    }
                    else if (this.selectedDot.Parent.Source is Path) //трансформация 4х угольника
                    {
                        Path path = this.selectedDot.Parent.Source as Path;
                        if (this.selectedDot.RectPoint == RectPoints.Center) // поворот
                            this.toolRotateRect(lastPos, path);
                        else // изменение размера
                            this.toolResizeRect(lastPos, path);
                    }
                    // обновляем положеие точек после трансформации + сохраняем выделенной такую же точку что и в старой коллекции
                    int olddot = this.selectedDot.DotID;
                    this.dots.SetSource(this.selectedDot.Parent.Source);
                    if (this.selectedDot != this.dots.Dots[olddot])
                        this.selectedDot = this.dots.Dots[olddot];

                }
                else if (this.Selection.Count > 0) // Перемещение фигур
                {
                    this.toolMoveShapes(lastPos);
                }
                else if (this.DrawCanvas.IsMouseOver) // перемещение рабочей области рисования
                {
                    this.toolMoveCanvas(lastPos);
                }
            }
        }

        /// <summary>
        ///  Перемещение рабочей области рисования  при движении мыши
        /// </summary>
        private void toolMoveCanvas(Point lastPos)
        {
            Point borderPos = new Point(Canvas.GetLeft(this.canvasBorder), Canvas.GetTop(this.canvasBorder));

            double left = borderPos.X + lastPos.X - this.mousePos.Value.X;
            double top = borderPos.Y + lastPos.Y - this.mousePos.Value.Y;

            if (left + this.DrawCanvas.ActualWidth < 100)
                left = -this.DrawCanvas.ActualWidth + 100;
            else if (left > this.workspace.ActualWidth - 100)
                left = this.workspace.ActualWidth - 100;

            if (top + this.DrawCanvas.ActualHeight < 100)
                top = -this.DrawCanvas.ActualHeight + 100;
            else if (top > this.workspace.ActualHeight - 100)
                top = this.workspace.ActualHeight - 100;

            this.mousePos = lastPos;

            if (left != Canvas.GetLeft(this.canvasBorder))
                Canvas.SetLeft(this.canvasBorder, left);
            if (top != Canvas.GetTop(this.canvasBorder))
                Canvas.SetTop(this.canvasBorder, top);
        }

        /// <summary>
        ///  Перемещение фигур  при движении мыши
        /// </summary>
        private void toolMoveShapes(Point lastPos)
        {
            Point move = new Point(lastPos.X - this.mousePos.Value.X, lastPos.Y - this.mousePos.Value.Y);
            //some linq magic
            this.Selection.ForEach(s =>
            {
                if (s is Path)
                {
                    PolyLineSegment line = (s as Path).GetSegment();
                    for (int i = 0; i < line.Points.Count; i++)
                    {
                        Point pt = line.Points[i];
                        pt.Offset(move.X, move.Y);
                        if (i > 0)
                            line.Points[i] = pt;
                        else
                            (s as Path).SetTopLeft(pt);
                    }
                }
                else if (s is Polyline)
                {
                    Polyline line = (s as Polyline);
                    for (int i = 0; i < line.Points.Count; i++)
                    {
                        Point pt = line.Points[i];
                        pt.Offset(move.X, move.Y);
                        line.Points[i] = pt;
                    }
                }
            });

            if (this.Selection.Count() == 1)
                this.dots.SetSource(this.Selection.First());
            this.mousePos = lastPos;
        }

        /// <summary>
        ///  Изменение размеров  4х угольника  при движении мыши
        /// </summary>
        private void toolResizeRect(Point lastPos, Path path)
        {
            RotateTransform rt = null;
            Point move = new Point(this.mousePos.Value.X - lastPos.X, this.mousePos.Value.Y - lastPos.Y);

            Vector v = new Vector(1, 0);
            Point p1 = this.dots.Dots[RectPoints.TopLeft].Point;
            Point p2 = this.dots.Dots[RectPoints.TopRight].Point;
            p2.Offset(-p1.X, -p1.Y);
            Vector rotated = new Vector(p2.X, p2.Y);
            double angle = Vector.AngleBetween(v, rotated); // угол между нормальным вектором и вектором по верхней стороне 4х угольника
            if (angle != 0) // если 4х угольник повернут, то разворачиваем его назад и приводим к номральному виду
            {
                rt = new RotateTransform(-angle, this.dots.Dots[RectPoints.Center].X, this.dots.Dots[RectPoints.Center].Y);
                path.Transform(rt);

                angle *= Math.PI / 180; // в радианы

                // Если перемещаем одну из точек в вершине 4х угольника, то координаты перемещения также разворачиваем и приводим к нормальному виду
                if (this.selectedDot.RectPoint == RectPoints.TopLeft || this.selectedDot.RectPoint == RectPoints.TopRight || this.selectedDot.RectPoint == RectPoints.BottomLeft || this.selectedDot.RectPoint == RectPoints.BottomRight)
                {
                    Point rp = rt.Transform(lastPos);
                    Point rpos = rt.Transform(this.mousePos.Value);
                    move = new Point(rpos.X - rp.X, rpos.Y - rp.Y);
                }
            }
            var rect = path.GetSegment();
            var rectPoints = rect.Points.ToArray();
            Point size = new Point(rectPoints[1].X - rectPoints[0].X, rectPoints[2].Y - rectPoints[1].Y);

            ScaleTransform st = new ScaleTransform();
            // задаем параметры трансформации в зависимости от используемой точки
            switch (this.selectedDot.RectPoint)
            {
                case RectPoints.Left:

                    move.X = move.X * Math.Cos(angle) + move.Y * Math.Sin(angle); // проекция на ось Y разницы координат
                    if (size.X == 0) //  защита от схлапывания
                    {
                        path.SetTopLeft(new Point(rectPoints[0].X - move.X, rectPoints[0].Y));
                        rect.Points[3] = new Point(rectPoints[3].X - move.X, rectPoints[3].Y);
                        st = null;
                    }
                    else
                    {
                        st.CenterX = rectPoints[1].X;
                        st.CenterY = (rectPoints[2].Y - rectPoints[1].Y) / 2;
                        st.ScaleX = 1 + move.X / size.X;
                    }
                    break;
                case RectPoints.Right:
                    move.X = move.X * Math.Cos(angle) + move.Y * Math.Sin(angle); // проекция на ось Y разницы координат
                    if (size.X == 0) //  защита от схлапывания
                    {
                        rect.Points[1] = new Point(rectPoints[1].X - move.X, rectPoints[1].Y);
                        rect.Points[2] = new Point(rectPoints[2].X - move.X, rectPoints[2].Y);
                        st = null;
                    }
                    else
                    {
                        st.CenterX = rectPoints[0].X;
                        st.CenterY = (rectPoints[3].Y - rectPoints[0].Y) / 2;
                        st.ScaleX = 1 + move.X / -size.X;
                    }
                    break;
                case RectPoints.Top:
                    move.Y = move.X * Math.Sin(-angle) + move.Y * Math.Cos(-angle); // проекция на ось Х разницы координат
                    if (size.Y == 0) //  защита от схлапывания
                    {
                        path.SetTopLeft(new Point(rectPoints[0].X, rectPoints[0].Y - move.Y));
                        rect.Points[1] = new Point(rectPoints[1].X, rectPoints[1].Y - move.Y);
                        st = null;
                    }
                    else
                    {
                        st.CenterX = (rectPoints[3].X - rectPoints[2].X) / 2;
                        st.CenterY = rectPoints[2].Y;
                        st.ScaleY = 1 + move.Y / size.Y;
                    }
                    break;
                case RectPoints.Bottom:
                    move.Y = move.X * Math.Sin(-angle) + move.Y * Math.Cos(-angle); // проекция на ось Х разницы координат
                    if (size.Y == 0) //  защита от схлапывания
                    {
                        rect.Points[2] = new Point(rectPoints[2].X, rectPoints[2].Y - move.Y);
                        rect.Points[3] = new Point(rectPoints[3].X, rectPoints[3].Y - move.Y);
                        st = null;
                    }
                    else
                    {
                        st.CenterX = (rectPoints[1].X - rectPoints[0].X) / 2;
                        st.CenterY = rectPoints[0].Y;
                        st.ScaleY = 1 + move.Y / -size.Y;
                    }
                    break;
                case RectPoints.TopLeft:
                    if (size.Y == 0) //  защита от схлапывания
                    {
                        path.SetTopLeft(new Point(rectPoints[0].X, rectPoints[0].Y - move.Y));
                        rect.Points[1] = new Point(rectPoints[1].X, rectPoints[1].Y - move.Y);
                    }
                    else
                        st.ScaleY = 1 + move.Y / size.Y;
                    if (size.X == 0) //  защита от схлапывания
                    {
                        path.SetTopLeft(new Point(rectPoints[0].X - move.X, rect.Points[0].Y));
                        rect.Points[3] = new Point(rectPoints[3].X - move.X, rectPoints[3].Y);
                    }
                    else
                        st.ScaleX = 1 + move.X / size.X;
                    if (st.ScaleX != 0 || st.ScaleY != 0)
                    {
                        st.CenterX = rectPoints[2].X;
                        st.CenterY = rectPoints[2].Y;
                    }
                    else
                        st = null;
                    break;
                case RectPoints.TopRight:
                    if (size.Y == 0) //  защита от схлапывания
                    {
                        path.SetTopLeft(new Point(rectPoints[0].X, rectPoints[0].Y - move.Y));
                        rect.Points[1] = new Point(rectPoints[1].X, rectPoints[1].Y - move.Y);
                    }
                    else
                        st.ScaleY = 1 + move.Y / size.Y;
                    if (size.X == 0) //  защита от схлапывания
                    {
                        rect.Points[1] = new Point(rectPoints[1].X - move.X, rect.Points[1].Y);
                        rect.Points[2] = new Point(rectPoints[2].X - move.X, rectPoints[2].Y);
                    }
                    else
                        st.ScaleX = 1 + move.X / -size.X;
                    if (st.ScaleX != 0 || st.ScaleY != 0)
                    {
                        st.CenterX = rectPoints[3].X;
                        st.CenterY = rectPoints[3].Y;
                    }
                    else
                        st = null;
                    break;
                case RectPoints.BottomRight:
                    if (size.Y == 0) //  защита от схлапывания
                    {
                        rect.Points[2] = new Point(rectPoints[2].X, rectPoints[2].Y - move.Y);
                        rect.Points[3] = new Point(rectPoints[3].X, rectPoints[3].Y - move.Y);
                    }
                    else
                        st.ScaleY = 1 + move.Y / -size.Y;
                    if (size.X == 0) //  защита от схлапывания
                    {
                        rect.Points[1] = new Point(rectPoints[1].X - move.X, rectPoints[1].Y);
                        rect.Points[2] = new Point(rectPoints[2].X - move.X, rect.Points[2].Y);
                    }
                    else
                        st.ScaleX = 1 + move.X / -size.X;
                    if (st.ScaleX != 0 || st.ScaleY != 0)
                    {
                        st.CenterX = rectPoints[0].X;
                        st.CenterY = rectPoints[0].Y;
                    }
                    else
                        st = null;
                    break;
                case RectPoints.BottomLeft:
                    if (size.Y == 0) //  защита от схлапывания
                    {
                        rect.Points[2] = new Point(rectPoints[2].X, rectPoints[2].Y - move.Y);
                        rect.Points[3] = new Point(rectPoints[3].X, rectPoints[3].Y - move.Y);
                    }
                    else
                        st.ScaleY = 1 + move.Y / -size.Y;
                    if (size.X == 0) //  защита от схлапывания
                    {
                        path.SetTopLeft(new Point(rectPoints[0].X - move.X, rectPoints[0].Y));
                        rect.Points[3] = new Point(rectPoints[3].X - move.X, rect.Points[3].Y);
                    }
                    else
                        st.ScaleX = 1 + move.X / size.X;
                    if (st.ScaleX != 0 || st.ScaleY != 0)
                    {
                        st.CenterX = rectPoints[1].X;
                        st.CenterY = rectPoints[1].Y;
                    }
                    else
                        st = null;
                    break;
            }

            path.Transform(st);
            if (rt != null)
                path.Transform(rt.Inverse);
            this.mousePos = lastPos;
        }

        /// <summary>
        ///  Поворот 4х угольника  при движении мыши
        /// </summary>
        private void toolRotateRect(Point lastPos, Path path)
        {
            RotateTransform rt = new RotateTransform();
            rt.CenterX = this.selectedDot.X;
            rt.CenterY = this.selectedDot.Y;
            rt.Angle = lastPos.X - this.mousePos.Value.X;
            path.Transform(rt);

            this.mousePos = lastPos;
        }

        /// <summary>
        ///  Перемещение точек трансформации линии  при движении мыши
        /// </summary>
        private void toolPolylineTransform(Point lastPos)
        {
            Point move = new Point(lastPos.X - this.mousePos.Value.X, lastPos.Y - this.mousePos.Value.Y);

            Polyline line = this.selectedDot.Parent.Source as Polyline;

            Point pt = line.Points[this.selectedDot.DotID];
            pt.Offset(move.X, move.Y);
            line.Points[this.selectedDot.DotID] = pt;
            this.mousePos = lastPos;
        }

        /// <summary>
        ///  Изменение рамки выделения при движении мыши
        /// </summary>
        private void toolSelection(Point lastPos)
        {
            Rect selectionRect = new Rect(Math.Min(lastPos.X, this.mousePos.Value.X), Math.Min(lastPos.Y, this.mousePos.Value.Y), Math.Abs(lastPos.X - this.mousePos.Value.X), Math.Abs(lastPos.Y - this.mousePos.Value.Y));
            Rect canvasRect = new Rect(Canvas.GetLeft(this.canvasBorder), Canvas.GetTop(this.canvasBorder), this.DrawCanvas.ActualWidth, this.DrawCanvas.ActualHeight);

            if (lastPos.X < this.mousePos.Value.X)
                this.border.SetValue(Canvas.LeftProperty, selectionRect.X);
            if (lastPos.Y < this.mousePos.Value.Y)
                this.border.SetValue(Canvas.TopProperty, selectionRect.Y);
            this.border.Width = selectionRect.Width;
            this.border.Height = selectionRect.Height;

            selectionRect.Intersect(canvasRect);

            if (!selectionRect.IsEmpty)
            {
                selectionRect.Offset(-canvasRect.X, -canvasRect.Y);
                var ss = this.DrawCanvas.Children.OfType<Shape>().Where(s => s.IntersectWith(selectionRect));
                this.selectShapes(ss);
            }
            else
                this.selectShapes(null);
        }

        /// <summary>
        /// Отпускание кнопки мыши инструмента трансформации
        /// </summary>
        private void toolMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (this.border.Visibility == Visibility.Visible)
                    this.border.Visibility = Visibility.Hidden;

                this.mousePos = null;
                this.selectedDot = null;
                Mouse.OverrideCursor = null;
            }
        }
        /// <summary>
        /// Перемещеине указателя мыши над 4х угольником точки трансформации
        /// </summary>
        private void dotsControl_MouseMove(object sender, MouseEventArgs e)
        {
            // грязный хак, ItemsControl не прорисовывает свои элементы во время обработки MouseEvent. Событие MouseUp перехватывается удаленым(не прорисованным?) елементом и не возникает в коде
            if (this.selectedDot != null && e.LeftButton == MouseButtonState.Released)
                this.dotsControl_MouseUp(sender, null);
        }

        /// <summary>
        /// Отпускание кнопки мыши на точке трансформации
        /// </summary>
        private void dotsControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if ((e == null || e.ChangedButton == MouseButton.Left) && this.selectedDot != null)
            {
                var intersectedDots = this.dots.Dots
                        .Where(d => Math.Abs(d.DotID - this.selectedDot.DotID) == 1 && DrawToolDot.IsDotsIntersect(this.selectedDot, d));
                if (this.selectedDot.Parent.Source is Polyline)
                {
                    if (intersectedDots.Count() > 0)
                    {
                        Polyline line = this.selectedDot.Parent.Source as Polyline;
                        if (intersectedDots.Count() == line.Points.Count - 1)
                        {
                            this.DrawCanvas.Children.Remove(line);
                            this.selectShape(null);
                        }
                        else
                        {
                            int i = 0;
                            foreach (var dot in intersectedDots.OrderBy(d => d.DotID))
                            {
                                line.Points.RemoveAt(dot.DotID + i);
                                i--;
                            }
                            this.dots.SetSource(line);
                        }
                    }
                }
                else if (this.dots.Dots.Count(d => d.Point == this.selectedDot.Point) == 9) // удаляем фигуру если все 9 точек СОВПАДАЮТ
                {
                    this.DrawCanvas.Children.Remove(this.selectedDot.Parent.Source);
                    this.selectShape(null);
                }
                this.mousePos = null;
                this.selectedDot = null;
            }
            Mouse.OverrideCursor = null;
        }


        protected static readonly String HatchBrushXaml =
                @"<VisualBrush  
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
            TileMode='Tile' Viewport='0,0,10,10'  ViewportUnits='Absolute' Viewbox='0,0,10,10'  ViewboxUnits='Absolute'>
            <VisualBrush.Visual>
                <Canvas>
                    <Rectangle Fill='White' Width='10' Height='10' />
                    <Path Stroke='Red' Data='M 0 0 l 10 10' />
                    <Path Stroke='Red' Data='M 0 10 l 10 -10' />
                </Canvas>
            </VisualBrush.Visual>
        </VisualBrush>";

            protected static readonly String BorderControlXaml =
                @"<Border 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                
                BorderBrush='#CCFF0000' BorderThickness='1' Visibility='Hidden'/>";

            protected static readonly String DotsItemsControlXaml =
                @"<ItemsControl 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:tool='clr-namespace:NIR.Tools;assembly=NIR'
                Canvas.ZIndex='200' >
            <ItemsControl.Resources>
                <tool:PointsToDotsCoorsConverter x:Key='CoorsConvert'></tool:PointsToDotsCoorsConverter>
                <Style x:Key='PolylineDotStyle' TargetType='Rectangle'>
                    <Setter Property='Fill' Value='Transparent'></Setter>
                    <Setter Property='Stroke' Value='Red'></Setter>
                    <Style.Triggers>
                        <Trigger Property='IsMouseOver' Value='True'>
                            <Setter Property='Fill' Value='#50FF0000'></Setter>
                        </Trigger>
                    </Style.Triggers>
                </Style>
            </ItemsControl.Resources>
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <Canvas/>
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
            <ItemsControl.ItemContainerStyle>
                <Style TargetType='ContentPresenter'>
                    <Setter Property='Canvas.Left'>
                        <Setter.Value>
                            <MultiBinding Converter='{StaticResource CoorsConvert}'>
                                <Binding Path='X' />
                                <Binding Path='Tag.DotSize' RelativeSource='{RelativeSource Mode=FindAncestor, AncestorType=ItemsControl}' />
                            </MultiBinding>
                        </Setter.Value>
                    </Setter>
                    <Setter Property='Canvas.Top'>
                        <Setter.Value>
                            <MultiBinding Converter='{StaticResource CoorsConvert}'>
                                <Binding Path='Y' />
                                <Binding Path='Tag.DotSize' RelativeSource='{RelativeSource Mode=FindAncestor, AncestorType=ItemsControl}' />
                            </MultiBinding>
                        </Setter.Value>
                    </Setter>
                </Style>
            </ItemsControl.ItemContainerStyle>
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <Rectangle
                                Tag='{Binding .}'
                                Width='{Binding Path=Tag.DotSize, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=ItemsControl}}'
                                Height='{Binding Path=Tag.DotSize, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=ItemsControl}}'
                                Style='{StaticResource PolylineDotStyle}'
                                Canvas.ZIndex='500'>
                    </Rectangle>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>";


        /// <summary>
        /// Выделение нескольких фигур
        /// </summary>
        private void selectShapes(IEnumerable<Shape> shapes)
        {
            if (shapes != null && shapes.Count() == 1)
                this.selectShape(shapes.First());
            else
            {
                this.Selection.ClearShapes();
                this.Selection.AddShapes(shapes);
                this.dots.SetSource(null);
                if (this.SelectionChange != null)
                    this.SelectionChange(this, new EventArgs());
            }
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
