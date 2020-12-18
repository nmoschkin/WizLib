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
using System.Threading;
using Newtonsoft.Json;

namespace WiZ.Observable
{
    /// <summary>
    /// Class or property decorator to specify the key property.
    /// </summary>
    public class KeyPropertyAttribute : Attribute
    {
        #region Public Constructors

        public KeyPropertyAttribute([CallerMemberName] string propertyName = null)
        {
            PropertyName = propertyName;
        }

        #endregion Public Constructors

        #region Public Properties

        public string PropertyName { get; private set; }

        #endregion Public Properties
    }

    /// <summary>
    /// Sortable, keyed observable dictionary for classes.
    /// </summary>
    /// <typeparam name="TKey">The key type.  <see cref="TKey"/> must implement <see cref="IComparable{TKey}"/> or a suitable comparison must be provided.</typeparam>
    /// <typeparam name="TValue">The value type.  <see cref="TValue"/> must contain a property of type <see cref="TKey"/>.</typeparam>
    public class ObservableDictionary<TKey, TValue> : ObservableBase, IDictionary<TKey, TValue>, IList<TValue>, INotifyCollectionChanged where TValue : class
    {
        #region Private Fields

        private int _autoBuffer = 10;

        private int _capacity = 0;

        private TKey[] _Keys;

        private int _size = 0;

        private TValue[] _Values;

        private bool dynamic = true;

        private int[] idxToKey;

        private bool isObs;

        private Comparison<TKey> keycomp;

        private PropertyInfo keyProp;

        private string keyPropName;

