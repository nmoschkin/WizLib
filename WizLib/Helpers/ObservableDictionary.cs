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
using Newtonsoft.Json;

namespace WizLib
{
    /// <summary>
    /// Sortable, keyed observable dictionary for classes.
    /// </summary>
    /// <typeparam name="TKey">The key type.  <see cref="TKey"/> must implement <see cref="IComparable{TKey}"/> or a suitable comparison must be provided.</typeparam>
    /// <typeparam name="TValue">The value type.  <see cref="TValue"/> must contain a property of type <see cref="TKey"/>.</typeparam>
    public class ObservableDictionary<TKey, TValue> : ObservableBase, IDictionary<TKey, TValue>, IList<TValue>, INotifyCollectionChanged where TValue : class
    {
        private enum ArrayOperation
        {
            Remove,
            Insert,
            Move
        }

        private PropertyInfo keyProp;
        private string keyPropName;

        private int _size = 0;

        private TValue[] innerList;
        private TKey[] innerKeys;
        private int[] lookup;
        private int[] revLookup;

        [JsonIgnore]
        public int Count => _size;

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
        [JsonIgnore]
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
        [JsonIgnore]
        public PropertyInfo KeyProperty
        {
            get => keyProp;        
        }

        /// <summary>
        /// Gets the name of the key property.
        /// </summary>
        [JsonProperty("KeyProperty")]
        public string KeyPropertyName
        {
            get => keyPropName;
        }

        /// <summary>
        /// Create a new <see cref="ObservableDictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="propertyName">The name of the property in the class object to use as the key.</param>
        /// <param name="keyComparison">The <see cref="Comparison{TKey}"/> to use to sort the keys.</param>
        public ObservableDictionary(string propertyName, Comparison<TKey> keyComparison)
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

            keyProp = typeof(TValue).GetProperty(propertyName);

            if (keyProp == null)
                throw new ArgumentException(nameof(propertyName), $"Property '{propertyName}' property does not exist in '{typeof(TValue).Name}'.");

            if (keyProp.PropertyType != typeof(TKey))
            {
                throw new ArgumentException(nameof(propertyName), $"Property '{propertyName}' property is not of type '{typeof(TKey).Name}'.");
            }

            keyPropName = propertyName;
        }

        /// <summary>
        /// Create a new <see cref="ObservableDictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="propertyName">The name of the property in the class object to use as the key.</param>
        /// <param name="keyComparison">The <see cref="Comparison{TKey}"/> to use to sort the keys.</param>
        /// <param name="items">An <see cref="IEnumerable{TValue}"/> of items used to initialize the collection.</param>
        public ObservableDictionary(string propertyName, Comparison<TKey> keyComparison, IEnumerable<TValue> items) : this(propertyName, keyComparison)
        {
            AddRange(items, true);
        }
        /// <summary>
        /// Create a new <see cref="ObservableDictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="propertyName">The name of the property in the class object to use as the key.</param>
        public ObservableDictionary(string propertyName) : this(propertyName, (Comparison<TKey>)null)
        {
        }
        /// <summary>
        /// Create a new <see cref="ObservableDictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="propertyName">The name of the property in the class object to use as the key.</param>
        /// <param name="keyComparer">The <see cref="IComparer{TKey}"/> to use to sort the keys.</param>
        public ObservableDictionary(string propertyName, IComparer<TKey> keyComparer) : this(propertyName, new Comparison<TKey>(keyComparer.Compare))
        {
        }

        /// <summary>
        /// Create a new <see cref="ObservableDictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="propertyName">The name of the property in the class object to use as the key.</param>
        /// <param name="keyComparer">The <see cref="IComparer{TKey}"/> to use to sort the keys.</param>
        /// <param name="items">An <see cref="IEnumerable{TValue}"/> of items used to initialize the collection.</param>
        public ObservableDictionary(string propertyName, IComparer<TKey> keyComparer, IEnumerable<TValue> items) : this(propertyName, new Comparison<TKey>(keyComparer.Compare), items)
        {

        }

