﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Utils;
using WASDSharedAPI;

namespace QuickDefuse;

public class QuickDefuse : BasePlugin
{
    public override string ModuleName => "QuickDefuse";
    public override string ModuleVersion => "1.0.3";
    public override string ModuleAuthor => "Interesting";

    public int Wire;
    public string[] Colors = { "Red", "Blue", "Green", "Yellow"};
    public char[] ChatColorsArray = { ChatColors.Red, ChatColors.Blue, ChatColors.Green, ChatColors.Yellow };
    public static IWasdMenuManager? MenuManager;
    public static CCSPlayerController? Defuser;
    
    public override void Load(bool hotReload)
    {
        RegisterEventHandler<EventBombBegindefuse>(OnBombBeginDefuse);
        RegisterEventHandler<EventBombAbortdefuse>((@event, info) =>
        {
            Defuser = null;
            var menuManager = GetMenuManager();
            menuManager?.CloseMenu(@event.Userid);
            return HookResult.Continue;
        });
        RegisterEventHandler<EventBombDefused>((@event, info) =>
        {
            Defuser = null;
            var menuManager = GetMenuManager();
            menuManager?.CloseMenu(@event.Userid);
            return HookResult.Continue;
        });
        RegisterEventHandler<EventBombExploded>((@event, info) =>
        {
            if (Defuser != null)
            {
                var menuManager = GetMenuManager();
                menuManager?.CloseMenu(Defuser);
                Defuser = null;
            }
            return HookResult.Continue;
        });
    }

    public IWasdMenuManager? GetMenuManager()
    {
        if (MenuManager == null)
            MenuManager = new PluginCapability<IWasdMenuManager>("wasdmenu:manager").Get();

        return MenuManager;
    }

    public IWasdMenu? CreateMenu()
    {
        var menuManager = GetMenuManager();
        IWasdMenu? menu = menuManager?.CreateMenu("Guess a wire to defuse the bomb!");
        for (int i = 0; i < Colors.Length; i++)
        {
            int choice = i;
            menu?.Add($"{Colors[i]} wire", (controller, option) =>
            {
                var bomb = Utilities.FindAllEntitiesByDesignerName<CPlantedC4>("planted_c4").ToList().FirstOrDefault();
                if (bomb == null)
                    return;
                if (Wire == choice)
                {
                    Server.NextFrame(() =>
                    {
                        bomb.DefuseCountDown = 0.0f;
                        Server.PrintToChatAll($" {ChatColors.Blue}[QuickDefuse]{ChatColors.White} {controller.PlayerName} just defused the bomb by cutting the{ChatColorsArray[Wire]} {Colors[Wire]} wire!");
                    });
                }
                else
                {
                    bomb.C4Blow = 1.0f;
                    Server.PrintToChatAll($" {ChatColors.Blue}[QuickDefuse]{ChatColors.White} {controller.PlayerName} just detonated the bomb by cutting the{ChatColorsArray[Wire]} {Colors[Wire]} wire!");
                }
                menuManager?.CloseMenu(controller);
            });
        }
        return menu;
    }
    
    public HookResult OnBombBeginDefuse(EventBombBegindefuse @event, GameEventInfo info)
    {
        if (@event.Userid == null || @event.Userid.IsBot)
            return HookResult.Continue;
        Defuser = @event.Userid;
        Wire = new Random().Next(0, Int32.MaxValue) % Colors.Length;
        GetMenuManager()?.OpenMainMenu(@event.Userid, CreateMenu());
        return HookResult.Continue;
    }
}