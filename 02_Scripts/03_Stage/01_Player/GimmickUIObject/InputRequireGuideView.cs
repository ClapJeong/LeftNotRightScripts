using Cysharp.Threading.Tasks;
using LR.Stage.Player.Enum;
using UnityEngine;
using Zenject;

namespace LR.Stage.Player.GimmickGuide
{
  public class InputRequireGuideView : BaseGimmickGuideView
  {
    [SerializeField] protected Vector2 leftLocalPosition;
    [SerializeField] protected Vector2 rightLocalPosition;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem failEffect;

    [Space(5)]
    [SerializeField] private float shakeBeginNormalized;
    [SerializeField] private float shakeInterval;
    [SerializeField] private float shakeRangeMin;
    [SerializeField] private float shakeRangeMax;

    private PlayerType playerType;
    private MaterialPropertyBlock matBlock;
    private float currentNormalized = 1.0f;
    private float shakeDuration = 0.0f;

    private void Awake()
    {
      matBlock = new();
      spriteRenderer.GetPropertyBlock(matBlock);
      shakeDuration = shakeInterval;
    }

    private void Update()
    {
      if (currentNormalized > shakeBeginNormalized)
      {
        contentTransform.transform.localPosition = playerType switch
        {
          PlayerType.Left => leftLocalPosition,
          PlayerType.Right => rightLocalPosition,
          _ => throw new System.NotImplementedException(),
        };
        return;
      }        

      shakeDuration -= Time.deltaTime;

      if(shakeDuration <= 0.0f)
      {
        var t = 1.0f - currentNormalized / (1.0f - shakeBeginNormalized);
        var range = Mathf.Lerp(shakeRangeMin, shakeRangeMax, t);
        var offset = new Vector2(Random.Range(-range, range), Random.Range(-range, range));

        contentTransform.transform.localPosition = playerType switch
        {
          PlayerType.Left => leftLocalPosition,
          PlayerType.Right => rightLocalPosition,
          _ => throw new System.NotImplementedException(),
        } + offset;

        shakeDuration = shakeInterval;
      }
    }

    public override async UniTask InitializeAsync(PlayerType playerType, DiContainer diContainer)
    {
      await base.InitializeAsync(playerType, diContainer);
      this.playerType = playerType;

      contentTransform.transform.localPosition = playerType switch
      {
        PlayerType.Left => leftLocalPosition,
        PlayerType.Right => rightLocalPosition,
        _ => throw new System.NotImplementedException(),
      };

      var targetColor = diContainer.Resolve<ColorSO>().GetPlayerColor(playerType);
      spriteRenderer.color = targetColor;
      var mainModule = failEffect.main;
      mainModule.startColor = targetColor;

      matBlock.SetFloat(ShaderHash.InputRequireGuide._Clockwise, playerType switch
      {
        PlayerType.Left => 1.0f,
        PlayerType.Right => -1.0f,
        _ => throw new System.NotImplementedException(),
      });

      matBlock.SetFloat(ShaderHash.InputRequireGuide._FillAmount, 1.0f);
      spriteRenderer.SetPropertyBlock(matBlock);
    }

    public void UpdateFillAmount(float fillAmount)
    {
      if (this == null)
        return;

      currentNormalized = fillAmount;
      matBlock.SetFloat(ShaderHash.InputRequireGuide._FillAmount, fillAmount);
      spriteRenderer.SetPropertyBlock(matBlock);
    }

    public void UpdateAlpha(float alpha)
    {
      spriteRenderer.SetAlpha(alpha);
      if (alpha < 1.0f)
        failEffect.Play();
    }
  }
}