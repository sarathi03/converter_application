using convertor_application;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace convertor_application
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<DeviceInfo> _devices;
        private ObservableCollection<DeviceInfo> _searchResults;
        private CollectionViewSource _deviceViewSource;
        private bool _isConnected = false;
        private string _currentDeviceIp = "192.168.1.130";
        private Dictionary<string, string> _userCredentials;
        private List<ConfigurationTabInfo> _configurationTabs;
        private static DeviceConfiguration _copiedConfiguration = null;
        private static List<DeviceConfigurationWindow> _openConfigWindows = new List<DeviceConfigurationWindow>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            // Initialize device collections
            _devices = new ObservableCollection<DeviceInfo>();
            _searchResults = new ObservableCollection<DeviceInfo>();

            // Setup collection view source for filtering and grouping
            _deviceViewSource = new CollectionViewSource();
            _deviceViewSource.Source = _devices;
            DeviceListGrid.ItemsSource = _deviceViewSource.View;

            SearchResultsGrid.ItemsSource = _searchResults;

            // Initialize user credentials
            _userCredentials = new Dictionary<string, string>
            {
                { "admin", "admin123" },
            };

            // Initialize configuration tabs
            _configurationTabs = new List<ConfigurationTabInfo>();

            // Load dummy data for testing
            LoadDummyDevices();
        }

        private void LoadDummyDevices()
        {
            // Add dummy devices with locations
            _devices.Add(new DeviceInfo
            {
                IpAddress = "192.168.1.100",
                Port = 502,
                DeviceName = "RTU Converter 1",
                Location = "Building A",
                IsConnected = true,
                StatusColor = Brushes.Green,
                Status = "Connected",
                DeviceType = "Modbus RTU/TCP",
                SerialNumber = "MC001",
                FirmwareVersion = "1.2.3",
                LastSeen = DateTime.Now.AddMinutes(-5)
            });

            _devices.Add(new DeviceInfo
            {
                IpAddress = "192.168.1.101",
                Port = 502,
                DeviceName = "RTU Converter 2",
                Location = "Building A",
                IsConnected = false,
                StatusColor = Brushes.Red,
                Status = "Disconnected",
                DeviceType = "Modbus RTU/TCP",
                SerialNumber = "MC002",
                FirmwareVersion = "1.2.1",
                LastSeen = DateTime.Now.AddHours(-2)
            });

            _devices.Add(new DeviceInfo
            {
                IpAddress = "192.168.1.102",
                Port = 502,
                DeviceName = "RTU Converter 3",
                Location = "Building B",
                IsConnected = true,
                StatusColor = Brushes.Green,
                Status = "Connected",
                DeviceType = "Modbus RTU/TCP",
                SerialNumber = "MC003",
                FirmwareVersion = "1.2.3",
                LastSeen = DateTime.Now.AddMinutes(-1)
            });

            _devices.Add(new DeviceInfo
            {
                IpAddress = "192.168.1.103",
                Port = 502,
                DeviceName = "RTU Converter 4",
                Location = "Building B",
                IsConnected = false,
                StatusColor = Brushes.Orange,
                Status = "Offline",
                DeviceType = "Modbus RTU/TCP",
                SerialNumber = "MC004",
                FirmwareVersion = "1.1.9",
                LastSeen = DateTime.Now.AddHours(-1)
            });

            _devices.Add(new DeviceInfo
            {
                IpAddress = "192.168.1.104",
                Port = 502,
                DeviceName = "RTU Converter 5",
                Location = "Building C",
                IsConnected = true,
                StatusColor = Brushes.Green,
                Status = "Connected",
                DeviceType = "Modbus RTU/TCP",
                SerialNumber = "MC005",
                FirmwareVersion = "1.2.3",
                LastSeen = DateTime.Now
            });

            _devices.Add(new DeviceInfo
            {
                IpAddress = "192.168.1.105",
                Port = 502,
                DeviceName = "RTU Converter 6",
                Location = "Building C",
                IsConnected = false,
                StatusColor = Brushes.Gray,
                Status = "Offline",
                DeviceType = "Modbus RTU/TCP",
                SerialNumber = "MC006",
                FirmwareVersion = "1.2.0",
                LastSeen = DateTime.Now.AddDays(-1)
            });
        }

        #region Authentication

        private void LoginPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login_Click(sender, e);
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (ValidateCredentials(username, password))
            {
                LoginPanel.Visibility = Visibility.Collapsed;
                MainPanel.Visibility = Visibility.Visible;
                ShowSuccessMessage("Login successful!");
            }
            else
            {
                ShowErrorMessage("Invalid username or password!");
            }
        }

        private bool ValidateCredentials(string username, string password)
        {
            return _userCredentials.ContainsKey(username) &&
                   _userCredentials[username] == password;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Reset application state
            _isConnected = false;
            _currentDeviceIp = "";
            _devices.Clear();
            _searchResults.Clear();

            // Close all configuration tabs
            CloseAllConfigurationTabs();

            // Show login panel
            MainPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;

            // Clear login fields
            UsernameTextBox.Clear();
            PasswordBox.Clear();

            // Reload dummy data
            LoadDummyDevices();
        }

        #endregion

        #region Menu Navigation

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            // Show Device Discovery panel by default when Home is clicked
            DeviceDiscovery_Click(sender, e);
        }

        private void DeviceDiscovery_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(DeviceDiscoveryPanel);
        }

        private void IpConfig_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected)
            {
                ShowErrorMessage("Please connect to a device first!");
                return;
            }
            ShowErrorMessage("Please double-click on a connected device to configure IP settings.");
        }

        private void Rs485Config_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected)
            {
                ShowErrorMessage("Please connect to a device first!");
                return;
            }
            ShowErrorMessage("Please double-click on a connected device to configure RS485 settings.");
        }

        private void SearchDevice_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(SearchDevicePanel);
        }

        private void ShowPanel(FrameworkElement panelToShow)
        {
            DeviceDiscoveryPanel.Visibility = Visibility.Collapsed;
            SearchDevicePanel.Visibility = Visibility.Collapsed;

            panelToShow.Visibility = Visibility.Visible;
        }

        #endregion

        #region Support Menu

        private void OpenWebsite_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://www.machdatum.com/",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Failed to open website: {ex.Message}");
            }
        }

        private void EmailSupport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "mailto:Support@MachDatum.com",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Failed to open email client: {ex.Message}");
            }
        }

        private void ContactSupport_Click(object sender, RoutedEventArgs e)
        {
            ShowInformationMessage("Contact Support", "Phone: 8825888892\nEmail: Support@MachDatum.com\nWebsite: https://www.machdatum.com/");
        }

        #endregion

        #region Documentation Menu

        private void PhysicalConfig_Click(object sender, RoutedEventArgs e)
        {
            ShowInformationMessage("Physical Configuration", "Physical Configuration documentation will be available here.\n\nThis section will contain:\n- Hardware setup instructions\n- Wiring diagrams\n- Installation guidelines");
        }

        private void ConfigMenu_Click(object sender, RoutedEventArgs e)
        {
            ShowInformationMessage("Configuration Menu", "Configuration Menu documentation will be available here.\n\nThis section will contain:\n- Menu navigation guide\n- Configuration options\n- Settings explanations");
        }

        private void AppFeatures_Click(object sender, RoutedEventArgs e)
        {
            ShowInformationMessage("App Features", "Application Features documentation will be available here.\n\nThis section will contain:\n- Feature descriptions\n- How-to guides\n- Advanced settings");
        }

        private void Troubleshoots_Click(object sender, RoutedEventArgs e)
        {
            ShowInformationMessage("Troubleshooting", "Troubleshooting guide will be available here.\n\nThis section will contain:\n- Common issues and solutions\n- Error codes\n- Diagnostic procedures");
        }

        private void ProductLink_Click(object sender, RoutedEventArgs e)
        {
            ShowInformationMessage("Product Link", "Product information and links will be available here.\n\nThis section will contain:\n- Product specifications\n- Download links\n- Related products");
        }

        #endregion

        #region Device Discovery

        private async void ScanNetwork_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string networkRange = NetworkRangeTextBox.Text;
                var button = sender as Button;
                await ScanNetworkRange(networkRange, button);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error scanning network: {ex.Message}");
            }
        }

        private async Task ScanNetworkRange(string networkRange, Button scanButton)
        {
            // Clear existing devices
            _devices.Clear();

            // Show loading indicator
            scanButton.IsEnabled = false;
            scanButton.Content = "🔍 Scanning...";

            try
            {
                // Simulate network scanning delay
                await Task.Delay(2000);

                // Reload dummy devices after scan
                LoadDummyDevices();

                // Add a few more devices to simulate discovery
                _devices.Add(new DeviceInfo
                {
                    IpAddress = "192.168.1.110",
                    Port = 502,
                    DeviceName = "New RTU Converter",
                    Location = "Building D",
                    IsConnected = false,
                    StatusColor = Brushes.Gray,
                    Status = "Discovered",
                    DeviceType = "Modbus RTU/TCP",
                    SerialNumber = "MC007",
                    FirmwareVersion = "1.2.3",
                    LastSeen = DateTime.Now
                });

                ShowSuccessMessage($"Network scan completed! Found {_devices.Count} devices.");
            }
            finally
            {
                scanButton.IsEnabled = true;
                scanButton.Content = "🔍 Scan Network";
            }
        }

        private void ConnectDevice_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var device = button.Tag as DeviceInfo;

            if (device != null)
            {
                ConnectToSpecificDevice(device);
            }
        }

        private async void PingDevice_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var device = button.Tag as DeviceInfo;

            if (device != null)
            {
                button.IsEnabled = false;
                button.Content = "Pinging...";

                try
                {
                    // Simulate ping operation
                    await Task.Delay(1000);

                    // Simulate ping success/failure based on device status
                    if (device.Status == "Connected" || device.Status == "Discovered")
                    {
                        var responseTime = new Random().Next(1, 100);
                        ShowSuccessMessage($"Ping successful!\n\nDevice: {device.DeviceName}\nIP: {device.IpAddress}\nResponse Time: {responseTime}ms\nStatus: {device.Status}");

                        // Update last seen time
                        device.LastSeen = DateTime.Now;
                    }
                    else
                    {
                        ShowErrorMessage($"Ping failed!\n\nDevice: {device.DeviceName}\nIP: {device.IpAddress}\nStatus: {device.Status}");
                    }
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"Ping error: {ex.Message}");
                }
                finally
                {
                    button.IsEnabled = true;
                    button.Content = "Ping";
                }
            }
        }

        private void RenameDevice_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var device = button.Tag as DeviceInfo;

            if (device != null)
            {
                string newName = ShowInputDialog("Enter new device name:", "Rename Device", device.DeviceName);

                if (!string.IsNullOrEmpty(newName))
                {
                    device.DeviceName = newName;
                    ShowSuccessMessage($"Device renamed to: {newName}");
                }
            }
        }

        private void ConnectToSpecificDevice(DeviceInfo device)
        {
            try
            {
                // Simulate connection to device
                _currentDeviceIp = device.IpAddress;
                _isConnected = true;

                // Update device status
                device.IsConnected = true;
                device.StatusColor = Brushes.Green;
                device.Status = "Connected";
                device.LastSeen = DateTime.Now;

                ShowSuccessMessage($"Connected to device '{device.DeviceName}' at {device.IpAddress}");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Failed to connect to device: {ex.Message}");
            }
        }

        private void DeviceListGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DeviceListGrid.SelectedItem is DeviceInfo device)
            {
                if (device.IsConnected)
                {
                    OpenConfigurationTab(device);
                }
                else
                {
                    ShowErrorMessage("Please connect to the device first!");
                }
            }
        }

        #endregion

        #region Search Device

        private void SearchDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            PerformDeviceSearch_Click(sender, e);
        }

        private void PerformDeviceSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string searchQuery = SearchQueryTextBox.Text?.ToLower() ?? "";
                _searchResults.Clear();

                if (string.IsNullOrWhiteSpace(searchQuery))
                {
                    ShowErrorMessage("Please enter a search query!");
                    return;
                }

                // Search through discovered devices
                var results = _devices.Where(d =>
                    d.DeviceName.ToLower().Contains(searchQuery) ||
                    d.IpAddress.Contains(searchQuery) ||
                    d.Port.ToString().Contains(searchQuery) ||
                    d.SerialNumber.ToLower().Contains(searchQuery) ||
                    d.Status.ToLower().Contains(searchQuery) ||
                    d.Location.ToLower().Contains(searchQuery)
                ).ToList();

                foreach (var device in results)
                {
                    _searchResults.Add(device);
                }

                if (_searchResults.Count == 0)
                {
                    ShowInformationMessage("Search Results", "No devices found matching your search criteria.");
                }
                else
                {
                    ShowSuccessMessage($"Found {_searchResults.Count} device(s) matching your search criteria.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Search error: {ex.Message}");
            }
        }

        private void SearchResultsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SearchResultsGrid.SelectedItem is DeviceInfo device)
            {
                if (device.IsConnected)
                {
                    OpenConfigurationTab(device);
                }
                else
                {
                    ShowErrorMessage("Please connect to the device first!");
                }
            }
        }

        private void ConfigureDevice_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var device = button.Tag as DeviceInfo;

            if (device != null)
            {
                if (device.IsConnected)
                {
                    OpenConfigurationTab(device);
                }
                else
                {
                    ShowErrorMessage("Please connect to the device first!");
                }
            }
        }

        #endregion

        #region Filter and Sort

        private void ShowFilterOptions_Click(object sender, RoutedEventArgs e)
        {
            FilterOptionsPopup.Visibility = FilterOptionsPopup.Visibility == Visibility.Visible ?
                Visibility.Collapsed : Visibility.Visible;
        }

        private void Filter_Click(object sender, FilterEventArgs args)
        {
            try
            {
                // Apply filtering
                _deviceViewSource.Filter -= null;
                _deviceViewSource.Filter += (s, args) =>
                {
                    if (args.Item is DeviceInfo device)
                    {
                        bool showDevice = false;

                        // Apply status filters
                        if (ShowConnectedCheckBox.IsChecked == true && device.Status == "Connected")
                            showDevice = true;
                        if (ShowDisconnectedCheckBox.IsChecked == true && device.Status == "Disconnected")
                            showDevice = true;
                        if (ShowOfflineCheckBox.IsChecked == true && device.Status == "Offline")
                            showDevice = true;

                        args.Accepted = showDevice;
                    }
                };

                // Apply grouping
                _deviceViewSource.GroupDescriptions.Clear();
                if (GroupByLocationCheckBox.IsChecked == true)
                {
                    _deviceViewSource.GroupDescriptions.Add(new PropertyGroupDescription("Location"));
                }

                // Apply sorting
                _deviceViewSource.SortDescriptions.Clear();
                if (SortAlphabetRadio.IsChecked == true)
                {
                    _deviceViewSource.SortDescriptions.Add(new SortDescription("DeviceName", ListSortDirection.Ascending));
                }
                else if (SortAlphabetReverseRadio.IsChecked == true)
                {
                    _deviceViewSource.SortDescriptions.Add(new SortDescription("DeviceName", ListSortDirection.Descending));
                }
                else if (SortLocationRadio.IsChecked == true)
                {
                    _deviceViewSource.SortDescriptions.Add(new SortDescription("Location", ListSortDirection.Ascending));
                    _deviceViewSource.SortDescriptions.Add(new SortDescription("DeviceName", ListSortDirection.Ascending));
                }

                _deviceViewSource.View.Refresh();
                FilterOptionsPopup.Visibility = Visibility.Collapsed;
                ShowSuccessMessage("Filter applied successfully!");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error applying filter: {ex.Message}");
            }
        }

        #endregion

        #region Configuration Tabs

         void OpenConfigurationTab(DeviceInfo device)
        {
            // Check if tab already exists for this device
            var existingTab = _configurationTabs.FirstOrDefault(t => t.DeviceIp == device.IpAddress);
            if (existingTab != null)
            {
                ShowInformationMessage("Configuration Tab", $"Configuration tab for {device.DeviceName} is already open.");
                return;
            }

            // Create new configuration tab
            var configTab = new ConfigurationTabInfo
            {
                DeviceIp = device.IpAddress,
                DeviceName = device.DeviceName,
                TabId = Guid.NewGuid().ToString()
            };

            _configurationTabs.Add(configTab);

            // Create and show configuration window
            var configWindow = new DeviceConfigurationWindow(device);
            configWindow.ConfigurationClosed += (s, e) => RemoveConfigurationTab(configTab.TabId);
            configWindow.Show();

            // Add to open windows list
            _openConfigWindows.Add(configWindow);
        }

        private void RemoveConfigurationTab(string tabId)
        {
            var tab = _configurationTabs.FirstOrDefault(t => t.TabId == tabId);
            if (tab != null)
            {
                _configurationTabs.Remove(tab);
            }
        }

        private void CloseAllConfigurationTabs()
        {
            _configurationTabs.Clear();
            foreach (var window in _openConfigWindows.ToList())
            {
                window.Close();
            }
            _openConfigWindows.Clear();
        }

        public static void NotifyConfigurationCopied(DeviceConfiguration config)
        {
            _copiedConfiguration = config;

            // Update all open configuration windows
            foreach (var window in _openConfigWindows)
            {
                window.UpdatePasteButtonState(true);
            }
        }

        public static DeviceConfiguration GetCopiedConfiguration()
        {
            return _copiedConfiguration;
        }

        #endregion

        #region Helper Methods

        private void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowInformationMessage(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string ShowInputDialog(string prompt, string title, string defaultValue = "")
        {
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            var stackPanel = new StackPanel { Margin = new Thickness(10) };

            var label = new Label { Content = prompt };
            stackPanel.Children.Add(label);

            var textBox = new TextBox { Text = defaultValue, Margin = new Thickness(0, 10, 0, 10) };
            stackPanel.Children.Add(textBox);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };

            var okButton = new Button { Content = "OK", Width = 75, Height = 25, Margin = new Thickness(5) };
            var cancelButton = new Button { Content = "Cancel", Width = 75, Height = 25, Margin = new Thickness(5) };

            string result = "";
            bool? dialogResult = null;

            okButton.Click += (s, e) => { result = textBox.Text; dialogResult = true; dialog.Close(); };
            cancelButton.Click += (s, e) => { dialogResult = false; dialog.Close(); };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            dialog.Content = stackPanel;
            dialog.ShowDialog();

            return dialogResult == true ? result : "";
        }

        #endregion
    }

    #region Data Models

    public class DeviceInfo : INotifyPropertyChanged
    {
        private string _ipAddress;
        private int _port;
        private string _deviceName;
        private string _location;
        private bool _isConnected;
        private Brush _statusColor;
        private string _status;
        private string _deviceType;
        private string _serialNumber;
        private string _firmwareVersion;
        private DateTime _lastSeen;

        public string IpAddress
        {
            get { return _ipAddress; }
            set
            {
                _ipAddress = value;
                OnPropertyChanged(nameof(IpAddress));
            }
        }

        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged(nameof(Port));
            }
        }

        public string DeviceName
        {
            get { return _deviceName; }
            set
            {
                _deviceName = value;
                OnPropertyChanged(nameof(DeviceName));
                OnPropertyChanged(nameof(DeviceNameWithLocation));
            }
        }

        public string Location
        {
            get { return _location; }
            set
            {
                _location = value;
                OnPropertyChanged(nameof(Location));
                OnPropertyChanged(nameof(DeviceNameWithLocation));
            }
        }

        public string DeviceNameWithLocation
        {
            get { return $"{_deviceName} | {_location}"; }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        public Brush StatusColor
        {
            get { return _statusColor; }
            set
            {
                _statusColor = value;
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public string DeviceType
        {
            get { return _deviceType; }
            set
            {
                _deviceType = value;
                OnPropertyChanged(nameof(DeviceType));
            }
        }

        public string SerialNumber
        {
            get { return _serialNumber; }
            set
            {
                _serialNumber = value;
                OnPropertyChanged(nameof(SerialNumber));
            }
        }

        public string FirmwareVersion
        {
            get { return _firmwareVersion; }
            set
            {
                _firmwareVersion = value;
                OnPropertyChanged(nameof(FirmwareVersion));
            }
        }

        public DateTime LastSeen
        {
            get { return _lastSeen; }
            set
            {
                _lastSeen = value;
                OnPropertyChanged(nameof(LastSeen));
                OnPropertyChanged(nameof(LastSeenFormatted));
            }
        }

        public string LastSeenFormatted
        {
            get { return _lastSeen.ToString("yyyy-MM-dd HH:mm:ss"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DeviceConfiguration : INotifyPropertyChanged
    {
        // RS485 Settings
        private int _baudRate = 9600;
        private string _parity = "None";
        private int _dataBit = 8;
        private string _stopBit = "1";

        // Ethernet Settings
        private bool _staticIP = true;
        private string _ipAddress = "192.168.1.100";
        private string _gateway = "192.168.1.1";
        private string _netmask = "255.255.255.0";
        private string _dnsMain = "8.8.8.8";
        private string _dnsBackup = "8.8.4.4";
        private int _port = 502;
        private string _macAddress = "00:00:00:00:00:00";

        // RS485 Properties
        public int BaudRate
        {
            get { return _baudRate; }
            set { _baudRate = value; OnPropertyChanged(nameof(BaudRate)); }
        }

        public string Parity
        {
            get { return _parity; }
            set { _parity = value; OnPropertyChanged(nameof(Parity)); }
        }

        public int DataBit
        {
            get { return _dataBit; }
            set { _dataBit = value; OnPropertyChanged(nameof(DataBit)); }
        }

        public string StopBit
        {
            get { return _stopBit; }
            set { _stopBit = value; OnPropertyChanged(nameof(StopBit)); }
        }

        // Ethernet Properties
        public bool StaticIP
        {
            get { return _staticIP; }
            set { _staticIP = value; OnPropertyChanged(nameof(StaticIP)); }
        }

        public string IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; OnPropertyChanged(nameof(IpAddress)); }
        }

        public string Gateway
        {
            get { return _gateway; }
            set { _gateway = value; OnPropertyChanged(nameof(Gateway)); }
        }

        public string Netmask
        {
            get { return _netmask; }
            set { _netmask = value; OnPropertyChanged(nameof(Netmask)); }
        }

        public string DnsMain
        {
            get { return _dnsMain; }
            set { _dnsMain = value; OnPropertyChanged(nameof(DnsMain)); }
        }

        public string DnsBackup
        {
            get { return _dnsBackup; }
            set { _dnsBackup = value; OnPropertyChanged(nameof(DnsBackup)); }
        }

        public int Port
        {
            get { return _port; }
            set { _port = value; OnPropertyChanged(nameof(Port)); }
        }

        public string MacAddress
        {
            get { return _macAddress; }
            set { _macAddress = value; OnPropertyChanged(nameof(MacAddress)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Deep copy method
        public DeviceConfiguration Clone()
        {
            return new DeviceConfiguration
            {
                BaudRate = this.BaudRate,
                Parity = this.Parity,
                DataBit = this.DataBit,
                StopBit = this.StopBit,
                StaticIP = this.StaticIP,
                IpAddress = this.IpAddress,
                Gateway = this.Gateway,
                Netmask = this.Netmask,
                DnsMain = this.DnsMain,
                DnsBackup = this.DnsBackup,
                Port = this.Port,
                MacAddress = this.MacAddress
            };
        }
    }

    public class ConfigurationTabInfo
    {
        public string DeviceIp { get; set; }
        public string DeviceName { get; set; }
        public string TabId { get; set; }
    }

    #endregion
}

// Device Configuration Window
public partial class DeviceConfigurationWindow : Window
{
    private DeviceInfo _device;
    private DeviceConfiguration _configuration;
    public event EventHandler ConfigurationClosed;

    // UI Controls
    private TabControl _tabControl;
    private Button _copyButton;
    private Button _pasteButton;

    // RS485 Controls
    private ComboBox _baudRateCombo;
    private ComboBox _parityCombo;
    private ComboBox _dataBitCombo;
    private ComboBox _stopBitCombo;

    // Ethernet Controls
    private CheckBox _staticIPCheckBox;
    private TextBox _ipAddressTextBox;
    private TextBox _gatewayTextBox;
    private TextBox _netmaskTextBox;
    private TextBox _dnsMainTextBox;
    private TextBox _dnsBackupTextBox;
    private TextBox _portTextBox;
    private TextBox _macAddressTextBox;

    public DeviceConfigurationWindow(DeviceInfo device)
    {
        _device = device;
        _configuration = new DeviceConfiguration
        {
            IpAddress = device.IpAddress,
            Port = device.Port
        };
        InitializeComponent();
        SetupWindow();
    }

    private void InitializeComponent()
    {
        this.Title = $"Configure {_device.DeviceName}";
        this.Width = 900;
        this.Height = 700;
        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        this.ResizeMode = ResizeMode.CanResize;
        this.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
    }

    private void SetupWindow()
    {
        // Create the main grid
        var mainGrid = new Grid();
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        this.Content = mainGrid;

        // Top panel with controls
        var topPanel = new DockPanel
        {
            LastChildFill = false,
            Background = Brushes.White,
            Margin = new Thickness(0, 0, 0, 10)
        };

        // Close button (right side)
        var closeButton = new Button
        {
            Content = "✕",
            Width = 30,
            Height = 30,
            Background = Brushes.Red,
            Foreground = Brushes.White,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        closeButton.Click += CloseButton_Click;
        DockPanel.SetDock(closeButton, Dock.Right);
        topPanel.Children.Add(closeButton);

        // Copy and Paste buttons (left side)
        var copyPastePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10)
        };

        _copyButton = new Button
        {
            Content = "📋 Copy",
            Width = 80,
            Height = 30,
            Background = new SolidColorBrush(Color.FromRgb(40, 167, 69)),
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 10, 0)
        };
        _copyButton.Click += CopyButton_Click;

        _pasteButton = new Button
        {
            Content = "📋 Paste",
            Width = 80,
            Height = 30,
            Background = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
            Foreground = Brushes.White,
            IsEnabled = MainWindow.GetCopiedConfiguration() != null
        };
        _pasteButton.Click += PasteButton_Click;

        copyPastePanel.Children.Add(_copyButton);
        copyPastePanel.Children.Add(_pasteButton);
        DockPanel.SetDock(copyPastePanel, Dock.Left);
        topPanel.Children.Add(copyPastePanel);

        Grid.SetRow(topPanel, 0);
        mainGrid.Children.Add(topPanel);

        // Tab control
        _tabControl = new TabControl
        {
            Margin = new Thickness(10, 0, 10, 10)
        };

        // RS485 Configuration Tab
        var rs485Tab = new TabItem
        {
            Header = "RS485 Configuration",
            Content = CreateRS485ConfigurationPanel()
        };

        // IP Configuration Tab
        var ipTab = new TabItem
        {
            Header = "Ethernet Configuration",
            Content = CreateIPConfigurationPanel()
        };

        _tabControl.Items.Add(rs485Tab);
        _tabControl.Items.Add(ipTab);

        Grid.SetRow(_tabControl, 1);
        mainGrid.Children.Add(_tabControl);
    }

    private Panel CreateRS485ConfigurationPanel()
    {
        var scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var panel = new StackPanel { Margin = new Thickness(20) };

        panel.Children.Add(new TextBlock { Text = "RS485 Settings", FontSize = 20, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 20) });

        // RS485 Settings Grid
        var settingsGrid = new Grid();
        settingsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        settingsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) });
        settingsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        settingsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        settingsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        settingsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        int row = 0;

        // Baud Rate
        AddLabelToGrid(settingsGrid, "Baud Rate:", row, 0);
        _baudRateCombo = new ComboBox { Margin = new Thickness(0, 5, 0, 10) };
        _baudRateCombo.Items.Add("4800");
        _baudRateCombo.Items.Add("9600");
        _baudRateCombo.Items.Add("19200");
        _baudRateCombo.Items.Add("38400");
        _baudRateCombo.Items.Add("57600");
        _baudRateCombo.Items.Add("115200");
        _baudRateCombo.SelectedValue = _configuration.BaudRate.ToString();
        Grid.SetRow(_baudRateCombo, row++);
        Grid.SetColumn(_baudRateCombo, 1);
        settingsGrid.Children.Add(_baudRateCombo);

        // Parity
        AddLabelToGrid(settingsGrid, "Parity:", row, 0);
        _parityCombo = new ComboBox { Margin = new Thickness(0, 5, 0, 10) };
        _parityCombo.Items.Add("None");
        _parityCombo.Items.Add("Even");
        _parityCombo.Items.Add("Odd");
        _parityCombo.SelectedValue = _configuration.Parity;
        Grid.SetRow(_parityCombo, row++);
        Grid.SetColumn(_parityCombo, 1);
        settingsGrid.Children.Add(_parityCombo);

        // Data Bit
        AddLabelToGrid(settingsGrid, "Data Bit:", row, 0);
        _dataBitCombo = new ComboBox { Margin = new Thickness(0, 5, 0, 10) };
        _dataBitCombo.Items.Add("7");
        _dataBitCombo.Items.Add("8");
        _dataBitCombo.SelectedValue = _configuration.DataBit.ToString();
        Grid.SetRow(_dataBitCombo, row++);
        Grid.SetColumn(_dataBitCombo, 1);
        settingsGrid.Children.Add(_dataBitCombo);

        // Stop Bit
        AddLabelToGrid(settingsGrid, "Stop Bit:", row, 0);
        _stopBitCombo = new ComboBox { Margin = new Thickness(0, 5, 0, 10) };
        _stopBitCombo.Items.Add("1");
        _stopBitCombo.Items.Add("2");
        _stopBitCombo.SelectedValue = _configuration.StopBit;
        Grid.SetRow(_stopBitCombo, row++);
        Grid.SetColumn(_stopBitCombo, 1);
        settingsGrid.Children.Add(_stopBitCombo);

        panel.Children.Add(settingsGrid);

        // Buttons
        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 30, 0, 0) };

        var sendButton = new Button
        {
            Content = "Send RS485",
            Width = 120,
            Height = 35,
            Background = new SolidColorBrush(Color.FromRgb(74, 144, 226)),
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 10, 0)
        };
        sendButton.Click += SendRS485Config_Click;

        var readButton = new Button
        {
            Content = "Read RS485",
            Width = 120,
            Height = 35,
            Background = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
            Foreground = Brushes.White
        };
        readButton.Click += ReadRS485Config_Click;

        buttonPanel.Children.Add(sendButton);
        buttonPanel.Children.Add(readButton);
        panel.Children.Add(buttonPanel);

        scrollViewer.Content = panel;
        return panel;
    }

    private Panel CreateIPConfigurationPanel()
    {
        var scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var panel = new StackPanel { Margin = new Thickness(20) };

        panel.Children.Add(new TextBlock { Text = "Ethernet Settings", FontSize = 20, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 20) });

        // Static IP Checkbox
        _staticIPCheckBox = new CheckBox
        {
            Content = "Static IP",
            IsChecked = _configuration.StaticIP,
            FontSize = 14,
            Margin = new Thickness(0, 0, 0, 15)
        };
        _staticIPCheckBox.Checked += (s, e) => UpdateIPFieldsState(true);
        _staticIPCheckBox.Unchecked += (s, e) => UpdateIPFieldsState(false);
        panel.Children.Add(_staticIPCheckBox);

        // Ethernet Settings Grid
        var settingsGrid = new Grid();
        settingsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        settingsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) });

        int row = 0;

        // IP Address
        AddLabelToGrid(settingsGrid, "IP Address:", row, 0);
        _ipAddressTextBox = CreateTextBox(_configuration.IpAddress);
        Grid.SetRow(_ipAddressTextBox, row++);
        Grid.SetColumn(_ipAddressTextBox, 1);
        settingsGrid.Children.Add(_ipAddressTextBox);

        // Gateway
        AddLabelToGrid(settingsGrid, "Gateway:", row, 0);
        _gatewayTextBox = CreateTextBox(_configuration.Gateway);
        Grid.SetRow(_gatewayTextBox, row++);
        Grid.SetColumn(_gatewayTextBox, 1);
        settingsGrid.Children.Add(_gatewayTextBox);

        // Netmask
        AddLabelToGrid(settingsGrid, "Netmask:", row, 0);
        _netmaskTextBox = CreateTextBox(_configuration.Netmask);
        Grid.SetRow(_netmaskTextBox, row++);
        Grid.SetColumn(_netmaskTextBox, 1);
        settingsGrid.Children.Add(_netmaskTextBox);

        // DNS Main
        AddLabelToGrid(settingsGrid, "DNS Main:", row, 0);
        _dnsMainTextBox = CreateTextBox(_configuration.DnsMain);
        Grid.SetRow(_dnsMainTextBox, row++);
        Grid.SetColumn(_dnsMainTextBox, 1);
        settingsGrid.Children.Add(_dnsMainTextBox);

        // DNS Backup
        AddLabelToGrid(settingsGrid, "DNS Backup:", row, 0);
        _dnsBackupTextBox = CreateTextBox(_configuration.DnsBackup);
        Grid.SetRow(_dnsBackupTextBox, row++);
        Grid.SetColumn(_dnsBackupTextBox, 1);
        settingsGrid.Children.Add(_dnsBackupTextBox);

        // Port
        AddLabelToGrid(settingsGrid, "Port:", row, 0);
        _portTextBox = CreateTextBox(_configuration.Port.ToString());
        Grid.SetRow(_portTextBox, row++);
        Grid.SetColumn(_portTextBox, 1);
        settingsGrid.Children.Add(_portTextBox);

        // Add row definitions
        for (int i = 0; i < row; i++)
        {
            settingsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        panel.Children.Add(settingsGrid);

        // Device MAC Address Section
        panel.Children.Add(new TextBlock { Text = "Device MAC Address", FontSize = 16, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 30, 0, 10) });

        var macPanel = new StackPanel { Orientation = Orientation.Horizontal };
        _macAddressTextBox = new TextBox
        {
            Text = _configuration.MacAddress,
            Width = 200,
            IsReadOnly = true,
            Background = Brushes.LightGray,
            Margin = new Thickness(0, 0, 10, 0)
        };

        var getMacButton = new Button
        {
            Content = "Get MAC",
            Width = 100,
            Height = 30,
            Background = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
            Foreground = Brushes.White
        };
        getMacButton.Click += GetMAC_Click;

        macPanel.Children.Add(_macAddressTextBox);
        macPanel.Children.Add(getMacButton);
        panel.Children.Add(macPanel);

        // Buttons
        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 30, 0, 0) };

        var readButton = new Button
        {
            Content = "Read Ethernet",
            Width = 120,
            Height = 35,
            Background = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 10, 0)
        };
        readButton.Click += ReadEthernetConfig_Click;

        var sendButton = new Button
        {
            Content = "Send Ethernet",
            Width = 120,
            Height = 35,
            Background = new SolidColorBrush(Color.FromRgb(74, 144, 226)),
            Foreground = Brushes.White
        };
        sendButton.Click += SendEthernetConfig_Click;

        buttonPanel.Children.Add(readButton);
        buttonPanel.Children.Add(sendButton);
        panel.Children.Add(buttonPanel);

        scrollViewer.Content = panel;
        return panel;
    }

    private void AddLabelToGrid(Grid grid, string text, int row, int column)
    {
        var label = new TextBlock
        {
            Text = text,
            FontSize = 14,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 5, 10, 5)
        };
        Grid.SetRow(label, row);
        Grid.SetColumn(label, column);
        grid.Children.Add(label);
    }

    private TextBox CreateTextBox(string text)
    {
        return new TextBox
        {
            Text = text,
            Margin = new Thickness(0, 5, 0, 10),
            Padding = new Thickness(5),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromRgb(204, 204, 204))
        };
    }

    private void UpdateIPFieldsState(bool isStatic)
    {
        _ipAddressTextBox.IsEnabled = isStatic;
        _gatewayTextBox.IsEnabled = isStatic;
        _netmaskTextBox.IsEnabled = isStatic;
        _dnsMainTextBox.IsEnabled = isStatic;
        _dnsBackupTextBox.IsEnabled = isStatic;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        ConfigurationClosed?.Invoke(this, EventArgs.Empty);
        MainWindow._openConfigWindows.Remove(this);
        this.Close();
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateConfigurationFromUI();
        MainWindow.NotifyConfigurationCopied(_configuration.Clone());
        MessageBox.Show("Configuration copied successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void PasteButton_Click(object sender, RoutedEventArgs e)
    {
        var copiedConfig = MainWindow.GetCopiedConfiguration();
        if (copiedConfig != null)
        {
            _configuration = copiedConfig.Clone();
            UpdateUIFromConfiguration();
            MessageBox.Show("Configuration pasted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public void UpdatePasteButtonState(bool hasCopiedConfig)
    {
        if (_pasteButton != null)
        {
            _pasteButton.IsEnabled = hasCopiedConfig;
            if (hasCopiedConfig)
            {
                _pasteButton.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69));
            }
            else
            {
                _pasteButton.Background = new SolidColorBrush(Color.FromRgb(108, 117, 125));
            }
        }
    }

    private void UpdateConfigurationFromUI()
    {
        // Update RS485 settings
        if (_baudRateCombo != null && _baudRateCombo.SelectedItem != null)
            _configuration.BaudRate = int.Parse(_baudRateCombo.SelectedItem.ToString());
        if (_parityCombo != null && _parityCombo.SelectedItem != null)
            _configuration.Parity = _parityCombo.SelectedItem.ToString();
        if (_dataBitCombo != null && _dataBitCombo.SelectedItem != null)
            _configuration.DataBit = int.Parse(_dataBitCombo.SelectedItem.ToString());
        if (_stopBitCombo != null && _stopBitCombo.SelectedItem != null)
            _configuration.StopBit = _stopBitCombo.SelectedItem.ToString();

        // Update Ethernet settings
        _configuration.StaticIP = _staticIPCheckBox.IsChecked ?? false;
        _configuration.IpAddress = _ipAddressTextBox.Text;
        _configuration.Gateway = _gatewayTextBox.Text;
        _configuration.Netmask = _netmaskTextBox.Text;
        _configuration.DnsMain = _dnsMainTextBox.Text;
        _configuration.DnsBackup = _dnsBackupTextBox.Text;
        if (int.TryParse(_portTextBox.Text, out int port))
            _configuration.Port = port;
    }

    private void UpdateUIFromConfiguration()
    {
        // Update RS485 settings
        if (_baudRateCombo != null)
            _baudRateCombo.SelectedItem = _configuration.BaudRate.ToString();
        if (_parityCombo != null)
            _parityCombo.SelectedItem = _configuration.Parity;
        if (_dataBitCombo != null)
            _dataBitCombo.SelectedItem = _configuration.DataBit.ToString();
        if (_stopBitCombo != null)
            _stopBitCombo.SelectedItem = _configuration.StopBit;

        // Update Ethernet settings
        _staticIPCheckBox.IsChecked = _configuration.StaticIP;
        _ipAddressTextBox.Text = _configuration.IpAddress;
        _gatewayTextBox.Text = _configuration.Gateway;
        _netmaskTextBox.Text = _configuration.Netmask;
        _dnsMainTextBox.Text = _configuration.DnsMain;
        _dnsBackupTextBox.Text = _configuration.DnsBackup;
        _portTextBox.Text = _configuration.Port.ToString();
        _macAddressTextBox.Text = _configuration.MacAddress;

        UpdateIPFieldsState(_configuration.StaticIP);
    }

    private void SendRS485Config_Click(object sender, RoutedEventArgs e)
    {
        UpdateConfigurationFromUI();
        MessageBox.Show("RS485 Configuration sent successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ReadRS485Config_Click(object sender, RoutedEventArgs e)
    {
        // Simulate reading configuration from device
        MessageBox.Show("RS485 Configuration read successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void SendEthernetConfig_Click(object sender, RoutedEventArgs e)
    {
        UpdateConfigurationFromUI();
        MessageBox.Show("Ethernet Configuration sent successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ReadEthernetConfig_Click(object sender, RoutedEventArgs e)
    {
        // Simulate reading configuration from device
        MessageBox.Show("Ethernet Configuration read successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void GetMAC_Click(object sender, RoutedEventArgs e)
    {
        // Simulate getting MAC address from device
        _configuration.MacAddress = "AA:BB:CC:DD:EE:FF";
        _macAddressTextBox.Text = _configuration.MacAddress;
        MessageBox.Show("MAC Address retrieved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}