using System.Globalization;
using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.Model;

namespace MedicalResultsTracker.Controls
{
    /// <summary>
    /// Простой график динамики одного показателя: полоса нормы, ломаная по значениям,
    /// точки, окрашенные по статусу. Рисуется средствами Microsoft.Maui.Graphics —
    /// сторонние библиотеки диаграмм не нужны.
    /// </summary>
    public sealed class TrendChartDrawable : IDrawable
    {
        public ParameterSeries? Series { get; set; }

        /// <summary>Компактный режим (спарклайн): без подписей осей.</summary>
        public bool Compact { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            IReadOnlyList<SeriesPoint>? points = Series?.Points;

            if (points is null || points.Count == 0)
            {
                DrawPlaceholder(canvas, dirtyRect, S.Trend_ChartNoData);
                return;
            }

            float leftPadding = Compact ? 4f : 44f;
            float rightPadding = Compact ? 4f : 12f;
            float topPadding = Compact ? 6f : 12f;
            float bottomPadding = Compact ? 6f : 22f;

            RectF plot = new(
                dirtyRect.X + leftPadding,
                dirtyRect.Y + topPadding,
                Math.Max(1f, dirtyRect.Width - leftPadding - rightPadding),
                Math.Max(1f, dirtyRect.Height - topPadding - bottomPadding));

            (double min, double max) = GetScale(points);

            if (max - min < double.Epsilon)
            {
                // Все значения одинаковые — раздвигаем шкалу, иначе линия ляжет на границу.
                double pad = Math.Abs(max) < double.Epsilon ? 1d : Math.Abs(max) * 0.1d;
                min -= pad;
                max += pad;
            }

            DrawRangeBand(canvas, plot, min, max);
            DrawLineAndPoints(canvas, plot, points, min, max);

            if (!Compact)
            {
                DrawLabels(canvas, dirtyRect, plot, points, min, max);
            }
        }

        private (double Min, double Max) GetScale(IReadOnlyList<SeriesPoint> points)
        {
            double min = points.Min(p => p.Value);
            double max = points.Max(p => p.Value);

            // Норму хочется видеть в кадре, но не любой ценой: у ферритина с нормой 30–300
            // и значениями 18–42 растяжка до 300 сплющила бы линию в неразличимую полоску.
            // Поэтому границу подтягиваем, только если она рядом с данными; иначе полоса
            // просто обрежется по краю графика.
            double span = max - min;
            double allowance = span > double.Epsilon
                ? span * 1.5d
                : Math.Max(Math.Abs(max) * 0.5d, 1d);

            if (Series?.RefMin is double refMin && refMin >= min - allowance && refMin <= max + allowance)
            {
                min = Math.Min(min, refMin);
                max = Math.Max(max, refMin);
            }

            if (Series?.RefMax is double refMax && refMax >= min - allowance && refMax <= max + allowance)
            {
                min = Math.Min(min, refMax);
                max = Math.Max(max, refMax);
            }

            double margin = (max - min) * 0.12d;

            return (min - margin, max + margin);
        }

        private void DrawRangeBand(ICanvas canvas, RectF plot, double min, double max)
        {
            if (Series?.RefMin is null && Series?.RefMax is null)
            {
                return;
            }

            // Полоса может уходить за пределы кадра — обрезаем её по графику,
            // тогда видно, что норма продолжается за краем, и масштаб не ломается.
            float top = Math.Clamp(ToY(Series?.RefMax ?? max, plot, min, max), plot.Top, plot.Bottom);
            float bottom = Math.Clamp(ToY(Series?.RefMin ?? min, plot, min, max), plot.Top, plot.Bottom);

            if (bottom - top < 0.5f)
            {
                return;
            }

            canvas.FillColor = StatusPalette.RangeBand;
            canvas.FillRectangle(plot.X, top, plot.Width, bottom - top);
        }

        private static void DrawLineAndPoints(
            ICanvas canvas,
            RectF plot,
            IReadOnlyList<SeriesPoint> points,
            double min,
            double max)
        {
            PointF[] screen = new PointF[points.Count];

            for (int i = 0; i < points.Count; i++)
            {
                float x = points.Count == 1
                    ? plot.Center.X
                    : plot.X + (plot.Width * i / (points.Count - 1));

                screen[i] = new PointF(x, ToY(points[i].Value, plot, min, max));
            }

            canvas.StrokeColor = StatusPalette.Line;
            canvas.StrokeSize = 2f;

            for (int i = 1; i < screen.Length; i++)
            {
                canvas.DrawLine(screen[i - 1], screen[i]);
            }

            for (int i = 0; i < screen.Length; i++)
            {
                canvas.FillColor = StatusPalette.For(points[i].Status);
                canvas.FillCircle(screen[i], i == screen.Length - 1 ? 5f : 3.5f);
            }
        }

        private void DrawLabels(
            ICanvas canvas,
            RectF dirtyRect,
            RectF plot,
            IReadOnlyList<SeriesPoint> points,
            double min,
            double max)
        {
            canvas.FontSize = 11f;
            canvas.FontColor = StatusPalette.Unknown;

            canvas.DrawString(
                Format(max),
                dirtyRect.X,
                plot.Y - 7f,
                plot.X - dirtyRect.X - 4f,
                14f,
                Microsoft.Maui.Graphics.HorizontalAlignment.Right,
                Microsoft.Maui.Graphics.VerticalAlignment.Center);

            canvas.DrawString(
                Format(min),
                dirtyRect.X,
                plot.Bottom - 7f,
                plot.X - dirtyRect.X - 4f,
                14f,
                Microsoft.Maui.Graphics.HorizontalAlignment.Right,
                Microsoft.Maui.Graphics.VerticalAlignment.Center);

            canvas.DrawString(
                DateDisplay.Compact(points[0].Date),
                plot.X,
                plot.Bottom + 4f,
                plot.Width / 2f,
                14f,
                Microsoft.Maui.Graphics.HorizontalAlignment.Left,
                Microsoft.Maui.Graphics.VerticalAlignment.Center);

            if (points.Count > 1)
            {
                canvas.DrawString(
                    DateDisplay.Compact(points[^1].Date),
                    plot.Center.X,
                    plot.Bottom + 4f,
                    plot.Width / 2f,
                    14f,
                    Microsoft.Maui.Graphics.HorizontalAlignment.Right,
                    Microsoft.Maui.Graphics.VerticalAlignment.Center);
            }
        }

        private static void DrawPlaceholder(ICanvas canvas, RectF dirtyRect, string text)
        {
            canvas.FontSize = 12f;
            canvas.FontColor = StatusPalette.Unknown;
            canvas.DrawString(
                text,
                dirtyRect.X,
                dirtyRect.Y,
                dirtyRect.Width,
                dirtyRect.Height,
                Microsoft.Maui.Graphics.HorizontalAlignment.Center,
                Microsoft.Maui.Graphics.VerticalAlignment.Center);
        }

        private static float ToY(double value, RectF plot, double min, double max)
        {
            double ratio = (value - min) / (max - min);

            return (float)(plot.Bottom - (ratio * plot.Height));
        }

        private static string Format(double value) => value.ToString("0.##", CultureInfo.CurrentCulture);
    }
}
