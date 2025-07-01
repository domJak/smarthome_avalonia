using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Timers;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SmartHome.ViewModels;

public class DeviceStatus
{
    public Dictionary<string, bool> Bulbs { get; set; } = new();
    public Dictionary<string, int> Blinds { get; set; } = new();
    public Dictionary<string, bool> Gates { get; set; } = new();
}

public class ScheduleItem : ViewModelBase
{
    public TimeSpan Time { get; set; }
    public string Action { get; set; } = "Otwórz";
}

public class BulbViewModel : ViewModelBase
{
    public string Name { get; set; }
    private bool _isOn;
    public bool IsOn
    {
        get => _isOn;
        set
        {
            if (SetProperty(ref _isOn, value))
                MainWindowViewModel.SaveStatusesStatic();
        }
    }
    public string Status => $"Status: {(IsOn ? "Włączona" : "Wyłączona")}";
    public IRelayCommand ToggleCommand { get; }
    public BulbViewModel(string name)
    {
        Name = name;
        ToggleCommand = new RelayCommand(Toggle);
    }
    private void Toggle()
    {
        IsOn = !IsOn;
        OnPropertyChanged(nameof(Status));
    }
}

public class BlindViewModel : ViewModelBase
{
    public string Name { get; set; }
    private int _level; // 0-100
    public int Level
    {
        get => _level;
        set
        {
            if (value < 0) value = 0;
            if (value > 100) value = 100;
            if (SetProperty(ref _level, value))
                MainWindowViewModel.SaveStatusesStatic();
            OnPropertyChanged(nameof(Status));
        }
    }
    public string Status => $"Poziom: {Level}% ({(Level == 0 ? "otwarta" : (Level == 100 ? "zamknięta" : "częściowo"))})";
    public IRelayCommand ToggleCommand { get; }
    public ObservableCollection<ScheduleItem> Schedules { get; } = new();
    public IRelayCommand AddScheduleCommand { get; }
    public IRelayCommand<ScheduleItem> RemoveScheduleCommand { get; }
    public IRelayCommand IncreaseLevelCommand { get; }
    public IRelayCommand DecreaseLevelCommand { get; }
    public BlindViewModel(string name)
    {
        Name = name;
        ToggleCommand = new RelayCommand(Toggle);
        AddScheduleCommand = new RelayCommand(AddSchedule);
        RemoveScheduleCommand = new RelayCommand<ScheduleItem>(RemoveSchedule);
        IncreaseLevelCommand = new RelayCommand(IncreaseLevel);
        DecreaseLevelCommand = new RelayCommand(DecreaseLevel);
        Level = 0;
    }
    private void Toggle()
    {
        Level = Level == 100 ? 0 : 100;
    }
    private void AddSchedule()
    {
        Schedules.Add(new ScheduleItem { Time = DateTime.Now.TimeOfDay, Action = "Otwórz" });
    }
    private void RemoveSchedule(ScheduleItem item)
    {
        if (item != null) Schedules.Remove(item);
    }
    private void IncreaseLevel()
    {
        Level += 10;
    }
    private void DecreaseLevel()
    {
        Level -= 10;
    }
    public void CheckSchedule(DateTime now)
    {
        foreach (var s in Schedules)
        {
            if (Math.Abs((s.Time - now.TimeOfDay).TotalMinutes) < 1)
            {
                if (s.Action == "Otwórz") Level = 0;
                else if (s.Action == "Zamknij") Level = 100;
            }
        }
    }
}

public class GateViewModel : ViewModelBase
{
    public string Name { get; set; }
    private bool _isOpen;
    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (SetProperty(ref _isOpen, value))
                MainWindowViewModel.SaveStatusesStatic();
        }
    }
    public string Status => IsOpen ? "otwarta" : "zamknięta";
    public IRelayCommand OpenCommand { get; }
    public IRelayCommand CloseCommand { get; }
    private readonly int _openTimeMs;
    public GateViewModel(string name, int openTimeMs)
    {
        Name = name;
        _openTimeMs = openTimeMs;
        OpenCommand = new RelayCommand(async () => await OpenAsync());
        CloseCommand = new RelayCommand(Close);
    }
    private async Task OpenAsync()
    {
        if (!IsOpen)
        {
            IsOpen = true;
            OnPropertyChanged(nameof(Status));
            await Task.Delay(_openTimeMs);
            IsOpen = false;
            OnPropertyChanged(nameof(Status));
        }
    }
    private void Close()
    {
        IsOpen = false;
        OnPropertyChanged(nameof(Status));
    }
}

public class CameraViewModel : ViewModelBase
{
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public CameraViewModel(string name, string imageUrl)
    {
        Name = name;
        ImageUrl = imageUrl;
    }
}

