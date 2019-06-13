using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NIR.Views
{
    public delegate void UpdateButtonFunc();
    public delegate void SetToolTypeFunc(DrawToolType type);
    /// <summary>
    /// Логика взаимодействия для WorkSpace.xaml
    /// </summary>
    public partial class WorkSpace : UserControl
    {
        public static WorkSpace Current { get; private set; }
        public WorkSpace()
        {
            InitializeComponent();
            if (Current == null)
            {
                Current = this;
            }
            else
            {
                throw new Exception("WorkSpace синглтон. WorkSpace уже существует"); 
            }
            
        }
        // TODO: переписать на команды
        private SetToolTypeFunc SetToolType { get { return WorkCanvas.Current.SetToolType; } }
        public DrawToolType ToolType { get; set; }
        //TODO: это не корректно. На самом деле нужно передавать WorkCanvas функцию для того, чтобы она обновлял кнопки. А не вот так.
        public void updateButtons()
        {
            this.DoLine.IsChecked = this.ToolType == DrawToolType.Polyline;
        }
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


    }
}
