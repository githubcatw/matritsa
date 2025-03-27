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
using matritsa.Properties;

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

    public MainView() {
        InitializeComponent();
        
        PickCSV.Click += PickCSV_Click;
        SaveFile.Click += SaveFile_Click;
        Cancel.Click += Cancel_Click;
        PrintPreview.Click += PrintPreview_Click;

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

    private void PrintPreview_Click(object? sender, RoutedEventArgs e) {
        if (DataContext is MainViewModel viewModel) {
            // проверяем параметры
            if ((!viewModel.IgnorePageSize && (viewModel.PageWidth == null || viewModel.PageHeight == null)) ||
                viewModel.MatrixFrameWidth == null || viewModel.MatrixFrameHeight == null ||
                viewModel.MatrixSize == null) {
                return;
            }
            viewModel.StartPrintPreview();
        }
    }

    private async void SaveFile_Click(object? sender, RoutedEventArgs e) {
        if (DataContext is MainViewModel viewModel) {
            if (viewModel.CSVFile != null) {
                // проверяем параметры
                if ((!viewModel.IgnorePageSize && (viewModel.PageWidth == null || viewModel.PageHeight == null)) ||
                    viewModel.MatrixFrameWidth == null || viewModel.MatrixFrameHeight == null ||
                    viewModel.MatrixSize == null) {
                    return;
                }
                var myTopLevel = TopLevel.GetTopLevel(this);
                Uri? outPath = null;
                if (myTopLevel != null) {
                    // запрашиваем файл
                    var file = await myTopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions() {
                        Title = Properties.Resources.dlgSaveFile,
                        FileTypeChoices = [FilePickerFileTypes.Pdf]
                    });
                    if (file == null) {
                        return;
                    }
                    outPath = file.Path;
                    viewModel.StartGeneration(outPath);
                }
            }
        }
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e) {
        if (DataContext is MainViewModel viewModel) {
            viewModel.CancelGeneration();
        }
    }

    private async void PickCSV_Click(object? sender, RoutedEventArgs e) {
        var myTopLevel = TopLevel.GetTopLevel(this);
        if (myTopLevel != null) {
            // запрашиваем файл
            var file = await myTopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions() {
                Title = Properties.Resources.dlgOpenCode,
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
