using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using LR.UI.Enum;
using Zenject;

namespace LR.Manager.UI
{
  public class UIManager : MonoBehaviour,
    ICanvasProvider,
    IInitializable
  {
    [System.Serializable]
    public class CanvasSet
    {
      public RootType type;
      public Canvas canvas;
    }

    [Header("[ Canvas ]")]
    [SerializeField] private List<CanvasSet> canvasSets = new();    

    [Inject] readonly private UISelectedGameObjectService selectedGameObjectService = null;
    [Inject] readonly private UIDepthService depthService = null;

    public void Initialize()
    {
      var updateDisposable = gameObject
        .UpdateAsObservable()
        .Subscribe(_ => OnUpdate());

      gameObject
        .OnDestroyAsObservable()
        .Subscribe(_ => updateDisposable.Dispose());
    }

    private void OnUpdate()
    {
      selectedGameObjectService.UpdateDetectingSelectedObject();
      depthService.UpdateFocusingSelectedGameObject();
    }

    #region ICanvasProvider
    public Canvas GetCanvas(RootType rootType)
    {
      var set = canvasSets.First(set => set.type == rootType);

      return set == null ? throw new System.NotImplementedException() : set.canvas;
    }
    #endregion

    private void OnDestroy()
    {
    }
  }
}