        /// <summary>
        /// Create a new <see cref="ObservableDictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="propertyName">The name of the property in the class object to use as the key.</param>
        /// <param name="items">An <see cref="IEnumerable{TValue}"/> of items used to initialize the collection.</param>
        public ObservableDictionary(string propertyName, IEnumerable<TValue> items) : this(propertyName, (Comparison<TKey>)null, items)
        {
        }
        private ObservableDictionary()
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
            else
            {
                item = null;
                return false;
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

        public TValue[] ToArray()
        {
            var x = new TValue[_size];
            if (_size == 0) return x;

            innerList.CopyTo(x, 0);
            return x;
        }

        public ICollection<TValue> Values
        {
            get => ToArray();
        }

        public ICollection<TKey> Keys
        {
            get
            {
                var x = new TKey[_size];
                if (_size == 0) return x;

                innerKeys.CopyTo(x, 0);
                return x;
            }
        }


        int ICollection<KeyValuePair<TKey, TValue>>.Count => _size;

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;


        public bool TryGetValue(TKey key, out TValue value)
        {
            int i = Search(key);
            if (i == -1)
            {
                value = null;
                return false;
            }
            else
            {
                value = innerList[i];
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the value of the item represented by the specified key.
        /// </summary>
        /// <param name="key">The key of the item.</param>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            get
            {
                int i = Search(key);

                if (i >= 0)
                {
                    return innerList[i];
                }
                else
                {
                    throw new KeyNotFoundException(key.ToString());
                }
            }
            set
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
                    Add(key, value);
                    //throw new KeyNotFoundException(key.ToString());
                }
            }
        }

        public int IndexOf(TValue item)
        {
            int i = 0;
            foreach (var t in innerList)
            {
                if (t == item) return i;
                i++;
            }
            return -1;
        }

        /// <summary>
        /// Gets the index of the item by key.
        /// </summary>
        /// <param name="key">The key of the item.</param>
        /// <returns></returns>
        public int IndexOfKey(TKey key) => Search(key);
        
        public void Insert(int index, TValue item)
        {
            Insert(index, item, false);
        }

        private void Insert(int index, TValue item, bool suppressEvent)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            TKey key = (TKey)keyProp.GetValue(item);

            if (ContainsKey(key))
                throw new ArgumentException($"Collection already contains key '{key}'.", nameof(item));


            for (int g = index; g < _size; g++)
            {
                lookup[revLookup[g]]++;
            }

            ArrOp(ArrayOperation.Insert, ref innerList, newIndex: index);
            ArrOp(ArrayOperation.Insert, ref revLookup, newIndex: index);

            int idx;
            Search(key, out idx, true);

            lookup[idx] = index;
            innerKeys[idx] = key;

            innerList[index] = item;
            revLookup[index] = idx;

            _size++;
            
