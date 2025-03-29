using Avalonia.Controls;
using matritsa.ViewModels;
using Matritsa.PDFGenerator.Data;

namespace matritsa;

public partial class PrintPreviewWindow : Window {

    public PrintPreviewWindow(PDFOptions options) {
        DataContext = new PrintPreviewWindowViewModel();
        ((PrintPreviewWindowViewModel)DataContext).SetOptions(options);
        InitializeComponent();

        ((PrintPreviewWindowViewModel)DataContext).StartRender();
    }
}