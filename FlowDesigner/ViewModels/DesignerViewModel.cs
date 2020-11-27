using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Aptacode.CSharp.Common.Utilities.Mvvm;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class DesignerViewModel : BindableBase
    {
        public static int ScaleFactor = 10;

        private readonly List<ConnectionViewModel> _connections = new List<ConnectionViewModel>();

        private readonly List<ItemViewModel> _items = new List<ItemViewModel>();

        public DesignerViewModel()
        {
            Width = 100;
            Height = 100;
        }

        public IEnumerable<ItemViewModel> Items => _items.OrderBy(i => i.Z);
        public IEnumerable<ConnectionViewModel> Connections => _connections;

        public int Width { get; set; }
        public int Height { get; set; }

        #region Commands

        private DelegateCommand<(string, Vector2, Vector2)> _createItem;

        public DelegateCommand<(string, Vector2, Vector2)> CreateItem => _createItem ??=
            new DelegateCommand<(string, Vector2, Vector2)>(details =>
            {
                var newItem = new ItemViewModel(Guid.NewGuid(), details.Item1, details.Item2, details.Item3);
                _items.Add(newItem);
                OnPropertyChanged(nameof(Connections));
            });

        public void BringToFront(ItemViewModel item)
        {
            item.Z = Items.Max(i => i.Z) + 1;
        }

        public void SendToBack(ItemViewModel item)
        {
            var min = Items.Min(i => i.Z);
            if (min <= 1)
            {
                foreach (var i in Items)
                {
                    i.Z++;
                }
            }

            item.Z = min - 1;
        }

        private DelegateCommand<ItemViewModel> _deleteItem;

        public DelegateCommand<ItemViewModel> DeleteItem => _deleteItem ??= new DelegateCommand<ItemViewModel>(item =>
        {
            foreach (var connection in _connections.Where(c => c.IsConnectedTo(item)).ToList())
            {
                _connections.Remove(connection);
            }

            _items.Remove(item);
            OnPropertyChanged(nameof(Items));
        });

        private DelegateCommand<(ItemViewModel item1, ItemViewModel item2)> _connect;

        public DelegateCommand<(ItemViewModel item1, ItemViewModel item2)> Connect => _connect ??=
            new DelegateCommand<(ItemViewModel item1, ItemViewModel item2)>(tuple =>
            {
                var newConnection =
                    new ConnectionViewModel(Guid.NewGuid(), "New Connection", this, tuple.item1, tuple.item2);
                _connections.Add(newConnection);
                OnPropertyChanged(nameof(Connections));
            });

        #endregion
    }
}