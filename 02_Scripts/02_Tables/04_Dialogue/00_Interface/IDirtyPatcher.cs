using UnityEngine.Events;

namespace LR.Table.Dialogue
{
  internal interface IDirtyPatcher
  {
    public void SetOnDirty(UnityAction onDirty);
  }
}
