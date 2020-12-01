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
    public class KeyedObservableCollection<T> : ObservableBase, IList<T>, INotifyCollectionChanged where T : class
    {

        private PropertyInfo kpi;

        private List<T> innerList = new List<T>();
        private List<string> innerKeys = new List<string>();

        public int Count => innerList.Count;

        public bool IsReadOnly => false;

        private bool isKeySorted = false;
        public bool IsKeySorted
        {
            get => isKeySorted;
            set
            {
                if (SetProperty(ref isKeySorted, value))
                {
                    if (value == true) Sort();
                }
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

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
        }

        public KeyedObservableCollection(string propertyName, IEnumerable<T> items) : this(propertyName)
        {
            AddRange(items);
        }

        public bool ContainsKey(string key)
        {
            return ContainsKey(key, out _);
        }

        public bool ContainsKey(string key, out T item)
        {
            int i = innerKeys.IndexOf(key);

            if (i > -1)
            {
                item = innerList[i];
                return true;
            }
            else
            {
                item = null;
                return false;
            }
        }

        public T this[int index]
        {
            get => innerList[index];
            set
            {
                var item = innerList[index];             
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
                int i = innerKeys.IndexOf(key);
            
                if (i >= 0)
                {
                    return innerList[i];
                }
                else
                {
                    throw new KeyNotFoundException(key);
                }
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                int i = innerKeys.IndexOf(key);

                if (i >= 0)
                {
                    var item = innerList[i];
                    
                    if (item.Equals(value)) return;

                    innerList[i] = value;

                    if (CollectionChanged != null)
                    {
                        var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, item);
                        CollectionChanged.Invoke(this, e);
                    }
                }
                else
                {
                    throw new KeyNotFoundException(key);
                }
            }
        }

        public int IndexOf(T item)
        {
            return innerList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {

            if (item == null) throw new ArgumentNullException(nameof(item));

            string s = kpi.GetValue(item).ToString();

            if (innerKeys.Contains(s))
                throw new ArgumentException($"Collection already contains '{s}'.", nameof(item));

            innerList.Insert(index, item);
            innerKeys.Insert(index, s);

            if (IsKeySorted)
            {
                Sort();
            }

            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index);
                CollectionChanged.Invoke(this, e);
                OnPropertyChanged(nameof(Count));
            }
        }

        public void RemoveAt(int index)
        {
            var item = innerList[index];

            innerList.RemoveAt(index);
            innerKeys.RemoveAt(index);

            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);
                CollectionChanged.Invoke(this, e);
                OnPropertyChanged(nameof(Count));
            }

        }

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            int x = innerList.Count;

            string s = kpi.GetValue(item).ToString();

            if (innerKeys.Contains(s)) 
                throw new ArgumentException($"Collection already contains '{s}'.", nameof(item));

            innerKeys.Add(s);
            innerList.Add(item);

            if (IsKeySorted)
            {
                Sort();
            }

            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, x);
                CollectionChanged.Invoke(this, e);
                OnPropertyChanged(nameof(Count));
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                if (item == null) throw new ArgumentNullException(nameof(items), "One or more elements in the source collection is null.");

                string s = kpi.GetValue(item).ToString();

                if (innerKeys.Contains(s))
                    throw new ArgumentException($"Collection already contains '{s}'.", nameof(item));

                innerKeys.Add(s);
                innerList.Add(item);
            }

            if (IsKeySorted)
            {
                Sort();
            }

            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged.Invoke(this, e);
                OnPropertyChanged(nameof(Count));
            }
        }

        public void Clear()
        {
            innerList.Clear();
            innerKeys.Clear();

            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged.Invoke(this, e);
                OnPropertyChanged(nameof(Count));
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

        public void CopyKeys(string[] array, int arrayIndex)
        {
            innerKeys.CopyTo(array, arrayIndex);
        }

        public void Move(int oldIndex, int newIndex)
        {
            var item = innerList[oldIndex];
            var key = innerKeys[oldIndex];

            innerList.RemoveAt(oldIndex);
            innerList.Insert(newIndex, item);

            innerKeys.RemoveAt(oldIndex);
            innerKeys.Insert(newIndex, key);

            if (IsKeySorted) Sort();

            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex);
                CollectionChanged.Invoke(this, e);
            }
        }

        public bool Remove(T item)
        {
            bool ret;

            int i = innerList.IndexOf(item);
            ret = i >= 0;

            if (ret)
            {
                innerList.RemoveAt(i);
                innerKeys.RemoveAt(i);
            }

            if (ret) OnPropertyChanged(nameof(Count));

            return ret;
        }

        /// <summary>
        /// Sort on keys
        /// </summary>
        public void Sort()
        {
            if (innerList.Count < 2) return;

            int lo = 0;
            int hi = innerList.Count - 1;

            Sort(ref innerList, ref innerKeys, null, lo, hi, true);
            IsKeySorted = true;

            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged.Invoke(this, e);
            }
        }

        public void Sort(Comparison<T> comparison)
        {
            if (innerList.Count < 2) return;

            int lo = 0;
            int hi = innerList.Count - 1;

            IsKeySorted = false;
            Sort(ref innerList, ref innerKeys, comparison, lo, hi, false);

            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged.Invoke(this, e);
            }
        }

        private void Sort(ref List<T> values, ref List<string> keys, Comparison<T> comparison, int lo, int hi, bool onKey)
        {
            if (lo < hi)
            {
                int p = Partition(ref values, ref keys, comparison, lo, hi, onKey);

                Sort(ref values, ref keys, comparison, lo, p, onKey);
                Sort(ref values, ref keys, comparison, p + 1, hi, onKey);
            }
        }

        private int Partition(ref List<T> values, ref List<string> keys, Comparison<T> comparison, int lo, int hi, bool onKey)
        {
            var ppt = (hi + lo) / 2;
            var pivot = values[ppt];

            string kpivot = keys[ppt];

            int i = lo - 1;
            int j = hi + 1;

            while (true)
            {
                if (onKey)
                {
                    do
                    {
                        ++i;
                    } while (i <= hi && string.Compare(keys[i], kpivot) < 0);
                    do
                    {
                        --j;
                    } while (j >= 0 && string.Compare(keys[j], kpivot) > 0);
                }
                else
                {
                    do
                    {
                        ++i;
                    } while (i <= hi && comparison(values[i], pivot) < 0);
                    do
                    {
                        --j;
                    } while (j >= 0 && comparison(values[j], pivot) > 0);
                }

                if (i >= j) return j;

                T sw = values[i];
                string sk = keys[i];

                values[i] = values[j];
                keys[i] = keys[j];

                values[j] = sw;
                keys[j] = sk;
            }
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
