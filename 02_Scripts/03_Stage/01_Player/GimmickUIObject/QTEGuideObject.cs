using UnityEngine;

namespace LR.Stage.Player.GimmickGuide.QTE
{
  public class QTEGuideObject : MonoBehaviour
  {
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private new ParticleSystem particleSystem;

    public void UpdateSprite(Sprite sprite)
      => spriteRenderer.sprite = sprite;
    
    public void UpdateColor(Color color)
    {
      spriteRenderer.color = color;
      var mainModule = particleSystem.main;
      mainModule.startColor = color;
    }

    public void UpdateScale(float scale)
      => spriteRenderer.transform.localScale = Vector3.one * scale;

    public void Deactivate(bool isSuccess)
    {
      spriteRenderer.enabled = false;
      if(isSuccess)
        particleSystem.Play();
    }

    public void Activate()
    {
      spriteRenderer.enabled = true;
    }
  }
}
