using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    /// Логика взаимодействия для PaymentTabPage.xaml
    /// </summary>
    public partial class PaymentTabPage : Page
    {
        private List<Payment> payments;

        public PaymentTabPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using (var db = new Entities())
            {
                payments = db.Payment
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .ToList();
                DataGridPayments.ItemsSource = payments;
            }
        }

        private void PaymentsTabPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
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
            NavigationService.Navigate(new AddPaymentPage());
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPayment = (Payment)((Button)sender).DataContext;
            NavigationService.Navigate(new AddPaymentPage(selectedPayment));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = DataGridPayments.SelectedItems.Cast<Payment>().ToList();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одну строку для удаления.");
                return;
            }

            if (MessageBox.Show($"Вы точно хотите удалить {selectedItems.Count} платеж(ей)?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new Entities())
                    {
                        foreach (var payment in selectedItems)
                        {
                            var paymentToDelete = db.Payment.Find(payment.ID);
                            if (paymentToDelete != null)
                                db.Payment.Remove(paymentToDelete);
                        }
                        db.SaveChanges();
                    }

                    MessageBox.Show("Платеж(и) успешно удалены!");
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
