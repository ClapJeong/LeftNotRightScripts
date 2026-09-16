using UnityEngine;

public static class ShaderHash
{
  public static class Indicator
  {
    public static readonly int _ScrollSpeed = Shader.PropertyToID(nameof(_ScrollSpeed));
  }

  public static class DialogueReplayButton
  {
    public static readonly int _ScrollSpeed = Shader.PropertyToID(nameof(_ScrollSpeed));
  }

  public static class WallHitPaint
  {
    public static readonly int _Color = Shader.PropertyToID(nameof(_Color));
  }

  public static class PlayerPortrait
  {
    public static readonly int _Alpha = Shader.PropertyToID(nameof(_Alpha));
    public static readonly int _ScanStrength = Shader.PropertyToID(nameof(_ScanStrength));
    public static readonly int _Flicker = Shader.PropertyToID(nameof(_Flicker));
    public static readonly int _RGBSplit = Shader.PropertyToID(nameof(_RGBSplit));
    public static readonly int _GlitchStrength = Shader.PropertyToID(nameof(_GlitchStrength));
    public static readonly int _GlitchSpeed = Shader.PropertyToID(nameof(_GlitchSpeed));
    public static readonly int _EdgeNoiseStrength = Shader.PropertyToID(nameof(_EdgeNoiseStrength));
    public static readonly int _EdgeNoiseWidth = Shader.PropertyToID(nameof(_EdgeNoiseWidth));
  }

  public static class LobbyBackground
  {
    public static readonly int _Offset = Shader.PropertyToID(nameof(_Offset));
  }

  public static class InputRequireGuide
  {
    public static readonly int _Clockwise = Shader.PropertyToID(nameof(_Clockwise));
    public static readonly int _FillAmount = Shader.PropertyToID(nameof(_FillAmount));
  }

  public static class Laser
  {
    public static readonly int _Fill = Shader.PropertyToID(nameof(_Fill));
    public static readonly int _Clockwise = Shader.PropertyToID(nameof(_Clockwise));
    public static readonly int _Intensity = Shader.PropertyToID(nameof(_Intensity));

    public static readonly int _Offset = Shader.PropertyToID(nameof(_Offset));
  }

  public static class InputRequireFill
  {
    public static readonly int _Direction = Shader.PropertyToID(nameof(_Direction));
    public static readonly int _ArrowSpeed = Shader.PropertyToID(nameof(_ArrowSpeed));
    public static readonly int _ArrowRotation = Shader.PropertyToID(nameof(_ArrowRotation));
  }

  public static class EnergyBar
  {
    public static readonly int _OutlineEnable = Shader.PropertyToID(nameof(_OutlineEnable));
    public static readonly int _Fill = Shader.PropertyToID(nameof(_Fill));
    public static readonly int _RectSize = Shader.PropertyToID(nameof(_RectSize));
    public static readonly int _ArrowScale = Shader.PropertyToID(nameof(_ArrowScale));
  }

  public static class Loading
  {
    public static readonly int _Scale = Shader.PropertyToID(nameof(_Scale));
    public static readonly int _ScrollDir = Shader.PropertyToID(nameof(_ScrollDir));
  }

  public static class CCTV
  {
    public static readonly int _ScanWidth = Shader.PropertyToID(nameof(_ScanWidth));
    public static readonly int _ScanAxis = Shader.PropertyToID(nameof(_ScanAxis));
    public static readonly int _ScanProgressX = Shader.PropertyToID(nameof(_ScanProgressX));
    public static readonly int _ScanProgressY = Shader.PropertyToID(nameof(_ScanProgressY));
  }

  public static class SignalPlatform
  {
    public static readonly int _OutlineColor = Shader.PropertyToID(nameof(_OutlineColor));
    public static readonly int _Enabled = Shader.PropertyToID(nameof(_Enabled));
  }

  public static class Glow
  {
    public static readonly int _GlowColor = Shader.PropertyToID(nameof(_GlowColor));
    public static readonly int _Intensity = Shader.PropertyToID(nameof(_Intensity));
  }

  public static class Player
  {
    public static readonly int _Flash = Shader.PropertyToID(nameof(_Flash));
    public static readonly int _Shear = Shader.PropertyToID(nameof(_Shear));
  }
}
