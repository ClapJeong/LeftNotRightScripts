using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using LR.Manager.Sound;
using LR.Stage.InteractiveObject;
using LR.Manager.Stage;
using LR.Stage.InteractiveObject.AutoMover;

#if UNITY_EDITOR
using UnityEditor.Events;
using UnityEditor;
#endif

namespace LR.Stage.SignalListener
{
  public class SignalListener : BaseInteractiveObject
  {
    public enum Direction
    {
      Horizontal,
      Vertical,
    }
    [SerializeField] private ColorSO colorSO;
    [field: Header("[ Key ]")]
    [field: SerializeField] public int RequireKey { get; private set; } = -1;

    [Header("[ Events ]")]
    [SerializeField] private UnityEvent<bool> onActivate = new();
    [SerializeField] private UnityEvent<bool> onDeactivate = new();

    [Header("[ Preview ]")]
    [SerializeField] private Direction direction = Direction.Horizontal;
    [SerializeField] private SpriteRenderer deactivatedSpriteRenderer;
    [SerializeField] private SpriteRenderer acSpriteRenderer;
    [SerializeField] private SpriteRenderer acdcSpriteRenderer;
    [SerializeField] private Transform previewSpriteMask;
    [SerializeField] private int countPreview = 3;
    [SerializeField] private Vector3 previewPosition = new (0.0f, 0.0f, 0.0f);
    [SerializeField] private float previewSpace = 0.15f;
    [SerializeField] private float prevSize = 0.4f;
    [SerializeField] private float PreviewBackgroundSpace = 0.1f;
    [SerializeField] private FloatingModule.Model floatingModel;

    private FloatingModule floatinModule;
    private SpriteRenderer targetSpriterRenderer;
    private MaterialPropertyBlock outlineBlock;
    private ISFXController sfxController;
    private bool isActivated = false;

    private bool IsKeyExist
      => RequireKey >= 0;

    [ContextMenu("[ Add Open+Close ]")]
    public void AddOpenClose()
    {
      if(TryGetDoor(out var autoDoor))
      {
#if UNITY_EDITOR
        Undo.RecordObject(this, "Add Door Events");

        UnityEventTools.AddPersistentListener(onActivate, autoDoor.Open);
        UnityEventTools.AddVoidPersistentListener(onDeactivate, autoDoor.Close);

        EditorUtility.SetDirty(this);
#endif
      }
    }

    [ContextMenu("[ Add Close+Open ]")]
    public void AddCloseOpen()
    {
      if (TryGetDoor(out var autoDoor))
      {
#if UNITY_EDITOR
        Undo.RecordObject(this, "Add Door Events");

        UnityEventTools.AddVoidPersistentListener(onActivate, autoDoor.Close);
        UnityEventTools.AddPersistentListener(onDeactivate, autoDoor.Open);

        EditorUtility.SetDirty(this);
#endif
      }
    }

    private bool TryGetDoor(out AutoDoor autoDoor)
    {
      autoDoor = gameObject.GetComponentInParent<AutoDoor>();
      return autoDoor != null;
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
      if (PrefabUtility.IsPartOfPrefabAsset(this))
        return;
#endif

      var size = GetPreviewSize(countPreview);
      UpdateSpriteRenderer(deactivatedSpriteRenderer, previewPosition, size);
      UpdateSpriteRenderer(acSpriteRenderer, previewPosition, size);
      UpdateSpriteRenderer(acdcSpriteRenderer, previewPosition, size);
      if (previewSpriteMask != null)
      {
        previewSpriteMask.localPosition = previewPosition;
        previewSpriteMask.localScale = size;
      }
    }

    private void UpdateSpriteRenderer(SpriteRenderer spriteRenderer, Vector3 position, Vector3 size)
    {
      if (spriteRenderer == null)
        return;

      spriteRenderer.transform.localPosition = position;
      spriteRenderer.size = size;
    }

    private void Update()
    {
      if (isActivated)
        return;

      floatinModule?.OnUpdate();
    }

