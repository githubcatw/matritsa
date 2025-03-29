using Matritsa.PDFGenerator;
using Matritsa.PDFGenerator.Data;
using ReactiveUI;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace matritsa.ViewModels {
    public class PrintPreviewWindowViewModel : ViewModelBase {

        private CancellationTokenSource? rendTaskCancel = null;

        public void SetOptions(PDFOptions options) {
            LayoutEngine = new CodeLayoutEngine(options);
            this.RaisePropertyChanged(nameof(PageDimens));
        }

        public CodeLayoutEngine LayoutEngine { get; private set; }

        public string Status {
            get => _Status;
            private set {
                this.RaiseAndSetIfChanged(ref _Status, value);
                this.RaisePropertyChanged(nameof(Blocks));
            }
        }

        public MatrixBlock[] Blocks {
            get => _Blocks;
            private set {
                this.RaiseAndSetIfChanged(ref _Blocks, value);
            }
        }

        public Dimensions<float> PageDimens => LayoutEngine?.Options.PaperType.Size;

        private string _Status = "meow";
        private MatrixBlock[] _Blocks = [];

        internal void StartRender() {
            rendTaskCancel = new CancellationTokenSource();
            // создаем токен и фоновую задачу
            var token = rendTaskCancel.Token;
            var rendTask = new Task(() => Render(
                token
            ), token);
            // запускаем задачу
            rendTask.Start();
        }

        private void Render(CancellationToken token) {
            SetStatus("Creating layout");
            Blocks = LayoutEngine.LayoutMultiple(
                [],
                null,
                token,
                true
            )[0];
            SetStatus("Layout created");
        }

        private void SetStatus(string v) {
            Status = v;
            Debug.WriteLine("Status: " + Status);
        }
    }
}
