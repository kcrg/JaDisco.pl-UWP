using Windows.UI.Xaml.Controls;

namespace Jadisco.UWP.Views.CustomDialogs
{
    public sealed partial class ErrorDialog : ContentDialog
    {
        public enum Type
        {
            Error,
            Information,
            Warning
        }

        public ErrorDialog(string message, Type type = Type.Information)
        {
            InitializeComponent();

            MessageText.Text = message;

            switch (type)
            {
                case Type.Error:
                    Title = "Błąd";
                    break;
                case Type.Information:
                    Title = "Informacja";
                    break;
                case Type.Warning:
                    Title = "Ostrzeżenie";
                    break;
            }
        }

        private new void PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Hide();
        }
    }
}