using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace _422_Dubkova.Pages
{
    public partial class UserTabPage : Page
    {
        private List<User> users;

        public UserTabPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using (var db = new Entities())
            {
                users = db.User.ToList();
                DataGridUsers.ItemsSource = users;
            }
        }

        private void UsersPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                Entities.GetContext().ChangeTracker.Entries()
                    .Where(x => x.State != EntityState.Added)
                    .ToList()
                    .ForEach(x => x.Reload());

                LoadData();
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddUserPage(OnUserSaved));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = (User)((Button)sender).DataContext;
            NavigationService.Navigate(new AddUserPage(selectedUser, OnUserSaved));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = DataGridUsers.SelectedItems.Cast<User>().ToList();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одну строку для удаления.");
                return;
            }

            if (MessageBox.Show($"Вы точно хотите удалить {selectedItems.Count} пользователь(ей)?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new Entities())
                    {
                        foreach (var user in selectedItems)
                        {
                            var userToDelete = db.User.Find(user.ID);
                            if (userToDelete != null)
                                db.User.Remove(userToDelete);
                        }
                        db.SaveChanges();
                    }

                    MessageBox.Show("Пользователь(и) успешно удалены!");
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 🔹 метод обратного вызова после добавления/редактирования пользователя
        private void OnUserSaved()
        {
            LoadData();
        }
    }
}
