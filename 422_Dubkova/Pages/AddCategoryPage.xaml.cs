using System;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace _422_Dubkova.Pages
{
    public partial class AddCategoryPage : Page
    {
        private Category _currentCategory;
        private bool _isEditMode;

        public AddCategoryPage()
        {
            InitializeComponent();
            _currentCategory = new Category();
            _isEditMode = false;
            DataContext = _currentCategory;
        }

        public AddCategoryPage(Category selectedCategory)
        {
            InitializeComponent();

            if (selectedCategory != null)
            {
                using (var db = new Entities())
                {
                    _currentCategory = db.Category.FirstOrDefault(c => c.ID == selectedCategory.ID);
                }

                if (_currentCategory == null)
                {
                    _currentCategory = new Category();
                    _isEditMode = false;
                }
                else
                {
                    _isEditMode = true;
                }
            }
            else
            {
                _currentCategory = new Category();
                _isEditMode = false;
            }

            DataContext = _currentCategory;
        }

        private void TBName_TextChanged(object sender, TextChangedEventArgs e)
        {
            NameHintText.Visibility = string.IsNullOrEmpty(TBName.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            _currentCategory.Name = TBName.Text;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentCategory.Name))
                errors.AppendLine("Укажите название категории!");

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
                        var categoryInDb = db.Category.FirstOrDefault(c => c.ID == _currentCategory.ID);
                        if (categoryInDb != null)
                        {
                            categoryInDb.Name = _currentCategory.Name;
                            db.Entry(categoryInDb).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        // ✅ создаем новый объект в контексте (чтобы EF не путался)
                        var newCategory = new Category
                        {
                            Name = _currentCategory.Name
                        };

                        // ✅ если ID не автоинкремент — генерируем его вручную
                        if (!db.Category.Any())
                            newCategory.ID = 1;
                        else
                            newCategory.ID = db.Category.Max(c => c.ID) + 1;

                        db.Category.Add(newCategory);
                    }

                    db.SaveChanges();
                }

                MessageBox.Show("Категория успешно сохранена!", "Успех",
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
            TBName.Text = "";
        }
    }
}
