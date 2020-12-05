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
using WizLib.Profiles;
using System.Threading;

namespace WizLib
{
    /// <summary>
    /// Sortable, keyed observable collection.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class KeyedObservableCollection<TKey, TValue> : ObservableBase, IList<TValue>, INotifyCollectionChanged where TValue : class
    {
        private enum ArrayOperation
        {
            Remove,
            Insert,
            Move
        }

        private struct Entry
        {
            public int index;
            public TKey key;

            public Entry(int index, TKey key)
            {
                this.index = index;
                this.key = key;
            }

            public override string ToString()
            {
                return key?.ToString();
            }
        }

        private PropertyInfo kpi;

        private int capacity = 0;

        private TValue[] innerList;
        private Entry[] entries;

        private int[] indexKey;
        private int[] keyIndex;

        public int Count => innerList?.Length ?? 0;

        public bool IsReadOnly => false;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// List of registered <see cref="Comparer{TKey}"/> instances.
        /// </summary>
        public static List<Type> Comparers { get; set; } = new List<Type>();


        private Comparison<TKey> keycomp;

        SynchronizationContext sc = new SynchronizationContext();

        /// <summary>
        /// Gets or sets the <see cref="Comparison{TKey}"/> to use to sort the keys.
        /// </summary>
        public Comparison<TKey> KeyComparison
        {
            get => keycomp;
            set
            {
                SetProperty(ref keycomp, value);
            }
        }

        /// <summary>
        /// Get the <see cref="PropertyInfo"/> for the <see cref="TKey"/> value.
        /// </summary>
        public PropertyInfo KeyProperty
        {
            get => kpi;
        }

        /// <summary>
        /// Create a new <see cref="KeyedObservableCollection{TKey, TValue}"/>
        /// </summary>
        /// <param name="propertyName">The name of the property in the class object to use as the key.</param>
        /// <param name="keyComparison">The <see cref="Comparison{TKey}"/> to use to sort the keys.</param>
        public KeyedObservableCollection(string propertyName, Comparison<TKey> keyComparison)
        {
            if (keyComparison != null)
            {
                keycomp = keyComparison;
            }
            else
            {
                if (!(typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey))))
                {
                    foreach (var c in Comparers)
                    {                        
                        if (typeof(IComparer<TKey>).IsAssignableFrom(c))
                        {
                            var tc = (IComparer<TKey>)System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(c.FullName);

                            keycomp = new Comparison<TKey>(tc.Compare);
                            break;
                        }
                    }

                    if (keycomp == null)
                    {
                        throw new NotSupportedException("No compatible comparer found for type {" + typeof(TKey).Name + "}.");
                    }
                }
            }

            kpi = typeof(TValue).GetProperty(propertyName);

            if (kpi == null)
                throw new ArgumentException(nameof(propertyName), $"Property '{propertyName}' property does not exist in '{typeof(TValue).Name}'.");

