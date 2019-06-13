using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace NIR.Views
{
    public partial class WorkCanvas : UserControl
    {
        protected static readonly String BorderControlXaml =
            @"<Border 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                
                BorderBrush='#CCFF0000' BorderThickness='1' Visibility='Hidden'/>";
    }
}
