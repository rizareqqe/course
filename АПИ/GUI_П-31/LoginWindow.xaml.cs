using GuiP31.Models;
using GuiP31.Services;
using System;
using System.Net.Http;
using System.Windows;

namespace GuiP31
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            ShowError(string.Empty);

            if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
            {
                ShowError("Введите логин.");
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ShowError("Введите пароль.");
                return;
            }

            try
            {
                var response = await ApiClient.SendAsync<LoginRequest, LoginResponse>(
                    HttpMethod.Post,
                    "api/auth/login",
                    new LoginRequest
                    {
                        Login = LoginTextBox.Text.Trim(),
                        Password = PasswordBox.Password
                    });

                if (response == null)
                {
                    ShowError("Сервер не вернул данные пользователя.");
                    return;
                }

                SessionContext.SetUser(response);
                var mainWindow = new MainWindow();
                mainWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void ShowError(string message)
        {
            ErrorPanel.Visibility = string.IsNullOrWhiteSpace(message) ? Visibility.Collapsed : Visibility.Visible;
            ErrorTextBlock.Text = message;
        }
    }
}
