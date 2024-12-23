using PanelController.Controller;
using PanelController.PanelObjects;
using PanelController.PanelObjects.Properties;
using System.Windows;
using System.Collections.Specialized;

namespace UtilityWindows
{
    public partial class LogsWindow : Window, IPanelObject
    {
        private static bool _closed = false;

        public Logger.Levels Level
        {
            get
            {
                if (LevelFilterComboBox.SelectedIndex == -1)
                    LevelFilterComboBox.SelectedIndex = 0;
                return (Logger.Levels)LevelFilterComboBox.SelectedItem;
            }
        }

        [UserConstructor("Create a window a shows logs.")]
        public LogsWindow()
        {
            InitializeComponent();
            LevelFilterComboBox.ItemsSource = Enum.GetValues<Logger.Levels>();
            RefreshLogs();

            Logger.Logged += (sender, log) => Dispatcher.Invoke(() =>
            {
                if (log.Level <= Level)
                    OutputLog(log);
            });
            LevelFilterComboBox.DropDownClosed += (sender, args) => RefreshLogs();
            Extensions.Objects.CollectionChanged += (sender, args) => Dispatcher.Invoke(() => Objects_CollectionChanged(sender, args));
            Closed += LogsWindow_Closed;
            FormatTextBox.TextChanged += (sender, args) => RefreshLogs();

            Show();
        }

        private void LogsWindow_Closed(object? sender, EventArgs e)
        {
            int index = Extensions.Objects.IndexOf(this);
            if (index == -1)
                return;
            _closed = true;
            Extensions.Objects.RemoveAt(index);
        }

        private void Objects_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Remove && e.Action != NotifyCollectionChangedAction.Remove)
                return;
            if (!e.OldItems?.Contains(this) ?? false)
                return;
            if (!_closed)
                Close();
        }

        private void RefreshLogs()
        {
            LogsBox.Clear();
            foreach (var log in Logger.Logs)
                if (log.Level <= Level)
                    OutputLog(log);
        }

        void OutputLog(Logger.HistoricalLog log) => LogsBox.AppendText($"{log.ToString(FormatTextBox.Text)}{Environment.NewLine}");
    }
}
