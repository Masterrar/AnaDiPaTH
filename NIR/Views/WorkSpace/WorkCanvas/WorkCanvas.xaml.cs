
using System;
using System.Collections.Generic;
using System.Linq;

using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

using System.Windows.Shapes;


namespace NIR.Views
{
    using ViewModel.DescriptionOfTask;
    using Tools;
    using ViewModels;
    using ShapesExtension;

    /// <summary>
    /// Логика взаимодействия для WorkCanvas.xaml
    /// </summary>
    public partial class WorkCanvas : UserControl
    {
        // TODO: переписать на команды
        public static WorkCanvas Current { get; private set; }
        public WorkCanvas()
        {
            
            InitializeComponent();
            if (Current == null)
            {
                Current = this;
            }
            else
            {
                throw new Exception("WorkCanvas синглтон. WorkSpace уже существует");
            }
            ImageBrush ib = new ImageBrush();
            difImage = new DifImageVM(new Uri("C:\\_Projects\\AnaDiPaTH\\NIR\\Resources\\(gray).JPG"));

            

            this.dotsControl = (ItemsControl)XamlReader.Parse(WorkCanvas.DotsItemsControlXaml);
            this.dots = new DrawToolDots();
            this.dotsControl.Tag = this.dots;
            this.dotsControl.ItemsSource = this.dots.Dots;

            this.DrawCanvas.Children.Add(this.dotsControl);
            this.Selection = new List<Shape>();

            this.SelectionChange += selectedTool_SelectionChange;
            
            this.canvasBorder = this.border;
            ib.ImageSource = difImage.Bitmap_Image;
            var h = DrawCanvas.Height;
            var w = DrawCanvas.Width;

            resizeIfSoLarge(out h, out w, ib.ImageSource.Height, ib.ImageSource.Width);


            DrawCanvas.Height = h;
            DrawCanvas.Width = w;
            DrawCanvas.Background = ib;


        }

        private void resizeIfSoLarge(out double resultHeight, out double resultWidth,
                                        double currentHeight, double currentWidth)
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
        //TODO: updateButtons стоит делать через команды
        public UpdateButtonFunc updateButtons { get { return WorkSpace.Current.updateButtons; } }
        //TODO: ToolType стоит делать через команды
        private DrawToolType ToolType
        {
            get { return WorkSpace.Current.ToolType; }
            set { WorkSpace.Current.ToolType = value; }
        }
        /// <summary>
        /// Событие возникает когда выделюятся новые фигуры/пропадает выделение
        /// </summary>
        public event EventHandler SelectionChange;

        public double CurrentLineWidth {get; set;} = 5;
        private System.Windows.Point? mousePos = null;
        //private Canvas DrawCanvas; // рабочая область для рисования
        private Shape lastShape = null; // текущая обрабатываемая фигура
        private Window window { get { return MainView.Current; } }
#pragma warning disable IDE1006 // Стили именования
        private Line lastShapeAsLine => this.lastShape as Line;

#pragma warning restore IDE1006 // Стили именования
        private System.Windows.Media.Brush CurrentBrush = null;

        private ItemsControl dotsControl; // точки трансформации
        private DrawToolDots dots; // источник данных точек трансформации
        private DrawToolDot selectedDot = null;
        private Border canvasBorder; // рамки рабочей области рисования (для позиционирования)
        private DrawTool selectedTool;
        private DifImageVM difImage { get; set; }
        public List<Shape> Selection { get; private set; }


        void selectedTool_SelectionChange(object sender, EventArgs e)
        {
            this.updateButtons();
            if (this.Selection.Count == 1)
            {
                Shape s = this.Selection.First();
                Brush b = this.CurrentBrush;

            }
        }

        //TODO: SetToolType стоит делать через команды
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

}
