using UnityEngine;
using Zenject;

public class BattleBootstrap : MonoBehaviour
{
    [SerializeField] private HPBarView playerHPBar;
    [SerializeField] private HPBarView enemyHPBar;
    [SerializeField] private ActionTextView playerTextView;
    [SerializeField] private ActionTextView enemyTextView;
    [SerializeField] private DamageTextView playerDamageView;
    [SerializeField] private DamageTextView enemyDamageView;

    private IBattleSceneTransitionService battleSceneTransitionService;
    private BattleSessionService battleSessionService;
    private BattleSessionController battleSessionController;
    private bool returnedToExploration;

    [Inject]
    public void Construct(
        IBattleSceneTransitionService battleSceneTransitionService,
        BattleSessionService battleSessionService,
        BattleSessionController battleSessionController)
    {
        this.battleSceneTransitionService = battleSceneTransitionService;
        this.battleSessionService = battleSessionService;
        this.battleSessionController = battleSessionController;
    }

    private void Start()
    {

        playerHPBar.Bind(battleSessionController.Player);
        enemyHPBar.Bind(battleSessionController.Enemy);

        playerTextView.Bind(battleSessionController.Player);
        enemyTextView.Bind(battleSessionController.Enemy);

        playerDamageView.Bind(battleSessionController.Player);
        enemyDamageView.Bind(battleSessionController.Enemy);
    }

    private void Update()
    {
        battleSessionController.Tick(Time.deltaTime);

        if (battleSessionController.IsBattleOver)
            ReturnToExploration();
    }

    private void ReturnToExploration()
    {
        if (returnedToExploration)
            return;

        returnedToExploration = true;
        battleSessionService.CompleteCurrentEncounter(battleSessionController.IsPlayerVictory);
        battleSceneTransitionService.RequestReturnToExploration();
    }
}