        private int[] keyToIdx;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Create a new <see cref="ObservableDictionary{TKey, TValue}"/>
        /// </summary>
        public ObservableDictionary() : this(null, (Comparison<TKey>)null)
        {
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
        /// <param name="items">An <see cref="IReadOnlyDictionary{TKey, TValue}"/> of items used to initialize the collection.</param>
        public ObservableDictionary(IReadOnlyDictionary<TKey, TValue> items) : this(null, (Comparison<TKey>)null, items)
        {
        }

        /// <summary>
        /// Create a new <see cref="ObservableDictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="items">An <see cref="IEnumerable{TValue}"/> of items used to initialize the collection.</param>
        public ObservableDictionary(IEnumerable<TValue> items) : this(null, (Comparison<TKey>)null, items)
        {
        }

        /// <summary>
        /// Create a new <see cref="ObservableDictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="propertyName">The name of the property in the class object to use as the key.</param>
        /// <param name="items">An <see cref="IReadOnlyDictionary{TKey, TValue}"/> of items used to initialize the collection.</param>
        public ObservableDictionary(string propertyName, IReadOnlyDictionary<TKey, TValue> items) : this(propertyName, (Comparison<TKey>)null, items)
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

        /// <summary>
        /// Create a new <see cref="ObservableDictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="keyComparer">The <see cref="IComparer{TKey}"/> to use to sort the keys.</param>
        /// <param name="items">An <see cref="IReadOnlyDictionary{TKey, TValue}"/> of items used to initialize the collection.</param>
        public ObservableDictionary(IComparer<TKey> keyComparer, IReadOnlyDictionary<TKey, TValue> items) : this(null, new Comparison<TKey>(keyComparer.Compare), items)
        {
        }

        /// <summary>
        /// Create a new <see cref="ObservableDictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="keyComparer">The <see cref="IComparer{TKey}"/> to use to sort the keys.</param>
        /// <param name="items">An <see cref="IEnumerable{TValue}"/> of items used to initialize the collection.</param>
        public ObservableDictionary(IComparer<TKey> keyComparer, IEnumerable<TValue> items) : this(null, new Comparison<TKey>(keyComparer.Compare), items)
        {
        }

        /// <summary>
        /// Create a new <see cref="ObservableDictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="keyComparison">The <see cref="Comparison{TKey}"/> to use to sort the keys.</param>
        /// <param name="items">An <see cref="IReadOnlyDictionary{TKey, TValue}"/> of items used to initialize the collection.</param>
        public ObservableDictionary(Comparison<TKey> keyComparison, IReadOnlyDictionary<TKey, TValue> items) : this(null, keyComparison, items)
        {
        }

        /// <summary>
        /// Create a new <see cref="ObservableDictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="keyComparison">The <see cref="Comparison{TKey}"/> to use to sort the keys.</param>
        /// <param name="items">An <see cref="IEnumerable{TValue}"/> of items used to initialize the collection.</param>
        public ObservableDictionary(Comparison<TKey> keyComparison, IEnumerable<TValue> items) : this(null, keyComparison, items)
        {
        }

        /// <summary>
        /// Create a new <see cref="ObservableDictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="propertyName">The name of the property in the class object to use as the key.</param>
        /// <param name="keyComparer">The <see cref="IComparer{TKey}"/> to use to sort the keys.</param>
        /// <param name="items">An <see cref="IReadOnlyDictionary{TKey, TValue}"/> of items used to initialize the collection.</param>
        public ObservableDictionary(string propertyName, IComparer<TKey> keyComparer, IReadOnlyDictionary<TKey, TValue> items) : this(propertyName, new Comparison<TKey>(keyComparer.Compare), items)
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
        /// <param name="keyComparison">The <see cref="Comparison{TKey}"/> to use to sort the keys.</param>
        /// <param name="items">An <see cref="IReadOnlyDictionary{TKey, TValue}"/> of items used to initialize the collection.</param>
        public ObservableDictionary(string propertyName, Comparison<TKey> keyComparison, IReadOnlyDictionary<TKey, TValue> items) : this(propertyName, keyComparison)
        {
            AddRange(items);
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
        /// <param name="keyComparison">The <see cref="Comparison{TKey}"/> to use to sort the keys.</param>
        public ObservableDictionary(string propertyName, Comparison<TKey> keyComparison)
        {
            if (keyComparison != null)
            {
                keycomp = keyComparison;
            }
            else
            {
                if (!typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)))
                {
                    foreach (var c in Comparers)
                    {
                        if (typeof(IComparer<TKey>).IsAssignableFrom(c))
                        {
                            var tc = (IComparer<TKey>)Assembly.GetExecutingAssembly().CreateInstance(c.FullName);

                            keycomp = new Comparison<TKey>(tc.Compare);
                            break;
                        }
                    }

                    if (keycomp == null)
                    {
                        throw new NotSupportedException("No compatible comparer found for type '" + typeof(TKey).FullName + "'.");
                    }
                }
            }

            Type valType = typeof(TValue);

            if (propertyName != null)
            {
                keyProp = valType.GetProperty(propertyName);
            }
            else
            {
                Attribute attr;

                attr = valType.GetCustomAttribute(typeof(KeyPropertyAttribute));

                if (attr is KeyPropertyAttribute kpa && !string.IsNullOrEmpty(kpa.PropertyName))
                {
                    keyProp = valType.GetProperty(kpa.PropertyName);
                }

                if (keyProp == null)
                {
                    var props = valType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    foreach (var prop in props)
                    {
                        attr = prop.GetCustomAttribute(typeof(KeyPropertyAttribute));
                        if (attr != null)
                        {
                            keyProp = prop;
                            break;
                        }
                    }
                }
            }

            if (keyProp == null)
            {
                if (propertyName == null)
                {
                    throw new ArgumentException(nameof(propertyName), $"No key property specified or found in '{typeof(TValue).FullName}'.");
                }
                else
                {
                    throw new ArgumentException(nameof(propertyName), $"Property '{propertyName}' does not exist in '{typeof(TValue).FullName}'.");
                }
            }

            if (keyProp.PropertyType != typeof(TKey))
            {
                throw new ArgumentException(nameof(propertyName), $"Property '{propertyName}' is not of type '{typeof(TKey).FullName}'.");
            }


            if (typeof(INotifyPropertyChanged).IsAssignableFrom(valType) && keyProp.CanWrite)
            {

                // we can observe a key that could change.
                isObs = true;
            }


            keyPropName = keyProp.Name;
            EnsureCapacity(_autoBuffer);
        }

        #endregion Public Constructors

        #region Public Events

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion Public Events

        #region Private Enums

        private enum ArrayOperation
        {
            Remove,
            Insert,
            Move
        }

        #endregion Private Enums

        #region Public Properties

        /// <summary>
        /// List of registered <see cref="Comparer{TKey}"/> instances.
        /// </summary>
        public static List<Type> Comparers { get; set; } = new List<Type>();

        /// <summary>
        /// Get or set the number of elements to expand the buffer when capacity is reached.
        /// </summary>
        /// <remarks>
        /// Set to 0 to disable.
        /// </remarks>
        [JsonIgnore]
        public int AutoBuffer
        {
            get => _autoBuffer;
            set
            {
                int oldVal = _autoBuffer;

                if (SetProperty(ref _autoBuffer, value) && _autoBuffer > oldVal)
                {
                    EnsureCapacity(_capacity + _autoBuffer);
                }
            }
        }

