using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Timers;
using System.ComponentModel;
using CounterStrikeSharp.API.Modules.Memory;

namespace CSSReservation;
[MinimumApiVersion(100)]

public partial class CSSReservation : BasePlugin, IPluginConfig<ConfigRes>
{
    public override string ModuleName => "Reservation slot";
    public override string ModuleAuthor => "DeadSwim";
    public override string ModuleDescription => "Simple reservation kicking.";
    public override string ModuleVersion => "V. 1.0.0";



    public ConfigRes Config { get; set; }
    public static int MaxPlayers => NativeAPI.GetCommandParamValue("-maxplayers", DataType.DATA_TYPE_INT, 64);

    public int PlayerConnected;

    public void OnConfigParsed(ConfigRes config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        RegisterListener<Listeners.OnMapStart>(name =>
        {
            PlayerConnected = 0;

        });
        RegisterListener<Listeners.OnClientAuthorized>((index, id) =>
        {
            var player = Utilities.GetPlayerFromSlot(index);

            Authorization_Client(player);
        });
    }
    public void Authorization_Client(CCSPlayerController player)
    {
        var client = player.Index;
        var maxplayers = MaxPlayers;

        int connected = 0;
        foreach (var player_l in Utilities.GetPlayers().Where(player => player is { IsBot: false, IsValid: true }))
        {
            connected++;
        }
        PlayerConnected = connected;
        Server.PrintToConsole($"CSS RESERVATION - Player {player.PlayerName} try to connect on server. Actuall players on server {PlayerConnected}");
        bool kicked = false;
        if (PlayerConnected == MaxPlayers)
        {
            if (AdminManager.PlayerHasPermissions(player, $"{Config.permission}"))
            {
                foreach (var l_player in Utilities.GetPlayers())
                {
                    CCSPlayerController player_res = l_player;

                    var el_player = player_res.Index;
                    if (kicked == false)
                    {
                        if (AdminManager.PlayerHasPermissions(player, $"{Config.permission}"))
                        {
                            kicked = true;
                            Server.PrintToChatAll($" {Config.Prefix}Player {ChatColors.Lime}{player_res.PlayerName} {ChatColors.Default}has been kicked, bcs {ChatColors.Lime}Admin{ChatColors.Default} need to connect.");
                            Server.ExecuteCommand($"kickid {player_res.UserId}");
                        }
                    }
                }
            }
            else
            {
                Server.ExecuteCommand($"kickid {player.UserId}");
            }
        }
        else
        {
            Server.PrintToConsole($"CSS RESERVATION - Player {player.PlayerName} is connected, server is not full. Actuall players on server {PlayerConnected}");
        }
        kicked = false;
    }
}
