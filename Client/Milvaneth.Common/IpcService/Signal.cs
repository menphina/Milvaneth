namespace Milvaneth.Common
{
    // @formatter:off
    public enum Signal
    {
        InternalException =                         SignalCatalog.Internal | 0x0000, // Message, Location
        InternalUnmanagedException =                SignalCatalog.Internal | 0x0001, // Message, Location
        InternalExternalException =                 SignalCatalog.Internal | 0x0002, // Message, Location
        InternalConnectionFin =                     SignalCatalog.Internal | 0x0003, // null - This not necessarily means client disconnected. A connection check should run before dispose.
        ClientInsuffcientPrivilege =                SignalCatalog.Client | 0x0000, // Pid - Show msgbox and exit (this should not occur but who knows)
        ClientNetworkFail =                         SignalCatalog.Client | 0x0001, // Pid - Msgbox and wait next alive (Client PC has no available interface or network)
        ClientProcessDown =                         SignalCatalog.Client | 0x0002, // Pid - Dispose and wait next alive
        ClientPacketParseFail =                     SignalCatalog.Client | 0x0003, // null - Do nothing (QA counter, may suggest switch to Pcap [if implemented])
        MilvanethSubprocessExit =                   SignalCatalog.Milvaneth | 0x0000, // Message, Location - check status
        MilvanethComponentExit =                    SignalCatalog.Milvaneth | 0x0001, // ComponentName, RoutineName - Try to revive, if failed, show msgbox and exit
        MilvanethSubprocessReady =                  SignalCatalog.Milvaneth | 0x0002, // Pid - start ev loop for another instance
        MilvanethNeedUpdate =                       SignalCatalog.Milvaneth | 0x0003, // ComponentName, RoutineName - Check update, show msgbox to hint or exit
        CommandParentExit =                         SignalCatalog.Command | 0x0000, // Parent Pid, ui process has exited
        CommandPurgeCache =                         SignalCatalog.Command | 0x0001, // Child Pid, make specific child purge cache
        CommandReloadCache =                        SignalCatalog.Command | 0x0002, // null, make all children reload cache
        CommandRescanMemory =                       SignalCatalog.Command | 0x0003, // Child Pid, make specific child rescan memory
        CommandRequireExit =                        SignalCatalog.Command | 0x0004, // child pid
    }

    public enum SignalCatalog
    {
        Internal =      0x0000_0000, // [local] some random internal things
        Client =        0x0001_0000, // [upstream] well, this is not our fault, but we have to deal with the mess
        Milvaneth =     0x0002_0000, // [upstream] threats the live of Nald
        Command =       0x0003_0000, // [downstream] request downstream thread to do something
    }
    // @formatter:on
}
