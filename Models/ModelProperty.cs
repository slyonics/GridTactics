using GridTactics.Models;
using GridTactics.SceneObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GridTactics.Models
{
    public delegate void ModelChangeCallback();

    public interface IModelProperty
    {
        public event ModelChangeCallback ModelChanged;
        public object GetValue();
        public void Unbind(ModelChangeCallback binding);
    }

    [Serializable]
    public class ModelProperty<T> : IModelProperty
    {
        private T model;

        public T Value
        {
            set
            {
                model = value;
                ModelChanged?.Invoke();
            }

            get => model;
        }

        public object GetValue()
        {
            return Value;
        }

        public void Unbind(ModelChangeCallback callback)
        {
            ModelChanged -= callback;
        }

        [field: NonSerialized]
        public event ModelChangeCallback ModelChanged;

        public ModelProperty()
        {

        }

        public ModelProperty(T iModel)
        {
            model = iModel;
        }

        public override string ToString()
        {
            return model.ToString();
        }
    }
}
