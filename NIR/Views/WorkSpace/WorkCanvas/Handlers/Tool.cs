using NIR.ShapesExtension;
using NIR.Tools;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NIR.Views
{
    public partial class WorkCanvas : UserControl
    {
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
    }
}
