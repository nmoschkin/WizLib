using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WiZ.Observable
{

    /// <summary>
    /// Abstract base class for observable classes.
    /// </summary>
    public abstract class ObservableBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Notifies a listener that a property has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fire the <see cref="PropertyChanged"/> event with the specified <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Compare <paramref name="backingStore"/> to <paramref name="value"/>, and if not equal set <paramref name="backingStore"/> to the specified <paramref name="value"/> and fire the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="backingStore">Reference to variable that stores a property value.</param>
        /// <param name="value">New value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>True if the property was changed and the <see cref="PropertyChanged"/> event was fired.</returns>
        protected virtual bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(backingStore, value))
            {
                backingStore = value;
                OnPropertyChanged(propertyName);

                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