        /// <summary>
        /// Gets or sets the capacity of the collection buffer.
        /// </summary>
        [JsonIgnore]
        public int Capacity
        {
            get => _capacity;
            set
            {
                EnsureCapacity(value); ;
            }
        }

        [JsonIgnore]
        public int Count => _size;
        int ICollection<KeyValuePair<TKey, TValue>>.Count => _size;

        /// <summary>
        /// Gets or sets a value indicating that items can be dynamically added via indexer reference if they are not already present.
        /// </summary>
        [JsonIgnore]
        public bool Dynamic
        {
            get => dynamic;
            set
            {
                SetProperty(ref dynamic, value);
            }
        }

        public bool IsReadOnly => false;
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

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

        public ICollection<TKey> Keys
        {
            get
            {
                var x = new TKey[_size];
                if (_size == 0) return x;

                Array.ConstrainedCopy(_Keys, 0, x, 0, _size);
                return x;
            }
        }

        public ICollection<TValue> Values
        {
            get => ToArray();
        }

        #endregion Public Properties

        #region Public Indexers

        TValue IList<TValue>.this[int index]
        {
            get => _Values[index];
            set
            {
                var item = _Values[index];
                _Values[index] = value;

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
            get
            {
                int i = Search(key);

                if (i >= 0)
                {
                    return _Values[i];
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
                    var item = _Values[i];

                    if (item.Equals(value)) return;

                    _Values[i] = value;

                    if (CollectionChanged != null)
                    {
                        var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, item);
                        CollectionChanged.Invoke(this, e);
                    }
                }
                else
                {
                    if (dynamic)
                        Add(key, value);
                    else
                        throw new KeyNotFoundException(key.ToString());
                }
            }
        }

        #endregion Public Indexers

        #region Public Methods

        public void Add(TValue item)
        {
            Add(item, false);
        }

        /// <summary>
        /// Add a range of items to the collection.
        /// </summary>
        /// <param name="items">The list of items to add.</param>
        public void AddRange(IEnumerable<TValue> items) => AddRange(items, false);

        public void AddRange(IReadOnlyDictionary<TKey, TValue> items)
        {
            foreach (var item in items)
            {
                TKey kv = (TKey)keyProp.GetValue(item.Value);

                if (!kv.Equals(item.Key))
                {
                    throw new InvalidOperationException($"Key must match value of '{keyPropName}' property.");
                }
            }

            AddRange(items.Values);
        }

