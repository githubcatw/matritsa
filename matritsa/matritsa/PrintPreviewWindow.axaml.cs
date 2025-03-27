using Avalonia.Controls;
using Matritsa.PDFGenerator.Data;

namespace matritsa;

public partial class PrintPreviewWindow : Window
{
    private readonly MatrixBlock[] Blocks = new MatrixBlock[0];
    public PrintPreviewWindow() {
        InitializeComponent();
    }
    public PrintPreviewWindow(MatrixBlock[] blocks) : base() {
        this.Blocks = blocks;
    }
}