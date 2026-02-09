using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PlcDiff.Core.Ladder.Parsing;
using PlcDiff.Core.Ladder.Layout;
using PlcDiff.Core.Models;

namespace PlcDiff.App.Controls;

public partial class LadderRenderer : UserControl
{
    public static readonly DependencyProperty RawTextProperty = DependencyProperty.Register(
        nameof(RawText),
        typeof(string),
        typeof(LadderRenderer),
        new PropertyMetadata(string.Empty, OnRenderPropertyChanged));

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

    public string RawText
    {
        get => (string)GetValue(RawTextProperty);
        set => SetValue(RawTextProperty, value);
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

        var width = Math.Max(ActualWidth - 8, 200);
        var cellWidth = 110.0;
        var cellHeight = 60.0;
        var railOffset = 12.0;
        var railTop = 6.0;
        var railBottomPadding = 6.0;

        var columns = Math.Max(1, (int)Math.Floor((width - railOffset * 2) / cellWidth));
        var plan = BuildPlan();
        var rows = Math.Max(1, plan?.Rows ?? 1);
        var height = rows * cellHeight + railTop + railBottomPadding;
        Surface.Width = width;
        Surface.Height = height;

        var leftRailX = railOffset;
        var rightRailX = width - railOffset;
        var railBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80));

        Surface.Children.Add(new Line
        {
            X1 = leftRailX,
            X2 = leftRailX,
            Y1 = railTop,
            Y2 = height - railBottomPadding,
            Stroke = railBrush,
            StrokeThickness = 2
        });

        Surface.Children.Add(new Line
        {
            X1 = rightRailX,
            X2 = rightRailX,
            Y1 = railTop,
            Y2 = height - railBottomPadding,
            Stroke = railBrush,
            StrokeThickness = 2
        });

        var highlightBrush = ResolveChangeHighlight(ChangeKind);
        var lineBrush = new SolidColorBrush(Color.FromRgb(60, 60, 60));
        for (var row = 0; row < rows; row++)
        {
            var rowTop = railTop + row * cellHeight;
            var rowCenterY = rowTop + cellHeight / 2;

            Surface.Children.Add(new Line
            {
                X1 = leftRailX,
                X2 = rightRailX,
                Y1 = rowCenterY,
                Y2 = rowCenterY,
                Stroke = lineBrush,
                StrokeThickness = 1
            });

            if (highlightBrush != null)
            {
                var highlight = new Rectangle
                {
                    Width = rightRailX - leftRailX,
                    Height = cellHeight - 8,
                    Fill = highlightBrush
                };
                Canvas.SetLeft(highlight, leftRailX);
                Canvas.SetTop(highlight, rowTop + 4);
                Surface.Children.Add(highlight);
            }
        }

        if (plan == null || plan.Instructions.Count == 0)
        {
            return;
        }
        foreach (var rail in plan.Rails)
        {
            var branchStartX = leftRailX + rail.ColStart * cellWidth + 6;
            var branchEndX = leftRailX + rail.ColEnd * cellWidth + 6;
            var branchTop = railTop + rail.RowStart * cellHeight + cellHeight / 2;
            var branchBottom = railTop + rail.RowEnd * cellHeight + cellHeight / 2;

            Surface.Children.Add(new Line
            {
                X1 = branchStartX,
                X2 = branchStartX,
                Y1 = branchTop,
                Y2 = branchBottom,
                Stroke = lineBrush,
                StrokeThickness = 2
            });

            Surface.Children.Add(new Line
            {
                X1 = branchEndX,
                X2 = branchEndX,
                Y1 = branchTop,
                Y2 = branchBottom,
                Stroke = lineBrush,
                StrokeThickness = 2
            });
        }

        foreach (var instruction in plan.Instructions)
        {
            var rowCenterY = railTop + instruction.Row * cellHeight + cellHeight / 2;
            var cellLeft = leftRailX + instruction.Column * cellWidth + 12;
            DrawInstruction(instruction, cellLeft, rowCenterY);
        }
    }

    private void DrawInstruction(LadderInstructionPlacement placement, double x, double centerY)
    {
        switch (placement.Opcode.ToUpperInvariant())
        {
            case "XIC":
                DrawContact(placement.Operands, x, centerY, false);
                break;
            case "XIO":
                DrawContact(placement.Operands, x, centerY, true);
                break;
            case "OTE":
                DrawCoil(placement.Operands, x, centerY);
                break;
            case "TON":
                DrawTimer(placement.Operands, x, centerY);
                break;
        }
    }

    private void DrawContact(string operand, double x, double centerY, bool negated)
    {
        var height = 26.0;
        var width = 54.0;
        var left = x;
        var right = x + width;
        var top = centerY - height / 2;
        var bottom = centerY + height / 2;

        Surface.Children.Add(new Line
        {
            X1 = left,
            X2 = left,
            Y1 = top,
            Y2 = bottom,
            Stroke = Brushes.Black,
            StrokeThickness = 2
        });

        Surface.Children.Add(new Line
        {
            X1 = right,
            X2 = right,
            Y1 = top,
            Y2 = bottom,
            Stroke = Brushes.Black,
            StrokeThickness = 2
        });

        if (negated)
        {
            Surface.Children.Add(new Line
            {
                X1 = left - 3,
                X2 = right + 3,
                Y1 = bottom,
                Y2 = top,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            });
        }

        AddOperandText(operand, x, centerY + 16);
    }

    private void DrawCoil(string operand, double x, double centerY)
    {
        var diameter = 26.0;
        var left = x + 10;
        var top = centerY - diameter / 2;

        var ellipse = new Ellipse
        {
            Width = diameter,
            Height = diameter,
            Stroke = Brushes.Black,
            StrokeThickness = 2
        };

        Canvas.SetLeft(ellipse, left);
        Canvas.SetTop(ellipse, top);
        Surface.Children.Add(ellipse);

        AddOperandText(operand, x, centerY + 16);
    }

    private void DrawTimer(string operand, double x, double centerY)
    {
        var width = 70.0;
        var height = 30.0;
        var left = x;
        var top = centerY - height / 2;

        var rect = new Rectangle
        {
            Width = width,
            Height = height,
            Stroke = Brushes.Black,
            StrokeThickness = 1.5,
            Fill = Brushes.White
        };

        Canvas.SetLeft(rect, left);
        Canvas.SetTop(rect, top);
        Surface.Children.Add(rect);

        var label = new TextBlock
        {
            Text = "TON",
            FontWeight = FontWeights.SemiBold,
            FontSize = 11
        };
        Canvas.SetLeft(label, left + 4);
        Canvas.SetTop(label, top + 2);
        Surface.Children.Add(label);

        AddOperandText(operand, x, centerY + 16);
    }

    private void AddOperandText(string operand, double x, double y)
    {
        var text = new TextBlock
        {
            Text = operand,
            FontSize = 10,
            Foreground = Brushes.Black,
            Width = 90,
            TextTrimming = TextTrimming.CharacterEllipsis
        };

        Canvas.SetLeft(text, x - 4);
        Canvas.SetTop(text, y);
        Surface.Children.Add(text);
    }

    private static Brush? ResolveChangeHighlight(ChangeKind changeKind)
    {
        return changeKind switch
        {
            ChangeKind.Added => new SolidColorBrush(Color.FromArgb(50, 46, 139, 87)),
            ChangeKind.Removed => new SolidColorBrush(Color.FromArgb(50, 205, 92, 92)),
            ChangeKind.Modified => new SolidColorBrush(Color.FromArgb(50, 255, 215, 0)),
            ChangeKind.Moved => new SolidColorBrush(Color.FromArgb(50, 30, 144, 255)),
            _ => null
        };
    }

    private LadderLayoutPlan? BuildPlan()
    {
        if (string.IsNullOrWhiteSpace(RawText))
        {
            return null;
        }

        try
        {
            var parser = new LadderParser();
            var ast = parser.Parse(RawText);
            var planner = new LadderLayoutPlanner();
            return planner.Plan(ast);
        }
        catch
        {
            return null;
        }
    }
}
