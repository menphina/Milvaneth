using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class UserConfig : IUserConfig
    {
        [Key(0)]
        public bool ConfAutoAttach { get; set; } = true;
        [Key(1)]
        public bool ConfLogDebugInfo { get; set; } = false;
        [Key(2)]
        public bool ConfLogChatLogContent { get; set; } = false;
        [Key(3)]
        public bool ConfUseParent { get; set; } = true;
        [Key(4)]
        public bool ConfEnableMultiSupport { get; set; } = true;
        [Key(5)]
        public bool ConfNotUseHunter { get; set; } = false;
        [Key(6)]
        public bool ConfEnableDevConsole { get; set; } = false;
        [Key(7)]
        public bool ConfAutoUpdate { get; set; } = true;
        [Key(8)]
        public bool ConfShowOverlay { get; set; } = true;
        [Key(9)]
        public bool ConfClickthough { get; set; } = false;
        [Key(10)]
        public bool ConfUploadDetail { get; set; } = false;
        [Key(11)]
        public bool ConfRunStartup { get; set; } = false;
        [Key(12)]
        public bool ConfMinimizeToTray { get; set; } = true;
        [Key(13)]
        public bool ConfFullExit { get; set; } = false;
        [Key(14)]
        public bool ConfEnableRecipe { get; set; } = false;
        [Key(15)]
        public bool ConfEnableArtisan { get; set; } = false;
        [Key(16)]
        public bool ConfEnableLoot { get; set; } = false;

        [Key(17)]
        public double OverlayHeight { get; set; } = 300;
        [Key(18)]
        public double OverlayWidth { get; set; } = 500;
        [Key(19)]
        public double OverlayTop { get; set; } = 20;
        [Key(20)]
        public double OverlayLeft { get; set; } = 20;
    }
}
