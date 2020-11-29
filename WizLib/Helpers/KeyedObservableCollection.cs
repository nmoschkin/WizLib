using System;
using System.ComponentModel;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizLib
{
    /// <summary>
    /// Keyed observable collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class KeyedObservableCollection<T> : IList<T>, INotifyPropertyChanged, INotifyCollectionChanged where T : class
    {

        private PropertyInfo kpi;

        private List<T> innerList = new List<T>();

        public int Count => innerList.Count;

        public bool IsReadOnly => false;


        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnNotifyPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PropertyInfo KeyProperty
        {
            get => kpi;
        }

        public KeyedObservableCollection(string propertyName)
        {
            var tt = typeof(T);

            kpi = tt.GetProperty(propertyName);

            if (kpi == null)
                throw new ArgumentException(nameof(propertyName), $"That property does not exist in '{tt}'.");

            //if (kpi.PropertyType != typeof(string))
            //{
            //    throw new ArgumentException(nameof(propertyName), $"'{kpi.Name}' is not a string.");
            //}
        }

        public KeyedObservableCollection(string propertyName, IEnumerable<T> items) : this(propertyName)
        {
            innerList.AddRange(items);
        }


        public bool ContainsKey(string key)
        {
            return ContainsKey(key, out _);
        }

        public bool ContainsKey(string key, out T item)
        {
            int i, c = innerList.Count;

            for (i = 0; i < c; i++)
            {
                var ichk = innerList[i];
                string s = kpi.GetValue(ichk).ToString();

                if (s == key)
                {
                    item = ichk;
                    return true;
                }
            }

            item = null;
            return false;
        }

        public T this[int index]
        {
            get => innerList[index];
            set
            {
                var item = innerList[index];
                string s = (string)kpi.GetValue(item);

                innerList[index] = value;

                if (CollectionChanged != null)
                {
                    var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, item);
                    CollectionChanged.Invoke(this, e);
                }
            }
        }

        public T this[string key]
        {
            get
            {
                int i, c = innerList.Count;

                for (i = 0; i < c; i++)
                {
                    var item = innerList[i];
                    string s = (string)kpi.GetValue(item);

                    if (s == key) return item;
                }

                return null;
            }
            set
            {
                int i, c = innerList.Count;

                for (i = 0; i < c; i++)
                {
                    var item = innerList[i];
                    string s = (string)kpi.GetValue(item);

                    if (s == key)
                    {
                        innerList[i] = value;

                        if (CollectionChanged != null)
                        {
                            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, item);
                            CollectionChanged.Invoke(this, e);
                        }

                        break;
                    }
                }
            }
        }

        public int IndexOf(T item)
        {
            return innerList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            innerList.Insert(index, item);

            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index);
                CollectionChanged.Invoke(this, e);
                OnNotifyPropertyChanged(nameof(Count));
            }
        }

        public void RemoveAt(int index)
        {
            var item = innerList[index];
            innerList.RemoveAt(index);

            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);
                CollectionChanged.Invoke(this, e);
                OnNotifyPropertyChanged(nameof(Count));
            }

        }

        public void Add(T item)
        {
            int x = innerList.Count;

            innerList.Add(item);

            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, x);
                CollectionChanged.Invoke(this, e);
                OnNotifyPropertyChanged(nameof(Count));
            }
        }

        public void Sort()
        {
            innerList.Sort();
            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged.Invoke(this, e);
            }
        }

        public void Sort(Comparison<T> cmp)
        {
            innerList.Sort(cmp);
            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged.Invoke(this, e);
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            innerList.AddRange(items);
            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged.Invoke(this, e);
                OnNotifyPropertyChanged(nameof(Count));
            }
        }

        public void Clear()
        {
            innerList.Clear();
            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged.Invoke(this, e);
                OnNotifyPropertyChanged(nameof(Count));
            }
        }

        public bool Contains(T item)
        {
            return innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            innerList.CopyTo(array, arrayIndex);
        }

        public void Move(int oldIndex, int newIndex)
        {
            var item = innerList[oldIndex];

            innerList.RemoveAt(oldIndex);
            innerList.Insert(newIndex, item);

            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex);
                CollectionChanged.Invoke(this, e);
            }
        }

        public bool Remove(T item)
        {
            var ret = innerList.Remove(item);
            if (ret) OnNotifyPropertyChanged(nameof(Count));

            return ret;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)innerList).GetEnumerator();
        }
    }


}
