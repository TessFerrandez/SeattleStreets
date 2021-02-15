using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

namespace SeattleStreets
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool[,] Filled = new bool[6, 7];
        const int GridSize = 6;
        const int SquareSize = 50;
        int moves = 0;

        Point lastPoint;
        bool isMouseCaptured = false;

        SolidColorBrush white = new SolidColorBrush(Colors.White);
        SolidColorBrush gray = new SolidColorBrush(Colors.Gray);
        SolidColorBrush darkGray = new SolidColorBrush(Colors.DarkGray);

        public MainWindow()
        {
            InitializeComponent();
            AddSquares();
            InitializeLevel("Level1");
        }

        private void AddSquares()
        {
            for (int i = 0; i < GridSize; i++)
            {
                RowDefinition rd = new RowDefinition();
                grid.RowDefinitions.Add(rd);
                ColumnDefinition cd = new ColumnDefinition();
                grid.ColumnDefinitions.Add(cd);
            }
            ColumnDefinition cdExit = new ColumnDefinition();
            grid.ColumnDefinitions.Add(cdExit);

            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    AddSquare(i, j);
                }
            }

            //Exit box
            AddSquare(2, 6);

            TextBlock tb = new TextBlock();
            tb.Text = "EXIT";
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            tb.VerticalAlignment = VerticalAlignment.Center;
            grid.Children.Add(tb);
            tb.SetValue(Grid.RowProperty, 2);
            tb.SetValue(Grid.ColumnProperty, 6);
        }

        void AddSquare(int row, int col)
        {
            Border brd = new Border();
            brd.Background = darkGray;
            brd.BorderBrush = gray;
            brd.BorderThickness = new Thickness(3.0);
            brd.Margin = new Thickness(2.0);
            brd.Height = SquareSize;
            brd.Width = SquareSize;
            grid.Children.Add(brd);
            brd.SetValue(Grid.RowProperty, row);
            brd.SetValue(Grid.ColumnProperty, col);
        }

        private void AddCars(string levelName)
        {
            XDocument levelDoc = XDocument.Load("Levels/" + levelName + ".xml");

            var cars = from car in levelDoc.Descendants("CAR")
                       select new
                       {
                           Orientation = car.Attribute("ORIENTATION").Value,
                           Length = Int32.Parse(car.Attribute("LENGTH").Value),
                           Row = Int32.Parse(car.Attribute("ROW").Value),
                           Column = Int32.Parse(car.Attribute("COL").Value),
                           Color = car.Attribute("COLOR").Value
                       };

            foreach (var car in cars)
            {
                Orientation orientation;
                Color color = GetColorFromName(car.Color);

                if (car.Orientation == "HORIZONTAL")
                    orientation = Orientation.Horizontal;
                else
                    orientation = Orientation.Vertical;

                AddCar(orientation, car.Length, color, car.Row, car.Column);
            }
        }

        private Color GetColorFromName(string color)
        {
            switch (color)
            {
                case "WHITE":
                    return Colors.White;
                case "BLACK":
                    return Colors.Black;
                case "RED":
                    return Colors.Red;
                case "BLUE":
                    return Colors.Blue;
                case "YELLOW":
                    return Colors.Yellow;
                case "GREEN":
                    return Colors.Green;
                case "PURPLE":
                    return Colors.Purple;
                case "ORANGE":
                    return Colors.Orange;
                default:
                    return Colors.Black;
            }
        }

        private void AddCar(Orientation orientation, int length, Color color, int row, int col)
        {
            Car c = new Car(orientation, length, color, row, col);
            Mark(row, col, length, orientation, true);
            CarCanvas.Children.Add(c);
            c.SetValue(Canvas.LeftProperty, col * 60.0);
            c.SetValue(Canvas.TopProperty, row * 60.0);

            c.MouseLeftButtonDown += new MouseButtonEventHandler(OnCarLeftButtonDown);
            c.MouseMove += new MouseEventHandler(OnCarMove);
            c.MouseLeftButtonUp += new MouseButtonEventHandler(OnCarLeftButtonUp);
        }

        void OnCarLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseCaptured = false;

            Car currentCar = sender as Car;
            currentCar.ReleaseMouseCapture();

            //unmark previous position
            Mark(currentCar.Row, currentCar.Column, currentCar.Length, currentCar.Orientation, false);

            if (currentCar.Orientation == Orientation.Horizontal)
            {
                double left = Canvas.GetLeft(currentCar);
                int col = PosToColOrRow(left);
                Canvas.SetLeft(currentCar, col * 60.0);
                currentCar.Column = col;
                if (currentCar.Column + currentCar.Length == 7)
                {
                    MessageBox.Show("Congratulations");
                }
            }
            else
            {
                double top = Canvas.GetTop(currentCar);
                int row = PosToColOrRow(top);
                Canvas.SetTop(currentCar, row * 60.0);
                currentCar.Row = row;
            }

            //mark new position
            Mark(currentCar.Row, currentCar.Column, currentCar.Length, currentCar.Orientation, true);
            moves++;
            tbMoves.Text = moves.ToString();
        }

        void OnCarMove(object sender, MouseEventArgs e)
        {
            if (isMouseCaptured)
            {
                Car currentCar = sender as Car;
                Point currentPoint = e.GetPosition(CarCanvas);

                if (currentCar.Orientation == Orientation.Horizontal)
                {
                    double left = Canvas.GetLeft(currentCar) + currentPoint.X - lastPoint.X;
                    double minX = GetMinX(currentCar.Row, currentCar.Column);
                    double maxX = GetMaxX(currentCar.Row, currentCar.Column, currentCar.Length);
                    if (left <= minX) left = minX;
                    if (left >= maxX) left = maxX;
                    Canvas.SetLeft(currentCar, left);
                    lastPoint = currentPoint;
                }
                else
                {
                    double top = Canvas.GetTop(currentCar) + currentPoint.Y - lastPoint.Y;
                    double minY = GetMinY(currentCar.Row, currentCar.Column);
                    double maxY = GetMaxY(currentCar.Row, currentCar.Column, currentCar.Length);
                    if (top <= minY) top = minY;
                    if (top >= maxY) top = maxY;
                    if (top < 0) top = 0;
                    Canvas.SetTop(currentCar, top);
                    lastPoint = currentPoint;
                }
            }
        }

        void OnCarLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseCaptured = true;
            lastPoint = e.GetPosition(CarCanvas);
            Car currentCar = sender as Car;
            currentCar.CaptureMouse();
        }

        private double GetMinX(int row, int column)
        {
            //find the earliest col that is not filled
            column--;
            while (column >= 0 && Filled[row, column] == false)
                column--;
            column++;
            return column * 60.0;
        }

        private double GetMinY(int row, int column)
        {
            //find the earliest col that is not filled
            row--;
            while (row >= 0 && Filled[row, column] == false)
                row--;
            row++;
            return row * 60.0;
        }

        private double GetMaxX(int row, int column, int length)
        {
            //find the latest col that is not filled
            //special case for row 2 since it has 7 columns

            if (row == 2)
            {
                column = column + length;
                while (column <= 6 && Filled[row, column] == false)
                    column++;
                column = column - length;
                return column * 60.0;
            }
            else
            {
                column += length;
                while (column <= 5 && Filled[row, column] == false)
                    column++;
                column -= length;
                return column * 60.0;
            }
        }

        private double GetMaxY(int row, int column, int length)
        {
            row += length;
            while (row <= 5 && Filled[row, column] == false)
                row++;
            row -= length;
            return row * 60.0;
        }

        private int PosToColOrRow(double position)
        {
            return (int)(position / 60);
        }

        private void Mark(int row, int col, int length, Orientation orientation, bool filled)
        {
            //mark positions as filled or unfilled
            if (orientation == Orientation.Horizontal)
                for (int i = 0; i < length; i++)
                    Filled[row, col + i] = filled;
            else
                for (int i = 0; i < length; i++)
                    Filled[row + i, col] = filled;
        }

        private void InitializeLevel(string levelName)
        {
            moves = 0;
            tbMoves.Text = "0";

            //clear filled
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 7; j++)
                    Filled[i, j] = false;

            //remove all the cars from previous rounds
            CarCanvas.Children.Clear();

            //add new cars and squares
            AddCars(levelName);
        }

        private void cboLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboLevel != null && CarCanvas != null)
            {
                string level = ((ComboBoxItem)cboLevel.Items[cboLevel.SelectedIndex]).Content.ToString();
                InitializeLevel(level);
                tbLevel.Text = level.Substring(5);
            }
        }
    }
}
