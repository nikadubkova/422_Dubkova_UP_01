using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace _422_Dubkova.Pages
{
    /// <summary>
    /// Логика взаимодействия для CategoryTabPage.xaml
    /// </summary>
    public partial class CategoryTabPage : Page
    {
        private List<Category> categories;

        public CategoryTabPage()
        {
            InitializeComponent();
            LoadData();
        }

        public void LoadData()
        {
            using (var db = new Entities())
            {
                categories = db.Category.ToList();
                DataGridCategories.ItemsSource = categories;
            }
        }

        private void CategoriesTabPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
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
            // ✅ Передаём ссылку на текущую страницу, чтобы потом обновить список
            NavigationService.Navigate(new AddCategoryPage(this));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedCategory = (Category)((Button)sender).DataContext;
            NavigationService.Navigate(new AddCategoryPage(selectedCategory, this));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = DataGridCategories.SelectedItems.Cast<Category>().ToList();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одну категорию для удаления.");
                return;
            }

            if (MessageBox.Show($"Вы точно хотите удалить {selectedItems.Count} категорию(ии)?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new Entities())
                    {
                        foreach (var category in selectedItems)
                        {
                            var categoryToDelete = db.Category.Find(category.ID);
                            if (categoryToDelete != null)
                                db.Category.Remove(categoryToDelete);
                        }
                        db.SaveChanges();
                    }

                    MessageBox.Show("Категория(ии) успешно удалены!");
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
