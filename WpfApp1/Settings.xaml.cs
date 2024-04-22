using System.ComponentModel;
using System.Windows;

namespace WpfApp1;

public partial class Settings : Window
{
    private bool _showOnStartup;
    private bool _spamEnabled;

    private readonly string _path = "appsettings.json";
    private readonly App.AppSettings _defaultSettings;

    public Settings(App.AppSettings settings)
    {
        InitializeComponent();
        
        _defaultSettings = settings;
        
        SetSettings(settings);
    }

    private void ShowOnStartUp_OnClick(object sender, RoutedEventArgs e)
    {
        _showOnStartup = ShowOnStartUp.IsChecked is true;
    }

    private void Save_OnClick(object sender, RoutedEventArgs e)
    {
        var settings = new App.AppSettings()
        {
            ShowSettingsOnStartup = _showOnStartup,
            MinUpTime = new Dictionary<string, string>()
            {
                ["Value"] = MinUptime.Text,
                ["Unit"] = (string)UptimeUnit.SelectedItem
            },
            AlertFrequency = new Dictionary<string, string>()
            {
                ["Value"] = AlertFreq.Text,
                ["Unit"] = (string)AlertFreqUnit.SelectedItem
            },
            SpamEnabled = _spamEnabled,
            SpamFrequency = new Dictionary<string, string>()
            {
                ["Value"] = SpamFreq.Text,
                ["Unit"] = (string)SpamFreqUnit.SelectedItem
            },
        };
        App.WriteSettings(_path, settings);
        Hide();
    }

    private void SpamMode_OnClick(object sender, RoutedEventArgs e)
    {
        _spamEnabled = SpamMode.IsChecked is true;
    }

    private void Reset_OnClick(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            SetSettings(_defaultSettings);
        }));
        // figure out how to refresh ui
    }
    
    private void SetSettings(App.AppSettings settings)
    {
        SpamMode.IsChecked = settings.SpamEnabled;
        ShowOnStartUp.IsChecked = settings.ShowSettingsOnStartup;
        UptimeUnit.ItemsSource = new List<string> { "Hour(s)", "Day(s)", "Week(s)" };
        MinUptime.Text = settings.MinUpTime["Value"];
        UptimeUnit.SelectedItem = settings.MinUpTime["Unit"];
        
        AlertFreqUnit.ItemsSource = new List<string> { "Minute(s)", "Hour(s)", "Day(s)", "Week(s)", "Never" };
        AlertFreq.Text = settings.AlertFrequency["Value"];
        AlertFreqUnit.SelectedItem = settings.AlertFrequency["Unit"];
        
        SpamFreqUnit.ItemsSource = new List<string> { "Second(s)", "Minute(s)" };
        SpamFreq.Text = settings.SpamFrequency["Value"];
        SpamFreqUnit.SelectedItem = settings.SpamFrequency["Unit"];
    }
    
    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        e.Cancel = true;
        Hide();
    }
}
