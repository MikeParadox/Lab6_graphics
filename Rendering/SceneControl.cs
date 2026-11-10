using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Lab6.ViewModels;

namespace Lab6.Rendering;

public sealed class SceneControl : Control
{
    private MainViewModel? _viewModel;

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_viewModel is not null)
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

        _viewModel = DataContext as MainViewModel;

        if (_viewModel is not null)
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        InvalidateVisual();
    }

    private void OnViewModelPropertyChanged(object? sender, EventArgs e) =>
        InvalidateVisual();

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (_viewModel is null)
            return;

        var background = new SolidColorBrush(Colors.White);
        context.FillRectangle(background, new Rect(0, 0, Bounds.Width, Bounds.Height));

        Renderer.Draw(
            _viewModel.Model,
            _viewModel.Camera,
            context,
            new Size(Bounds.Width, Bounds.Height));
    }
}
