using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace JaDisco_UWP.AttachedProperties
{
    public class RichTextBlockProperty : DependencyObject
    {
        public static readonly DependencyProperty DataSourceProperty = 
            DependencyProperty.RegisterAttached(
                "DataSource", 
                typeof(Paragraph), 
                typeof(RichTextBlockProperty), 
                new PropertyMetadata(false, OnDataSourceChanged)
                );

        private static void OnDataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var richTextBlock = d as RichTextBlock;

            richTextBlock.Blocks.Clear();

            var value = e.NewValue as Paragraph;
            richTextBlock.Blocks.Add(value);
        }

        public static void SetDataSource(RichTextBlock element, Paragraph value)
        {
            element.SetValue(DataSourceProperty, value);
        }

        public static Paragraph GetDataSource(RichTextBlock element)
        {
            return (Paragraph)element.GetValue(DataSourceProperty);
        }
    }
}
