using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

public class PauseMenuExitTransitionTests
{
    [Test]
    public async Task ExitFromPauseClosesOverlaysRestoresTimeAndBlocksGameplayInput()
    {
        TestContext context = CreateContext(0.75f);
        await context.GameStateMachine.ReplaceMainAsync<ExplorationPhase>(CancellationToken.None);
        await context.GameStateMachine.PushOverlay<PauseMenuPhase>();

        context.ExitCommand.RequestExitToMainMenu();

        Assert.That(context.GameStateMachine.CurrentMainPhase, Is.TypeOf<MainMenuPhase>());
        Assert.That(context.GameStateMachine.HasOverlay, Is.False);
        Assert.That(context.TimeScaleController.TimeScale, Is.EqualTo(0.75f));
        Assert.That(context.InputBlockService.IsChannelBlocked(InputBlockChannels.Gameplay), Is.True);
    }

    [Test]
    public async Task ExitAfterSaveBrowserBackClosesPauseAndRestoresTime()
    {
        TestContext context = CreateContext(1f);
        await context.GameStateMachine.ReplaceMainAsync<ExplorationPhase>(CancellationToken.None);
        await context.GameStateMachine.PushOverlay<PauseMenuPhase>();
        await context.GameStateMachine.PushOverlay<SaveBrowserPhase>();
        await context.GameStateMachine.PopOverlay();

        context.ExitCommand.RequestExitToMainMenu();

        Assert.That(context.GameStateMachine.CurrentMainPhase, Is.TypeOf<MainMenuPhase>());
        Assert.That(context.GameStateMachine.HasOverlay, Is.False);
        Assert.That(context.TimeScaleController.TimeScale, Is.EqualTo(1f));
    }

    [Test]
    public async Task DoubleRequestStartsOnlyOneMainMenuTransition()
    {
        TestContext context = CreateContext(1f);
        await context.GameStateMachine.ReplaceMainAsync<ExplorationPhase>(CancellationToken.None);
        await context.GameStateMachine.PushOverlay<PauseMenuPhase>();
        context.SceneLoader.HoldNextSwitch();

        context.ExitCommand.RequestExitToMainMenu();
        context.ExitCommand.RequestExitToMainMenu();

        Assert.That(context.SceneLoader.RequestedScenes.Count(scene => scene == SceneNames.MainMenu), Is.EqualTo(1));
        Assert.That(context.ExitCommand.IsTransitionInProgress, Is.True);

        context.SceneLoader.CompleteHeldSwitch();
        await Task.Yield();

        Assert.That(context.ExitCommand.IsTransitionInProgress, Is.False);
    }

    [Test]
    public async Task StartingExplorationAgainReleasesMainMenuGameplayBlock()
    {
        TestContext context = CreateContext(1f);
        await context.GameStateMachine.ReplaceMainAsync<ExplorationPhase>(CancellationToken.None);
        await context.GameStateMachine.PushOverlay<PauseMenuPhase>();
        context.ExitCommand.RequestExitToMainMenu();

        Assert.That(context.InputBlockService.IsChannelBlocked(InputBlockChannels.Gameplay), Is.True);

        context.SceneLoader.HoldNextSwitch();
        UniTask transition = context.GameStateMachine.ReplaceMainAsync<ExplorationPhase>(CancellationToken.None);

        Assert.That(context.GameStateMachine.CurrentMainPhase, Is.TypeOf<MainMenuPhase>());
        Assert.That(context.InputBlockService.IsChannelBlocked(InputBlockChannels.Gameplay), Is.True);

        context.SceneLoader.CompleteHeldSwitch();
        await transition;

        Assert.That(context.GameStateMachine.CurrentMainPhase, Is.TypeOf<ExplorationPhase>());
        Assert.That(context.InputBlockService.IsChannelBlocked(InputBlockChannels.Gameplay), Is.False);
    }

    [Test]
    public async Task StartingExplorationClearsStaleGameplayBlocks()
    {
        TestContext context = CreateContext(1f);
        await context.GameStateMachine.ReplaceMainAsync<MainMenuPhase>(CancellationToken.None);
        context.InputBlockService.AcquireBlock(InputBlockChannels.Gameplay);

        await context.GameStateMachine.ReplaceMainAsync<ExplorationPhase>(CancellationToken.None);

        Assert.That(context.GameStateMachine.CurrentMainPhase, Is.TypeOf<ExplorationPhase>());
        Assert.That(context.InputBlockService.IsChannelBlocked(InputBlockChannels.Gameplay), Is.False);
    }

