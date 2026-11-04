using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace System.Collections.Generic {
    public class ItemListViewModel<T> : INotifyPropertyChanged {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string info) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        #endregion

        public ItemListViewModel() {
            Items = new ObservableCollection<T>();
        }

        #region nonModifiedCode

        private ObservableCollection<T> _collection;
        public ObservableCollection<T> Items {
            get { return _collection; }
            set {
                _collection = value;
                OnPropertyChanged("Items");
            }
        }

        #endregion

        public T SelectedItem { get; set; }

        public void SetItems(List<T> list) {
            Items = new ObservableCollection<T>(list);
        }

        public void SelectIndex(int index) {
            if (index < 0 || index >= Items.Count) return;
            SelectedItem = Items.ElementAt(index);
        }
    }

}
