using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using matritsa.ViewModels;
using Matritsa.PDFGenerator;
using Matritsa.PDFGenerator.Data;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Avalonia.Threading;
using Avalonia.Media;
using matritsa.Util;
using System.Web;

namespace matritsa.Views;

public partial class MainView : UserControl {

    /// <summary>
    /// Тип для CSV файлов
    /// </summary>
    public static FilePickerFileType CSV { get; } = new("CSV") {
        Patterns = ["*.csv"],
        AppleUniformTypeIdentifiers = ["public.comma-separated-values-text"],
        MimeTypes = ["text/csv"]
    };

    private PDFGenerator generator = new(new PDFOptions(PaperType.A4, new Dimensions<float>(15, 15, MeasurementUnit.Millimeter), 10));
    private CancellationTokenSource? genTaskCancel = null;

    public MainView() {
        InitializeComponent();
        
        PickCSV.Click += PickCSV_Click;
        SaveFile.Click += SaveFile_Click;
        Cancel.Click += Cancel_Click;

        LoadingCover.Background = new SolidColorBrush(new Color(0xaa, 0x0, 0x0, 0x0));
        Width.PointerEntered += (x, args) => {
            Preview.SetWholeVisibility(true);
        };
        Width.PointerExited += (x, args) => {
            Preview.SetWholeVisibility(false);
        };
        Height.PointerEntered += (x, args) => {
            Preview.SetWholeVisibility(true);
        };
        Height.PointerExited += (x, args) => {
            Preview.SetWholeVisibility(false);
        };
        /*
        MatPadding.PointerEntered += (x, args) => {
            Preview.SetBetweenVisibility(true);
        };
        MatPadding.PointerExited += (x, args) => {
            Preview.SetBetweenVisibility(false);
        };*/

        MatSize.PointerEntered += (x, args) => {
            Preview.SetCodesVisibility(true);
        };
        MatSize.PointerExited += (x, args) => {
            Preview.SetCodesVisibility(false);
        };

    }

    private void SaveFile_Click(object? sender, RoutedEventArgs e) {
        if (DataContext is MainViewModel viewModel) {
            if (viewModel.CSVFile != null) {
                genTaskCancel = new CancellationTokenSource();
                // проверяем параметры
                if (viewModel.PageWidth == null || viewModel.PageHeight == null ||
                    viewModel.MatrixFrameWidth == null || viewModel.MatrixFrameHeight == null ||
                    viewModel.MatrixSize == null) {
                    return;
                }
                // записываем параметры
                generator.Options = new PDFOptions(
                    new PaperType(
                        new Dimensions<float>(
                            (float)viewModel.PageWidth,
                            (float)viewModel.PageHeight,
                            MeasurementUnit.Millimeter
                        ),
                        10
                    ),
                    new Dimensions<float>(
                        (float)viewModel.MatrixFrameWidth,
                        (float)viewModel.MatrixFrameHeight,
                        MeasurementUnit.Millimeter
                    ),
                    (float)viewModel.MatrixSize
                );
                // создаем токен и фоновую задачу
                var token = genTaskCancel.Token;
                var genTask = new Task(() => GeneratePDF(HttpUtility.UrlDecode(viewModel.CSVFile.Replace("file:///", "")), token), token);
                // запускаем задачу
                genTask.Start();
            }
        }
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e) {
        genTaskCancel?.Cancel();
        LoadingCover.IsVisible = false;
    }

    private async void GeneratePDF(string url, CancellationToken token) {
        var myTopLevel = TopLevel.GetTopLevel(this);
        Uri? outPath = null;
        if (myTopLevel != null) {
            // запрашиваем файл
            var file = await myTopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions() {
                Title = "Сохранить файл",
                FileTypeChoices = [FilePickerFileTypes.Pdf]
            });
            // передаем его в viewmodel
            if (file == null) {
                return;
            }
            outPath = file.Path;
        }

        using var stream = File.Open(url, FileMode.Open);
        using StreamReader reader = new(stream);

        string fileContents = reader.ReadToEnd();

        stream.Close();

        await Dispatcher.UIThread.InvokeAsync(() => {
            LoadingCover.IsVisible = true;
        });
        try {
            var pdf = generator.Generate(
                fileContents.Split("\n", StringSplitOptions.RemoveEmptyEntries),
                string.Format("Коды продуктов - {0}", Path.GetFileNameWithoutExtension(url)),
                async (mat, progress) => {
                    await Dispatcher.UIThread.InvokeAsync(() => {
                        ProgressIndicator.Value = progress * 100;
                    });
                },
                token
            );
            if (outPath != null) {
                pdf.Save(HttpUtility.UrlDecode(outPath.LocalPath));
                Utils.OpenUrl(HttpUtility.UrlDecode(outPath.OriginalString));
            }
            Debug.WriteLine("Generated");
        } catch (OperationCanceledException) {
            Debug.WriteLine("Operation was canceled!");
        }

        await Dispatcher.UIThread.InvokeAsync(() => {
            LoadingCover.IsVisible = false;
        });
    }

    private async void PickCSV_Click(object? sender, RoutedEventArgs e) {
        var myTopLevel = TopLevel.GetTopLevel(this);
        if (myTopLevel != null) {
            // запрашиваем файл
            var file = await myTopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions() {
                Title = "Выбрать файл",
                FileTypeFilter = [CSV]
            });
            // передаем его в viewmodel
            if (file != null && DataContext is MainViewModel viewModel) {
                if (file.Count > 0) {
                    viewModel.SetCSVFile(file[0].Path.ToString());
                }
            }
        }
    }
}
