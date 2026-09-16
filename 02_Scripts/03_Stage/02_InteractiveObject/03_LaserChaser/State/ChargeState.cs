using LR.Manager.Sound;
using UnityEngine;
using UnityEngine.Events;

namespace LR.Stage.InteractiveObject.LaserChaserState
{
  public class ChargeState : IState
  {
    private readonly Transform root;
    private readonly Transform lineRoot;
    private readonly Transform alignUp;
    private readonly Transform alignDown;
    private readonly LaserChaser.Model model;
    private readonly ISFXController sfxController;

    private float duration;
    private AudioLoopHandle audioLoopHandle;

    public ChargeState(
      Transform root,
      Transform lineRoot,
      Transform alignUp, 
      Transform alignDown, 
      LaserChaser.Model model,
      ISFXController sfxController)
    {
      this.root = root;
      this.lineRoot = lineRoot;
      this.alignUp = alignUp;
      this.alignDown = alignDown;
      this.model = model;
      this.sfxController = sfxController;

      duration = model.ChargeDuration;
    }

    public void OnEnter()
    {
      audioLoopHandle = sfxController.CreateLoopSource(CharacterPositionType.Center, SFX.LaserCharge, model.ChargeingSFXPitchBegin);
      duration = model.ChargeDuration;      
      lineRoot.position = GetEdgePoint(root, Camera.main);
      alignUp.localEulerAngles = Vector3.forward * model.AlignAngle;
      alignDown.localEulerAngles = Vector3.back * model.AlignAngle;
      alignUp.gameObject.SetActive(true);
      alignDown.gameObject.SetActive(true);
    }

    public void OnExit()
    {
      audioLoopHandle?.Stop();
      audioLoopHandle = null;
      alignUp.gameObject.SetActive(false);
      alignDown.gameObject.SetActive(false);
    }

    public void OnUpdate(UnityAction onComplete)
    {
      duration = Mathf.Max(0.0f, duration -= Time.deltaTime);
      var t = 1.0f - duration / model.ChargeDuration;
      var angle = Mathf.Lerp(model.AlignAngle, 0.0f, model.ChargeAlignCurve.Evaluate(t));
      alignUp.localEulerAngles = Vector3.forward * angle;
      alignDown.localEulerAngles = Vector3.back * angle;
      if(audioLoopHandle != null)
      {
        if(!audioLoopHandle.IsPlaying)
          audioLoopHandle.Play();
        audioLoopHandle.Pitch = Mathf.Lerp(model.ChargeingSFXPitchBegin, 1.0f, t);
      }        

      if (duration <= 0.0f)
        onComplete?.Invoke();
    }

    public void OnPuase()
    {
      audioLoopHandle?.Pause();
    }

    public static Vector2 GetEdgePoint(Transform origin, Camera cam, float screenScale = 1.1f)
    {
      Vector2 pos = origin.position;
      Vector2 dir = -(Vector2)origin.up;

      float halfHeight = cam.orthographicSize * screenScale;
      float halfWidth = halfHeight * cam.aspect;

      Vector2 camPos = cam.transform.position;

      float left = camPos.x - halfWidth;
      float right = camPos.x + halfWidth;
      float bottom = camPos.y - halfHeight;
      float top = camPos.y + halfHeight;

      float tMin = float.PositiveInfinity;

      if (dir.x != 0)
      {
        float t1 = (left - pos.x) / dir.x;
        float y1 = pos.y + dir.y * t1;
        if (t1 > 0 && y1 >= bottom && y1 <= top) tMin = Mathf.Min(tMin, t1);

        float t2 = (right - pos.x) / dir.x;
        float y2 = pos.y + dir.y * t2;
        if (t2 > 0 && y2 >= bottom && y2 <= top) tMin = Mathf.Min(tMin, t2);
      }

      if (dir.y != 0)
      {
        float t3 = (bottom - pos.y) / dir.y;
        float x3 = pos.x + dir.x * t3;
        if (t3 > 0 && x3 >= left && x3 <= right) tMin = Mathf.Min(tMin, t3);

        float t4 = (top - pos.y) / dir.y;
        float x4 = pos.x + dir.x * t4;
        if (t4 > 0 && x4 >= left && x4 <= right) tMin = Mathf.Min(tMin, t4);
      }

      return pos + dir * tMin;
    }

    public void UpdateTarget(Transform target)
    {
      
    }
  }
}
