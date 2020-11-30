using System;
using System.Collections.Generic;
using System.Drawing;
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

        public void RedrawConnections()
        {
            foreach (var connection in _connections)
            {
                connection.Redraw();
            }
        }

        #region Events

        public event EventHandler<ItemViewModel> SelectedItemChanged;
        public event EventHandler<ConnectionViewModel> SelectedConnectionChanged;

        #endregion

        #region Commands

        private DelegateCommand<(string, Vector2, Vector2)> _createItem;

        public DelegateCommand<(string, Vector2, Vector2)> CreateItem => _createItem ??=
            new DelegateCommand<(string, Vector2, Vector2)>(details =>
            {
                var (name, position, size) = details;
                var newItem = new ItemViewModel(Guid.NewGuid(), name, position, size);
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

        #region Mouse

        private ItemViewModel? _selectedItem;

        public ItemViewModel? SelectedItem
        {
            get { return _selectedItem; }
            set {

                var prevSelectedItem = _selectedItem;

                SetProperty(ref _selectedItem, value);

                //Highlight Item
                if(prevSelectedItem != null)
                {
                    prevSelectedItem.BorderColor = Color.Black;
                }
                if (_selectedItem != null)
                {
                    _selectedItem.BorderColor = Color.Green;
                }

                SelectedItemChanged?.Invoke(this, _selectedItem);
            }
        }

        private ConnectionViewModel? _selectedConnection;

        public ConnectionViewModel? SelectedConnection
        {
            get { return _selectedConnection; }
            set
            {

                var prevSelectedItem = _selectedConnection;

                SetProperty(ref _selectedConnection, value);

                //Highlight Connection
                if (prevSelectedItem != null)
                {
                    prevSelectedItem.BorderColor = Color.Black;
                }
                if (_selectedConnection != null)
                {
                    _selectedConnection.BorderColor = Color.Green;
                }

                SelectedConnectionChanged?.Invoke(this, _selectedConnection);
            }
        }


        private ConnectedItem? _connectedItem { get; set; }

        private Vector2 MouseDelta { get; set; }

        private bool _movingItem;
        private bool _resizingItem;


        public void MouseDown(Vector2 position)
        {
            if (SelectedConnection == null)
            {
                ClickConnection(position);
            }

            if (SelectedItem == null && SelectedConnection == null)
            {
                ClickItem(position);
            }
        }

        public void MouseMove(Vector2 position)
        {
            if (SelectedItem != null)
            {
                MoveItem(position);
            }

            if (SelectedConnection != null)
            {
                MoveConnection(position);
            }
        }

        public void MouseUp(Vector2 position)
        {
            if (SelectedItem != null)
            {
                ReleaseItem(position);
            }

            if (SelectedConnection != null)
            {
                ReleaseConnection(position);
            }
        }

        private Vector2 _lastDrawPoint = Vector2.Zero;

        private void ClickItem(Vector2 position)
        {
            foreach (var item in Items)
            {
                if (!item.CollidesWith(position))
                {
                    continue;
                }

                SelectedItem = item;
                break;
            }

            if (SelectedItem == null)
            {
                return;
            }

            BringToFront(SelectedItem);

            if (SelectedItem.CollidesWithEdge(position))
            {
                MouseDelta = position - (SelectedItem.Position + SelectedItem.Size);

                _resizingItem = true;
            }
            else
            {
                MouseDelta = position - SelectedItem.Position;
                _movingItem = true;
            }
        }

        private void MoveItem(Vector2 position)
        {
            var newPosition = position - MouseDelta;

            if (_movingItem)
            {
                SelectedItem.Position = newPosition;

                if ((newPosition - _lastDrawPoint).Length() <= 1.0f)
                {
                    return;
                }
                _lastDrawPoint = newPosition;

                foreach (var connection in Connections)
                {
                    if(connection.Item1.Item == SelectedItem || connection.Item2.Item == SelectedItem)
                    {
                        connection.Redraw();
                        continue;
                    }
                    if (connection.ConnectionPath.Any(p => SelectedItem.CollidesWith(p)))
                    {
                        connection.Redraw();
                        continue;
                    }
                }
            }
            else if (_resizingItem)
            {
                SelectedItem.Size = newPosition - SelectedItem.Position;
            }
        }

        private void ReleaseItem(Vector2 position)
        {
            var newPosition = position - MouseDelta;

            if (_movingItem)
            {
                SelectedItem.Position = newPosition;
            }
            else if (_resizingItem)
            {
                SelectedItem.Size = newPosition - SelectedItem.Position;
            }

            foreach (var connection in Connections)
            {
                connection.Redraw();
            }

            SelectedItem = null;
            _movingItem = false;
            _resizingItem = false;
        }

        private void ClickConnection(Vector2 position)
        {
            foreach (var connection in Connections)
            {
                if (connection.Item1.CollidesWith(position))
                {
                    _connectedItem = connection.Item1;
                    SelectedConnection = connection;
                    break;
                }

                if (connection.Item2.CollidesWith(position))
                {
                    _connectedItem = connection.Item2;
                    SelectedConnection = connection;
                    break;
                }
            }
        }

        private void MoveConnection(Vector2 position)
        {
            if (SelectedConnection == null)
            {
                return;
            }

            if (_connectedItem.Item.CollidesWith(position))
            {
                return;
            }

            _connectedItem.UpdateAnchorPointDelta(position);

            SelectedConnection.Redraw();
        }

        private void ReleaseConnection(Vector2 position)
        {
            SelectedConnection = null;
        }

        #endregion
    }
}