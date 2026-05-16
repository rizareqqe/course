using GuiP31.Models;
using GuiP31.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;

namespace GuiP31
{
    public partial class DirectorWindow : Window
    {
        public DirectorWindow()
        {
            InitializeComponent();
            Loaded += DirectorWindow_Loaded;
        }

        private async void DirectorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDirectorsAsync();
        }

        private async System.Threading.Tasks.Task LoadDirectorsAsync(string search = "")
        {
            try
            {
                var suffix = string.IsNullOrWhiteSpace(search) ? string.Empty : $"?search={Uri.EscapeDataString(search)}";
                var directors = await ApiClient.GetAsync<List<LookupItemDto>>($"api/directors{suffix}") ?? new List<LookupItemDto>();
                DirectorGrid.ItemsSource = directors;
                StatusTextBlock.Text = $"Записей: {directors.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка режиссеров", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Search_Click(object sender, RoutedEventArgs e) => await LoadDirectorsAsync(SearchTextBox.Text.Trim());

        private async void Reset_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            await LoadDirectorsAsync();
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            var editor = new LookupItemEditWindow("Новый режиссер", "Имя режиссера:");
            if (editor.ShowDialog() != true)
            {
                return;
            }

            await SaveAsync(HttpMethod.Post, "api/directors", new LookupItemDto { Name = editor.EnteredValue });
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (DirectorGrid.SelectedItem is not LookupItemDto director)
            {
                MessageBox.Show("Выберите режиссера.", "Изменение", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var editor = new LookupItemEditWindow("Изменение режиссера", "Имя режиссера:", director.Name);
            if (editor.ShowDialog() != true)
            {
                return;
            }

            await SaveAsync(HttpMethod.Put, $"api/directors/{director.Id}", new LookupItemDto { Id = director.Id, Name = editor.EnteredValue });
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (DirectorGrid.SelectedItem is not LookupItemDto director)
            {
                MessageBox.Show("Выберите режиссера.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Удалить режиссера \"{director.Name}\"?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                await ApiClient.DeleteAsync($"api/directors/{director.Id}");
                await LoadDirectorsAsync(SearchTextBox.Text.Trim());
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
                await LoadDirectorsAsync(SearchTextBox.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
