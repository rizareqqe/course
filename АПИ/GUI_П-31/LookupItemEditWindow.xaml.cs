using System.Windows;

namespace GuiP31
{
    public partial class LookupItemEditWindow : Window
    {
        public string EnteredValue => ValueTextBox.Text.Trim();

        public LookupItemEditWindow(string title, string prompt, string initialValue = "")
        {
            InitializeComponent();
            Title = title;
            PromptTextBlock.Text = prompt;
            ValueTextBox.Text = initialValue;
            ValueTextBox.SelectAll();
            ValueTextBox.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EnteredValue))
            {
                MessageBox.Show("Введите значение.", "Проверка ввода", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
