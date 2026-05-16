using GuiP31.Models;
using GuiP31.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace GuiP31
{
    public partial class MovieWindow : Window
    {
        private List<MovieListItemDto> movies = new List<MovieListItemDto>();

        public MovieWindow()
        {
            InitializeComponent();
            Loaded += MovieWindow_Loaded;
        }

        private async void MovieWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLookupsAsync();
            await LoadMoviesAsync();
        }

        private async System.Threading.Tasks.Task LoadLookupsAsync()
        {
            try
            {
                var lookups = await ApiClient.GetAsync<MovieLookupsDto>("api/movies/lookups");
                DirectorFilterComboBox.ItemsSource = CreateLookupWithEmpty(lookups?.Directors);
                DirectorFilterComboBox.SelectedIndex = 0;
                GenreFilterComboBox.ItemsSource = CreateLookupWithEmpty(lookups?.Genres);
                GenreFilterComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка загрузки справочников", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadMoviesAsync()
        {
            try
            {
                movies = await ApiClient.GetAsync<List<MovieListItemDto>>($"api/movies{BuildFilterQuery()}") ?? new List<MovieListItemDto>();
                MovieGrid.ItemsSource = movies;
                StatusTextBlock.Text = $"Записей: {movies.Count}. Пользователь: {SessionContext.CurrentUser?.FullName}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка загрузки фильмов", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string BuildFilterQuery()
        {
            var query = new StringBuilder("?");
            Append(query, "search", SearchTextBox.Text);
            Append(query, "directorId", GetSelectedLookupId(DirectorFilterComboBox));
            Append(query, "genreId", GetSelectedLookupId(GenreFilterComboBox));
            Append(query, "yearFrom", ParseOptionalInt(YearFromTextBox.Text));
            Append(query, "yearTo", ParseOptionalInt(YearToTextBox.Text));
            Append(query, "primarySort", GetSelectedSortTag(PrimarySortComboBox));
            Append(query, "primaryDescending", PrimaryDescendingCheckBox.IsChecked == true ? "true" : "false");

            var secondarySort = GetSelectedSortTag(SecondarySortComboBox);
            if (!string.IsNullOrWhiteSpace(secondarySort))
            {
                Append(query, "secondarySort", secondarySort);
                Append(query, "secondaryDescending", SecondaryDescendingCheckBox.IsChecked == true ? "true" : "false");
            }

            return query.Length == 1 ? string.Empty : query.ToString().TrimEnd('&');
        }

        private static void Append(StringBuilder builder, string name, object? value)
        {
            if (value == null)
            {
                return;
            }

            var text = value.ToString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            builder.Append($"{name}={Uri.EscapeDataString(text)}&");
        }

        private static string GetSelectedSortTag(ComboBox comboBox)
        {
            return (comboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? string.Empty;
        }

        private static string? ParseOptionalInt(string text)
        {
            return int.TryParse(text.Trim(), out var value) ? value.ToString() : null;
        }

        private static int? GetSelectedLookupId(ComboBox comboBox)
        {
            return comboBox.SelectedValue is int value && value > 0 ? value : null;
        }

        private static List<LookupItemDto> CreateLookupWithEmpty(List<LookupItemDto>? items)
        {
            var result = new List<LookupItemDto> { new LookupItemDto { Id = 0, Name = "Все" } };
            if (items != null)
            {
                result.AddRange(items);
            }

            return result;
        }

        private async void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            await LoadMoviesAsync();
        }

        private async void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            YearFromTextBox.Text = string.Empty;
            YearToTextBox.Text = string.Empty;
            DirectorFilterComboBox.SelectedIndex = 0;
            GenreFilterComboBox.SelectedIndex = 0;
            PrimarySortComboBox.SelectedIndex = 0;
            SecondarySortComboBox.SelectedIndex = 0;
            PrimaryDescendingCheckBox.IsChecked = false;
            SecondaryDescendingCheckBox.IsChecked = false;
            await LoadMoviesAsync();
        }

        private async void AddMovie_Click(object sender, RoutedEventArgs e)
        {
            var editor = new MovieEditWindow(null) { Owner = this };
            if (editor.ShowDialog() == true)
            {
                await LoadLookupsAsync();
                await LoadMoviesAsync();
            }
        }

        private async void EditMovie_Click(object sender, RoutedEventArgs e)
        {
            if (MovieGrid.SelectedItem is not MovieListItemDto movie)
            {
                MessageBox.Show("Выберите фильм для изменения.", "Изменение фильма", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var editor = new MovieEditWindow(movie.MovieId) { Owner = this };
            if (editor.ShowDialog() == true)
            {
                await LoadLookupsAsync();
                await LoadMoviesAsync();
            }
        }

        private async void DeleteMovie_Click(object sender, RoutedEventArgs e)
        {
            if (MovieGrid.SelectedItem is not MovieListItemDto movie)
            {
                MessageBox.Show("Выберите фильм для удаления.", "Удаление фильма", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Удалить фильм \"{movie.Title}\"?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                await ApiClient.DeleteAsync($"api/movies/{movie.MovieId}");
                await LoadMoviesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ShowStatistics_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var statistics = await ApiClient.GetAsync<MovieStatisticsDto>($"api/movies/statistics{BuildFilterQuery()}");
                new StatisticsWindow(statistics ?? new MovieStatisticsDto()) { Owner = this }.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка статистики", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadMoviesAsync();
        }
    }
}
