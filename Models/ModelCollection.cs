using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.Models
{
    [Serializable]
    public class ModelCollection<T> : IEnumerable<ModelProperty<T>>, IModelProperty
    {


        private List<ModelProperty<T>> modelList = new List<ModelProperty<T>>();
        public List<ModelProperty<T>> ModelList
        {
            set
            {
                modelList = value;
                ModelChanged?.Invoke();
            }

            get => modelList;
        }

        public ModelCollection()
        {

        }

        public void Add(T model)
        {
            modelList.Add(new ModelProperty<T>(model));
            CollectionChanged?.Invoke();
        }

        public void Remove(ModelProperty<T> modelProperty)
        {
            modelList.Remove(modelProperty);
            CollectionChanged?.Invoke();
        }
        public void RemoveAt(int index)
        {
            modelList.RemoveAt(index);
            CollectionChanged?.Invoke();
        }

        public T this[int i]
        {
            get => modelList[i].Value;
        }

        public object GetValue()
        {
            return modelList;
        }

        public void Unbind(ModelChangeCallback callback)
        {
            ModelChanged -= callback;
        }

        IEnumerator<ModelProperty<T>> IEnumerable<ModelProperty<T>>.GetEnumerator()
        {
            foreach (ModelProperty<T> modelProperty in ModelList) yield return modelProperty;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (ModelProperty<T> modelProperty in ModelList) yield return modelProperty;
        }

        public void SubscribeModelChanged(ModelChangeCallback callback)
        {
            ModelChanged += callback;
        }

        public void SubscribeCollectionChanged(ModelChangeCallback callback)
        {
            CollectionChanged += callback;
        }

        [field: NonSerialized]
        public event ModelChangeCallback ModelChanged;

        [field: NonSerialized]
        public event ModelChangeCallback CollectionChanged;
    }
}
