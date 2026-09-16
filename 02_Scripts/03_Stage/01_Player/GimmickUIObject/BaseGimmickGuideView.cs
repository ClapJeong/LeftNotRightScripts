using Cysharp.Threading.Tasks;
using LR.Stage.Player.Enum;
using UnityEngine;
using Zenject;

namespace LR.Stage.Player.GimmickGuide
{
  public class BaseGimmickGuideView : MonoBehaviour
  {
    [SerializeField] protected Transform contentTransform;

    public virtual async UniTask InitializeAsync(PlayerType playerType, DiContainer diContainer)
    {
      transform.localPosition = Vector3.zero;
      await UniTask.CompletedTask;
    }
  }
}
