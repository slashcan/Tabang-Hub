using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tabang_Hub.Contract;
using Tabang_Hub.Utils;

namespace Tabang_Hub.Repository
{
    public class BaseRepository<T> : IBaseRepository<T>
    {
        public ErrorCode Create(T t, out string errorMsg)
        {
            throw new NotImplementedException();
        }

        public ErrorCode Delete(object id, out string errorMsg)
        {
            throw new NotImplementedException();
        }

        public T Get(object id)
        {
            throw new NotImplementedException();
        }

        public List<T> GetAll()
        {
            throw new NotImplementedException();
        }

        public ErrorCode Update(object id, T t, out string errorMsg)
        {
            throw new NotImplementedException();
        }
    }
}