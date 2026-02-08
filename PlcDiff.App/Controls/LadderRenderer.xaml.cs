using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PlcDiff.Core.Ladder;
using PlcDiff.Core.Models;

namespace PlcDiff.App.Controls;

public partial class LadderRenderer : UserControl
{
    public static readonly DependencyProperty TokensProperty = DependencyProperty.Register(
        nameof(Tokens),
        typeof(IEnumerable<LadderToken>),
        typeof(LadderRenderer),
        new PropertyMetadata(null, OnRenderPropertyChanged));

    public static readonly DependencyProperty ChangeKindProperty = DependencyProperty.Register(
        nameof(ChangeKind),
        typeof(ChangeKind),
        typeof(LadderRenderer),
        new PropertyMetadata(ChangeKind.None, OnRenderPropertyChanged));

    public LadderRenderer()
    {
        InitializeComponent();
        SizeChanged += (_, _) => Render();
    }

    public IEnumerable<LadderToken>? Tokens
    {
        get => (IEnumerable<LadderToken>?)GetValue(TokensProperty);
        set => SetValue(TokensProperty, value);
    }

    public ChangeKind ChangeKind
    {
        get => (ChangeKind)GetValue(ChangeKindProperty);
        set => SetValue(ChangeKindProperty, value);
    }

    private static void OnRenderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LadderRenderer renderer)
        {
            renderer.Render();
        }
    }

    private void Render()
    {
        if (Surface == null)
        {
            return;
        }

        Surface.Children.Clear();

        var width = ActualWidth > 0 ? ActualWidth : 300;
        var height = 100.0;
        Surface.Width = width - 8;
        Surface.Height = height;

        var leftRailX = 8.0;
        var rightRailX = Surface.Width - 8.0;
        var railBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80));

        Surface.Children.Add(new Line
        {
            X1 = leftRailX,
            X2 = leftRailX,
            Y1 = 4,
            Y2 = height - 4,
            Stroke = railBrush,
            StrokeThickness = 2
        });

        Surface.Children.Add(new Line
        {
            X1 = rightRailX,
            X2 = rightRailX,
            Y1 = 4,
            Y2 = height - 4,
            Stroke = railBrush,
            StrokeThickness = 2
        });

        var tokens = Tokens?.ToList() ?? new List<LadderToken>();
        if (tokens.Count == 0)
        {
            return;
        }

        var blockBrush = ResolveChangeBrush(ChangeKind);
        var blockWidth = 90.0;
        var blockHeight = 36.0;
        var spacing = 12.0;
        var startX = leftRailX + 12.0;
        var centerY = height / 2 - blockHeight / 2;

        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            var x = startX + i * (blockWidth + spacing);
            if (x + blockWidth > rightRailX - 8)
            {
                break;
            }

            var rect = new Rectangle
            {
                Width = blockWidth,
                Height = blockHeight,
                RadiusX = 4,
                RadiusY = 4,
                Fill = blockBrush,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, centerY);
            Surface.Children.Add(rect);

            var text = new TextBlock
            {
                Text = $"{token.Instruction}\n{token.Operand}",
                Foreground = Brushes.White,
                FontSize = 11,
                TextAlignment = TextAlignment.Center,
                Width = blockWidth,
                TextWrapping = TextWrapping.Wrap
            };

            Canvas.SetLeft(text, x);
            Canvas.SetTop(text, centerY + 4);
            Surface.Children.Add(text);
        }
    }

    private static Brush ResolveChangeBrush(ChangeKind changeKind)
    {
        return changeKind switch
        {
            ChangeKind.Added => Brushes.ForestGreen,
            ChangeKind.Removed => Brushes.IndianRed,
            ChangeKind.Modified => Brushes.Goldenrod,
            ChangeKind.Moved => Brushes.DodgerBlue,
            _ => Brushes.Gray
        };
    }
}
