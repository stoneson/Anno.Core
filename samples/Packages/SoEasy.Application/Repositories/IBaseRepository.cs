
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoEasy.Application.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        T GetById(int id);
#if NET40
        bool SaveAsync(T t);
#else
        Task<bool> SaveAsync(T t);
#endif
    }
}
