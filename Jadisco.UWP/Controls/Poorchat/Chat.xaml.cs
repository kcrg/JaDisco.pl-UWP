using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Jadisco.UWP.Controls.Poorchat
{
    public sealed partial class Chat : UserControl
    {
        bool autoScroll = true;

        public Chat()
        {
            this.InitializeComponent();

            ScrollViewer.LayoutUpdated += ScrollViewer_LayoutUpdated;
            ScrollViewer.PointerWheelChanged += ScrollViewer_PointerWheelChanged;
        }

        private void ScrollViewer_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var delta = e.GetCurrentPoint(this).Properties.MouseWheelDelta;

            ScrollViewer.ChangeView(0, ScrollViewer.VerticalOffset - delta, 1);

            if (ScrollViewer.VerticalOffset - delta >= ScrollViewer.ScrollableHeight)
            {
                //Debug.WriteLine("AutoScroll: true");
                autoScroll = true;
            }
            else if (delta > 0)
            {
                //Debug.WriteLine("AutoScroll: false");
                autoScroll = false;
            }
        }

        private void ScrollViewer_LayoutUpdated(object sender, object e)
        {
            if (autoScroll)
            {
                ScrollViewer.ChangeView(0, ScrollViewer.ScrollableHeight, 1);
            }
        }
    }
}
