using MedicalResultsTracker.Controls;
using MedicalResultsTracker.Resources.Strings;
using MedicalResultsTracker.ViewModel;

namespace MedicalResultsTracker.View
{
    /// <summary>
    /// Сводная таблица строится в коде: количество столбцов зависит от числа сдач,
    /// а разметка XAML умеет только заранее известную сетку.
    /// </summary>
    public partial class MatrixPage : ContentPage
    {
        /// <summary>Высота строки одна для обеих половин таблицы — иначе они разъедутся по вертикали.</summary>
        private const double RowHeight = 44;
        private const double HeaderHeight = 40;
        private const double NameWidth = 150;
        private const double RangeWidth = 118;
        private const double UnitWidth = 78;
        private const double ValueWidth = 96;

        private readonly MatrixViewModel _viewModel;

        public MatrixPage(MatrixViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = _viewModel = viewModel;
            _viewModel.Rebuilt += (_, _) => Build();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _viewModel.InitializeAsync();
        }

        private void Build()
        {
            FixedColumn.Clear();
            FixedColumn.RowDefinitions.Clear();
            FixedColumn.ColumnDefinitions.Clear();

            ScrollingColumns.Clear();
            ScrollingColumns.RowDefinitions.Clear();
            ScrollingColumns.ColumnDefinitions.Clear();

            FixedColumn.ColumnDefinitions.Add(new ColumnDefinition(NameWidth));
            ScrollingColumns.ColumnDefinitions.Add(new ColumnDefinition(RangeWidth));
            ScrollingColumns.ColumnDefinitions.Add(new ColumnDefinition(UnitWidth));

            foreach (string _ in _viewModel.Dates)
            {
                ScrollingColumns.ColumnDefinitions.Add(new ColumnDefinition(ValueWidth));
            }

            FixedColumn.RowDefinitions.Add(new RowDefinition(HeaderHeight));
            ScrollingColumns.RowDefinitions.Add(new RowDefinition(HeaderHeight));

            FixedColumn.Add(Header(S.Csv_Parameter, TextAlignment.Start), 0, 0);
            ScrollingColumns.Add(Header(S.Csv_Reference, TextAlignment.Center), 0, 0);
            ScrollingColumns.Add(Header(S.Csv_Unit, TextAlignment.Center), 1, 0);

            for (int i = 0; i < _viewModel.Dates.Count; i++)
            {
                ScrollingColumns.Add(Header(_viewModel.Dates[i], TextAlignment.Center), i + 2, 0);
            }

            for (int r = 0; r < _viewModel.Rows.Count; r++)
            {
                MatrixRowViewModel row = _viewModel.Rows[r];
                int line = r + 1;

                FixedColumn.RowDefinitions.Add(new RowDefinition(RowHeight));
                ScrollingColumns.RowDefinitions.Add(new RowDefinition(RowHeight));

                Label name = Cell(row.Name, TextAlignment.Start);
                name.FontAttributes = FontAttributes.Bold;
                name.LineBreakMode = LineBreakMode.TailTruncation;

                // По названию открывается график этого показателя.
                TapGestureRecognizer tap = new();
                tap.Tapped += (_, _) => _viewModel.OpenRowCommand.Execute(row);
                name.GestureRecognizers.Add(tap);

                FixedColumn.Add(name, 0, line);
                ScrollingColumns.Add(Cell(row.Range, TextAlignment.Center), 0, line);
                ScrollingColumns.Add(Cell(row.Unit, TextAlignment.Center), 1, line);

                for (int c = 0; c < row.Cells.Count; c++)
                {
                    MatrixCellViewModel cell = row.Cells[c];

                    Label value = Cell(cell.Text, TextAlignment.Center);
                    value.TextColor = cell.Color;

                    if (cell.IsOutOfRange)
                    {
                        value.FontAttributes = FontAttributes.Bold;
                    }

                    ScrollingColumns.Add(value, c + 2, line);
                }
            }
        }

        private static Label Header(string text, TextAlignment alignment) => new()
        {
            Text = text,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            Padding = new Thickness(6, 0),
            HorizontalTextAlignment = alignment,
            VerticalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.TailTruncation,
        };

        private static Label Cell(string text, TextAlignment alignment) => new()
        {
            Text = text,
            FontSize = 14,
            Padding = new Thickness(6, 0),
            HorizontalTextAlignment = alignment,
            VerticalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.TailTruncation,
        };
    }
}
