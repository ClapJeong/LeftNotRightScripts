using Cysharp.Threading.Tasks;
using LR.UI.GameScene.InputProgress;
using System;
using UnityEngine;

public interface IInputProgressUIService : IDisposable
{
  public UniTask<IUIInputProgressPresenter> GetPresenterAsync(
    InputProgressEnum.UIType type,
    Transform followTarget);
}
