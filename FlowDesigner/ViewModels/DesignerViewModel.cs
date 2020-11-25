using Aptacode.CSharp.Common.Utilities.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class DesignerViewModel : BindableBase
    {
        public static int ScaleFactor = 10;

        private List<ItemViewModel> _items = new List<ItemViewModel>();
        public IEnumerable<ItemViewModel> Items => _items.OrderBy(i => i.Z);

        private List<ConnectionViewModel> _connections = new List<ConnectionViewModel>();
        public IEnumerable<ConnectionViewModel> Connections => _connections;

        public DesignerViewModel()
        {
            Width = 100;
            Height = 100;
        }

        public int Width { get; set; }
        public int Height { get; set; }

        #region Commands

        private DelegateCommand<string> _createItem;
        public DelegateCommand<string> CreateItem => _createItem ??= new DelegateCommand<string>((itemName) =>
        {
            var newItem = new ItemViewModel(Guid.NewGuid(), itemName, 10,10,10,5);
            _items.Add(newItem);
            base.OnPropertyChanged(nameof(Connections));
        });

        public void BringToFront(ItemViewModel item)
        {
            item.Z = Items.Select(i => i.Z).Max() + 1;
        }
        public void SendToBack(ItemViewModel item)
        {
            var min = Items.Select(i => i.Z).Min();
            if(min <= 1)
            {
                foreach(var i in Items)
                {
                    i.Z++;
                }
            }

            item.Z = min - 1;
        }

        private DelegateCommand<ItemViewModel> _deleteItem;
        public DelegateCommand<ItemViewModel> DeleteItem => _deleteItem ??= new DelegateCommand<ItemViewModel>((item) =>
        {
            foreach(var connection in _connections.Where(c => c.IsConnectedTo(item)).ToList())
            {
                _connections.Remove(connection);
            }

            _items.Remove(item);
            base.OnPropertyChanged(nameof(Items));
        });

        private DelegateCommand<(ItemViewModel item, int x, int y)> _moveItem;
        public DelegateCommand<(ItemViewModel item, int x, int y)> MoveItem => _moveItem ??= new DelegateCommand<(ItemViewModel item, int x, int y)>((tuple) =>
        {
            tuple.item.SetPosition(tuple.x, tuple.y);
            base.OnPropertyChanged(nameof(Connections));
        });

        private DelegateCommand<(ItemViewModel item, int width, int height)> _resizeItem;
        public DelegateCommand<(ItemViewModel item, int width, int height)> ResizeItem => _resizeItem ??= new DelegateCommand<(ItemViewModel item, int width, int height)>((tuple) =>
        {
            tuple.item.SetSize(tuple.width, tuple.height);
            base.OnPropertyChanged(nameof(Items));
        });

        private DelegateCommand<(ItemViewModel item1, ItemViewModel item2)> _connect;
        public DelegateCommand<(ItemViewModel item1, ItemViewModel item2)> Connect => _connect ??= new DelegateCommand<(ItemViewModel item1, ItemViewModel item2)>((tuple) =>
        {
            var newConnection = new ConnectionViewModel(Guid.NewGuid(),"New Connection", this, tuple.item1, tuple.item2);
            _connections.Add(newConnection);
            base.OnPropertyChanged(nameof(Connections));
        });

        #endregion

    }
}
