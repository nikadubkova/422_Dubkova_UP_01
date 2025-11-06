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
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Forms.DataVisualization.Charting;

namespace _422_Dubkova.Pages
{
    /// <summary>
    /// Логика взаимодействия для DiagrammPage.xaml
    /// </summary>
    public partial class DiagrammPage : Page
    {
        private Entities _context = new Entities();

        public DiagrammPage()
        {
            InitializeComponent();

            // Настройка диаграммы
            ChartPayments.ChartAreas.Add(new ChartArea("Main"));

            var currentSeries = new Series("Платежи")
            {
                IsValueShownAsLabel = true
            };
            ChartPayments.Series.Add(currentSeries);

            // Загрузка данных в ComboBox
            CmbUser.ItemsSource = _context.User.ToList();
            CmbDiagram.ItemsSource = System.Enum.GetValues(typeof(SeriesChartType));

            // Опционально, установить выбранные по умолчанию элементы
            if (CmbUser.Items.Count > 0) CmbUser.SelectedIndex = 0;
            if (CmbDiagram.Items.Count > 0) CmbDiagram.SelectedIndex = 0;
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (CmbUser.SelectedItem is User currentUser &&
                CmbDiagram.SelectedItem is SeriesChartType currentType)
            {
                Series currentSeries = ChartPayments.Series.FirstOrDefault();
                if (currentSeries == null)
                    return;

                currentSeries.ChartType = currentType;
                currentSeries.Points.Clear();

                var categoriesList = _context.Category.ToList();

                foreach (var category in categoriesList)
                {
                    var sum = _context.Payment
                        .Where(p => p.UserID == currentUser.ID && p.CategoryID == category.ID)
                        .Sum(p => (decimal?)(p.Price * p.Num)) ?? 0;

                    currentSeries.Points.AddXY(category.Name, sum);
                }
            }
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            // Получаем отсортированный список пользователей
            var allUsers = _context.User.ToList().OrderBy(u => u.FIO).ToList();

            var application = new Excel.Application();
            application.SheetsInNewWorkbook = allUsers.Count;
            Excel.Workbook workbook = application.Workbooks.Add(Type.Missing);

            double grandTotal = 0; // Общая сумма по всем пользователям

            for (int i = 0; i < allUsers.Count; i++)
            {
                int startRowIndex = 1;
                Excel.Worksheet worksheet = application.Worksheets[i + 1];
                worksheet.Name = allUsers[i].FIO;

                // Заголовки колонок
                worksheet.Cells[startRowIndex, 1] = "Дата платежа";
                worksheet.Cells[startRowIndex, 2] = "Название";
                worksheet.Cells[startRowIndex, 3] = "Стоимость";
                worksheet.Cells[startRowIndex, 4] = "Количество";
                worksheet.Cells[startRowIndex, 5] = "Сумма";

                Excel.Range columnHeaderRange = worksheet.Range[worksheet.Cells[startRowIndex, 1], worksheet.Cells[startRowIndex, 5]];
                columnHeaderRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                columnHeaderRange.Font.Bold = true;

                startRowIndex++;

                // Группируем платежи пользователя по категориям, сортируем по дате
                var userCategories = allUsers[i].Payment.OrderBy(p => p.Date).GroupBy(p => p.Category).OrderBy(g => g.Key.Name);

                foreach (var groupCategory in userCategories)
                {
                    // Название категории (объединённая ячейка)
                    Excel.Range headerRange = worksheet.Range[worksheet.Cells[startRowIndex, 1], worksheet.Cells[startRowIndex, 5]];
                    headerRange.Merge();
                    headerRange.Value = groupCategory.Key.Name;
                    headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    headerRange.Font.Italic = true;

                    startRowIndex++;

                    // Платежи по категории
                    foreach (var payment in groupCategory)
                    {
                        worksheet.Cells[startRowIndex, 1] = payment.Date.ToString("dd.MM.yyyy");
                        worksheet.Cells[startRowIndex, 2] = payment.Name;

                        worksheet.Cells[startRowIndex, 3] = payment.Price;
                        (worksheet.Cells[startRowIndex, 3] as Excel.Range).NumberFormat = "0.00";

                        worksheet.Cells[startRowIndex, 4] = payment.Num;

                        // Формула для суммы: Цена * Кол-во
                        worksheet.Cells[startRowIndex, 5].Formula = $"=C{startRowIndex}*D{startRowIndex}";
                        (worksheet.Cells[startRowIndex, 5] as Excel.Range).NumberFormat = "0.00";

                        startRowIndex++;
                    }

                    // Итого по категории (объединяем 1-4 столбцы)
                    Excel.Range sumRange = worksheet.Range[worksheet.Cells[startRowIndex, 1], worksheet.Cells[startRowIndex, 4]];
                    sumRange.Merge();
                    sumRange.Value = "ИТОГО:";
                    sumRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                    // Формула суммы по столбцу E для текущей категории
                    worksheet.Cells[startRowIndex, 5].Formula = $"=SUM(E{startRowIndex - groupCategory.Count()}:" +
                                                              $"E{startRowIndex - 1})";

                    sumRange.Font.Bold = true;
                    worksheet.Cells[startRowIndex, 5].Font.Bold = true;

                    startRowIndex++;

                    // Границы таблицы
                    Excel.Range rangeBorders = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[startRowIndex - 1, 5]];
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlInsideHorizontal].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlInsideVertical].LineStyle = Excel.XlLineStyle.xlContinuous;
                }

