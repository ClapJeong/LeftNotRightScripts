using System;
using UnityEngine;
using UnityEngine.Events;

public interface IEffectService : IDisposable
{
  public void Create(
    InstanceEffectType effectType, 
    Vector3 position,     
    Quaternion rotation,
    UnityAction onComplete = null,
    Transform root = null);

  public void Create(
    InstanceEffectType effectType, 
    Vector3 position, 
    Vector3 euler, 
    UnityAction onComplete = null,
    Transform root = null);
}
