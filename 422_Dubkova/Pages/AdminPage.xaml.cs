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
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
        }

        private void BtnTabUsers_Click(object sender, RoutedEventArgs e)
        {
            AdminPanel.Visibility = Visibility.Collapsed;
            // Показываем фрейм и навигируем на страницу с пользователями
            AdminFrame.Visibility = Visibility.Visible;
            AdminFrame.Navigate(new UserTabPage());

        }
        private void BtnTabCategory_Click(object sender, RoutedEventArgs e)
        {
            AdminPanel.Visibility = Visibility.Collapsed;
            // Показываем фрейм и навигируем на страницу с категориями
            AdminFrame.Visibility = Visibility.Visible;
            AdminFrame.Navigate(new CategoryTabPage());
        }
        private void BtnTabPayment_Click(object sender, RoutedEventArgs e)
        {
            AdminPanel.Visibility = Visibility.Collapsed;
            // Показываем фрейм и навигируем на страницу с покупками
            AdminFrame.Visibility = Visibility.Visible;
            AdminFrame.Navigate(new PaymentTabPage());
        }
        private void BtnTabDiagram_Click(object sender, RoutedEventArgs e)
        {
            AdminPanel.Visibility = Visibility.Collapsed;
            // Показываем фрейм и навигируем на страницу с диаграммами
            AdminFrame.Visibility = Visibility.Visible;
            AdminFrame.Navigate(new DiagrammPage());
        }

      
    }
}
