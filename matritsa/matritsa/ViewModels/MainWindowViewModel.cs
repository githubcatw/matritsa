using ReactiveUI;
using System.Windows.Input;

namespace matritsa.ViewModels {
    public class MainWindowViewModel : ViewModelBase {
        public MainWindowViewModel() {
            ShowDialog = new Interaction<MainWindowViewModel, PrintPreviewWindow?>();

            BuyMusicCommand = ReactiveCommand.CreateFromTask(async () => {
                //var result = await ShowDialog.Handle(null);
            });
        }

        public ICommand BuyMusicCommand { get; }

        public Interaction<MainWindowViewModel, PrintPreviewWindow?> ShowDialog { get; }
    }
}