                worksheet.Columns.AutoFit();

                // Считаем сумму всех платежей пользователя и добавляем в общий итог
                Excel.Range lastRowSumCell = worksheet.Cells[startRowIndex - 1, 5];
                var cellValue = lastRowSumCell.Value2;
                double cellDouble = 0;

                if (cellValue != null)
                {
                    if (cellValue is double)
                    {
                        cellDouble = (double)cellValue;
                    }
                    else
                    {
                        double.TryParse(cellValue.ToString(), out cellDouble);
                    }
                }

                grandTotal += cellDouble;
            }

            // Добавляем лист с общим итогом после всех пользователей
            Excel.Worksheet summarySheet = workbook.Worksheets.Add(After: workbook.Worksheets[workbook.Worksheets.Count]);
            summarySheet.Name = "Общий итог";

            summarySheet.Cells[1, 1] = "Общий итог:";
            summarySheet.Cells[1, 2] = grandTotal;

            Excel.Range summaryRange = summarySheet.Range[summarySheet.Cells[1, 1], summarySheet.Cells[1, 2]];
            summaryRange.Font.Color = Excel.XlRgbColor.rgbRed;
            summaryRange.Font.Bold = true;

            summarySheet.Columns.AutoFit();

            application.Visible = true;
        }

        private void ExportToWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allUsers = _context.User.ToList();
                var allCategories = _context.Category.ToList();

                var application = new Word.Application();
                Word.Document document = application.Documents.Add();

                foreach (var user in allUsers)
                {
                    // Абзац с ФИО пользователя — заголовок
                    Word.Paragraph userParagraph = document.Paragraphs.Add();
                    Word.Range userRange = userParagraph.Range;
                    userRange.Text = user.FIO;

                    // Проверьте у себя стиль: "Заголовок" или "Заголовок1"
                    try
                    {
                        userParagraph.set_Style("Заголовок");
                    }
                    catch
                    {
                        userParagraph.set_Style("Заголовок1");
                    }

                    userRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    userRange.InsertParagraphAfter();

                    // Пустая строка
                    document.Paragraphs.Add();

                    // Абзац для таблицы с платежами
                    Word.Paragraph tableParagraph = document.Paragraphs.Add();
                    Word.Range tableRange = tableParagraph.Range;

                    Word.Table paymentsTable = document.Tables.Add(tableRange, allCategories.Count() + 1, 2);
                    paymentsTable.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    paymentsTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    paymentsTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                    // Заголовки таблицы
                    Word.Range cellRange = paymentsTable.Cell(1, 1).Range;
                    cellRange.Text = "Категория";
                    cellRange = paymentsTable.Cell(1, 2).Range;
                    cellRange.Text = "Сумма расходов";

                    paymentsTable.Rows[1].Range.Font.Name = "Times New Roman";
                    paymentsTable.Rows[1].Range.Font.Size = 14;
                    paymentsTable.Rows[1].Range.Bold = 1;
                    paymentsTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    // Заполнение данных по категориям
                    for (int i = 0; i < allCategories.Count(); i++)
                    {
                        var currentCategory = allCategories[i];

                        cellRange = paymentsTable.Cell(i + 2, 1).Range;
                        cellRange.Text = currentCategory.Name;
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;

                        decimal sumPayments = user.Payment
                            .Where(p => p.Category == currentCategory)
                            .Sum(p => p.Num * p.Price);

                        cellRange = paymentsTable.Cell(i + 2, 2).Range;
                        cellRange.Text = sumPayments.ToString("N2") + " руб.";
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;
                    }

                    // Пустая строка после таблицы
                    document.Paragraphs.Add();

                    // Самый дорогой платеж
                    var maxPayment = user.Payment.OrderByDescending(p => p.Price * p.Num).FirstOrDefault();
                    if (maxPayment != null)
                    {
                        Word.Paragraph maxPaymentParagraph = document.Paragraphs.Add();
                        Word.Range maxPaymentRange = maxPaymentParagraph.Range;
                        maxPaymentRange.Text = $"Самый дорогостоящий платеж - {maxPayment.Name} за {(maxPayment.Price * maxPayment.Num).ToString("N2")} руб. от {maxPayment.Date:dd.MM.yyyy}";

                        try
                        {
                            maxPaymentParagraph.set_Style("Подзаголовок");
                        }
                        catch
                        {
                            maxPaymentParagraph.set_Style("Заголовок2");
                        }

                        maxPaymentRange.Font.Color = Word.WdColor.wdColorDarkRed;
                        maxPaymentRange.InsertParagraphAfter();
                    }

                    // Самый дешевый платеж
                    var minPayment = user.Payment.OrderBy(p => p.Price * p.Num).FirstOrDefault();
                    if (minPayment != null)
                    {
                        Word.Paragraph minPaymentParagraph = document.Paragraphs.Add();
                        Word.Range minPaymentRange = minPaymentParagraph.Range;
                        minPaymentRange.Text = $"Самый дешевый платеж - {minPayment.Name} за {(minPayment.Price * minPayment.Num).ToString("N2")} руб. от {minPayment.Date:dd.MM.yyyy}";

                        try
                        {
                            minPaymentParagraph.set_Style("Подзаголовок");
                        }
                        catch
                        {
                            minPaymentParagraph.set_Style("Заголовок2");
                        }

                        minPaymentRange.Font.Color = Word.WdColor.wdColorDarkGreen;
                        minPaymentRange.InsertParagraphAfter();
                    }

                    // Пустая строка
                    document.Paragraphs.Add();

                    // Разрыв страницы, кроме для последнего пользователя
                    if (user != allUsers.LastOrDefault())
                    {
                        document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                    }
                }
                // Добавляем нижний колонтитул с номером страницы по центру
                foreach (Word.Section section in document.Sections)
                {
                    Word.HeaderFooter footer = section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                    footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter);
                }

                // Альтернативный способ добавить поле "Текущая страница из общего количества страниц" (по желанию)
                /*
                document.ActiveWindow.ActivePane.View.SeekView = Word.WdSeekView.wdSeekCurrentPageFooter;

                object oMissing = System.Reflection.Missing.Value;
                object currentPageField = Word.WdFieldType.wdFieldPage;
                object totalPagesField = Word.WdFieldType.wdFieldNumPages;

                document.ActiveWindow.Selection.Fields.Add(document.ActiveWindow.Selection.Range, ref currentPageField, ref oMissing, ref oMissing);
                document.ActiveWindow.Selection.TypeText(" из ");
                document.ActiveWindow.Selection.Fields.Add(document.ActiveWindow.Selection.Range, ref totalPagesField, ref oMissing, ref oMissing);

                document.ActiveWindow.Selection.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                document.ActiveWindow.Selection.Sections[1].Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range.Select();

                document.ActiveWindow.View.SeekView = Word.WdSeekView.wdSeekMainDocument;
                */

                // Добавляем верхний колонтитул с текущей датой, выровненный по центру
                foreach (Word.Section section in document.Sections)
                {
                    Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.Text = DateTime.Now.ToString("dd/MM/yyyy");
                    headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    headerRange.Font.ColorIndex = Word.WdColorIndex.wdBlack;
                    headerRange.Font.Size = 10;
                }

                // Показать документ Word пользователю
                application.Visible = true;

                // Сохранение документа (укажите свои пути)
                string pathDocx = @"D:\Payments.docx";
                string pathPdf = @"D:\Payments.pdf";

                document.SaveAs2(pathDocx);
                document.SaveAs2(pathPdf, Word.WdExportFormat.wdExportFormatPDF);

                // Не закрывать приложение Word, чтобы пользователь мог работать с документом
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при экспорте в Word: " + ex.Message);
            }
        }
    }
}
