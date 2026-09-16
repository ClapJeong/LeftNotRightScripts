using System;
using System.Threading;

public class CTSContainer : IDisposable
{
  public CancellationTokenSource cts;
  public CancellationToken token
  {
    get
    {
      isTokenCalled = true;
      return cts.Token;
    }
  }

  private bool isTokenCalled = false;

  public CTSContainer()
  {
    cts = new CancellationTokenSource();
  }

  public void Dispose()
  {
    if (cts != null)
    {
      cts.Cancel();
      cts.Dispose();
      cts = null;
    }
  }

  public void Cancel()
  {
    if (!isTokenCalled)
      return;

    cts?.Cancel();
  }

  public void Create()
  {
    cts?.Dispose();

    cts = new CancellationTokenSource();
    isTokenCalled = false;
  }
}
