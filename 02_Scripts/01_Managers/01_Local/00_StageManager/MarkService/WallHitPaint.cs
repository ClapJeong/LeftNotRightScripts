using UnityEngine;

namespace LR.Manager.Stage.Marking
{
  public class WallHitPaint : MonoBehaviour
  {
    [SerializeField] private float idleAlpha;
    [SerializeField] private float previewAlpha;
    [Space(5)]
    [SerializeField] private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock materialPropertyBlock;

    private void Awake()
    {
      var color = spriteRenderer.sharedMaterial.GetColor("_Color");
      materialPropertyBlock = new();
      spriteRenderer.GetPropertyBlock(materialPropertyBlock);
      materialPropertyBlock.SetColor(ShaderHash.WallHitPaint._Color, color);
      spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    public void UpdatePreviewAlpha()
      => UpdateAlpha(previewAlpha);

    public void UpdateIdleAlpha()
      => UpdateAlpha(idleAlpha);

    private void UpdateAlpha(float alpha)
    {
      var color = materialPropertyBlock.GetColor(ShaderHash.WallHitPaint._Color);
      color.a = alpha;
      materialPropertyBlock.SetColor(ShaderHash.WallHitPaint._Color, color);
      spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }
  }
}
