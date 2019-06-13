using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NIR.ViewModels
{
    public class DrawTool
    {
        [FlagsAttribute]
        public enum ShapeTag : byte
        {
            None = 0,
            Select = 1,
            Deleting = 4,
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
        public static Style CalculatePolylineStyle(Brush brush, double stroke)
        {
            Brush b = (VisualBrush)XamlReader.Parse(DrawTool.HatchBrushXaml);
            Style style = new Style
            {
                TargetType = typeof(Line)
            };
            //style.Setters.Add(new Setter(Line.StrokeProperty, brush));
            style.Setters.Add(new Setter(Line.StrokeThicknessProperty, stroke));
            style.Setters.Add(new Setter(Line.StrokeLineJoinProperty, PenLineJoin.Round));

            MultiTrigger mt = new MultiTrigger();
            mt.Conditions.Add(new Condition(Line.IsMouseOverProperty, true));
            mt.Conditions.Add(new Condition(Line.TagProperty, ShapeTag.None));
            mt.Setters.Add(new Setter(Line.StrokeProperty, Brushes.Red));
            style.Triggers.Add(mt);

            Trigger t = new Trigger() { Property = Line.TagProperty, Value = ShapeTag.Select };
            t.Setters.Add(new Setter(Line.StrokeProperty, Brushes.Red));
            style.Triggers.Add(t);

            t = new Trigger() { Property = Line.TagProperty, Value = ShapeTag.Select | ShapeTag.Deleting };
            t.Setters.Add(new Setter(Line.StrokeProperty, b));
            style.Triggers.Add(t);

            t = new Trigger() { Property = Line.TagProperty, Value = ShapeTag.Deleting };
            t.Setters.Add(new Setter(Line.StrokeProperty, b));
            style.Triggers.Add(t);

            return style;
        }

    }
}