    private static TestContext CreateContext(float initialTimeScale)
    {
        var sceneLoader = new FakeSceneLoader();
        var timeScaleController = new FakeTimeScaleController(initialTimeScale);
        var gamePauseService = new GamePauseService(timeScaleController);
        var inputBlockService = new GameplayInputBlockService();
        var phaseFactory = new FakePhaseFactory();

        phaseFactory.Register(new ExplorationPhase(sceneLoader));
        phaseFactory.Register(new MainMenuPhase(sceneLoader));
        phaseFactory.Register(new PauseMenuPhase());
        phaseFactory.Register(new SaveBrowserPhase());

        var gameStateMachine = new GameStateMachine(phaseFactory, gamePauseService, inputBlockService);
        var exitCommand = new PauseMenuExitCommandService(
            gameStateMachine,
            gamePauseService,
            new FakeRuntimeErrorLogger());

        return new TestContext(
            gameStateMachine,
            exitCommand,
            sceneLoader,
            timeScaleController,
            inputBlockService);
    }

    private sealed class TestContext
    {
        public TestContext(
            GameStateMachine gameStateMachine,
            PauseMenuExitCommandService exitCommand,
            FakeSceneLoader sceneLoader,
            FakeTimeScaleController timeScaleController,
            GameplayInputBlockService inputBlockService)
        {
            GameStateMachine = gameStateMachine;
            ExitCommand = exitCommand;
            SceneLoader = sceneLoader;
            TimeScaleController = timeScaleController;
            InputBlockService = inputBlockService;
        }

        public GameStateMachine GameStateMachine { get; }
        public PauseMenuExitCommandService ExitCommand { get; }
        public FakeSceneLoader SceneLoader { get; }
        public FakeTimeScaleController TimeScaleController { get; }
        public GameplayInputBlockService InputBlockService { get; }
    }

    private sealed class FakePhaseFactory : IPhaseFactory
    {
        private readonly Dictionary<Type, GamePhase> phases = new();

        public void Register<T>(T phase) where T : GamePhase
        {
            phases[typeof(T)] = phase;
        }

        public T Get<T>() where T : GamePhase
        {
            return (T)phases[typeof(T)];
        }

        public GamePhase Get(Type phaseType)
        {
            return phases[phaseType];
        }
    }

    private sealed class FakeSceneLoader : ISceneLoader
    {
        private UniTaskCompletionSource heldSwitch;

        public string LoadedGameplayScene { get; private set; }
        public List<string> RequestedScenes { get; } = new();

        public UniTask SwitchTo(string sceneName)
        {
            return SwitchToAsync(sceneName, CancellationToken.None);
        }

        public async UniTask SwitchToAsync(string sceneName, CancellationToken cancellationToken)
        {
            RequestedScenes.Add(sceneName);

            if (heldSwitch != null)
                await heldSwitch.Task.AttachExternalCancellation(cancellationToken);

            LoadedGameplayScene = sceneName;
        }

        public UniTask ReloadAsync(string sceneName, CancellationToken cancellationToken)
        {
            return SwitchToAsync(sceneName, cancellationToken);
        }

        public UniTask LoadAdditive(string sceneName)
        {
            return UniTask.CompletedTask;
        }

        public UniTask LoadAdditiveAsync(string sceneName, CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        public UniTask Unload(string sceneName)
        {
            return UniTask.CompletedTask;
        }

        public bool IsLoaded(string sceneName)
        {
            return LoadedGameplayScene == sceneName;
        }

        public void SetCurrentScene(string sceneName)
        {
            LoadedGameplayScene = sceneName;
        }

        public void HoldNextSwitch()
        {
            heldSwitch = new UniTaskCompletionSource();
        }

        public void CompleteHeldSwitch()
        {
            heldSwitch.TrySetResult();
            heldSwitch = null;
        }
    }

    private sealed class FakeTimeScaleController : ITimeScaleController
    {
        public FakeTimeScaleController(float initialTimeScale)
        {
            TimeScale = initialTimeScale;
        }

        public float TimeScale { get; set; }
    }

    private sealed class FakeRuntimeErrorLogger : IRuntimeErrorLogger
    {
        public void LogException(Exception exception, string context)
        {
        }
    }
}
