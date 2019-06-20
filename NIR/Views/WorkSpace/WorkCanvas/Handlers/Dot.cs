using NIR.ShapesExtension;
using NIR.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NIR.Views
{
    public partial class WorkCanvas : UserControl
    {
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
                if (this.selectedDot.Parent.Source is Line)
                {
                    if (intersectedDots.Count() > 0)
                    {
                        Line line = this.selectedDot.Parent.Source as Line;
                        var PointsList = line.Points();
                        if (intersectedDots.Count() == PointsList.Count - 1)
                        {
                            this.DrawCanvas.Children.Remove(line);
                            this.selectShape(null);
                        }
                        else
                        {
                            int i = 0;
                            foreach (var dot in intersectedDots.OrderBy(d => d.DotID))
                            {
                                PointsList.RemoveAt(dot.DotID + i);
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

    }
}