            if (!suppressEvent && CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index);
                CollectionChanged.Invoke(this, e);
                OnPropertyChanged(nameof(Count));
            }
        }

        internal void EnsureCapacity(int size)
        {
            int c = _size;

            if (size <= c) return;

            Array.Resize(ref innerList, size);
            Array.Resize(ref innerKeys, size);
            Array.Resize(ref lookup, size);
            Array.Resize(ref revLookup, size);

            _size = size;
        }

        public void RemoveAt(int index)

        {
            RemoveAt(index, false);
        }

        private void RemoveAt(int index, bool suppressEvent)
        {
            var item = innerList[index];
            var idx = revLookup[index];

            ArrOp(ArrayOperation.Remove, ref innerList, oldIndex: index);
            ArrOp(ArrayOperation.Remove, ref revLookup, oldIndex: index);
            ArrOp(ArrayOperation.Remove, ref innerKeys, oldIndex: idx);
            ArrOp(ArrayOperation.Remove, ref lookup, oldIndex: idx);

            --_size;

            for (int g = 0; g < _size; g++)
            {
                if (lookup[g] >= index)
                {
                    lookup[g]--;
                }
                if (revLookup[g] >= idx)
                {
                    revLookup[g]--;
                }
            }
            
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
        public bool RemoveKey(TKey key)
        {
            int i = Search(key);
            if (i == -1) return false;
            RemoveAt(i);
            return true;
        }

        public void Add(TValue item)
        {
            Add(item, false);
        }

        private void Add(TValue item, bool suppressEvent)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            int x = innerList?.Length ?? 0;

            TKey key = (TKey)keyProp.GetValue(item);

            if (ContainsKey(key))
                throw new ArgumentException($"Collection already contains key '{key}'.", nameof(item));

            Array.Resize(ref innerList, x + 1);
            Array.Resize(ref revLookup, x + 1);

            int idx;
            Search(key, out idx, true);

            lookup[idx] = x;
            innerKeys[idx] = key;

            innerList[x] = item;
            revLookup[x] = idx;

            _size = x + 1;

            // KeySort();

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
            int c = items.Count();
            int x = _size;

            var ns = x + c;

            Array.Resize(ref innerList, ns);
            Array.Resize(ref revLookup, ns);
            Array.Resize(ref innerKeys, ns);
            Array.Resize(ref lookup, ns);

            foreach (var item in items)
            {
                if (item == null) throw new ArgumentNullException(nameof(item));

                TKey key = (TKey)keyProp.GetValue(item);

                if (ContainsKey(key))
                    throw new ArgumentException($"Collection already contains key '{key}'.", nameof(item));

                int idx;
                Search(key, out idx, true, true, x);

                lookup[idx] = x;
                innerKeys[idx] = key;

                innerList[x] = item;
                revLookup[x] = idx;

                x++;
            }

            _size = ns;
            //KeySort();

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
            innerKeys = null;
            _size = 0;

            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged.Invoke(this, e);
                OnPropertyChanged(nameof(Count));
            }
        }

        public bool Contains(TValue item)
        {
            TKey tk = (TKey)keyProp.GetValue(item);
            int idx = Search(tk);
            return idx != -1;
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            innerList.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Move an item in the collection from one index to another.
        /// </summary>
        /// <param name="oldIndex">The source item index.</param>
        /// <param name="newIndex">The destination item index.</param>
        public void Move(int oldIndex, int newIndex)
        {
            var item = innerList[oldIndex];

            RemoveAt(oldIndex, true);
            Insert(newIndex, item, true);

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

        /// <summary>
        /// Sorts a collection where <see cref="TValue"/> implements <see cref="IComparable{TValue}"/>.
        /// </summary>
        public void Sort()
        {
            if (typeof(IComparable<TValue>).IsAssignableFrom(typeof(TValue)))
            {
                var comp = new Comparison<TValue>((a, b) => {

                    if (a is IComparable<TValue> ai)
                    {
                        return ai.CompareTo(b);
                    }
                    else if (b is IComparable<TValue> bi)
                    {
                        return -bi.CompareTo(a);
                    }
                    else return 0;
                });

                Sort(comp);
            }
            else
            {
                throw new NotSupportedException("No compatible comparer found for type {" + typeof(TValue).Name + "}.");
            }
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
        /// <remarks>
        /// In <see cref="ArrayOperation.Insert"/> mode, the item space is created, but the item is not, itself, inserted.
        /// Inserting is up to the caller.
        /// </remarks>
        private void ArrOp<U>(
            ArrayOperation mode, 
            ref U[] arr, 
            int oldIndex = -1, 
            int newIndex = -1,
            bool expanded = false,
            int virtSize = -1) 
        {
            U[] a2;

            int i;
            int c, d;

            if (expanded)
            {
                c = d = virtSize;
            }
            else
            {
                c = d = arr?.Length ?? 0;
            }

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

            if (newIndex < 0 || newIndex > (arr?.Length ?? 0)) 
                throw new ArgumentOutOfRangeException(nameof(newIndex));
            
            if (mode == ArrayOperation.Insert) // insert 
            {
                if (!expanded)
                {
                    ++c;
                    a2 = new U[c];

                    if (newIndex > 0)
                    {
                        Array.Copy(arr, 0, a2, 0, newIndex);
                    }
                }
                else
                {
                    a2 = arr;
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
                try
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

                    lookup[revLookup[i]] = i;
                    lookup[revLookup[j]] = j;
                }
                catch (Exception ex)
                {
                    var e = ex;
                }
            }
        }

        private int PartitionOnKey(int lo, int hi)
        {
            var ppt = (hi + lo) / 2;

            TKey kpivot = innerKeys[ppt];

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
                } while (i <= hi && def(innerKeys[i], kpivot) < 0);
                do
                {
                    --j;
                } while (j >= 0 && def(innerKeys[j], kpivot) > 0);

                if (i >= j) return j;

                TKey sw = innerKeys[i];

                innerKeys[i] = innerKeys[j];
                innerKeys[j] = sw;

                int si = lookup[i];

                lookup[i] = lookup[j];
                lookup[j] = si;

                revLookup[lookup[i]] = i;
                revLookup[lookup[j]] = j;
            }
        }

        #endregion

        #region Binary Search 

        private int Search(TKey value)
        {
            return Search(value, out _, false);
        }

        private int Search(TKey value, out int index, bool insert, bool expanded = false, int virtSize = -1)
        {
            int max = virtSize > -1 ? virtSize - 1 : _size - 1;
            int lo = 0, hi = max;

            Comparison<TKey> def = keycomp;

            if (def == null)
            {
                def = new Comparison<TKey>((a, b) => ((IComparable<TKey>)a).CompareTo(b));
            }

            while (true)
            {
                int p;

                if (lo > hi)
                {
                    if (insert)
                    {
                        if (hi >= 0)
                        {
                            p = def(value, innerKeys[hi]);
                            if (p < 0) p = hi - 1;
                            else if (p > 0) p = hi + 1;
                        }                       
                        else
                        {
                            p = 0;
                        }

                        if (max >= 0)
                        {
                            for (int g = p; g <= max; g++)
                            {
                                revLookup[lookup[g]]++;
                            }
                        }

                        ArrOp(ArrayOperation.Insert, 
                            ref innerKeys, 
                            newIndex: p, 
                            expanded: expanded, 
                            virtSize: max + 1);

                        ArrOp(ArrayOperation.Insert,
                            ref lookup,
                            newIndex: p,
                            expanded: expanded,
                            virtSize: max + 1);

                        index = p;
                        return -1;
                    }
                    break;
                }

                p = ((hi + lo) / 2);


                TKey elem = innerKeys[p];
                int c;

                c = def(value, elem);

                if (c == 0)
                {
                    index = p;
                    return lookup[p];
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

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new KeyValueEnumerator(this);
        }

        public void Add(TKey key, TValue value)
        {
            TKey kchk = (TKey)keyProp.GetValue(value);
            if (!Equals(kchk, key))
                throw new InvalidOperationException("Key must match key property of object.");

            this.Add(value);
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key) => RemoveKey(key);

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        void ICollection<KeyValuePair<TKey, TValue>>.Clear() => Clear();

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            int x = arrayIndex;

            foreach(KeyValuePair<TKey, TValue> item in (IDictionary<TKey, TValue>) this)
            {
                array[x] = item;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            TKey kchk = (TKey)keyProp.GetValue(item.Value);
            if (!Equals(kchk, item.Key))
                throw new InvalidOperationException("Key must match key property of object.");

            return Remove(item.Value);
        }

        private class KeyedCollectionEnumerator : IEnumerator<TValue>
        {
            private int idx = -1;
            private IList<TValue> objs;

            public KeyedCollectionEnumerator(ObservableDictionary<TKey, TValue> list)
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

        private class KeyValueEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private int idx = -1;
            private ObservableDictionary<TKey, TValue> objs;

            public KeyValueEnumerator(ObservableDictionary<TKey, TValue> list)
            {
                objs = list;               
            }

            public KeyValuePair<TKey, TValue> Current => new KeyValuePair<TKey, TValue>(objs.innerKeys[objs.revLookup[idx]], objs.innerList[idx]);

            object IEnumerator.Current => new KeyValuePair<TKey, TValue>(objs.innerKeys[objs.revLookup[idx]], objs.innerList[idx]);

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