        public void Clear()
        {
            if (_Values == null) return;

            if (isObs)
            {
                int c = _capacity = _Values?.Length ?? 0;
                for (int i = 0; i < c; i++)
                {
                    if (_Values[i] != null && _Values[i] is INotifyPropertyChanged v)
                    {
                        v.PropertyChanged -= Value_PropertyChanged;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            Array.Clear(_Values, 0, _Values?.Length ?? 0);
            Array.Clear(_Keys, 0, _Keys?.Length ?? 0);
            Array.Clear(keyToIdx, 0, keyToIdx?.Length ?? 0);
            Array.Clear(idxToKey, 0, idxToKey?.Length ?? 0);

            _Values = null;
            _Keys = null;
            keyToIdx = null;
            idxToKey = null;

            _size = 0;

            if (_autoBuffer != 0)
                EnsureCapacity(_autoBuffer);

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
                item = _Values[i];
                return true;
            }
            else
            {
                item = null;
                return false;
            }
        }
        public void CopyTo(TValue[] array, int arrayIndex)
        {
            _Values.CopyTo(array, arrayIndex);
        }

        public int IndexOf(TValue item)
        {
            int i = 0;
            foreach (var t in _Values)
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

        /// <summary>
        /// Move an item in the collection from one index to another.
        /// </summary>
        /// <param name="oldIndex">The source item index.</param>
        /// <param name="newIndex">The destination item index.</param>
        public void Move(int oldIndex, int newIndex)
        {
            var item = _Values[oldIndex];

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
                if (isObs && _Values[i] is INotifyPropertyChanged p)
                {
                    p.PropertyChanged -= Value_PropertyChanged;
                }

                RemoveAt(i);
                OnPropertyChanged(nameof(Count));
            }

            return ret;
        }

        public void RemoveAt(int index)

        {
            RemoveAt(index, false);
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

        /// <summary>
        /// Sorts a collection where <see cref="TValue"/> implements <see cref="IComparable{TValue}"/>.
        /// </summary>
        public void Sort()
        {
            if (typeof(IComparable<TValue>).IsAssignableFrom(typeof(TValue)))
            {
                var comp = new Comparison<TValue>((a, b) =>
                {

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

        public TValue[] ToArray()
        {
            var x = new TValue[_size];
            if (_size == 0) return x;

            Array.ConstrainedCopy(_Values, 0, x, 0, _size);
            return x;
        }
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
                value = _Values[i];
                return true;
            }
        }

        #endregion Public Methods

        #region Internal Methods

        internal bool EnsureCapacity(int size)
        {
            int c = size;

            if (size <= _size) return false;

            if (_size == 0 || keyToIdx == null)
            {
                _Values = new TValue[c];
                _Keys = new TKey[c];
                keyToIdx = new int[c];
                idxToKey = new int[c];
            }
            else
            {
                Array.Resize(ref _Values, c);
                Array.Resize(ref _Keys, c);
                Array.Resize(ref keyToIdx, c);
                Array.Resize(ref idxToKey, c);
            }

            _capacity = c;
            OnPropertyChanged(nameof(Capacity));

            return true;
        }

        #endregion Internal Methods

        #region Private Methods

        private void Add(TValue item, bool suppressEvent)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            int x = _size;

            TKey key = (TKey)keyProp.GetValue(item);

            if (ContainsKey(key))
                throw new ArgumentException($"Collection already contains key '{key}'.", nameof(item));

            Array.Resize(ref _Values, x + 1);
            Array.Resize(ref idxToKey, x + 1);

            int idx;
            Search(key, out idx, true);

            keyToIdx[idx] = x;
            _Keys[idx] = key;

            _Values[x] = item;
            idxToKey[x] = idx;

            if (isObs && item is INotifyPropertyChanged p)
            {
                p.PropertyChanged += Value_PropertyChanged;
            }

            _size++;

            // KeySort();

            if (!suppressEvent && CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, x);
                CollectionChanged.Invoke(this, e);
                OnPropertyChanged(nameof(Count));
            }

        }

        private void AddRange(IEnumerable<TValue> items, bool suppressEvent)
        {
            int c = items.Count();
            int x = _size;

            var ns = x + c;
            var zp = ns + (_autoBuffer - ns % _autoBuffer);

            EnsureCapacity(zp);

            foreach (var item in items)
            {
                if (item == null) throw new ArgumentNullException(nameof(item));

                TKey key = (TKey)keyProp.GetValue(item);

                if (ContainsKey(key))
                {
                    TValue tv;
                    var tk = ContainsKey(key, out tv);

                    throw new ArgumentException($"Collection already contains key '{key}'.", nameof(item));
                }

                int idx;
                Search(key, out idx, true);

                keyToIdx[idx] = x;
                _Keys[idx] = key;

                idxToKey[x] = idx;
                _Values[x] = item;

                if (isObs && item is INotifyPropertyChanged p)
                {
                    p.PropertyChanged += Value_PropertyChanged;
                }

                x++;
                _size++;
            }

            //KeySort();

            if (!suppressEvent && CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged.Invoke(this, e);
                OnPropertyChanged(nameof(Count));
            }
        }

        private void Insert(int index, TValue item, bool suppressEvent)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            TKey key = (TKey)keyProp.GetValue(item);

            if (ContainsKey(key))
                throw new ArgumentException($"Collection already contains key '{key}'.", nameof(item));


            for (int g = index; g < _size; g++)
            {
                keyToIdx[idxToKey[g]]++;
            }

            ArrOp(ArrayOperation.Insert, ref _Values, newIndex: index);
            ArrOp(ArrayOperation.Insert, ref idxToKey, newIndex: index);

            int idx;
            Search(key, out idx, true);

            keyToIdx[idx] = index;
            _Keys[idx] = key;

            _Values[index] = item;
            idxToKey[index] = idx;

            _size++;

            if (!suppressEvent && CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index);
                CollectionChanged.Invoke(this, e);
                OnPropertyChanged(nameof(Count));
            }
        }
        private void RemoveAt(int index, bool suppressEvent)
        {
            var item = _Values[index];
            var idx = idxToKey[index];

            ArrOp(ArrayOperation.Remove, ref _Values, oldIndex: index);
            ArrOp(ArrayOperation.Remove, ref idxToKey, oldIndex: index);
            ArrOp(ArrayOperation.Remove, ref _Keys, oldIndex: idx);
            ArrOp(ArrayOperation.Remove, ref keyToIdx, oldIndex: idx);

            --_size;

            for (int g = 0; g < _size; g++)
            {
                if (keyToIdx[g] >= index)
                {
                    keyToIdx[g]--;
                }
                if (idxToKey[g] >= idx)
                {
                    idxToKey[g]--;
                }
            }
            
            if (!suppressEvent && CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);
                CollectionChanged.Invoke(this, e);
                OnPropertyChanged(nameof(Count));
            }
        }
        private void Value_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == KeyPropertyName)
            {
                var key = (TKey)keyProp.GetValue(sender);
                int idx = Search(key);

                if (idx != -1)
                {
                    throw new InvalidOperationException($"Key property changed to a key that already exists.\r\nItem #: {idx}, Key: '{key}'.");
                }
                else
                {
                    int i = IndexOf((TValue)sender);

                    _Keys[idxToKey[i]] = key;

                    Sort(null, 0, _size - 1, true);
                }
            }
        }

        #endregion Private Methods

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
                    Array.ConstrainedCopy(arr, 0, a2, 0, oldIndex);
                }

                if (oldIndex < c - 1)
                {
                    Array.ConstrainedCopy(arr, oldIndex + 1, a2, oldIndex, d - oldIndex);
                }

                arr = a2;
                return;
            }

            if (newIndex < 0 || newIndex > (arr?.Length ?? 0))
                throw new ArgumentOutOfRangeException(nameof(newIndex));

            if (mode == ArrayOperation.Insert) // insert 
            {
                ++c;
                if (!expanded)
                {
                    a2 = new U[c];

                    if (newIndex > 0)
                    {
                        Array.ConstrainedCopy(arr, 0, a2, 0, newIndex);
                    }
                }
                else
                {
                    a2 = arr;
                }

                if (newIndex < c - 1)
                {
                    Array.ConstrainedCopy(arr, newIndex, a2, newIndex + 1, d - newIndex);
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

                    Array.ConstrainedCopy(arr, oldIndex + 1, a2, 0, i);
                    Array.ConstrainedCopy(a2, 0, arr, oldIndex, i);
                }
                else
                {
                    i = oldIndex - newIndex;
                    a2 = new U[i];

                    Array.ConstrainedCopy(arr, newIndex, a2, 0, i);
                    Array.ConstrainedCopy(a2, 0, arr, newIndex + 1, i);
                }

                arr[newIndex] = elem;
            }

        }


        #endregion

        #region QuickSort
        private int Partition(Comparison<TValue> comparison, int lo, int hi)
        {
            var ppt = (hi + lo) / 2;
            var pivot = _Values[ppt];

            int i = lo - 1;
            int j = hi + 1;

            while (true)
            {
                try
                {
                    do
                    {
                        ++i;
                    } while (i <= hi && comparison(_Values[i], pivot) < 0);
                    do
                    {
                        --j;
                    } while (j >= 0 && comparison(_Values[j], pivot) > 0);

                    if (i >= j) return j;

                    TValue sw = _Values[i];
                    _Values[i] = _Values[j];
                    _Values[j] = sw;

                    int si = idxToKey[i];

                    idxToKey[i] = idxToKey[j];
                    idxToKey[j] = si;

                    keyToIdx[idxToKey[i]] = i;
                    keyToIdx[idxToKey[j]] = j;
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

            TKey kpivot = _Keys[ppt];

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
                } while (i <= hi && def(_Keys[i], kpivot) < 0);
                do
                {
                    --j;
                } while (j >= 0 && def(_Keys[j], kpivot) > 0);

                if (i >= j) return j;

                TKey sw = _Keys[i];

                _Keys[i] = _Keys[j];
                _Keys[j] = sw;

                int si = keyToIdx[i];

                keyToIdx[i] = keyToIdx[j];
                keyToIdx[j] = si;

                idxToKey[keyToIdx[i]] = i;
                idxToKey[keyToIdx[j]] = j;
            }
        }

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
        #endregion

        #region Binary Search 

        private int GetInsertIndex(int inLo, int inHi, TKey value, Comparison<TKey> comp, int max)
        {
            if (max < 0) return 0;

            int lo = inLo <= inHi ? inLo : inHi;

            if (lo < 2)
                lo = 0;
            else
                --lo;

            int hi = inLo >= inHi ? inLo : inHi;

            if (hi > max - 2)
                hi = max;
            else
                hi++;

            int i, test;
            int lt = -1;

            for (i = lo; i <= hi; i++)
            {
                test = comp(value, _Keys[i]);

                if (test <= 0)
                {
                    return i;
                }
                else
                {
                    lt = i;
                }
            }

            if (lt != -1)
                return lt + 1;
            else
                return i;
        }

        private int Search(TKey value)
        {
            return Search(value, out _, false);
        }

        private int Search(TKey value, out int keyIdx, bool insert) 
        {
            int max = _size - 1;
            int lo = 0, hi = max;

            Comparison<TKey> def = keycomp;

            if (def == null)
            {
                def = new Comparison<TKey>((a, b) =>
                {

                    if (a == null && b == null) return 0;

                    if (a == null && b != null) return -1;

                    if (a != null && b == null) return 1;

                    return ((IComparable<TKey>)a).CompareTo(b);


                });

                KeyComparison = def;
            }

            while (true)
            {
                int p;

                if (lo > hi)
                {
                    if (insert)
                    {
                        if (_capacity <= _size && _autoBuffer > 0)
                        {
                            EnsureCapacity(_size + _autoBuffer);
                        }

                        bool expanded = _capacity > _size;

                        p = GetInsertIndex(lo, hi, value, def, max);

                        ArrOp(ArrayOperation.Insert,
                            ref _Keys,
                            newIndex: p,
                            expanded: expanded,
                            virtSize: max + 1);

                        ArrOp(ArrayOperation.Insert,
                            ref keyToIdx,
                            newIndex: p,
                            expanded: expanded,
                            virtSize: max + 1);

                        if (max >= 0)
                        {
                            for (int g = 0; g <= max + 1; g++)
                            {
                                if (idxToKey[g] >= p)
                                {
                                    idxToKey[g]++;
                                }
                            }
                        }

                        keyIdx = p;
                        return -1;
                    }
                    break;
                }

                p = (hi + lo) / 2;


                TKey elem = _Keys[p];
                int c;

                c = def(value, elem);

                if (c == 0)
                {
                    keyIdx = p;
                    return keyToIdx[p];
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

            keyIdx = -1;
            return -1;
        }
        #endregion

        #region IEnumerable

        public void Add(TKey key, TValue value)
        {
            TKey kchk = (TKey)keyProp.GetValue(value);
            if (!Equals(kchk, key))
                throw new InvalidOperationException($"Key must match value of '{keyPropName}' property.");

            Add(value);
        }

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        void ICollection<KeyValuePair<TKey, TValue>>.Clear() => Clear();

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            int x = arrayIndex;

            foreach (KeyValuePair<TKey, TValue> item in (IDictionary<TKey, TValue>)this)
            {
                array[x] = item;
            }
        }

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
        bool IDictionary<TKey, TValue>.Remove(TKey key) => RemoveKey(key);
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            TKey kchk = (TKey)keyProp.GetValue(item.Value);
            if (!Equals(kchk, item.Key))
                throw new InvalidOperationException($"Key must match value of '{keyPropName}' property.");

            return Remove(item.Value);
        }

        private class KeyedCollectionEnumerator : IEnumerator<TValue>
        {
            #region Private Fields

            private int idx = -1;
            private IList<TValue> objs;

            #endregion Private Fields

            #region Public Constructors

            public KeyedCollectionEnumerator(ObservableDictionary<TKey, TValue> list)
            {
                objs = list;
            }

            #endregion Public Constructors

            #region Public Properties

            public TValue Current => objs[idx];

            object IEnumerator.Current => objs[idx];

            #endregion Public Properties

            #region Public Methods

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

            #endregion Public Methods
        }

        private class KeyValueEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            #region Private Fields

            private int idx = -1;
            private ObservableDictionary<TKey, TValue> objs;

            #endregion Private Fields

            #region Public Constructors

            public KeyValueEnumerator(ObservableDictionary<TKey, TValue> list)
            {
                objs = list;
            }

            #endregion Public Constructors

            #region Public Properties

            public KeyValuePair<TKey, TValue> Current => new KeyValuePair<TKey, TValue>(objs._Keys[objs.idxToKey[idx]], objs._Values[idx]);

            object IEnumerator.Current => new KeyValuePair<TKey, TValue>(objs._Keys[objs.idxToKey[idx]], objs._Values[idx]);

            #endregion Public Properties

            #region Public Methods

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

            #endregion Public Methods
        }

        #endregion

    }

}
