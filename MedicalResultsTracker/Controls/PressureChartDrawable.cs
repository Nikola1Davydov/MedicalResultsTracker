using System.Globalization;
using MedicalResultsTracker.Model;
using MedicalResultsTracker.Resources.Strings;

namespace MedicalResultsTracker.Controls
{
    /// <summary>
    /// График давления: две ломаные на одном полотне и общая вертикальная шкала.
    ///
    /// Отдельный рисователь, а не доработка <see cref="TrendChartDrawable"/>. Тот устроен
    /// под один ряд с полосой нормы вокруг него; давление — это пара значений, которые
    /// имеют смысл только рядом, и интересно в них не «выше или ниже», а сходятся линии
    /// или расходятся. Переделка одного рисователя в «N рядов» задела бы работающие
    /// графики анализов ради задачи с другой геометрией.
    /// </summary>
    public sealed class PressureChartDrawable : IDrawable
    {
        /// <summary>Измерения от старых к новым: слева направо читается как хронология.</summary>
        public IReadOnlyList<BloodPressureReading> Readings { get; set; } = Array.Empty<BloodPressureReading>();

        public int TargetSystolic { get; set; }

        public int TargetDiastolic { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            IReadOnlyList<BloodPressureReading> readings = Readings;

            if (readings.Count == 0)
            {
                DrawPlaceholder(canvas, dirtyRect, S.Bp_ChartNoData);
                return;
            }

            RectF plot = new(
                dirtyRect.X + 40f,
                dirtyRect.Y + 12f,
                Math.Max(1f, dirtyRect.Width - 52f),
                Math.Max(1f, dirtyRect.Height - 34f));

            (double min, double max) = GetScale(readings);

            DrawTargetLines(canvas, plot, min, max);

            DrawSeries(canvas, plot, readings, min, max, r => r.Systolic, StatusPalette.High);
            DrawSeries(canvas, plot, readings, min, max, r => r.Diastolic, StatusPalette.Low);

            DrawLabels(canvas, dirtyRect, plot, readings, min, max);
        }

        /// <summary>
        /// Шкала охватывает оба ряда и оба порога: смысл графика в том, где линии проходят
        /// относительно черты, а не сами по себе.
        /// </summary>
        private (double Min, double Max) GetScale(IReadOnlyList<BloodPressureReading> readings)
        {
            double min = readings.Min(r => (double)r.Diastolic);
            double max = readings.Max(r => (double)r.Systolic);

            if (TargetDiastolic > 0)
            {
                min = Math.Min(min, TargetDiastolic);
            }

            if (TargetSystolic > 0)
            {
                max = Math.Max(max, TargetSystolic);
            }

            double margin = Math.Max((max - min) * 0.12d, 4d);

            return (min - margin, max + margin);
        }

        /// <summary>
        /// Пороговые черты — пунктиром: это не измерение, а ориентир, который человек
        /// вписал сам. Сплошная линия читалась бы как ещё один ряд данных.
        /// </summary>
        private void DrawTargetLines(ICanvas canvas, RectF plot, double min, double max)
        {
            canvas.StrokeSize = 1f;
            canvas.StrokeDashPattern = new float[] { 4f, 4f };

            DrawTarget(canvas, plot, min, max, TargetSystolic, StatusPalette.High);
            DrawTarget(canvas, plot, min, max, TargetDiastolic, StatusPalette.Low);

            canvas.StrokeDashPattern = null;
        }

        private static void DrawTarget(ICanvas canvas, RectF plot, double min, double max, int target, Color color)
        {
            if (target <= 0)
            {
                return;
            }

            float y = ToY(target, plot, min, max);

            if (y < plot.Top || y > plot.Bottom)
            {
                return;
            }

            canvas.StrokeColor = color.WithAlpha(0.45f);
            canvas.DrawLine(plot.X, y, plot.Right, y);
        }

        private static void DrawSeries(
            ICanvas canvas,
            RectF plot,
            IReadOnlyList<BloodPressureReading> readings,
            double min,
            double max,
            Func<BloodPressureReading, int> value,
            Color color)
        {
            PointF[] screen = new PointF[readings.Count];

            for (int i = 0; i < readings.Count; i++)
            {
                float x = readings.Count == 1
                    ? plot.Center.X
                    : plot.X + (plot.Width * i / (readings.Count - 1));

                screen[i] = new PointF(x, ToY(value(readings[i]), plot, min, max));
            }

            canvas.StrokeColor = color;
            canvas.StrokeSize = 2f;

            for (int i = 1; i < screen.Length; i++)
            {
                canvas.DrawLine(screen[i - 1], screen[i]);
            }

            canvas.FillColor = color;

            for (int i = 0; i < screen.Length; i++)
            {
                // Последняя точка крупнее: взгляд ищет именно её.
                canvas.FillCircle(screen[i], i == screen.Length - 1 ? 5f : 3f);
            }
        }

        private static void DrawLabels(
            ICanvas canvas,
            RectF dirtyRect,
            RectF plot,
            IReadOnlyList<BloodPressureReading> readings,
            double min,
            double max)
        {
            canvas.FontSize = 11f;
            canvas.FontColor = StatusPalette.Unknown;

            Label(canvas, Format(max), dirtyRect.X, plot.Y - 7f, plot.X - dirtyRect.X - 4f,
                Microsoft.Maui.Graphics.HorizontalAlignment.Right);

            Label(canvas, Format(min), dirtyRect.X, plot.Bottom - 7f, plot.X - dirtyRect.X - 4f,
                Microsoft.Maui.Graphics.HorizontalAlignment.Right);

            Label(canvas, readings[0].MeasuredAt.ToString("dd.MM.yy"), plot.X, plot.Bottom + 4f, plot.Width / 2f,
                Microsoft.Maui.Graphics.HorizontalAlignment.Left);

            if (readings.Count > 1)
            {
                Label(canvas, readings[^1].MeasuredAt.ToString("dd.MM.yy"), plot.Center.X, plot.Bottom + 4f,
                    plot.Width / 2f, Microsoft.Maui.Graphics.HorizontalAlignment.Right);
            }
        }

        private static void Label(
            ICanvas canvas,
            string text,
            float x,
            float y,
            float width,
            Microsoft.Maui.Graphics.HorizontalAlignment alignment) =>
            canvas.DrawString(
                text,
                x,
                y,
                Math.Max(1f, width),
                14f,
                alignment,
                Microsoft.Maui.Graphics.VerticalAlignment.Center);

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
            double span = max - min;

            if (span < double.Epsilon)
            {
                return plot.Center.Y;
            }

            return (float)(plot.Bottom - ((value - min) / span * plot.Height));
        }

        private static string Format(double value) => value.ToString("0", CultureInfo.CurrentCulture);
    }
}
