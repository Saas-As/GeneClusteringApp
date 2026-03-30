using GeneClusteringApp.Clustering;
using GeneClusteringApp.Data;
using GeneClusteringApp.Evaluation;
using GeneClusteringApp.Utils;
using GeneClusteringWPFApp.Services;
using GeneClusteringWPFApp.ViewModels;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GeneClusteringWPFApp
{
    public partial class MainWindow : Window
    {
        // Поля для хранения результатов
        private GeneMatrix _currentData;
        private int[] _currentLabels;
        private double[][] _currentCentroids;
        private double _currentInertia;
        private double _currentSilhouette;
        private int _currentK;
        private int _currentIterations;
        private long _currentTimeMs;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FilePathTextBox.Text))
            {
                MessageBox.Show("Выберите файл с данными", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            RunButton.IsEnabled = false;
            StatusText.Text = "Загрузка данных...";

            try
            {
                // 1. Загрузка данных
                var data = CSVLoader.LoadFromCsv(FilePathTextBox.Text);
                _currentData = data;

                GenesInfo.Text = $"Генов: {data.GeneCount}";
                SamplesInfo.Text = $"Образцов: {data.SampleCount}";

                // 2. Нормализация
                StatusText.Text = "Нормализация...";
                var normalizedData = NormalizeData(data);

                // 3. Получение k
                int k;
                KSelection.KSelectionResult kSelection = null;

                if (int.TryParse(KTextBox.Text, out int userK) && userK > 0)
                {
                    k = userK;
                    KInfo.Text = $"k: {k} (вручную)";
                }
                else
                {
                    StatusText.Text = "Выбор оптимального k...";
                    kSelection = KSelection.SelectK(normalizedData, minK: 2, maxK: 10);
                    KSelection.PrintResult(kSelection);
                    k = kSelection.RecommendedK;
                    KInfo.Text = $"Оптимальное k: {k} (силуэт = {kSelection.Silhouettes[k]:F4})";
                }

                // 4. Кластеризация
                StatusText.Text = "Кластеризация...";
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var kmeans = new KMeans(k, useKMeansPlusPlus: true);
                var (labels, centroids) = kmeans.ClusterWithCentroids(normalizedData);
                stopwatch.Stop();

                // Сохраняем результаты
                _currentLabels = labels;
                _currentCentroids = centroids;
                _currentK = k;
                _currentIterations = kmeans.LastIterationCount;
                _currentTimeMs = stopwatch.ElapsedMilliseconds;

                // 5. Оценка качества
                double silhouette = SilhouetteAnalyzer.AverageSilhouette(normalizedData, labels, k);
                double inertia = InertiaCalculator.Calculate(normalizedData, labels, centroids);

                _currentSilhouette = silhouette;
                _currentInertia = inertia;

                SilhouetteInfo.Text = $"Silhouette: {silhouette:F4} - {SilhouetteAnalyzer.Interpret(silhouette)}";
                TimeInfo.Text = $"Время: {stopwatch.ElapsedMilliseconds} мс";

                // 6. Размеры кластеров
                int[] clusterSizes = new int[k];
                for (int i = 0; i < labels.Length; i++)
                    clusterSizes[labels[i]]++;

                ClusterSizesList.Items.Clear();
                for (int i = 0; i < k; i++)
                {
                    double percent = (double)clusterSizes[i] / labels.Length * 100;
                    ClusterSizesList.Items.Add($"Кластер {i}: {clusterSizes[i]} генов ({percent:F1}%)");
                }

                // 7. Визуализация PCA
                StatusText.Text = "Построение PCA графика...";
                var pcaResult = VisualizationService.PerformPCA(normalizedData, labels);
                var pcaPlot = VisualizationService.CreatePCAPlot(pcaResult);
                ((MainViewModel)DataContext).PlotModel = pcaPlot;

                // 8. Включаем кнопку сохранения
                SaveButton.IsEnabled = true;

                StatusText.Text = "Готов";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Ошибка";
            }
            finally
            {
                RunButton.IsEnabled = true;
            }
        }

        private GeneMatrix NormalizeData(GeneMatrix data)
        {
            string selected = (NormalizationComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            return selected switch
            {
                "Z-score" => Normalizer.ZScoreNormalize(data),
                "Min-Max" => Normalizer.MinMaxNormalize(data),
                "Log2" => Normalizer.Log2Normalize(data),
                _ => data
            };
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*",
                Title = "Выберите файл с данными экспрессии генов"
            };

            if (dialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = dialog.FileName;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentData == null || _currentLabels == null)
            {
                MessageBox.Show("Нет результатов для сохранения. Сначала выполните кластеризацию.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveButton.IsEnabled = false;
            StatusText.Text = "Сохранение результатов...";

            try
            {
                // Используем SaveFileDialog для выбора папки (обходной путь без WinForms)
                var dialog = new SaveFileDialog
                {
                    Title = "Выберите папку для сохранения (укажите любое имя файла)",
                    Filter = "Папка|*.tmp",
                    FileName = "select_folder.tmp"
                };

                if (dialog.ShowDialog() == true)
                {
                    string folder = System.IO.Path.GetDirectoryName(dialog.FileName);

                    var exportResult = await Task.Run(() =>
                        ResultExporter.ExportAll(
                            _currentData,
                            _currentLabels,
                            _currentCentroids,
                            _currentInertia,
                            _currentSilhouette,
                            _currentK,
                            _currentIterations,
                            _currentTimeMs,
                            folder
                        ));

                    if (exportResult.Success)
                    {
                        MessageBox.Show($"Результаты сохранены в папку:\n{folder}\n\n" +
                            $"Созданы файлы:\n" +
                            $"  - {System.IO.Path.GetFileName(exportResult.LabelsCsvPath)}\n" +
                            $"  - {System.IO.Path.GetFileName(exportResult.CentroidsCsvPath)}\n" +
                            $"  - {System.IO.Path.GetFileName(exportResult.ReportTxtPath)}\n" +
                            $"  - {System.IO.Path.GetFileName(exportResult.ClusterGenesTxtPath)}",
                            "Сохранение завершено", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка сохранения: {exportResult.ErrorMessage}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SaveButton.IsEnabled = true;
                StatusText.Text = "Готов";
            }
        }
    }
}