using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test.Services
{
    public interface ICollectionService
    {
        void Clear();
    }

    public interface ICollectionService<T> : ICollectionService
    {
    
        void Add(T item);

        void Remove(T item);


       
    
    }

}
