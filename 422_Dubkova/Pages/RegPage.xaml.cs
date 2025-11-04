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
    /// Логика взаимодействия для RegPage.xaml
    /// </summary>
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
            comboBxRole.SelectedIndex = 0;
        }
        // Обработчики для Label (клика по плейсхолдерам)
        private void lblLogHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtbxLog.Focus();
        }
        private void lblPassHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            passBxFrst.Focus();
        }
        private void lblPassSecHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            passBxScnd.Focus();
        }
        private void lblFioHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtbxFIO.Focus();
        }

        // Обработчики для TextChanged и PasswordChanged - скрываем/показываем плейсхолдеры
        private void txtbxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblLogHitn.Visibility = string.IsNullOrEmpty(txtbxLog.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        private void txtbxFIO_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblFioHitn.Visibility = string.IsNullOrEmpty(txtbxFIO.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        private void passBxFrst_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassHitn.Visibility = string.IsNullOrEmpty(passBxFrst.Password) ? Visibility.Visible : Visibility.Hidden;
        }

        private void passBxScnd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassSecHitn.Visibility = string.IsNullOrEmpty(passBxScnd.Password) ? Visibility.Visible : Visibility.Hidden;
        }
        public static string GetHash(string password)
        {
            using (var sha1 = SHA1.Create())
            {
                return string.Concat(sha1.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(b => b.ToString("X2")));
            }
        }


        private void regButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на заполненность
            if (string.IsNullOrEmpty(txtbxLog.Text) ||
                string.IsNullOrEmpty(txtbxFIO.Text) ||
                string.IsNullOrEmpty(passBxFrst.Password) ||
                string.IsNullOrEmpty(passBxScnd.Password))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            // Проверка уникальности логина в базе данных
            using (Entities db = new Entities())
            {
                var user = db.User
                             .AsNoTracking()
                             .FirstOrDefault(u => u.Login == txtbxLog.Text);
                if (user != null)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!");
                    return;
                }
            }

            // Проверка пароля: длина >= 6, только английские буквы, минимум 1 цифра
            if (passBxFrst.Password.Length < 6)
            {
                MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!");
                return;
            }

            bool en = true;
            bool number = false;
            foreach (char ch in passBxFrst.Password)
            {
                if (ch >= '0' && ch <= '9')
                    number = true;
                else if (!((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z')))
                    en = false;
            }

            if (!en)
            {
                MessageBox.Show("Используйте только английскую раскладку!");
                return;
            }
            if (!number)
            {
                MessageBox.Show("Добавьте хотя бы одну цифру!");
                return;
            }
            bool hasUpper = passBxFrst.Password.Any(char.IsUpper);
            bool hasDigit = passBxFrst.Password.Any(char.IsDigit);

            if (!hasUpper || !hasDigit)
            {
                MessageBox.Show("Пароль должен содержать хотя бы одну заглавную букву и одну цифру.");
                return;
            }

            // Проверка совпадения паролей
            if (passBxFrst.Password != passBxScnd.Password)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }

            // Если все проверки пройдены — создаём пользователя
            using (Entities db = new Entities())
            {
                User userObject = new User
                {
                    FIO = txtbxFIO.Text,
                    Login = txtbxLog.Text,
                    Password = GetHash(passBxFrst.Password),
                    Role = ((ComboBoxItem)comboBxRole.SelectedItem).Content.ToString()
                };

                db.User.Add(userObject);
                db.SaveChanges();
            }

            MessageBox.Show("Пользователь успешно зарегистрирован!");
            //NavigationService?.Navigate(new AuthPage()); чтобы после регистрации отправяло сразу на страницу авторизации

            // Очистка полей
            txtbxLog.Clear();
            txtbxFIO.Clear();
            passBxFrst.Clear();
            passBxScnd.Clear();
            comboBxRole.SelectedIndex = 0;

            // Визуальное обновление плейсхолдеров (можно вызвать вручную)
            lblLogHitn.Visibility = Visibility.Visible;
            lblFioHitn.Visibility = Visibility.Visible;
            lblPassHitn.Visibility = Visibility.Visible;
            lblPassSecHitn.Visibility = Visibility.Visible;
        }
    }
}
