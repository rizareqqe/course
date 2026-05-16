using GuiP31.Models;
using GuiP31.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Windows;

namespace GuiP31
{
    public partial class MovieEditWindow : Window
    {
        private readonly int? movieId;
        private MovieLookupsDto lookups = new MovieLookupsDto();

        public MovieEditWindow(int? movieId)
        {
            InitializeComponent();
            this.movieId = movieId;
            Loaded += MovieEditWindow_Loaded;
        }

        private async void MovieEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CuratorPanel.Visibility = SessionContext.IsAdministrator ? Visibility.Visible : Visibility.Collapsed;
            await LoadLookupsAsync();

            if (movieId.HasValue)
            {
                await LoadMovieAsync(movieId.Value);
            }
            else
            {
                YearTextBox.Text = DateTime.Now.Year.ToString();
                if (CuratorComboBox.Items.Count > 0)
                {
                    CuratorComboBox.SelectedIndex = 0;
                }
            }
        }

        private async System.Threading.Tasks.Task LoadLookupsAsync()
        {
            lookups = await ApiClient.GetAsync<MovieLookupsDto>("api/movies/lookups") ?? new MovieLookupsDto();

            DirectorComboBox.ItemsSource = CreateLookupWithEmpty(lookups.Directors, "Без режиссера");
            CuratorComboBox.ItemsSource = lookups.Curators;
            GenresListBox.ItemsSource = lookups.Genres;
            ActorsListBox.ItemsSource = lookups.Actors;

            DirectorComboBox.SelectedIndex = 0;
        }

        private async System.Threading.Tasks.Task LoadMovieAsync(int id)
        {
            var movie = await ApiClient.GetAsync<MovieEditDto>($"api/movies/{id}/edit");
            if (movie == null)
            {
                ShowError("Не удалось получить данные фильма.");
                return;
            }

            TitleTextBox.Text = movie.Title;
            YearTextBox.Text = movie.Year.ToString();
            DescriptionTextBox.Text = movie.Description;
            DirectorComboBox.SelectedValue = movie.DirectorId ?? 0;
            CuratorComboBox.SelectedValue = movie.CuratorUserId;
            SelectItems(GenresListBox, movie.GenreIds);
            SelectItems(ActorsListBox, movie.ActorIds);
        }

        private static void SelectItems(System.Windows.Controls.ListBox listBox, IEnumerable<int> selectedIds)
        {
            var ids = selectedIds.ToHashSet();
            listBox.SelectedItems.Clear();

            foreach (var item in listBox.Items.OfType<LookupItemDto>())
            {
                if (ids.Contains(item.Id))
                {
                    listBox.SelectedItems.Add(item);
                }
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            ShowError(string.Empty);

            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                ShowError("Введите название фильма.");
                return;
            }

            if (!int.TryParse(YearTextBox.Text.Trim(), out var year))
            {
                ShowError("Год должен быть целым числом.");
                return;
            }

            var request = new MovieEditRequest
            {
                Title = TitleTextBox.Text.Trim(),
                Year = year,
                Description = DescriptionTextBox.Text.Trim(),
                DirectorId = DirectorComboBox.SelectedValue is int directorId && directorId > 0 ? directorId : null,
                CuratorUserId = CuratorComboBox.SelectedValue as int?,
                GenreIds = GenresListBox.SelectedItems.OfType<LookupItemDto>().Select(item => item.Id).ToList(),
                ActorIds = ActorsListBox.SelectedItems.OfType<LookupItemDto>().Select(item => item.Id).ToList()
            };

            try
            {
                if (movieId.HasValue)
                {
                    await ApiClient.SendAsync(HttpMethod.Put, $"api/movies/{movieId.Value}", request);
                }
                else
                {
                    await ApiClient.SendAsync(HttpMethod.Post, "api/movies", request);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void ManageDirectors_Click(object sender, RoutedEventArgs e)
        {
            new DirectorWindow { Owner = this }.ShowDialog();
            await LoadLookupsAsync();
        }

        private async void ManageGenres_Click(object sender, RoutedEventArgs e)
        {
            new GenreWindow { Owner = this }.ShowDialog();
            await LoadLookupsAsync();
        }

        private async void ManageActors_Click(object sender, RoutedEventArgs e)
        {
            new ActorWindow { Owner = this }.ShowDialog();
            await LoadLookupsAsync();
        }

        private static List<LookupItemDto> CreateLookupWithEmpty(List<LookupItemDto> source, string emptyLabel)
        {
            var result = new List<LookupItemDto> { new LookupItemDto { Id = 0, Name = emptyLabel } };
            result.AddRange(source);
            return result;
        }

        private void ShowError(string message)
        {
            ErrorPanel.Visibility = string.IsNullOrWhiteSpace(message) ? Visibility.Collapsed : Visibility.Visible;
            ErrorTextBlock.Text = message;
        }
    }
}
