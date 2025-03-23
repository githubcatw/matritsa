using Avalonia;
using Avalonia.Controls;

namespace matritsa;

public partial class MiniLayoutPreview : UserControl {

    public bool EnableIPS {
        get => GetValue(EnableIPSProperty);
        set => SetValue(EnableIPSProperty, value);
    }

    public static readonly StyledProperty<bool> EnableIPSProperty =
            AvaloniaProperty.Register<MiniLayoutPreview, bool>(nameof(EnableIPS), defaultValue: false);

    public MiniLayoutPreview()
    {
        InitializeComponent();
    }

    public void SetHBetweenVisibility(bool visible) {
        BetweenHPadding.IsVisible = visible;
    }
    public void SetVBetweenVisibility(bool visible) {
        BetweenVPadding.IsVisible = visible;
    }
    public void SetBetweenVisibility(bool visible) {
        BetweenHPadding.IsVisible = visible;
        BetweenVPadding.IsVisible = visible;
    }

    public void SetStartVisibility(bool visible) {
        StartPadding.IsVisible = visible;
    }
    public void SetTopVisibility(bool visible) {
        TopPadding.IsVisible = visible;
    }
    public void SetAroundVisibility(bool visible) {
        StartPadding.IsVisible = visible;
        TopPadding.IsVisible = visible;
    }

    public void SetWholeVisibility(bool visible) {
        PageSize.IsVisible = visible;
    }

    public void SetCodesVisibility(bool visible) {
        Code1High.IsVisible = visible;
        Code2High.IsVisible = visible;
        Code3High.IsVisible = visible;
        Code4High.IsVisible = visible;
    }
}