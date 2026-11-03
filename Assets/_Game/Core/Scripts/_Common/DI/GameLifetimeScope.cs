using Cysharp.Threading.Tasks;
using Game.Biomes;
using Game.Configs;
using Game.Core.Input;
using Game.Core.SaveLoad;
using Game.Gameplay;
using Game.Levels;
using Game.Settings;
using Game.Sound;
using Game.UI;
using Game.UI.PopupWindow;
using Game.Utils;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scopes
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameSettingsConfig _gameSettingsConfig;
        [SerializeField] private LevelListConfig _levelListConfig;

        protected override void Awake()
        {
            if (!autoRun)
            {
                base.Awake();
                return;
            }
            BuildAfterSaveLoadedAsync().Forget(Debug.LogException);
        }

        private async UniTask BuildAfterSaveLoadedAsync()
        {
            var root = VContainerSettings.Instance?.GetOrCreateRootLifetimeScopeInstance();
            if (root != null)
            {
                if (root.Container == null) root.Build();
                var save = root.Container.Resolve<ISaveLoadService>();
                if (!save.IsSaveDataLoaded)
                    await save.LoadDataAsync().AttachExternalCancellation(this.GetCancellationTokenOnDestroy());
            }
            if (this != null) base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_gameSettingsConfig);
            builder.RegisterInstance(_levelListConfig);

            builder.Register<PopupWindowService>(Lifetime.Singleton).As<IPopupWindowService>();
            builder.Register<SceneInjector>(Lifetime.Singleton).As<IInitializable>().As<IStartable>();
            builder.Register<GameplayService>(Lifetime.Singleton).As<IStartable>();
            builder.Register<LevelService>(Lifetime.Singleton).As<ILevelService>();
            builder.Register<GUIService>(Lifetime.Singleton).As<IGUIService>().As<IStartable>();
            builder.Register<InputService>(Lifetime.Singleton).As<IInputService>();
            builder.Register<MMSoundProvider>(Lifetime.Singleton).As<ISoundProvider>();
            builder.Register<SoundService>(Lifetime.Singleton).As<ISoundService>();
            builder.Register<SoundLoader>(Lifetime.Singleton).As<IInitializable>().As<IStartable>();

            builder.RegisterComponentInHierarchy<UIRoot>();
            builder.RegisterComponentInHierarchy<PopupWindow>();
            builder.RegisterComponentInHierarchy<BiomeCreator>();
        }
    }
}
