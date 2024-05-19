using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Menu;

namespace QuickDefuse;

public class QuickDefuse : BasePlugin
{
    public override string ModuleName => "QuickDefuse";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Interesting";

    public Dictionary<int, int> Wire = new();
    public string[] colors = { "Red", "Blue", "Green", "Yellow"};
    
    public override void Load(bool hotReload)
    {
        RegisterEventHandler<EventBombBegindefuse>(OnBombBeginDefuse);
    }

    public CenterHtmlMenu CreateMenu()
    {
        CenterHtmlMenu menu = new ("Guess a wire to instantly defuse the bomb", this);
        for (int i = 0; i < colors.Length; i++)
        {
            int choice = i;
            menu.AddMenuOption($"{colors[i]} wire", (controller, option) =>
            {
                var bomb = Utilities.FindAllEntitiesByDesignerName<CPlantedC4>("planted_c4").ToList().FirstOrDefault();
                if (bomb == null)
                    return;
                if (Wire[controller.Slot] == choice)
                {
                    Server.NextFrame(() =>
                    {
                        var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
                        gameRules.TerminateRound(0 , RoundEndReason.BombDefused);
                    });
                }
                else
                {
                    bomb.C4Blow = 1.0f;
                }
                MenuManager.CloseActiveMenu(controller);
            });
        }
        return menu;
    }
    
    public HookResult OnBombBeginDefuse(EventBombBegindefuse @event, GameEventInfo info)
    {
        if (@event.Userid == null)
            return HookResult.Continue;
        Wire[@event.Userid.Slot] = new Random().Next(0, colors.Length);
        MenuManager.OpenCenterHtmlMenu(this, @event.Userid, CreateMenu());
        
        return HookResult.Continue;
    }
    
    
}