using Cysharp.Threading.Tasks;

public class BattlePhase : OverlayPhase
{
    private readonly BattleService battleService;

    private BattleRequest request;

    public BattlePhase(BattleService battleService)
    {
        this.battleService = battleService;
    }

    public void Configure(BattleRequest request)
    {
        this.request = request;
    }

    public override async UniTask Enter()
    {
        await battleService.Initialize();

        await battleService.StartBattle(request);
    }

    public override async UniTask Exit()
    {
        await battleService.Dispose();
    }
}