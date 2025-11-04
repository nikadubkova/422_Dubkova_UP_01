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
using System.Windows.Threading;

namespace _422_Dubkova
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string currentUserRole = null;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (o, t) =>
            {
                DateTimeNow.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            };
            timer.Start();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
            else
            {
                WelcomePanel.Visibility = Visibility.Collapsed;
                MainFrame.Visibility = Visibility.Visible;

                if (currentUserRole == "admin")
                {
                    MainFrame.Navigate(new Pages.AdminPage());
                }
                else if (currentUserRole == "user")
                {
                    MainFrame.Navigate(new Pages.UserPage());
                }
                else
                {
                    // Если роль не определена, возвращаем на страницу авторизации
                    MainFrame.Navigate(new Pages.AuthPage());
                }

            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите закрыть окно?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
        private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeSelector.SelectedItem is ComboBoxItem selectedItem)
            {
                string themeFile = selectedItem.Tag.ToString();
                ChangeTheme(themeFile);
            }
        }

        private void ChangeTheme(string themeFile)
        {
            try
            {
                var uri = new Uri(themeFile, UriKind.Relative);
                ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;

                Application.Current.Resources.Clear();
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить тему: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            // Скрываем панель с приветствием и кнопкой
            WelcomePanel.Visibility = Visibility.Collapsed;

            // Показываем фрейм и навигируем на страницу авторизации
            MainFrame.Visibility = Visibility.Visible;
            MainFrame.Navigate(new Pages.AuthPage());
        }
    }
}
