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
        Line,
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
        
        
        

        public static MainView Current { get; private set; }

        public MainView()
        {
            InitializeComponent();
            if (Current == null)
            {
                Current = this;
            }
            else
            {
                throw new Exception("MainView синглтон. MainView уже существует");
            }
            
        }
        
        
        
        
        
        

        
        [FlagsAttribute]
        public enum ShapeTag : byte
        {
            None = 0,
            Select = 1,
            Deleting = 4,
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
