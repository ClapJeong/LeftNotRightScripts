using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace LR.UI.VolumeControl 
{
  public class UIVolumeControlView : BaseUIView
  {
    [field: SerializeField] public UIVolumeSet MasterVolumeSet { get; private set; }
    [field: SerializeField] public UIVolumeSet BGMVolumeSet { get; private set; }
    [field: SerializeField] public UIVolumeSet SFXVolumeSet { get; private set; }

    public async override UniTask HideAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.CompletedTask;
    }

    public async override UniTask ShowAsync(bool isImmediately = false, CancellationToken token = default)
    {
      await UniTask.CompletedTask;
    }
  }
}
