using System.Collections.Generic;
using LR.UI;

namespace LR.Manager.UI
{
  public interface IUIPresenterContainer
  {
    public void Add(IUIPresenter presenter);

    public void Remove(IUIPresenter presenter);

    public IReadOnlyList<T> GetAll<T>() where T : IUIPresenter;

    public T GetFirst<T>() where T : IUIPresenter;

    public T GetLast<T>() where T : IUIPresenter;
  }
}