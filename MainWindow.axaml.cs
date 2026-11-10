using Avalonia.Controls;
using Avalonia.Input;
using Lab6.Geometry;
using Lab6.Rendering;
using Lab6.ViewModels;

namespace Lab6;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();

    private bool _isDragging;
    private Avalonia.Point _previousPointer;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel.AttachWindow(this);
        DataContext = _viewModel;

        var scene = this.FindControl<SceneControl>("Scene");

        if (scene is null)
            throw new InvalidOperationException("Элемент Scene не найден в MainWindow.axaml.");

        scene.PointerPressed += OnScenePointerPressed;
        scene.PointerMoved += OnScenePointerMoved;
        scene.PointerReleased += OnScenePointerReleased;
        scene.PointerWheelChanged += OnScenePointerWheelChanged;
    }

    private void OnScenePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isDragging = true;
        _previousPointer = e.GetPosition((Control)sender!);
    }

    private void OnScenePointerReleased(object? sender, PointerReleasedEventArgs e) =>
        _isDragging = false;

    private void OnScenePointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging)
            return;

        var scene = (Control)sender!;
        var current = e.GetPosition(scene);

        double deltaX = current.X - _previousPointer.X;
        double deltaY = current.Y - _previousPointer.Y;
        _previousPointer = current;

        var rotation =
            Matrix4.RotationY(deltaX * 0.01) *
            Matrix4.RotationX(deltaY * 0.01);

        _viewModel.Model.Apply(rotation);
        scene.InvalidateVisual();
    }

    private void OnScenePointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        double factor = e.Delta.Y > 0 ? 1.1 : 1 / 1.1;

        var center = _viewModel.Model.Center();

        var matrix =
            Matrix4.Translation(-center.X, -center.Y, -center.Z) *
            Matrix4.Scale(factor, factor, factor) *
            Matrix4.Translation(center.X, center.Y, center.Z);

        _viewModel.Model.Apply(matrix);

        var scene = (Control)sender!;
        scene.InvalidateVisual();
    }
}
