using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace _422_Dubkova.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddUserPage.xaml
    /// </summary>
    public partial class AddUserPage : Page
    {
        private User _user;
        private bool _isEditMode;

        public AddUserPage()
        {
            InitializeComponent();
            _user = new User();
            _isEditMode = false;
            DataContext = _user;
        }

        public AddUserPage(User selectedUser)
        {
            InitializeComponent();
            _user = selectedUser;
            _isEditMode = true;
            DataContext = _user;
        }

        // 🔹 Метод хэширования пароля (как в RegPage)
        public static string GetHash(string password)
        {
            using (var sha1 = SHA1.Create())
            {
                return string.Concat(sha1.ComputeHash(Encoding.UTF8.GetBytes(password))
                    .Select(b => b.ToString("X2")));
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_user.Login)) errors.AppendLine("Укажите логин!");
            if (string.IsNullOrWhiteSpace(TBPass.Text)) errors.AppendLine("Укажите пароль!");
            if (string.IsNullOrWhiteSpace(_user.Role)) errors.AppendLine("Выберите роль!");
            if (string.IsNullOrWhiteSpace(_user.FIO)) errors.AppendLine("Укажите ФИО!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new Entities())
                {
                    if (!_isEditMode)
                    {
                        // 🔹 Хэшируем пароль перед сохранением
                        _user.Password = GetHash(TBPass.Text);
                        db.User.Add(_user);
                    }
                    else
                    {
                        var toUpdate = db.User.FirstOrDefault(u => u.ID == _user.ID);
                        if (toUpdate != null)
                        {
                            toUpdate.Login = _user.Login;

                            // 🔹 Если пароль изменён — хэшируем новый
                            if (!string.IsNullOrWhiteSpace(TBPass.Text))
                                toUpdate.Password = GetHash(TBPass.Text);

                            toUpdate.Role = _user.Role;
                            toUpdate.FIO = _user.FIO;
                            toUpdate.Photo = _user.Photo;

                            db.Entry(toUpdate).State = EntityState.Modified;
                        }
                    }

                    db.SaveChanges();
                }

                MessageBox.Show("Данные успешно сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new UserTabPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBLogin.Text = "";
            TBPass.Text = "";
            TBFio.Text = "";
            cmbRole.SelectedIndex = 0;
            TBPhoto.Text = "";
        }

        private void TBLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoginHintText.Visibility = string.IsNullOrEmpty(TBLogin.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TBPass_TextChanged(object sender, TextChangedEventArgs e)
        {
            PassHintText.Visibility = string.IsNullOrEmpty(TBPass.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TBFio_TextChanged(object sender, TextChangedEventArgs e)
        {
            FioHintText.Visibility = string.IsNullOrEmpty(TBFio.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TBPhoto_TextChanged(object sender, TextChangedEventArgs e)
        {
            PhotoHintText.Visibility = string.IsNullOrEmpty(TBPhoto.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void cmbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool noSelection = cmbRole.SelectedItem == null || string.IsNullOrEmpty(cmbRole.Text);

            if (!noSelection && cmbRole.SelectedItem is ComboBoxItem selectedItem)
            {
                _user.Role = selectedItem.Content.ToString();
            }
        }
    }
}
