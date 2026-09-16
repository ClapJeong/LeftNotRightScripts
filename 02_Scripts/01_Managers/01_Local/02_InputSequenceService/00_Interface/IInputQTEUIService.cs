using Cysharp.Threading.Tasks;
using LR.UI.GameScene.InputQTE;
using System;
using UnityEngine;

public interface IInputQTEUIService: IDisposable
{
  public UniTask<IUIInputQTEPresenter> GetPrsenterAsync(InputQTEEnum.UIType type, Transform followTarget);
}
