using GuiP31.Models;
using GuiP31.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;

namespace GuiP31
{
    public partial class ActorWindow : Window
    {
        public ActorWindow()
        {
            InitializeComponent();
            Loaded += ActorWindow_Loaded;
        }

        private async void ActorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadActorsAsync();
        }

        private async System.Threading.Tasks.Task LoadActorsAsync(string search = "")
        {
            try
            {
                var suffix = string.IsNullOrWhiteSpace(search) ? string.Empty : $"?search={Uri.EscapeDataString(search)}";
                var actors = await ApiClient.GetAsync<List<LookupItemDto>>($"api/actors{suffix}") ?? new List<LookupItemDto>();
                ActorGrid.ItemsSource = actors;
                StatusTextBlock.Text = $"Записей: {actors.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка актеров", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Search_Click(object sender, RoutedEventArgs e) => await LoadActorsAsync(SearchTextBox.Text.Trim());

        private async void Reset_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            await LoadActorsAsync();
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            var editor = new LookupItemEditWindow("Новый актер", "Имя актера:");
            if (editor.ShowDialog() != true)
            {
                return;
            }

            await SaveAsync(HttpMethod.Post, "api/actors", new LookupItemDto { Name = editor.EnteredValue });
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (ActorGrid.SelectedItem is not LookupItemDto actor)
            {
                MessageBox.Show("Выберите актера.", "Изменение", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var editor = new LookupItemEditWindow("Изменение актера", "Имя актера:", actor.Name);
            if (editor.ShowDialog() != true)
            {
                return;
            }

            await SaveAsync(HttpMethod.Put, $"api/actors/{actor.Id}", new LookupItemDto { Id = actor.Id, Name = editor.EnteredValue });
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ActorGrid.SelectedItem is not LookupItemDto actor)
            {
                MessageBox.Show("Выберите актера.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Удалить актера \"{actor.Name}\"?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                await ApiClient.DeleteAsync($"api/actors/{actor.Id}");
                await LoadActorsAsync(SearchTextBox.Text.Trim());
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
                await LoadActorsAsync(SearchTextBox.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
