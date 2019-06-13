using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace NIR.Views
{
    public partial class WorkCanvas : UserControl
    {
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
    }
}