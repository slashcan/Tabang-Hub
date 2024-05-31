using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tabang_Hub.Contracts
{
    public enum ErrorCode
    { 
        Success,
        Error
    }
    public interface IBaseRepository<T>
    {
        T Get(Object id);
        List<T> GetAll();
        ErrorCode Create(T t);
        ErrorCode Update(Object id, T t);
        ErrorCode Delete(Object id);
    }
}
