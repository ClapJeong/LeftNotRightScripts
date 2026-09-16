using System;
using System.Collections.Generic;
using System.Text;

namespace LR.Manager.Store
{
  public interface IStoreTypeProvider
  {
    public StoreType StoreType { get; }
  }
}
