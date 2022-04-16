using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoEasy.Application.Repositories
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        public T GetById(int id)
        {
            return default(T);
        }
#if NET40
        public bool SaveAsync(T t)
        {
            return true;
        }
#else
        public async Task<bool> SaveAsync(T t)
        {
            return await Task.FromResult(true);
        }
#endif
    }
}
