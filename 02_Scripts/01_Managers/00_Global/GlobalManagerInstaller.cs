using LR.Manager.Device;
using LR.Manager.Input;
using LR.Manager.Sound;
using LR.Manager.Store;
using LR.Manager.UI;
using LR.Manager.Scene;
using UnityEngine;
using Zenject;
using LR.Manager.GameDataManager;

namespace LR.Manager
{
  public class GlobalManagerInstaller : MonoInstaller
  {
    public static GlobalManagerInstaller instance;

    private void Awake()
    {
      instance = this;
    }
    [field: SerializeField] public SceneContext GlobalSceneContext { get; private set; }

    [SerializeField] private GlobalManager globalManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private StoreManager storeManager;
    [SerializeField] private InputActionManager inputActionManager;
    [SerializeField] private TableContainer table;
    [SerializeField] private AudioSourceContainer audioSourceContainer;
    [SerializeField] private Transform indicatorDisableRoot;

    public override void InstallBindings()
    {
      Container.BindInterfacesAndSelfTo<UIManager>().FromInstance(uiManager).AsSingle();
      Container.BindInterfacesAndSelfTo<SceneService>().AsSingle();
      Container.BindInterfacesAndSelfTo<StoreManager>().FromInstance(storeManager).AsSingle();

      Container.BindInstance(globalManager).AsSingle();      
      Container.BindInstance(table).AsSingle();
      Container.BindInstance(table.AddressableKeySO).AsSingle();
      Container.BindInstance(table.PlayerEnergyDataSO).AsSingle();
      Container.BindInstance(table.TriggerTileModelSO).AsSingle();
      Container.BindInstance(table.UISO).AsSingle();
      Container.BindInstance(table.LocalizationSO).AsSingle();
      Container.BindInstance(table.DialogueUIDataSO).AsSingle();
      Container.BindInstance(table.EffectTableSO).AsSingle();
      Container.BindInstance(table.StageGimmickSO).AsSingle();
      Container.BindInstance(table.CameraDataSO).AsSingle();
      Container.BindInstance(table.ColorSO).AsSingle();
      Container.BindInstance(table.SoundSO).AsSingle();
      Container.BindInstance(table.StageCompleteDataSO).AsSingle();
      Container.BindInstance(table.StageShowDataSO).AsSingle();
      Container.BindInstance(table.PlayerModelSO).AsSingle();
      Container.BindInstance(table.WallHitObjectSO).AsSingle();
      Container.BindInstance(table.GlobalModifierSO).AsSingle();

      Container.BindInstance(audioSourceContainer).AsSingle();

      Container.Bind<Transform>()
          .WithId("IndicatorDisableRoot")
          .FromInstance(indicatorDisableRoot);

      Container.BindInterfacesAndSelfTo<UIPresenterContainer>().AsSingle();
      Container.BindInterfacesAndSelfTo<UISelectedGameObjectService>().AsSingle();
      Container.BindInterfacesAndSelfTo<UIDepthService>().AsSingle();
      Container.BindInterfacesAndSelfTo<UIIndicatorService>().AsSingle();
      Container.BindInterfacesAndSelfTo<UISubmitController>().AsSingle();

      Container.BindInterfacesAndSelfTo<VeryFirstService>().AsSingle();

      Container.BindInterfacesAndSelfTo<InputActionManager>().FromInstance(inputActionManager).AsSingle();

      Container.BindInterfacesAndSelfTo<DeviceManager>().AsSingle();

      Container.BindInterfacesAndSelfTo<ResourceManager>().AsSingle();

      Container.BindInterfacesAndSelfTo<GameDataService>().AsSingle();

      Container.BindInterfacesAndSelfTo<LocaleService>().AsSingle();

      Container.BindInterfacesAndSelfTo<SoundService>().AsSingle();   
      
      Container.Inject(uiManager);
      uiManager.Initialize();
      Container.Inject(globalManager);
      globalManager.Initialize(Container.Resolve<LocaleService>());
    }
  }
}