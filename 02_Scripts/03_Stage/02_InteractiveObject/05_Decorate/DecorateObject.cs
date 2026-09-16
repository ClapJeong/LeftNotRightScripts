using LR.Manager.Sound;
using LR.Manager.Stage;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LR.Stage.InteractiveObject
{
  public class DecorateObject : MonoBehaviour
  {
    public enum Decorate
    {
      Window1,
      Window2,
      Window3,
      Box,
      Crack,
      Line_Vertical,
      Lint_Diagonal,
      Vent,
      LeftCount1,
      LeftCount2,
      LeftCount3,
      LeftCount4,
      RightCount1,
      RightCount2,
      RightCount3,
      RightCount4,
      WallHole,
    }

    [System.Serializable]
    public class SpriteSet
    {
      public Decorate decorateType;
      public List<Sprite> sprites = new();

      public SpriteSet(Decorate decorateType)
      {
        this.decorateType = decorateType;
      }
    }

    [SerializeField] private SpriteRenderer spriteRenderer;
    [Space(10)]
    [SerializeField] private Decorate type;
    [SerializeField] private bool isFlipX = false;
    [SerializeField] private bool playRandom;
    [Space(10)]
    [SerializeField] private List<SpriteSet> spriteSets = new();

    private Decorate innerType;
    private bool innerPlayRandom;
    private bool isEnable = true;

    private void OnValidate()
    {
      if (Application.isPlaying)
        return;

      UpdateList();
      spriteRenderer.flipX = isFlipX;

      if (innerType != type || playRandom != innerPlayRandom)
      {
        var targetSet = spriteSets.FirstOrDefault(set => set.decorateType == type);
        if (targetSet.sprites.Count > 0)
        {
          var targetSprite = targetSet.sprites[Random.Range(0, targetSet.sprites.Count)];
          spriteRenderer.sprite = targetSprite;
        }
        innerType = type;
        innerPlayRandom = playRandom;
      }      
    }

    private void UpdateList()
    {
      var decorates = System.Enum.GetValues(typeof(Decorate));
      for(int i = 0; i < decorates.Length; i++)
      {
        var decorate = (Decorate)i;
        if (spriteSets.Count < i + 1)
          spriteSets.Add(new(decorate));
      }
    }

    public void Enable(bool enable)
      => this.isEnable = enable;

    public void Restart()
    {
      isEnable = true;      
    }
  }
}
