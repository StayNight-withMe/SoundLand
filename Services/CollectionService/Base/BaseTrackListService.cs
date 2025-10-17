using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using System.Threading.Tasks;
using test.ViewModel.CollectionClass;

namespace test.Services
{
    public abstract class BaseCollectionService<T> : ICollectionService<T> where T: class
    {

        private ObservableCollection<T> _collection;
        public ObservableCollection<T> Collection { get => _collection; set { _collection = value;} } 

        public BaseCollectionService()
        {
            Collection = new ObservableCollection<T>();
        }
        
        public virtual void Add(T item) => Collection.Add(item);
        public virtual void Remove(T item) => Collection.Remove(item); 
        public virtual void Clear() => Collection.Clear(); 

        
        

    }
}
