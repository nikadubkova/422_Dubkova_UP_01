using System;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace _422_Dubkova.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddPaymentPage.xaml
    /// </summary>
    public partial class AddPaymentPage : Page
    {
        private Payment _currentPayment;
        private bool _isEditMode;

        public AddPaymentPage()
        {
            InitializeComponent();
            _currentPayment = new Payment
            {
                Date = DateTime.Today
            };
            _isEditMode = false;
            DataContext = _currentPayment;

            LoadComboBoxes();
        }

        public AddPaymentPage(Payment paymentToEdit)
        {
            InitializeComponent();

            if (paymentToEdit != null)
            {
                _isEditMode = true;
                _currentPayment = paymentToEdit;
            }
            else
            {
                _isEditMode = false;
                _currentPayment = new Payment { Date = DateTime.Today };
            }

            DataContext = _currentPayment;
            LoadComboBoxes();
        }

        private void LoadComboBoxes()
        {
            using (var db = new Entities())
            {
                cmbUser.ItemsSource = db.User.ToList();
                cmbCategory.ItemsSource = db.Category.ToList();
            }
        }

        private void cmbUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbUser.SelectedItem is User selectedUser)
                _currentPayment.User = selectedUser;
        }

        private void TBName_TextChanged(object sender, TextChangedEventArgs e)
        {
            NameHintText.Visibility = string.IsNullOrEmpty(TBName.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
            _currentPayment.Name = TBName.Text;
        }

        private void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCategory.SelectedItem is Category selectedCategory)
                _currentPayment.Category = selectedCategory;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var errors = new StringBuilder();

            if (_currentPayment.Date == null)
                errors.AppendLine("Укажите дату!");
            if (_currentPayment.User == null)
                errors.AppendLine("Выберите пользователя!");
            if (string.IsNullOrWhiteSpace(_currentPayment.Name))
                errors.AppendLine("Укажите название!");
            if (_currentPayment.Num <= 0)
                errors.AppendLine("Количество должно быть больше 0!");
            if (_currentPayment.Price <= 0)
                errors.AppendLine("Сумма должна быть больше 0!");
            if (_currentPayment.Category == null)
                errors.AppendLine("Выберите категорию!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка ввода",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new Entities())
                {
                    if (_isEditMode)
                    {
                        var paymentInDb = db.Payment
                            .Include(p => p.User)
                            .Include(p => p.Category)
                            .FirstOrDefault(p => p.ID == _currentPayment.ID);

                        if (paymentInDb != null)
                        {
                            paymentInDb.Date = _currentPayment.Date;
                            paymentInDb.Name = _currentPayment.Name;
                            paymentInDb.Num = _currentPayment.Num;
                            paymentInDb.Price = _currentPayment.Price;
                            paymentInDb.UserID = _currentPayment.User.ID;
                            paymentInDb.CategoryID = _currentPayment.Category.ID;

                            db.Entry(paymentInDb).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        // ✅ создаем новый объект в контексте (чистый, не связанный с другим контекстом)
                        var newPayment = new Payment
                        {
                            Date = _currentPayment.Date,
                            Name = _currentPayment.Name,
                            Num = _currentPayment.Num,
                            Price = _currentPayment.Price,
                            UserID = _currentPayment.User.ID,
                            CategoryID = _currentPayment.Category.ID
                        };

                        // ✅ если нет автоинкремента, выставляем ID вручную
                        if (db.Payment.Any())
                            newPayment.ID = db.Payment.Max(p => p.ID) + 1;
                        else
                            newPayment.ID = 1;

                        db.Payment.Add(newPayment);
                    }

                    db.SaveChanges();
                }

                MessageBox.Show("Платёж успешно сохранён!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            DPDate.Text = null;
            cmbUser.SelectedItem = null;
            TBName.Text = "";
            TBNum.Text = "";
            TBPrice.Text = "";
            cmbCategory.SelectedItem = null;
        }

        private void TBNum_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TBPrice_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
