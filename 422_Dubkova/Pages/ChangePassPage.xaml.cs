using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// Логика взаимодействия для ChangePassPage.xaml
    /// </summary>
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }
        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполненности
            if (string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
                string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password) ||
                string.IsNullOrEmpty(TbLogin.Text))
            {
                MessageBox.Show("Все поля обязательны к заполнению!");
                return;
            }

            // Проверка логина и текущего пароля
            string hashedPass = GetHash(CurrentPasswordBox.Password);

            var user = Entities.GetContext().User
                .FirstOrDefault(u => u.Login == TbLogin.Text && u.Password == hashedPass);

            if (user == null)
            {
                MessageBox.Show("Текущий пароль или логин неверный!");
                return;
            }

            // Проверка нового пароля
            if (NewPasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Пароль должен быть не менее 6 символов.");
                return;
            }

            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Новый пароль и его подтверждение не совпадают!");
                return;
            }

            bool hasUpper = NewPasswordBox.Password.Any(char.IsUpper);
            bool hasDigit = NewPasswordBox.Password.Any(char.IsDigit);

            if (!hasUpper || !hasDigit)
            {
                MessageBox.Show("Пароль должен содержать хотя бы одну заглавную букву и одну цифру.");
                return;
            }

            // Сохраняем новый пароль
            user.Password = GetHash(NewPasswordBox.Password);
            Entities.GetContext().SaveChanges();
            MessageBox.Show("Пароль успешно изменен!");

            // Переход на страницу авторизации
            NavigationService?.Navigate(new AuthPage());
        }
        public static string GetHash(string password)
        {
            using (var sha1 = SHA1.Create())
            {
                return string.Concat(sha1.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(b => b.ToString("X2")));
            }
        }
        // Обработка изменения текста в логине
        private void TbLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtHintLogin.Visibility = string.IsNullOrEmpty(TbLogin.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        // Обработка изменения текущего пароля
        private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtHintCurrentPass.Visibility = string.IsNullOrEmpty(CurrentPasswordBox.Password) ? Visibility.Visible : Visibility.Collapsed;
        }

        // Обработка изменения нового пароля
        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtHintNewPass.Visibility = string.IsNullOrEmpty(NewPasswordBox.Password) ? Visibility.Visible : Visibility.Collapsed;
        }

        // Обработка изменения подтверждения пароля
        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtHintConfirmPass.Visibility = string.IsNullOrEmpty(ConfirmPasswordBox.Password) ? Visibility.Visible : Visibility.Collapsed;
        }

        // Обработчики клика по подсказкам — устанавливаем фокус в соответствующее поле
        private void txtHintLogin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TbLogin.Focus();
        }

        private void txtHintCurrentPass_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CurrentPasswordBox.Focus();
        }

        private void txtHintNewPass_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            NewPasswordBox.Focus();
        }

        private void txtHintConfirmPass_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ConfirmPasswordBox.Focus();
        }
    }
}
