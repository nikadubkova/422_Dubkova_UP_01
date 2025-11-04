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
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;
        private User currentUser;

        public AuthPage()
        {
            InitializeComponent();
        }
        public static string GetHash(string password)
        {
            using (var sha1 = SHA1.Create())
            {
                return string.Concat(sha1.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(b => b.ToString("X2")));
            }
        }
        // Запрет копирования/вставки капчи
        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        // Переключение отображения капчи и полей логина/пароля
        public void CaptchaSwitch()
        {
            if (CaptchaPanel.Visibility == Visibility.Visible)
            {
                // Показываем поля логина и пароля
                CaptchaPanel.Visibility = Visibility.Hidden;

                TextBoxLogin.Visibility = Visibility.Visible;
                txtHintLogin.Visibility = Visibility.Visible;
                txtHintPass.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Visible;

                ButtonChangePassword.Visibility = Visibility.Visible;
                ButtonEnter.Visibility = Visibility.Visible;
                ButtonReg.Visibility = Visibility.Visible;

                captchaInput.Clear();
            }
            else
            {
                // Скрываем поля логина и пароля, показываем капчу
                CaptchaPanel.Visibility = Visibility.Visible;

                TextBoxLogin.Visibility = Visibility.Hidden;
                txtHintLogin.Visibility = Visibility.Hidden;
                txtHintPass.Visibility = Visibility.Hidden;
                PasswordBox.Visibility = Visibility.Hidden;

                ButtonChangePassword.Visibility = Visibility.Hidden;
                ButtonEnter.Visibility = Visibility.Hidden;
                ButtonReg.Visibility = Visibility.Hidden;

                CaptchaChange(); // генерация капчи
            }
        }


        // Генерация капчи
        public void CaptchaChange()
        {
            string allowchar = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z," +
                               "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,y,z," +
                               "1,2,3,4,5,6,7,8,9,0";

            char[] a = { ',' };
            string[] ar = allowchar.Split(a);
            string pwd = "";
            Random r = new Random();

            for (int i = 0; i < 6; i++)
            {
                pwd += ar[r.Next(0, ar.Length)];
            }
            captcha.Text = pwd;
        }
        private void submitCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (captchaInput.Text != captcha.Text)
            {
                MessageBox.Show("Неверно введена капча", "Ошибка");
                CaptchaChange();
                captchaInput.Clear();
            }
            else
            {
                MessageBox.Show("Капча введена успешно, можете продолжить авторизацию", "Успех");

                // Очистка полей
                TextBoxLogin.Clear();
                PasswordBox.Clear();
                captchaInput.Clear();

                // Отображение подсказок
                txtHintLogin.Visibility = Visibility.Visible;
                txtHintPass.Visibility = Visibility.Visible;

                // Скрываем капчу, возвращаем поля логина и пароля
                CaptchaSwitch();

                // Сброс счетчика
                failedAttempts = 0;
            }
        }

        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {

            if (CaptchaPanel.Visibility == Visibility.Visible)
            {
                MessageBox.Show("Пожалуйста, подтвердите капчу перед авторизацией.");
                return;
            }

            if (string.IsNullOrEmpty(TextBoxLogin.Text) || string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            string hashedPassword = GetHash(PasswordBox.Password);

            try
            {
                using (var db = new Entities())
                {
                    var user = db.User
                        .AsNoTracking()
                        .FirstOrDefault(u => u.Login == TextBoxLogin.Text && u.Password == hashedPassword);

                    if (user == null)
                    {
                        MessageBox.Show("Пользователь с такими данными не найден!");
                        failedAttempts++;
                        if (failedAttempts >= 3)
                        {
                            if (CaptchaPanel.Visibility != Visibility.Visible)
                            {
                                CaptchaSwitch();
                            }
                            CaptchaChange();
                        }
                        return;
                    }
                    else
                    {
                        // Пользователь найден — сбрасываем счетчик попыток
                        failedAttempts = 0;
                        MessageBox.Show("Пользователь успешно найден!");

                        var mainWindow = Application.Current.MainWindow as MainWindow;
                        if (mainWindow != null)
                        {
                            mainWindow.currentUserRole = user.Role;
                            mainWindow.WelcomePanel.Visibility = Visibility.Collapsed;
                            mainWindow.MainFrame.Visibility = Visibility.Visible;

                            if (user.Role == "admin")
                            {
                                mainWindow.MainFrame.Navigate(new AdminPage());
                            }
                            else if (user.Role == "user")
                            {
                                mainWindow.MainFrame.Navigate(new UserPage());
                            }
                            else
                            {
                                MessageBox.Show("Роль пользователя не определена.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка доступа к базе данных: {ex.Message}");
            }
        }

        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            AuthPanel.Visibility = Visibility.Collapsed;

            // Показываем фрейм и навигируем на страницу регистрации
            AuthFrame.Visibility = Visibility.Visible;
            AuthFrame.Navigate(new RegPage());
        }

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            AuthPanel.Visibility = Visibility.Collapsed;

            // Показываем фрейм и навигируем на страницу смены пароля
            AuthFrame.Visibility = Visibility.Visible;
            AuthFrame.Navigate(new ChangePassPage());
        }

        // Заглушки на события TextChanged и PasswordChanged
        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Если текст есть — прячем подсказку
            if (!string.IsNullOrEmpty(TextBoxLogin.Text))
            {
                txtHintLogin.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtHintLogin.Visibility = Visibility.Visible;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Если пароль введен — прячем подсказку
            if (!string.IsNullOrEmpty(PasswordBox.Password))
            {
                txtHintPass.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtHintPass.Visibility = Visibility.Visible;
            }
        }
        // Обработчики клика по подсказкам
        private void txtHintLogin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBoxLogin.Focus();
        }

        private void txtHintPass_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PasswordBox.Focus();
        }
    }
}