public partial class MainWindowViewModel : ViewModelBase
{
    public static MainWindowViewModel? Instance { get; private set; }
    public ObservableCollection<BulbViewModel> Bulbs { get; }
    public ObservableCollection<BlindViewModel> Blinds { get; }
    public ObservableCollection<GateViewModel> Gates { get; }
    public ObservableCollection<CameraViewModel> Cameras { get; }
    private System.Timers.Timer _timer;
    private System.Threading.Timer? _presenceSimTimer;
    private const string StatusFile = "status.json";
    public IRelayCommand LeaveHomeCommand { get; }
    public IRelayCommand ComeHomeCommand { get; }
    public IRelayCommand TogglePresenceSimCommand { get; }
    private bool _isPresenceSimOn;
    public bool IsPresenceSimOn
    {
        get => _isPresenceSimOn;
        set
        {
            if (SetProperty(ref _isPresenceSimOn, value))
            {
                if (value) StartPresenceSimulation();
                else StopPresenceSimulation();
            }
        }
    }
    private string _currentMode = "Tryb domyślny";
    public string CurrentMode
    {
        get => _currentMode;
        set => SetProperty(ref _currentMode, value);
    }
    public MainWindowViewModel()
    {
        Instance = this;
        Bulbs = new ObservableCollection<BulbViewModel>
        {
            new BulbViewModel("oświetlenie salon ogólne"),
            new BulbViewModel("oświetlenie salon led"),
            new BulbViewModel("oswietlenie kuchnia blat"),
            new BulbViewModel("oswietlenie kuchnia szafka"),
            new BulbViewModel("oświetlenie kuchnia led"),
            new BulbViewModel("oświetlenie łazienka lustro"),
            new BulbViewModel("oświetlenie łazienka led"),
            new BulbViewModel("oświetlenie przedpokój led"),
            new BulbViewModel("oświetlenie zewnętrzne"),
            new BulbViewModel("oświetlenie sypialnia led")
        };
        Blinds = new ObservableCollection<BlindViewModel>
        {
            new BlindViewModel("roleta salon"),
            new BlindViewModel("roleta kuchnia"),
            new BlindViewModel("roleta łazienka"),
            new BlindViewModel("roleta sypialnia")
        };
        Gates = new ObservableCollection<GateViewModel>
        {
            new GateViewModel("brama", 3 * 60 * 1000), // 3 minuty
            new GateViewModel("bramka", 10 * 1000) // 10 sekund
        };
        Cameras = new ObservableCollection<CameraViewModel>
        {
            new CameraViewModel("Salon", "https://placehold.co/320x180?text=Salon"),
            new CameraViewModel("Kuchnia", "https://placehold.co/320x180?text=Kuchnia"),
            new CameraViewModel("Przedpokój", "https://placehold.co/320x180?text=Przedpokój"),
            new CameraViewModel("Ogród", "https://placehold.co/320x180?text=Ogród")
        };
        LeaveHomeCommand = new RelayCommand(LeaveHome);
        ComeHomeCommand = new RelayCommand(ComeHome);
        TogglePresenceSimCommand = new RelayCommand(() => IsPresenceSimOn = !IsPresenceSimOn);
        _timer = new System.Timers.Timer(60 * 1000);
        _timer.Elapsed += (s, e) =>
        {
            var now = DateTime.Now;
            foreach (var blind in Blinds)
                blind.CheckSchedule(now);
        };
        _timer.Start();
        LoadStatuses();
    }
    private void LoadStatuses()
    {
        if (!File.Exists(StatusFile)) return;
        try
        {
            var json = File.ReadAllText(StatusFile);
            var data = JsonSerializer.Deserialize<DeviceStatus>(json);
            if (data != null)
            {
                foreach (var bulb in Bulbs)
                    if (data.Bulbs.TryGetValue(bulb.Name, out var on)) bulb.IsOn = on;
                foreach (var blind in Blinds)
                    if (data.Blinds.TryGetValue(blind.Name, out var lvl)) blind.Level = lvl;
                foreach (var gate in Gates)
                    if (data.Gates.TryGetValue(gate.Name, out var open)) gate.IsOpen = open;
            }
        }
        catch { }
    }
    public static void SaveStatusesStatic()
    {
        Instance?.SaveStatuses();
    }
    private void SaveStatuses()
    {
        var data = new DeviceStatus
        {
            Bulbs = Bulbs.ToDictionary(b => b.Name, b => b.IsOn),
            Blinds = Blinds.ToDictionary(b => b.Name, b => b.Level),
            Gates = Gates.ToDictionary(g => g.Name, g => g.IsOpen)
        };
        try
        {
            File.WriteAllText(StatusFile, JsonSerializer.Serialize(data));
        }
        catch { }
    }
    private void LeaveHome()
    {
        foreach (var bulb in Bulbs) bulb.IsOn = false;
        foreach (var blind in Blinds) blind.Level = 100;
        foreach (var gate in Gates) gate.IsOpen = false;
        CurrentMode = "Wychodzę z domu";
    }
    private void ComeHome()
    {
        foreach (var blind in Blinds) blind.Level = 0;
        foreach (var bulb in Bulbs)
            if (bulb.Name.Contains("salon")) bulb.IsOn = true;
        foreach (var gate in Gates) gate.IsOpen = true;
        CurrentMode = "Wracam do domu";
    }
    private void StartPresenceSimulation()
    {
        _presenceSimTimer = new System.Threading.Timer(_ => SimulatePresence(), null, 0, 2 * 60 * 1000); // co 2 minuty
    }
    private void StopPresenceSimulation()
    {
        _presenceSimTimer?.Dispose();
        _presenceSimTimer = null;
        foreach (var bulb in Bulbs)
            if (bulb.Name.Contains("salon") || bulb.Name.Contains("kuchnia")) bulb.IsOn = false;
    }
    private void SimulatePresence()
    {
        var hour = DateTime.Now.Hour;
        if (hour >= 18 && hour <= 23)
        {
            var rand = new Random();
            foreach (var bulb in Bulbs)
            {
                if (bulb.Name.Contains("salon") || bulb.Name.Contains("kuchnia"))
                    bulb.IsOn = rand.Next(2) == 0;
            }
        }
    }
}
