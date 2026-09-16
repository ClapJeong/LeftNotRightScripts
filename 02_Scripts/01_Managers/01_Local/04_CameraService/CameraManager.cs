using Cysharp.Threading.Tasks;
using LR.Table.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Zenject;

namespace LR.Manager.Local.CameraService
{
  public class CameraManager : MonoBehaviour, 
    ICameraValueService,
    ICameraEffectService
  {
    [System.Serializable]
    public class ImpulseSet
    {
      [field: SerializeField] public ICameraEffectService.ImpulseType Type { get; private set; }
      [field: SerializeField] public CinemachineImpulseSource ImpulseSource { get; private set; }
    }
    [System.Serializable]
    public class NoiseSet
    {
      [field: SerializeField] public ICameraEffectService.NoiseType Type { get; private set; }
      [field: SerializeField] public NoiseSettings NoiseSetting { get; private set; }
    }

    [SerializeField] private CinemachineBrain brain;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private List<CinemachineCamera> cameras;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private List<ImpulseSet> impulseSets;
    [SerializeField] private CinemachineImpulseListener impulseListener;
    [SerializeField] private Volume volume;
    [SerializeField] private CinemachineBasicMultiChannelPerlin noisePerlin;
    [SerializeField] private List<NoiseSet> noiseSets;

    private ImpulseData impulseData;
    private RendererVolumeData rendererVolumeData;
    private LaserZoomData laserZoomData;

    private readonly CTSContainer laserCTS = new();
    private readonly CTSContainer chromaticCTS = new();
    private readonly CTSContainer grainCTS = new();
    private readonly CTSContainer noiseCTS = new();
    private Vector3 position = new Vector3(0.0f, 0.0f, -10.0f);
    private Vector3 positionOffset = Vector2.zero;
    private float initializedSize;

    [Inject]
    public void Construct()
    {
      var cameraDataSO = GlobalManager.instance.Table.CameraDataSO;
      this.impulseData = cameraDataSO.ImpulseData;
      this.rendererVolumeData = cameraDataSO.rendererVolumeData;
      this.laserZoomData = cameraDataSO.LaserZoomData;
      if (noisePerlin != null)
        noisePerlin.AmplitudeGain = 0.0f;
    }

    #region ICameraValueService
    public float GetCurrentOrthographizSize()
      => mainCamera.orthographicSize;

    public float GetInitializedOrthographizSize()
      => initializedSize;

    public Vector2 GetScreenPosition(Vector3 worldPosition)
      => mainCamera.WorldToScreenPoint(worldPosition);

    public Vector2 WorldToViewportPoint(Vector3 worldPosition)
      => mainCamera.WorldToViewportPoint(worldPosition);

    public void SetEuler(Vector3 euler)
      => (brain.ActiveVirtualCamera as CinemachineCamera).transform.eulerAngles = euler;

    public void UpdateOffset(Vector3 offset)
    {
      this.positionOffset = offset;
      (brain.ActiveVirtualCamera as CinemachineCamera).transform.position = position + positionOffset;
    }

    public void SetPosition(Vector3 position)
    {
      this.position = position;
      (brain.ActiveVirtualCamera as CinemachineCamera).transform.position = position + positionOffset;
    }

    public Vector3 GetPosition()
      => position;


    public void SetSize(float size, bool isInitialize = false)
    {
      if (isInitialize)
        initializedSize = size;

      mainCamera.orthographicSize = size;
      foreach (var camera in cameras)
        camera.Lens.OrthographicSize = size;
    }
    #endregion

    #region ICameraEffectService
    public void LaserZoom()
    {
      laserCTS.Cancel();
      laserCTS.Create();
      var token = laserCTS.token;
      LaserZoomAsync(token).Forget();
    }

    private async UniTask LaserZoomAsync(CancellationToken token)
    {
      try
      {
        var data = laserZoomData;
        var originSize = GetInitializedOrthographizSize();
        var targetSize = originSize * data.ZoomOutValue;
        var duration = 0.0f;
        var zoomOutDuration = data.ZoomOutDuration;
        var zoomInDuration = data.RevertDuration;

        while (duration < zoomOutDuration)
        {
          token.ThrowIfCancellationRequested();

          duration += Time.deltaTime;
          var size = Mathf.Lerp(originSize, targetSize, duration / zoomOutDuration);
          SetSize(size);
          await UniTask.Yield();
        }

        duration = 0.0f;
        while (duration < zoomInDuration)
        {
          token.ThrowIfCancellationRequested();

          duration += Time.deltaTime;
          var size = Mathf.Lerp(targetSize, originSize, duration / zoomInDuration);          
          SetSize(size);
          await UniTask.Yield();
        }

        SetSize(originSize);
      }
      catch (OperationCanceledException)
      {
        if (this != null)
          SetSize(GetInitializedOrthographizSize());          
      }
    }

    public void GenerateImpulse(ICameraEffectService.ImpulseType type, float value = 1.0f)
    {
      var direction = type switch
      {
        ICameraEffectService.ImpulseType.LeftWallBump => new Vector2(-impulseData.WallBumpHorizontalValue, UnityEngine.Random.Range(-impulseData.WallBumpVerticalRange, impulseData.WallBumpVerticalRange)),
        ICameraEffectService.ImpulseType.LeftDamaged => new Vector2(-impulseData.HitHorizontalValue, UnityEngine.Random.Range(-impulseData.HitVerticalRange, impulseData.HitVerticalRange)),
        ICameraEffectService.ImpulseType.RightWallBump => new Vector2(impulseData.WallBumpHorizontalValue, UnityEngine.Random.Range(-impulseData.WallBumpVerticalRange, impulseData.WallBumpVerticalRange)),
        ICameraEffectService.ImpulseType.RightDamaged => new Vector2(impulseData.HitHorizontalValue, UnityEngine.Random.Range(-impulseData.HitVerticalRange, impulseData.HitVerticalRange)),
        ICameraEffectService.ImpulseType.GimmickStun => Vector2.one * impulseData.GimmickStunForce,
        ICameraEffectService.ImpulseType.ExitOpen => Vector2.right * impulseData.ExitOpenForce,
        _ => throw new NotImplementedException(),
      };
      impulseSets.FirstOrDefault(set => set.Type == type).ImpulseSource.GenerateImpulse(GetCurrentOrthographizSize() * value * direction);
    }

    public void ResetImpulse()
    {
      impulseListener.enabled = false;
      CinemachineImpulseManager.Instance.Clear();
      impulseListener.enabled = true;
    }

    public async UniTask PlayGimmickStunChromaticAsync()
    {
      chromaticCTS.Cancel();
      chromaticCTS.Create();
      var token = chromaticCTS.token;

      if (volume.profile.TryGet<ChromaticAberration>(out var chromatic))
      {
        try
        {
          chromatic.intensity.value = rendererVolumeData.GimmickStunChromaticMaxIntensity;
          var currentDuration = rendererVolumeData.GimmickStunChromaticDuration;
          while (currentDuration > 0.0f)
          {
            token.ThrowIfCancellationRequested();

            chromatic.intensity.value = (currentDuration / rendererVolumeData.GimmickStunChromaticDuration) * rendererVolumeData.GimmickStunChromaticMaxIntensity;
            currentDuration -= Time.deltaTime;
            await UniTask.Yield();
          }
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
          chromatic.intensity.value = 0.0f;
        }
      }
    }

    public async UniTask PlaySwapChromaticAsync()
    {
      chromaticCTS.Cancel();
      chromaticCTS.Create();
      var token = chromaticCTS.token;
      if (volume.profile.TryGet<ChromaticAberration>(out var chromatic))
      {
        try
        {
          var duration = rendererVolumeData.SwapChromaticDuration;
          while (duration > 0.0f)
          {
            token.ThrowIfCancellationRequested();

            chromatic.intensity.value = 1.0f - (duration / rendererVolumeData.SwapChromaticDuration);
            duration -= Time.deltaTime;
            await UniTask.Yield();
          }

          duration = rendererVolumeData.SwapChromaticDuration;
          while (duration > 0.0f)
          {
            token.ThrowIfCancellationRequested();

            chromatic.intensity.value = (duration / rendererVolumeData.SwapChromaticDuration);
            duration -= Time.deltaTime;
            await UniTask.Yield();
          }

          chromatic.intensity.value = 0.0f;
        }
        catch (OperationCanceledException)
        {
          chromatic.intensity.value = 0.0f;
        }
      }
    }

    public async UniTask PlayGrainAsync()
    {
      grainCTS.Cancel();
      grainCTS.Create();
      var token = grainCTS.token;
      if (volume.profile.TryGet<FilmGrain>(out var grain))
      {
        try
        {
          var duration = rendererVolumeData.GrainPlayduration;
          while (duration > 0.0f)
          {
            token.ThrowIfCancellationRequested();

            grain.intensity.value = Mathf.Lerp(0.0f, rendererVolumeData.GrainMaxValue, 1.0f - (duration / rendererVolumeData.GrainPlayduration));
            duration -= Time.deltaTime;
            await UniTask.Yield();
          }

          grain.intensity.value = rendererVolumeData.GrainMaxValue;
        }
        catch (OperationCanceledException)
        {
          grain.intensity.value = rendererVolumeData.GrainMaxValue;
        }
      }
    }

    public void StopGrain()
    {
      grainCTS.Cancel();
      if (volume.profile.TryGet<FilmGrain>(out var grain))
        grain.intensity.value = 0.0f;
    }

    public void PlayNoise(ICameraEffectService.NoiseType type)
    {
      noisePerlin.NoiseProfile = noiseSets.FirstOrDefault(set => set.Type == type).NoiseSetting;

      noiseCTS.Cancel();
      noiseCTS.Create();
      var token = noiseCTS.token;
      ApplyExhaustNoiseAsync(1.0f, token).Forget();
    }

    public void StopNoise()
    {
      noiseCTS.Cancel();
      noiseCTS.Create();
      var token = noiseCTS.token;
      ApplyExhaustNoiseAsync(0.0f, token).Forget();
    }

    private async UniTask ApplyExhaustNoiseAsync(float value, CancellationToken token)
    {
      var beginValue = noisePerlin.AmplitudeGain;
      var duration = 0.0f;
      try
      {
        while(duration < impulseData.ExhaustNoiseChangeDuration)
        {
          token.ThrowIfCancellationRequested();

          var t = duration / impulseData.ExhaustNoiseChangeDuration;
          noisePerlin.AmplitudeGain = Mathf.Lerp(beginValue, value, t);

          duration += Time.deltaTime;
          await UniTask.Yield();
        }
        noisePerlin.AmplitudeGain = value;
      }
      catch (OperationCanceledException) { }
    }
    #endregion

    private void OnDestroy()
    {
      laserCTS.Dispose();
      noiseCTS.Dispose();
      grainCTS.Dispose();
      chromaticCTS.Dispose();
    }

    private void OnDisable()
    {
      if (mainCamera != null)
        mainCamera.targetTexture = null;
    }
  }
}