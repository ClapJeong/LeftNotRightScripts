using LR.Manager.Sound;
using LR.Manager.Stage;
using UnityEngine;

namespace LR.Stage.InteractiveObject
{
  public class SteamPiper : BaseInteractiveObject
  {
    [SerializeField] private ParticleSystem steamParticle;

    [Space(5)]
    [SerializeField] private float minDelay;
    [SerializeField] private float maxDelay;

    [Space(5)]
    [SerializeField] private float eulerRange;
    [SerializeField] private float minDuration;
    [SerializeField] private float maxDuration;

    private float currentDelay;
    private float targetDelay;

    private bool isEnable;

    public override void Enable(bool isEnable)
      => this.isEnable = isEnable;

    public override void Initialize(StageManager stageManager, ISFXController sfxController)
    {
      var chapter = stageManager.StageIndex / StageConst.StageUnit;
      var mainModule = steamParticle.main;
      mainModule.duration = Mathf.Lerp(minDuration, maxDuration, 1.0f - (12 - chapter) / 3.0f);

      currentDelay = 0.0f;
      SetRandomDelay();
      SetRandomEuler();
    }

    public override void Restart()
    {
      
    }

    private void SetRandomDelay()
      => targetDelay = Random.Range(minDelay, maxDelay);

    private void SetRandomEuler()
      => steamParticle.transform.eulerAngles = new Vector3(0.0f, 0.0f, Random.Range(-eulerRange, eulerRange));

    private void Update()
    {
      if (!isEnable)
        return;

      if(currentDelay < targetDelay)
      {
        currentDelay += Time.deltaTime;
      }
      else
      {
        currentDelay = 0.0f;
        steamParticle.Play();
        SetRandomDelay();
        SetRandomEuler();
      }
    }
  }
}
