using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _422_Dubkova.Pages
{
    /// <summary>
    /// Логика взаимодействия для UserPage.xaml
    /// </summary>
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                var currentUsers = Entities.GetContext().User.ToList();
                ListUser.ItemsSource = currentUsers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}");
            }
        }

        private void clearFiltersButton_Click_1(object sender, RoutedEventArgs e)
        {
            fioFilterTextBox.Text = "";
            sortComboBox.SelectedIndex = 0;
            onlyAdminCheckBox.IsChecked = false;
        }

        private void fioFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUsers();
        }

        private void sortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUsers();
        }

        private void onlyAdminCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        private void onlyAdminCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        private void UpdateUsers()
        {
            if (!IsInitialized) return;

            try
            {
                List<User> currentUsers = Entities.GetContext().User.ToList();

                // Фильтрация по ФИО
                if (!string.IsNullOrWhiteSpace(fioFilterTextBox.Text))
                {
                    currentUsers = currentUsers
                        .Where(x => x.FIO != null && x.FIO.IndexOf(fioFilterTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                // Фильтрация по роли (только админы)
                if (onlyAdminCheckBox.IsChecked == true)
                {
                    currentUsers = currentUsers.Where(x => x.Role == "admin").ToList();
                }

                // Сортировка по ФИО
                if (sortComboBox.SelectedIndex == 0)
                {
                    currentUsers = currentUsers.OrderBy(x => x.FIO).ToList();
                }
                else
                {
                    currentUsers = currentUsers.OrderByDescending(x => x.FIO).ToList();
                }

                ListUser.ItemsSource = currentUsers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении списка пользователей: {ex.Message}");
            }
        }
    }
}
