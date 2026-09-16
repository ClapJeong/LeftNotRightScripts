using LR.Manager.Local.CameraService;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace LR.Manager.Local
{
  public class LocalInstaller : MonoInstaller
  {
    [SerializeField] private LocalManager localManager;
    [SerializeField] private CameraManager cameraManager;

    [Space(10)]
    [SerializeField] private Transform effectDefaultroot;
    [SerializeField] private Transform gameObjectInstantiateRoot;
    [SerializeField] private RawImage shaderRawImge;
    [SerializeField] private Transform markRoot;

    private void Awake()
    {
      Container.Inject(cameraManager);
      Container.Inject(localManager);
      localManager.InjectInitialize();
    }

    public override void InstallBindings()
    {
      var newContainer = new DiContainer(GlobalManagerInstaller.instance.GlobalSceneContext.Container);
      Container = newContainer;

      Container.BindInterfacesAndSelfTo<CameraManager>().FromInstance(cameraManager).AsSingle();      
      Container.BindInstance(localManager).AsSingle();            

      Container.Bind<Transform>()
          .WithId("effectDefaultroot")
          .FromInstance(effectDefaultroot);

      Container.Bind<Transform>()
          .WithId("InstantiateRoot")
          .FromInstance(gameObjectInstantiateRoot);
      Container.Bind<Transform>()
          .WithId("MarkRoot")
          .FromInstance(markRoot);

      Container.Bind<RawImage>()
          .WithId("ShaderImage")
          .FromInstance(shaderRawImge);
    }
  }
}
