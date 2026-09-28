using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace FunctionPlotter
{
    // -------- Команда --------
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _exec;
        private readonly Func<object, bool> _can;

        public RelayCommand(Action<object> exec, Func<object, bool> can = null)
        { _exec = exec; _can = can; }

        public bool CanExecute(object p) => _can == null || _can(p);
        public void Execute(object p) => _exec(p);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    // -------- Набор точек --------
    public class PointCollectionData
    {
        public List<Point> Points { get; } = new List<Point>();
        public double YMin { get; set; }
        public double YMax { get; set; }
    }

    // -------- ViewModel --------
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _fn = "sin";
        private double _a = 1, _b = 1, _c = 0, _xMin = -10, _xMax = 10;

        private PointCollectionData _plotData = new PointCollectionData();
        private string _plotTitle = "Выберите функцию и нажмите «Построить»";
        private string _status = "Готово";

        public MainViewModel()
        {
            // Инициализация "БД"
            try
            {
                PlotRepository.EnsureTable();
                Status = "Работаем без PostgreSQL (данные в памяти)";
            }
            catch (Exception ex)
            {
                Status = "Ошибка инициализации БД: " + ex.Message;
            }

            SelectFunctionCommand = new RelayCommand(p =>
            {
                SelectedFunction = p == null ? "sin" : p.ToString();
            });
            PlotCommand = new RelayCommand(_ => Plot());
            LoadHistory();
        }

        public string SelectedFunction
        {
            get => _fn;
            set
            {
                _fn = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FunctionInfo));
            }
        }

        public double A { get => _a; set { _a = value; OnPropertyChanged(); } }
        public double B { get => _b; set { _b = value; OnPropertyChanged(); } }
        public double C { get => _c; set { _c = value; OnPropertyChanged(); } }
        public double XMin { get => _xMin; set { _xMin = value; OnPropertyChanged(); } }
        public double XMax { get => _xMax; set { _xMax = value; OnPropertyChanged(); } }

        public string FunctionInfo
        {
            get
            {
                switch (SelectedFunction)
                {
                    case "sin": return "A * sin(B * x + C)";
                    case "cos": return "A * cos(B * x + C)";
                    case "exp": return "A * exp(B * x) + C";
                    default: return "";
                }
            }
        }

        public PointCollectionData PlotData
        {
            get => _plotData;
            private set { _plotData = value; OnPropertyChanged(); }
        }

        public string PlotTitle
        {
            get => _plotTitle;
            private set { _plotTitle = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => _status;
            private set { _status = value; OnPropertyChanged(); }
        }

        public ObservableCollection<PlotRecord> History { get; }
            = new ObservableCollection<PlotRecord>();

        public RelayCommand SelectFunctionCommand { get; }
        public RelayCommand PlotCommand { get; }

        // -------- Построение --------
        private void Plot()
        {
            if (XMax <= XMin)
            {
                MessageBox.Show("XMax должен быть больше XMin");
                return;
            }

            var startTime = DateTime.Now;   // время начала вычисления

            int steps = 1000;
            double dx = (XMax - XMin) / steps;

            var xs = new double[steps + 1];
            var ys = new double[steps + 1];
            double yMin = double.MaxValue, yMax = double.MinValue;

            for (int i = 0; i <= steps; i++)
            {
                double x = XMin + i * dx;
                double y;
                switch (SelectedFunction)
                {
                    case "sin": y = A * Math.Sin(B * x + C); break;
                    case "cos": y = A * Math.Cos(B * x + C); break;
                    case "exp": y = A * Math.Exp(B * x) + C; break;
                    default: y = 0; break;
                }
                if (double.IsNaN(y) || double.IsInfinity(y)) y = 0;
                xs[i] = x;
                ys[i] = y;
                if (y < yMin) yMin = y;
                if (y > yMax) yMax = y;
            }

            double spanX = XMax - XMin;
            double spanY = (yMax - yMin) == 0 ? 1 : (yMax - yMin);

            var data = new PointCollectionData();
            for (int i = 0; i <= steps; i++)
            {
                double nx = (xs[i] - XMin) / spanX;
                double ny = (ys[i] - yMin) / spanY;
                data.Points.Add(new Point(nx, ny));
            }
            data.YMin = yMin;
            data.YMax = yMax;

            PlotData = data;
            PlotTitle = string.Format("{0}: {1}", SelectedFunction, FunctionInfo);

            // -------- Сохранение записи --------
            // Сейчас уходит в заглушку PlotRepository (память).
            // Когда появится Npgsql — раскомментируйте блок внутри PlotRepository.Add.
            try
            {
                PlotRepository.Add(new PlotRecord
                {
                    FunctionName = SelectedFunction,
                    A = A,
                    B = B,
                    C = C,
                    XMin = XMin,
                    XMax = XMax,
                    StartTime = startTime
                });
                Status = "Сохранено (в память): " + startTime.ToString("HH:mm:ss");
                // Когда БД будет включена — замените на:
                // Status = "Сохранено в БД: " + startTime.ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {
                Status = "Ошибка сохранения: " + ex.Message;
            }

            LoadHistory();
        }

        private void LoadHistory()
        {
            History.Clear();
            try
            {
                foreach (var r in PlotRepository.LoadLast(50))
                    History.Add(r);
            }
            catch (Exception ex)
            {
                Status = "Ошибка загрузки истории: " + ex.Message;
            }
        }

        // -------- INotifyPropertyChanged --------
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string n = null)
        {
            var h = PropertyChanged;
            if (h != null) h(this, new PropertyChangedEventArgs(n));
        }
    }
}