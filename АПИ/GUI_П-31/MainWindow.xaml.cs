using GuiP31.Services;
using System.Windows;

namespace GuiP31
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var user = SessionContext.CurrentUser;
            WelcomeTextBlock.Text = $"Пользователь: {user?.FullName}\nРоль: {user?.Role}";
            StatusTextBlock.Text = SessionContext.IsAdministrator
                ? "Режим администратора: доступны фильмы и справочники."
                : "Режим редактора: доступны только закрепленные фильмы.";

            var canManageLookups = SessionContext.IsAdministrator;
            ActorsButton.Visibility = canManageLookups ? Visibility.Visible : Visibility.Collapsed;
            DirectorsButton.Visibility = canManageLookups ? Visibility.Visible : Visibility.Collapsed;
            GenresButton.Visibility = canManageLookups ? Visibility.Visible : Visibility.Collapsed;
            ActorsMenuItem.Visibility = canManageLookups ? Visibility.Visible : Visibility.Collapsed;
            DirectorsMenuItem.Visibility = canManageLookups ? Visibility.Visible : Visibility.Collapsed;
            GenresMenuItem.Visibility = canManageLookups ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OpenMovies_Click(object sender, RoutedEventArgs e)
        {
            new MovieWindow { Owner = this }.ShowDialog();
        }

        private void OpenActors_Click(object sender, RoutedEventArgs e)
        {
            new ActorWindow { Owner = this }.ShowDialog();
        }

        private void OpenDirectors_Click(object sender, RoutedEventArgs e)
        {
            new DirectorWindow { Owner = this }.ShowDialog();
        }

        private void OpenGenres_Click(object sender, RoutedEventArgs e)
        {
            new GenreWindow { Owner = this }.ShowDialog();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            SessionContext.Clear();
            new LoginWindow().Show();
            Close();
        }
    }
}
