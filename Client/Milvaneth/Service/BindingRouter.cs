using Milvaneth.Common;
using Milvaneth.Communication.Vendor;
using Milvaneth.Overlay;
using Milvaneth.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace Milvaneth.Service
{
    public class BindingRouter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal delegate void OnOverlayToggleDelegate(bool toggle);
        internal delegate void OnOverlayEventDelegate();
        internal event OnOverlayToggleDelegate OnOverlayVisibilityChanged;
        internal event OnOverlayToggleDelegate OnOverlayClickthroughChanged;
        internal event OnOverlayEventDelegate OnOverlayReloading;

        private readonly EnvironmentFile _envFile;
        private bool _enableInstaSave;

        #region LogPage

        // Toggle
        public bool LogRealtimeRefresh;

        // LogLines
        private readonly FixedSizedQueue<string> _logBuffer = new FixedSizedQueue<string>(1000);
        public string LogLines => string.Join(Environment.NewLine, _logBuffer);

        // CommandLine
        public static RoutedCommand LogCommandLineEnterCommand { get; set; } = new RoutedCommand();
        public string LogCommandLine { get; set; }

        #endregion

        #region ConfPage

        public bool ConfAutoAttach
        {
            get => SubprocessManagementService.AutoAttach;
            set
            {
                SubprocessManagementService.AutoAttach = value;
                OnPropertyChanged(nameof(ConfAutoAttach));
            }
        }
        public bool ConfLogDebugInfo
        {
            get => SubprocessManagementService.LogDebugInfo;
            set
            {
                SubprocessManagementService.LogDebugInfo = value;
                OnPropertyChanged(nameof(ConfLogDebugInfo));
            }
        }
        public bool ConfLogChatLogContent
        {
            get => SubprocessManagementService.LogChatLogContent;
            set
            {
                SubprocessManagementService.LogChatLogContent = value;
                OnPropertyChanged(nameof(ConfLogChatLogContent));
            }
        }
        public bool ConfUseParent
        {
            get => SubprocessManagementService.UseParent;
            set
            {
                SubprocessManagementService.UseParent = value;
                OnPropertyChanged(nameof(ConfUseParent));
            }
        }
        public bool ConfEnableMultiSupport
        {
            get => SubprocessManagementService.EnableMultiSupport;
            set
            {
                SubprocessManagementService.EnableMultiSupport = value;
                OnPropertyChanged(nameof(ConfEnableMultiSupport));
            }
        }
        public bool ConfNotUseHunter
        {
            get => !SubprocessManagementService.UseHunter;
            set
            {
                SubprocessManagementService.UseHunter = !value;
                OnPropertyChanged(nameof(ConfNotUseHunter));
            }
        }

        private bool _confEnableDevConsole;
        public bool ConfEnableDevConsole
        {
            get => _confEnableDevConsole;
            set
            {
                _confEnableDevConsole = value;
                OnPropertyChanged(nameof(ConfEnableDevConsole));
            }
        }


        private bool _confAutoUpdate;
        public bool ConfAutoUpdate
        {
            get => _confAutoUpdate;
            set
            {
                _confAutoUpdate = value;
                OnPropertyChanged(nameof(ConfAutoUpdate));
            }
        }

        private bool _confShowOverlay;
        public bool ConfShowOverlay
        {
            get => _confShowOverlay;
            set
            {
                _confShowOverlay = value;
                OnOverlayVisibilityChanged?.Invoke(_confShowOverlay);
                OnPropertyChanged(nameof(ConfShowOverlay));
            }
        }

        private bool _confClickthrough;
        public bool ConfClickthough
        {
            get => _confClickthrough;
            set
            {
                _confClickthrough = value;
                OnOverlayClickthroughChanged?.Invoke(_confClickthrough);
                OnPropertyChanged(nameof(ConfClickthough));
            }
        }
        public bool ConfUploadDetail
        {
            get => SystemManagementService.AllowInfoGather;
            set
            {
                SystemManagementService.AllowInfoGather = value;
                OnPropertyChanged(nameof(ConfUploadDetail));
            }
        }

        private bool _confRunStartup;
        public bool ConfRunStartup
        {
            get => _confRunStartup;
            set
            {
                _confRunStartup = value;
                SystemManagementService.SetStartup(_confRunStartup);
                OnPropertyChanged(nameof(ConfRunStartup));
            }
        }

        private bool _confMinimizeToTray;
        public bool ConfMinimizeToTray
        {
            get => _confMinimizeToTray;
            set
            {
                _confMinimizeToTray = value;
                OnPropertyChanged(nameof(ConfMinimizeToTray));
            }
        }

        private bool _confFullExit;
        public bool ConfFullExit
        {
            get => _confFullExit;
            set
            {
                _confFullExit = value;
                OnPropertyChanged(nameof(ConfFullExit));
            }
        }

        private bool _confEnableRecipe;
        public bool ConfEnableRecipe
        {
            get => _confEnableRecipe;
            set
            {
                _confEnableRecipe = value;
                OnPropertyChanged(nameof(ConfEnableRecipe));
            }
        }

        private bool _confEnableArtisan;
        public bool ConfEnableArtisan
        {
            get => _confEnableArtisan;
            set
            {
                _confEnableArtisan = value;
                OnPropertyChanged(nameof(ConfEnableArtisan));
            }
        }

        private bool _confEnableLoot;
        public bool ConfEnableLoot
        {
            get => _confEnableLoot;
            set
            {
                _confEnableLoot = value;
                OnPropertyChanged(nameof(ConfEnableLoot));
            }
        }

        #endregion

        #region AboutPage

        public int AboutMilvanethVersion => MilvanethConfig.Store.Global.MilVersion;

        public int AboutDataVersion => MilvanethConfig.Store.Global.DataVersion;

        public int AboutGameVersion => MilvanethConfig.Store.Global.GameVersion;

        #endregion

        #region MainUI

        // Tab Selector
        private int _uiActiveTab = 3;
        public int UiActiveTab
        {
            get => _uiActiveTab;
            set
            {
                _uiActiveTab = value;
                OnPropertyChanged(nameof(UiActiveTab), true);
            }
        }

        // Search Box
        private string _uiSearchLine;
        public static RoutedCommand UiSearchLineEnterCommand { get; set; } = new RoutedCommand();
        public string UiSearchLine
        {
            get => _uiSearchLine;
            set
            {
                _uiSearchLine = value;
                OnPropertyChanged(nameof(UiSearchLine), true);
            }
        }

        #endregion

        #region ItemPage & Overlay

        private double _overlayHeight;
        public double OverlayHeight
        {
            get => _overlayHeight;
            set
            {
                _overlayHeight = value;
                OnPropertyChanged(nameof(OverlayHeight), true);
            }
        }

        private double _overlayWidth;
        public double OverlayWidth
        {
            get => _overlayWidth;
            set
            {
                _overlayWidth = value;
                OnPropertyChanged(nameof(OverlayWidth), true);
            }
        }

        private double _overlayTop;
        public double OverlayTop
        {
            get => _overlayTop;
            set
            {
                _overlayTop = value;
                OnPropertyChanged(nameof(OverlayTop), true);
            }
        }

        private double _overlayLeft;
        public double OverlayLeft
        {
            get => _overlayLeft;
            set
            {
                _overlayLeft = value;
                OnPropertyChanged(nameof(OverlayLeft), true);
            }
        }

        private int _overlayItemId;
        public int OverlayItemId
        {
            get => _overlayItemId;
            set
            {
                _overlayItemId = value;
                OnPropertyChanged(nameof(OverlayItemId), true);
            }
        }

        private int _overlayActiveTab;
        public int OverlayActiveTab
        {
            get => _overlayActiveTab;
            set
            {
                _overlayActiveTab = value;
                OnPropertyChanged(nameof(OverlayActiveTab), true);
            }
        }

        private List<OverviewData> _overlayOverviewData;
        public List<OverviewData> OverlayOverviewData
        {
            get => _overlayOverviewData ?? new List<OverviewData>();
            set
            {
                _overlayOverviewData = value;
                OnPropertyChanged(nameof(OverlayOverviewData), true);
            }
        }

        private List<ListingData> _overlayListingData;
        public List<ListingData> OverlayListingData
        {
            get => _overlayListingData ?? new List<ListingData>();
            set
            {
                _overlayListingData = value;
                OnPropertyChanged(nameof(OverlayListingData), true);
            }
        }

        private List<HistoryData> _overlayHistoryData;
        public List<HistoryData> OverlayHistoryData
        {
            get => _overlayHistoryData ?? new List<HistoryData>();
            set
            {
                _overlayHistoryData = value;
                OnPropertyChanged(nameof(OverlayHistoryData), true);
            }
        }

        #endregion

        #region Internal

        private string _internalOverlayAssemblyPath;
        internal string InternalOverlayAssemblyPath
        {
            get => _internalOverlayAssemblyPath;
            set
            {
                _internalOverlayAssemblyPath = value;

                if(_internalOverlayAssemblyPath != null)
                    OnOverlayReloading?.Invoke();

                OnPropertyChanged(nameof(InternalOverlayAssemblyPath));
            }
        }

        private string _internalPerferredApiEntry;
        internal string InternalPerferredApiEntry
        {
            get => _internalPerferredApiEntry;
            set
            {
                _internalPerferredApiEntry = value;
                ApiVendor.SetPreferredEndpoint(_internalPerferredApiEntry);
                LoggingManagementService.WriteLine($@"API Endpoint now prefer ""{_internalPerferredApiEntry}""", "ApiMgmt");
                OnPropertyChanged(nameof(InternalPerferredApiEntry));
            }
        }

        #endregion

        internal BindingRouter(EnvironmentFile env)
        {
            LoggingManagementService.OnNewLogLine += _logBuffer.Enqueue;
            _envFile = env;
            LoadConfig();
            _confRunStartup = SystemManagementService.GetStartup();
        }

        internal void OnPropertyChanged(string name, bool noConfigSave = false)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            if(_enableInstaSave && !noConfigSave)
                SaveConfig();
        }

        internal void DummyListener(string __)
        {
            if(LogRealtimeRefresh)
                OnPropertyChanged(nameof(LogLines), true);
        }

        private void LoadConfig()
        {
            var conf = (UserConfig) _envFile.ReadEnvFile(DataStore.Milvaneth) ?? new UserConfig();
            var intern = (UserInternalConfig) _envFile.ReadEnvFile(DataStore.Internal) ?? new UserInternalConfig();

            CopyPropertiesTo(conf, this);
            CopyPropertiesTo(intern, this);

            ApiVendor.SetUsername(intern.InternalUsername);
            ApiVendor.SetRenew(intern.InternalRenewToken);
        }

        internal void SaveConfig()
        {
            var conf = new UserConfig();
            var intern = new UserInternalConfig();

            CopyPropertiesTo(this, conf);
            CopyPropertiesTo(this, intern);

            intern.InternalUsername = ApiVendor.GetUsername();
            intern.InternalRenewToken = ApiVendor.GetRenew();

            _envFile.WriteEnvFile(DataStore.Milvaneth, conf, false);
            _envFile.WriteEnvFile(DataStore.Internal, intern);
        }

        internal void ClearOverlayDelegates()
        {
            OnOverlayVisibilityChanged = null;
            OnOverlayClickthroughChanged = null;
        }

        internal void HideOverlay()
        {
            OnOverlayVisibilityChanged?.Invoke(false);
        }

        private void CopyPropertiesTo<TSource, TDest>(TSource source, TDest dest)
        {
            _enableInstaSave = false;

            var sourceProps = typeof(TSource).GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.CanRead)
                .ToList();
            var destProps = typeof(TDest).GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.CanWrite)
                .ToList();

            foreach (var sourceProp in sourceProps)
            {
                if (destProps.All(x => x.Name != sourceProp.Name))
                    continue;

                var p = destProps.First(x => x.Name == sourceProp.Name);
                p.SetValue(dest, sourceProp.GetValue(source, null), null);
            }

            _enableInstaSave = true;
        }
    }
}
