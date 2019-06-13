using NIR.ShapesExtension;
using NIR.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace NIR.Views
{
    public partial class WorkCanvas : UserControl
    {
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
    }
}
