using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

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
                LoadData();
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            // Передаём колбэк для обновления таблицы после сохранения
            NavigationService.Navigate(new AddPaymentPage(OnPaymentSaved));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPayment = (Payment)((Button)sender).DataContext;
            // Передаём выбранный платеж и колбэк
            NavigationService.Navigate(new AddPaymentPage(selectedPayment, OnPaymentSaved));
        }

        // Метод обратного вызова — обновляет список платежей
        private void OnPaymentSaved()
        {
            LoadData();
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = DataGridPayments.SelectedItems.Cast<Payment>().ToList();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одну строку для удаления.");
                return;
            }

            if (MessageBox.Show($"Вы точно хотите удалить {selectedItems.Count} платеж(ей)?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
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
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
