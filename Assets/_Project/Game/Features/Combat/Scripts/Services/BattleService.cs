// using Cysharp.Threading.Tasks;

// public class BattleService : SceneService
// {
//     private UniTaskCompletionSource completionSource;

//     public override UniTask Initialize()
//     {
//         return UniTask.CompletedTask;
//     }

//     public async UniTask StartBattle(BattleRequest request)
//     {
//         completionSource = new UniTaskCompletionSource();

//         BattleManager manager = BattleBootstrap.Instance.Manager;

//         manager.BattleFinished += OnBattleFinished;

//         await completionSource.Task;

//         manager.BattleFinished -= OnBattleFinished;
//     }

//     private void OnBattleFinished()
//     {
//         completionSource.TrySetResult();
//     }

//     public override UniTask Dispose()
//     {
//         return UniTask.CompletedTask;
//     }
// }