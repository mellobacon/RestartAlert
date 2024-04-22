using System.ComponentModel;
using System.Windows;

namespace WpfApp1;

public partial class Settings : Window
{
    private bool _showOnStartup;
    private bool _spamEnabled;

    private readonly string _path = "../../../appsettings.json";
    private readonly App.AppSettings _defaultSettings;

    public Settings()
    {
        InitializeComponent();
        
        _defaultSettings = App._appsettings;
        
        SetSettings(App._appsettings);
    }

    private void ShowOnStartUp_OnClick(object sender, RoutedEventArgs e)
    {
        _showOnStartup = (bool)ShowOnStartUp.IsChecked!;
    }

    void Save_OnClick(object sender, RoutedEventArgs e)
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
        _ = App.WriteSettings(_path, settings);
        Hide();
    }

    void SpamMode_OnClick(object sender, RoutedEventArgs e)
    {
        _spamEnabled = (bool)SpamMode.IsChecked!;
    }

    void Reset_OnClick(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            SetSettings(_defaultSettings);
        }));
        // figure out how to refresh ui
    }

    void SetSettings(App.AppSettings settings)
    {
        SpamMode.IsChecked = settings.SpamEnabled;
        ShowOnStartUp.IsChecked = settings.ShowSettingsOnStartup;
        UptimeUnit.ItemsSource = new List<string> { "Hours", "Days", "Weeks" };
        MinUptime.Text = settings.MinUpTime["Value"];
        UptimeUnit.SelectedItem = settings.MinUpTime["Unit"];
        
        AlertFreqUnit.ItemsSource = new List<string> { "Hours", "Days", "Weeks", "Never" };
        AlertFreq.Text = settings.AlertFrequency["Value"];
        AlertFreqUnit.SelectedItem = settings.AlertFrequency["Unit"];
        
        SpamFreqUnit.ItemsSource = new List<string> { "Seconds", "Minutes" };
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
 