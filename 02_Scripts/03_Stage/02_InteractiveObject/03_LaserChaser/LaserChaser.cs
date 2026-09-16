using LR.Manager.Sound;
using LR.Manager.Stage;
using LR.Stage.InteractiveObject.LaserChaserState;
using LR.Stage.Player.Enum;
using System.Collections.Generic;
using UnityEngine;

namespace LR.Stage.InteractiveObject
{
  public class LaserChaser : BaseInteractiveObject
  {
    private enum State
    {
      Chase,
      Charge,
      Shoot,
      Regen,
    }

    [System.Serializable]
    public class Model
    {
      [field: SerializeField] public ColorSO ColorSO { get; private set; }
      [field: Header("[ Chase ]")]
      [field: SerializeField] public float ChaseDuration { get; private set; }
      [field: SerializeField] public float MaxRotateValue { get; private set; }
      [field: SerializeField] public float ChaseSmooth {  get; private set; }
      [field: SerializeField] public float SFXIntervalMin { get; private set; }
      [field: SerializeField] public float SFXIntervalMax { get; private set; }
      [field: SerializeField] public float LineSpeedMin { get; private set; }
      [field: SerializeField] public float LineSpeedMax { get; private set; }
      [field: SerializeField] public float ChaseGlowIntensityMax { get; private set; }
      [field: SerializeField] public float ChaseGlowIntensityMin { get; private set; }

      [field: Header("[ Charge ]")]
      [field: SerializeField] public float ChargeDuration { get; private set;  }
      [field: SerializeField] public AnimationCurve ChargeAlignCurve { get; private set; }
      [field: SerializeField] public float AlignAngle { get; private set; }
      [field: SerializeField] public float ChargeingSFXPitchBegin {  get; private set; }

      [field: Header("[ Shoot ]")]
      [field: SerializeField] public float ShootDuration {  get; private set; }      
      [field: SerializeField] public float ShootWidth { get; private set; }
      [field: SerializeField] public float ShootDamage {  get; private set; }
      [field: SerializeField] public float ShootGlowIntensity { get; private set; }

      [field: Header("[ Regen ]")]
      [field: SerializeField] public float RegenDuration { get; private set; }
    }
    [SerializeField] Model model;

    [Space(10)]
    [SerializeField] private PlayerType targetPlayer;
    [SerializeField] private bool bothTarget = false;

    [Space(10)]
    [SerializeField] private Animator effectAnimator;
    [SerializeField] private GameObject chaseLine;
    [SerializeField] private SpriteRenderer chaseSpriteRenderer;
    [SerializeField] private Transform lineRoot;
    [SerializeField] private Transform alignUp;
    [SerializeField] private Transform alignDown;
    [SerializeField] private Transform laser;
    [SerializeField] private SpriteRenderer fillSpriteRenderer;

    private readonly Dictionary<State, IState> states = new();
    private StageEnum.State prevStageState;
    private State currentState;    
    private IStageStateProvider stageStateProvider;
    private IPlayerGetter playerGetter;
    private Quaternion initializedQuaternion;
    private bool isEnable;
    private MaterialPropertyBlock matBlock;

    private void OnValidate()
    {
      UpdateColor();
    }

    private void UpdateColor()
    {
      if (fillSpriteRenderer != null && model.ColorSO != null)
      {
        fillSpriteRenderer.color = model.ColorSO.GetPlayerColor(targetPlayer);
      }
    }

    private void Update()
    {
      if (!isEnable || stageStateProvider == null)
        return;

      var currentStageState = stageStateProvider.GetState();

      if (prevStageState == StageEnum.State.Playing && currentStageState != StageEnum.State.Playing)
        states[currentState].OnPuase();

      prevStageState = currentStageState;
      if (!stageStateProvider.IsPlayingState)
        return;

      states[currentState].OnUpdate(NextState);
    }

    private void NextState()
    {
      switch (currentState)
      {
        case State.Chase: SetState(State.Charge); break;
        case State.Charge: SetState(State.Shoot); break;
        case State.Shoot: SetState(State.Regen); break;
        case State.Regen: SetState(State.Chase); break;
      }

      var hash = currentState switch
      {
        State.Chase => AnimatorHash.LaserEffect.Idle,
        State.Charge => AnimatorHash.LaserEffect.Charge,
        State.Shoot => AnimatorHash.LaserEffect.Shoot,
        State.Regen => AnimatorHash.LaserEffect.Idle,
        _ => throw new System.NotImplementedException(),
      };
      effectAnimator.Play(hash);

      if(currentState == State.Regen && bothTarget)
      {
        targetPlayer = targetPlayer.ParseOpposite();
        var nextTarget = playerGetter.GetPlayer(targetPlayer).GetTransform();
        foreach (var state in states.Values)
          state.UpdateTarget(nextTarget);

        UpdateColor();
      }      
    }

    private void SetState(State state)
    {
      states[currentState]?.OnExit();
      currentState = state;
      states[currentState]?.OnEnter();
    }  

    public override void Initialize(StageManager stageManager, ISFXController sfxController)
    {
      this.stageStateProvider = stageManager;
      playerGetter = stageManager;
      var chaseTarget = playerGetter.GetPlayer(targetPlayer).GetTransform();
      initializedQuaternion = transform.rotation;

      matBlock = new();
      fillSpriteRenderer.GetPropertyBlock(matBlock);
      matBlock.SetFloat(ShaderHash.Laser._Fill, 0.0f);
      matBlock.SetFloat(ShaderHash.Laser._Clockwise, targetPlayer == PlayerType.Left ? 1.0f : -1.0f);
      fillSpriteRenderer.SetPropertyBlock(matBlock);

      states[State.Chase] = new ChaseState(
        transform,
        chaseTarget,
        chaseLine,
        model,
        sfxController,
        chaseSpriteRenderer,
        UpdateFill,
        UpdateIntensity);
      states[State.Charge] = new ChargeState(
        transform,
        lineRoot,
        alignUp,
        alignDown,
        model,
        sfxController);
      states[State.Shoot] = new ShootState(
        transform,
        laser,
        stageManager,
        LocalManager.instance.CameraManager,
        model,
        sfxController,
        UpdateIntensity);
      states[State.Regen] = new RegenState(
        fillSpriteRenderer,
        UpdateFill,
        model);

      ResetCanon();
    }

    public override void Enable(bool isEnable)
    {
      this.isEnable = isEnable;

      if(!isEnable)
        states[currentState].OnPuase();
    }

    public override void Restart()
    {
      ResetCanon();
    }

    private void ResetCanon()
    {
      SetState(State.Chase);
      transform.rotation = initializedQuaternion;
      chaseLine.SetActive(true);
      alignUp.gameObject.SetActive(false);
      alignDown.gameObject.SetActive(false);
      UpdateFill(0.0f);
      UpdateIntensity(model.ChaseGlowIntensityMin);
      effectAnimator.Play(AnimatorHash.LaserEffect.Idle);
    }

    private void UpdateFill(float fillAmount)
    {
      matBlock.SetFloat(ShaderHash.Laser._Fill, fillAmount);
      fillSpriteRenderer.SetPropertyBlock(matBlock);
    }

    private void UpdateIntensity(float intesity)
    {
      matBlock.SetFloat(ShaderHash.Laser._Intensity, intesity);
      fillSpriteRenderer.SetPropertyBlock(matBlock);
    }
  }
}
