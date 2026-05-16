using GuiP31.Models;
using GuiP31.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;

namespace GuiP31
{
    public partial class GenreWindow : Window
    {
        public GenreWindow()
        {
            InitializeComponent();
            Loaded += GenreWindow_Loaded;
        }

        private async void GenreWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadGenresAsync();
        }

        private async System.Threading.Tasks.Task LoadGenresAsync(string search = "")
        {
            try
            {
                var suffix = string.IsNullOrWhiteSpace(search) ? string.Empty : $"?search={Uri.EscapeDataString(search)}";
                var genres = await ApiClient.GetAsync<List<LookupItemDto>>($"api/genres{suffix}") ?? new List<LookupItemDto>();
                GenreGrid.ItemsSource = genres;
                StatusTextBlock.Text = $"Записей: {genres.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка жанров", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Search_Click(object sender, RoutedEventArgs e) => await LoadGenresAsync(SearchTextBox.Text.Trim());

        private async void Reset_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            await LoadGenresAsync();
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            var editor = new LookupItemEditWindow("Новый жанр", "Название жанра:");
            if (editor.ShowDialog() != true)
            {
                return;
            }

            await SaveAsync(HttpMethod.Post, "api/genres", new LookupItemDto { Name = editor.EnteredValue });
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (GenreGrid.SelectedItem is not LookupItemDto genre)
            {
                MessageBox.Show("Выберите жанр.", "Изменение", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var editor = new LookupItemEditWindow("Изменение жанра", "Название жанра:", genre.Name);
            if (editor.ShowDialog() != true)
            {
                return;
            }

            await SaveAsync(HttpMethod.Put, $"api/genres/{genre.Id}", new LookupItemDto { Id = genre.Id, Name = editor.EnteredValue });
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (GenreGrid.SelectedItem is not LookupItemDto genre)
            {
                MessageBox.Show("Выберите жанр.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Удалить жанр \"{genre.Name}\"?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                await ApiClient.DeleteAsync($"api/genres/{genre.Id}");
                await LoadGenresAsync(SearchTextBox.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task SaveAsync(HttpMethod method, string url, LookupItemDto payload)
        {
            try
            {
                await ApiClient.SendAsync(method, url, payload);
                await LoadGenresAsync(SearchTextBox.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
