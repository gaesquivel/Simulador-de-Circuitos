using System.Windows.Controls;

namespace CircuitMVVMBase.UserControls
{
    /// <summary>
    /// Interaction logic for ItemCollection.xaml
    /// </summary>
    public partial class ItemCollection : UserControl
    {
        public ItemCollection()
        {
            InitializeComponent();
        }

        //<!--Source="{Binding ImageUrl, Converter={x:Static s:ImageUrlConverter.Instance}}"-->
        //<Setter Property = "s:DragAndDropProps.EnabledForDrag"
        //                            Value="True" />

         //<ItemsControl ItemsSource = "{Binding ToolBoxItems}" >
         //           < ItemsControl.Template >
         //               < ControlTemplate TargetType="{x:Type ItemsControl}">
         //                   <Border BorderThickness = "{TemplateBinding Border.BorderThickness}"
         //                           Padding="{TemplateBinding Control.Padding}"
         //                           BorderBrush="{TemplateBinding Border.BorderBrush}"
         //                           Background="{TemplateBinding Panel.Background}"
         //                           SnapsToDevicePixels="True">
         //                       <ScrollViewer VerticalScrollBarVisibility = "Auto" >
         //                           < ItemsPresenter SnapsToDevicePixels="{TemplateBinding UIElement.SnapsToDevicePixels}" />
         //                       </ScrollViewer>
         //                   </Border>
         //               </ControlTemplate>
         //           </ItemsControl.Template>
         //           <ItemsControl.ItemsPanel>
         //               <ItemsPanelTemplate>
         //                   <WrapPanel Margin = "0,5,0,5"
         //                              ItemHeight="50"
         //                              ItemWidth="50" />
         //               </ItemsPanelTemplate>
         //           </ItemsControl.ItemsPanel>
         //           <ItemsControl.ItemContainerStyle>
         //               <Style TargetType = "{x:Type ContentPresenter}" >
         //                   < Setter Property="Control.Padding"
         //                           Value="10" />
         //                   <Setter Property = "ContentControl.HorizontalContentAlignment"
         //                           Value="Stretch" />
         //                   <Setter Property = "ContentControl.VerticalContentAlignment"
         //                           Value="Stretch" />
         //                   <Setter Property = "ToolTip"
         //                           Value="{Binding ToolTip}" />
                            
         //               </Style>
         //           </ItemsControl.ItemContainerStyle>
         //           <ItemsControl.ItemTemplate>
         //               <DataTemplate>
         //                   <TextBox IsHitTestVisible = "True"
         //                       Text="{Binding Name}"
         //                            />
         //               </DataTemplate>
         //           </ItemsControl.ItemTemplate>
         //       </ItemsControl>

    }
}
