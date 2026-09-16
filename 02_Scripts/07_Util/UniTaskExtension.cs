using Cysharp.Threading.Tasks;

public static class UniTaskExtension
{
  public static async UniTask Chain(this UniTask task, UniTask chain)
  {
    await task;
    await chain;
  }
}