    public void Initialize(
      int idCount, 
      ISFXController sfxController,
      bool isContainACDC)
    {
      floatinModule = new(transform, floatingModel);

      this.sfxController = sfxController;
      var previewSize = GetPreviewSize(idCount);
      deactivatedSpriteRenderer.size = previewSize;
      acSpriteRenderer.size = previewSize;
      acdcSpriteRenderer.size = previewSize;
      previewSpriteMask.localScale = previewSize;

      var color = colorSO.SignalColors[RequireKey];
      var colorBlock = new MaterialPropertyBlock();
      acSpriteRenderer.GetPropertyBlock(colorBlock);
      colorBlock.SetColor(ShaderHash.SignalPlatform._OutlineColor, color);
      acSpriteRenderer.SetPropertyBlock(colorBlock);
      acdcSpriteRenderer.GetPropertyBlock(colorBlock);
      colorBlock.SetColor(ShaderHash.SignalPlatform._OutlineColor, color);
      acdcSpriteRenderer.SetPropertyBlock(colorBlock);
      deactivatedSpriteRenderer.color = color;

      outlineBlock = new();
      targetSpriterRenderer = isContainACDC ? acdcSpriteRenderer 
                                            : acSpriteRenderer;
      targetSpriterRenderer.GetPropertyBlock(outlineBlock);
    }

    public override void Enable(bool isEnable)
    {

    }

    public void OnActivate(bool isACDC)
    {
      isActivated = true;
      sfxController.PlayOnce(AudioSourceType.Center, SFX.SignalListener);
      onActivate.Invoke(isACDC);

      deactivatedSpriteRenderer.enabled = false;
      SetOutline(true);
    }
    
    public void OnDeactivate(bool isACDC)
    {
      isActivated = false;
      onDeactivate.Invoke(isACDC);
      SetOutline(false);
      deactivatedSpriteRenderer.enabled = true;
    }

    public override void Restart()
    {
      isActivated = false;
      deactivatedSpriteRenderer.enabled = true;
      SetOutline(false);
    }

    public List<Vector3> GetPreviewPositions(int count)
    {
      if (count == 0)
        return new List<Vector3>();

      var lists = new List<Vector3>();
      var totalLength = prevSize * count + previewSpace * (count - 1);
      var currentLength = 0.0f;
      for (int i = 0; i < count; i++)
      {
        var sizeHalf = prevSize * 0.5f;
        currentLength += sizeHalf;
        lists.Add(direction switch
        {
          Direction.Horizontal => transform.TransformPoint(previewPosition + new Vector3(currentLength - totalLength * 0.5f, 0.0f, 0.0f)),
          Direction.Vertical => transform.TransformPoint(previewPosition + new Vector3(0.0f, currentLength - totalLength * 0.5f, 0.0f)),
          _ => throw new NotImplementedException(),
        });
        currentLength += sizeHalf;
        if (i < count - 1)
          currentLength += previewSpace;
      }

      return lists;
    }

    private Vector3 GetPreviewSize(int count)
      => direction switch
      {
        Direction.Horizontal => new(
          prevSize * count + previewSpace * (Mathf.Max(0, count - 1)) + PreviewBackgroundSpace * 2.0f
             , prevSize + PreviewBackgroundSpace * 2.0f
          ,1.0f),

        Direction.Vertical => new(
          prevSize + PreviewBackgroundSpace * 2.0f          
             , prevSize * count + previewSpace * (Mathf.Max(0, count - 1)) + PreviewBackgroundSpace * 2.0f
          ,1.0f),

        _ => throw new NotImplementedException(),
      };

    private void SetOutline(bool isEnable)
    {
      outlineBlock.SetFloat(ShaderHash.SignalPlatform._Enabled, isEnable ? 1.0f : 0.0f);
      targetSpriterRenderer.SetPropertyBlock(outlineBlock);
    }

    public override void Initialize(StageManager stageManager, ISFXController sfxController)
    {
      
    }


#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
      base.OnDrawGizmos();

      if (IsKeyExist && colorSO != null)
      {
        var labelCenterStyle = new GUIStyle(EditorStyles.label)
        {
          alignment = TextAnchor.MiddleCenter
        };
        labelCenterStyle.normal.textColor = colorSO.SignalColors[RequireKey];
        Handles.Label(transform.position + Vector3.up, "[ " + RequireKey + " ]", labelCenterStyle);

        if (countPreview > 0)
        {
          Gizmos.color = colorSO.SignalColors[RequireKey];
          var positions = GetPreviewPositions(countPreview);
          foreach(var position in positions)
          {
            var sizeHalf = prevSize * 0.5f;
            var leftUp = position + new Vector3(-sizeHalf, sizeHalf);
            var leftDown = position + new Vector3(-sizeHalf, -sizeHalf);
            var rightUp = position + new Vector3(sizeHalf, sizeHalf);
            var rightDown = position + new Vector3(sizeHalf, -sizeHalf);

            Gizmos.DrawLine(leftUp, rightUp);
            Gizmos.DrawLine(leftUp, leftDown);
            Gizmos.DrawLine(rightDown, rightUp);
            Gizmos.DrawLine(rightDown, leftDown);
          }
        }        
      }
    }
#endif
  }
}