            if (kpi.PropertyType != typeof(TKey))
            {
                throw new ArgumentException(nameof(propertyName), $"Property '{propertyName}' property is not of type '{typeof(TKey).Name}'.");
            }
        }

        /// <summary>
        /// Create a new <see cref="KeyedObservableCollection{TKey, TValue}"/>
        /// </summary>
        /// <param name="propertyName">The name of the property in the class object to use as the key.</param>
        /// <param name="keyComparison">The <see cref="Comparison{TKey}"/> to use to sort the keys.</param>
        /// <param name="items">An <see cref="IEnumerable{TValue}"/> of items used to initialize the collection.</param>
        public KeyedObservableCollection(string propertyName, Comparison<TKey> keyComparison, IEnumerable<TValue> items) : this(propertyName, keyComparison)
        {
            AddRange(items, true);
        }
        /// <summary>
        /// Create a new <see cref="KeyedObservableCollection{TKey, TValue}"/>
        /// </summary>
        /// <param name="propertyName">The name of the property in the class object to use as the key.</param>
        public KeyedObservableCollection(string propertyName) : this(propertyName, (Comparison<TKey>)null)
        {
        }
        /// <summary>
        /// Create a new <see cref="KeyedObservableCollection{TKey, TValue}"/>
        /// </summary>
        /// <param name="propertyName">The name of the property in the class object to use as the key.</param>
        /// <param name="keyComparer">The <see cref="IComparer{TKey}"/> to use to sort the keys.</param>
        public KeyedObservableCollection(string propertyName, IComparer<TKey> keyComparer) : this(propertyName, new Comparison<TKey>(keyComparer.Compare))
        {
        }

        /// <summary>
        /// Create a new <see cref="KeyedObservableCollection{TKey, TValue}"/>
        /// </summary>
        /// <param name="propertyName">The name of the property in the class object to use as the key.</param>
        /// <param name="keyComparer">The <see cref="IComparer{TKey}"/> to use to sort the keys.</param>
        /// <param name="items">An <see cref="IEnumerable{TValue}"/> of items used to initialize the collection.</param>
        public KeyedObservableCollection(string propertyName, IComparer<TKey> keyComparer, IEnumerable<TValue> items) : this(propertyName, new Comparison<TKey>(keyComparer.Compare), items)
        {

        }

        /// <summary>
        /// Create a new <see cref="KeyedObservableCollection{TKey, TValue}"/>
        /// </summary>
        /// <param name="propertyName">The name of the property in the class object to use as the key.</param>
        /// <param name="items">An <see cref="IEnumerable{TValue}"/> of items used to initialize the collection.</param>
        public KeyedObservableCollection(string propertyName, IEnumerable<TValue> items) : this(propertyName, (Comparison<TKey>)null, items)
        {
        }

        /// <summary>
        /// Check whether a key exists in the collection.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>True if the key exists</returns>
        public bool ContainsKey(TKey key)
        {
            return ContainsKey(key, out _);
        }

        /// <summary>
        /// Check whether a key exists in the collection.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <param name="item">Receives the item at the location indicated by the key.</param>
        /// <returns>True if the key exists</returns>
        public bool ContainsKey(TKey key, out TValue item)
        {
            int i;
            i = Search(key);
            if (i != -1)
            {
                item = innerList[i];
                return true;
            }

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


        private TValue getItem(TKey key)
        {
            int i;
            i = Search(key);

            if (i >= 0)
            {
                return innerList[i];
            }
            else
            {
                throw new KeyNotFoundException(key.ToString());
            }
        }

        private void setItem(TKey key, TValue value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            int i;
            i = Search(key);

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
                throw new KeyNotFoundException(key.ToString());
            }

        }

        TValue IList<TValue>.this[int index]
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

        /// <summary>
        /// Gets or sets the value of the item represented by the specified key.
        /// </summary>
        /// <param name="key">The key of the item.</param>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            get => getItem(key);
            set => setItem(key, value);
        }


        public int IndexOf(TValue item)
        {
            int i = 0;
            foreach (var t in innerList)
            {
                if (Equals(t, item)) return i;
                i++;
            }
            return -1;
        }


        /// <summary>
        /// Gets the index of the item by key.
        /// </summary>
        /// <param name="key">The key of the item.</param>
        /// <returns></returns>
        public int IndexOfKey(TKey key)
        {
            int i;
            i = Search(key);
            return i;
        }
        public void Insert(int index, TValue item)
        {
            Insert(index, item, false);
        }

        private void Insert(int index, TValue item, bool suppressEvent)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            TKey k = (TKey)kpi.GetValue(item);

            if (ContainsKey(k))
                throw new ArgumentException($"Collection already contains key '{k}'.", nameof(item));

            ArrOp(ArrayOperation.Insert, ref innerList, newIndex: index);
            ArrOp(ArrayOperation.Insert, ref entries, newIndex: index);
            ArrOp(ArrayOperation.Insert, ref indexKey, newIndex: index);
            ArrOp(ArrayOperation.Insert, ref keyIndex, newIndex: index);

            innerList[index] = item;
            entries[index] = new Entry(index, k);
            indexKey[index] = index;
            keyIndex[index] = index;

            capacity++;
            KeySort();

            if (!suppressEvent && CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index);
                CollectionChanged.Invoke(this, e);
                OnPropertyChanged(nameof(Count));
            }
        }

        //private void EnsureCapacity(int size)
        //{
        //    int c = innerList?.Length ?? 0;

        //    if (size <= c) return;

        //    Array.Resize(ref innerList, size);
        //    Array.Resize(ref entries, size);

        //    capacity = size;
        //}

        public void RemoveAt(int index)

        {
            RemoveAt(index, false);
        }

        private void RemoveAt(int index, bool suppressEvent)
        {
            var item = innerList[index];

            ArrOp(ArrayOperation.Remove, ref innerList, oldIndex: index);
            
            int idx = KeyForItemAt(index);
            ArrOp(ArrayOperation.Remove, ref entries, oldIndex: idx);

            capacity--;

            if (!suppressEvent && CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);
                CollectionChanged.Invoke(this, e);
                OnPropertyChanged(nameof(Count));
            }
        }

        /// <summary>
        /// Remove an item by its key.
        /// </summary>
        /// <param name="key">Key of item to remove.</param>
        public void RemoveKey(TKey key)
        {
            int i, c;

            i = Search(key, out c);

            RemoveAt(i);
        }

        public void Add(TValue item)
        {
            Add(item, false);
        }

        private void Add(TValue item, bool suppressEvent)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            int x = innerList?.Length ?? 0;

            TKey s = (TKey)kpi.GetValue(item);

            if (ContainsKey(s))
                throw new ArgumentException($"Collection already contains key '{s}'.", nameof(item));

            Array.Resize(ref innerList, x + 1);
            Array.Resize(ref entries, x + 1);
            Array.Resize(ref keyIndex, x + 1);
            Array.Resize(ref indexKey, x + 1);

            innerList[x] = item;
            entries[x] = new Entry(x, s);
            keyIndex[x] = x;
            indexKey[x] = x;

            capacity = x + 1;

            KeySort();

            if (!suppressEvent && CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, x);
                CollectionChanged.Invoke(this, e);
                OnPropertyChanged(nameof(Count));
            }

        }

        /// <summary>
        /// Add a range of items to the collection.
        /// </summary>
        /// <param name="items">The list of items to add.</param>
        public void AddRange(IEnumerable<TValue> items) => AddRange(items, false);

        private void AddRange(IEnumerable<TValue> items, bool suppressEvent)
        {
            int c = 0;
            int x = innerList?.Length ?? 0;

            foreach (var item in items) c++;
            var ns = x + c;

            Array.Resize(ref innerList, ns);
            Array.Resize(ref entries, ns);
            Array.Resize(ref keyIndex, ns);
            Array.Resize(ref indexKey, ns);

            foreach (var item in items)
            {
                if (item == null) throw new ArgumentNullException(nameof(item));

                TKey k = (TKey)kpi.GetValue(item);

                if (ContainsKey(k))
                    throw new ArgumentException($"Collection already contains key '{k}'.", nameof(item));

                innerList[x] = item;
                entries[x] = new Entry(x, k);
                keyIndex[x] = x;
                indexKey[x] = x;

                x++;
            }

            capacity = ns;
            KeySort();

            if (!suppressEvent && CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged.Invoke(this, e);
                OnPropertyChanged(nameof(Count));
            }
        }

        public void Clear()
        {
            Array.Clear(innerList, 0, innerList?.Length ?? 0);

            innerList = null;
            entries = null;
            indexKey = null;
            keyIndex = null;
            capacity = 0;

            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged.Invoke(this, e);
                OnPropertyChanged(nameof(Count));
            }
        }

        public bool Contains(TValue item)
        {
            return innerList.Contains(item);
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            innerList.CopyTo(array, arrayIndex);
        }

        private void ReIndex()
        {
            int i, c = innerList.Length;
            
            if (c < 2) return;

            for (i = 0; i < c; i++)
            {
                entries[i].index = i;
                entries[i].key = (TKey)kpi.GetValue(innerList[i]);

                keyIndex[i] = i;
                indexKey[i] = i;
            }

            KeySort();
        }

        /// <summary>
        /// Move an item in the collection from one index to another.
        /// </summary>
        /// <param name="oldIndex">The source item index.</param>
        /// <param name="newIndex">The destination item index.</param>
        public void Move(int oldIndex, int newIndex)
        {
            var item = innerList[oldIndex];

            int odx, pdx;

            odx = indexKey[oldIndex];
            pdx = indexKey[newIndex];

            ArrOp(ArrayOperation.Move, ref innerList, oldIndex, newIndex);
            //ArrOp(ArrayOperation.Move, ref indexKey, oldIndex, newIndex);

            //ArrOp(ArrayOperation.Move, ref entries, oldIndex, newIndex);
            //KeySort();
            ReIndex();

            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex);
                CollectionChanged.Invoke(this, e);
            }
        }

        public bool Remove(TValue item)
        {
            bool ret;

            int i = IndexOf(item);
            ret = i >= 0;

            if (ret)
            {
                RemoveAt(i);
                OnPropertyChanged(nameof(Count));
            }

            return ret;
        }

        int KeyForItemAt(int index)
        {
            int c = entries?.Length ?? 0;
            int i;
            for (i = 0; i < c; i++)
            {
                if (entries[i].index == index) return i;
            }

            return -1;
        }

        /// <summary>
        /// Sort keys in ascending order
        /// </summary>
        private void KeySort()
        {
            if (Count < 2) return;

            int lo = 0;
            int hi = Count - 1;

            Sort(null, lo, hi, true);
        }

        /// <summary>
        /// Sort the collection using the specified <see cref="Comparison{T}"/>.
        /// </summary>
        /// <param name="comparison">The comparison to use to sort the collection.</param>
        public void Sort(Comparison<TValue> comparison)
        {
            if (Count < 2) return;
            
            int lo = 0;
            int hi = Count - 1;

            Sort(comparison, lo, hi, false);
            ReIndex();

            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged.Invoke(this, e);
            }
        }

        #region ArrOp

        /// <summary>
        /// Remove, Insert, Move operations.
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="mode"></param>
        /// <param name="arr"></param>
        /// <param name="oldIndex"></param>
        /// <param name="newIndex"></param>
        private void ArrOp<U>(
            ArrayOperation mode, 
            ref U[] arr, 
            int oldIndex = -1, 
            int newIndex = -1) 
        {
            U[] a2;

            int i;
            int c, d;

            c = d = arr.Length;

            if (mode != ArrayOperation.Insert && (oldIndex < 0 || oldIndex >= arr.Length)) 
                throw new ArgumentOutOfRangeException(nameof(oldIndex));

            if (mode == ArrayOperation.Remove) // remove
            {

                --d;

                a2 = new U[d]; // dest array

                if (oldIndex > 0)
                {
                    Array.Copy(arr, 0, a2, 0, oldIndex);
                }

                if (oldIndex < (c - 1))
                {
                    Array.Copy(arr, oldIndex + 1, a2, oldIndex, d - oldIndex);
                }

                arr = a2;
                return;
            }

            if (newIndex < 0 || newIndex > arr.Length) 
                throw new ArgumentOutOfRangeException(nameof(newIndex));
            
            if (mode == ArrayOperation.Insert) // insert 
            {
                ++c;

                a2 = new U[c];

                if (newIndex > 0)
                {
                    Array.Copy(arr, 0, a2, 0, newIndex);
                }

                if (newIndex < (c - 1))
                {
                    Array.Copy(arr, newIndex, a2, newIndex + 1, d - newIndex);
                }
                arr = a2;
            }            
            else if (mode == ArrayOperation.Move) // move
            {
                U elem = arr[oldIndex]; 

                if (oldIndex < newIndex)
                {
                    i = newIndex - oldIndex;
                    a2 = new U[i];

                    Array.Copy(arr, oldIndex + 1, a2, 0, i);
                    Array.Copy(a2, 0, arr, oldIndex, i);
                }
                else
                {
                    i = oldIndex - newIndex;
                    a2 = new U[i];

                    Array.Copy(arr, newIndex, a2, 0, i);
                    Array.Copy(a2, 0, arr, newIndex + 1, i);
                }

                arr[newIndex] = elem;
            }

        }


        #endregion

        #region QuickSort
        private void Sort(Comparison<TValue> comparison, int lo, int hi, bool onKey)
        {
            if (lo < hi)
            {
                int p;

                if (onKey)
                {
                    p = PartitionOnKey(lo, hi);
                }
                else
                {
                    p = Partition(comparison, lo, hi);
                }

                Sort(comparison, lo, p, onKey);
                Sort(comparison, p + 1, hi, onKey);
            }
        }

        private int Partition(Comparison<TValue> comparison, int lo, int hi)
        {
            var ppt = (hi + lo) / 2;
            var pivot = innerList[ppt];

            int i = lo - 1;
            int j = hi + 1;

            while (true)
            {
                do
                {
                    ++i;
                } while (i <= hi && comparison(innerList[i], pivot) < 0);
                do
                {
                    --j;
                } while (j >= 0 && comparison(innerList[j], pivot) > 0);

                if (i >= j) return j;

                TValue sw = innerList[i];
                innerList[i] = innerList[j];
                innerList[j] = sw;

                var t = indexKey[i];
                indexKey[i] = indexKey[j];
                indexKey[j] = t;
            }
        }

        private int PartitionOnKey(int lo, int hi)
        {
            var ppt = (hi + lo) / 2;

            TKey kpivot = entries[ppt].key;

            int i = lo - 1;
            int j = hi + 1;

            Comparison<TKey> def = keycomp;

            if (def == null)
            {
                def = new Comparison<TKey>((a, b) => ((IComparable<TKey>)a).CompareTo(b));
            }

            while (true)
            {
                do
                {
                    ++i;
                } while (i <= hi && def(entries[i].key, kpivot) < 0);
                do
                {
                    --j;
                } while (j >= 0 && def(entries[j].key, kpivot) > 0);

                if (i >= j) return j;

                Entry sw = entries[i];

                entries[i] = entries[j];
                entries[j] = sw;

                var t = keyIndex[i];
                keyIndex[i] = keyIndex[j];
                keyIndex[j] = t;
            }
        }

        #endregion

        #region Binary Search 

        private int Search(TKey value)
        {
            return Search(value, out _);
        }

        private int Search(TKey value, out int index)
        {
            int lo = 0, hi = Count - 1;
            Comparison<TKey> def = keycomp;

            if (def == null)
            {
                def = new Comparison<TKey>((a, b) => ((IComparable<TKey>)a).CompareTo(b));
            }

            while (true)
            {
                if (lo > hi) break;

                int p = ((hi + lo) / 2);
                Entry elem = entries[p];
                int c;

                c = def(value, elem.key);

                if (c == 0)
                {
                    index = p;
                    return elem.index;
                }
                else if (c < 0)
                {
                    hi = p - 1;
                }
                else
                {
                    lo = p + 1;
                }
            }

            index = -1;
            return -1;
        }

        #endregion

        #region IEnumerable

        public IEnumerator<TValue> GetEnumerator()
        {
            return new KeyedCollectionEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new KeyedCollectionEnumerator(this);
        }

        private class KeyedCollectionEnumerator : IEnumerator<TValue>
        {
            private int idx = -1;
            private IList<TValue> objs;

            public KeyedCollectionEnumerator(KeyedObservableCollection<TKey, TValue> list)
            {
                objs = list;
            }

            public TValue Current => objs[idx];

            object IEnumerator.Current => objs[idx];

            public void Dispose()
            {
                idx = -1;
                objs = null;
            }

            public bool MoveNext()
            {
                return ++idx < (objs?.Count ?? -1);
            }

            public void Reset()
            {
                idx = -1;
            }
        }

        #endregion

    }

}
