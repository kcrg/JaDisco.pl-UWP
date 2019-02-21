using Windows.UI.Xaml.Controls;

namespace JaDisco_UWP.Views.CustomDialogs
{
    public sealed partial class ErrorDialog : ContentDialog
    {
        public ErrorDialog()
        {
            InitializeComponent();
        }

        private void PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Hide();
        }
    }
}
