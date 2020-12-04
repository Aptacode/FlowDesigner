#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Aptacode.CSharp.Common.Utilities.Extensions;
using Aptacode.CSharp.Common.Utilities.Mvvm;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public enum ResizeDirection
    {
        None, N, NE, NW, S, SE, SW, E, W
    }
    public class DesignerViewModel : BindableBase
    {

        private readonly List<ConnectionViewModel> _connections = new List<ConnectionViewModel>();

        private readonly List<ItemViewModel> _items = new List<ItemViewModel>();
        private ConnectedItem? _connectedItem { get; set; }

        private Vector2 LastMousePosition { get; set; }
        private Vector2 MouseDownPosition { get; set; }

        private bool _movingItem;
        private ResizeDirection _resizingItem;
        public ResizeDirection ResizingItem
        {
            get => _resizingItem;
            set => SetProperty(ref _resizingItem, value);
        }

        public bool MovingItem
        {
            get => _movingItem;
            set => SetProperty(ref _movingItem, value);
        }
        public string? KeyPressed;

        public DesignerViewModel(int width, int height)
        {
            Width = width;
            Height = height;
            Selection = new SelectionViewModel(Vector2.Zero, Vector2.Zero);
            ResizingItem = ResizeDirection.None;
        }
        public SelectionViewModel Selection { get; set; }

        public IEnumerable<ItemViewModel> Items => _items.OrderBy(i => i.Z);
        public IEnumerable<ConnectionViewModel> Connections => _connections.OrderBy(i => i.Z);

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

        public event EventHandler<IEnumerable<ItemViewModel>> SelectedItemChanged;
        public event EventHandler<ConnectionViewModel> SelectedConnectionChanged;

        #endregion

        #region Commands

        private DelegateCommand<(string, Vector2, Vector2)>? _createItem;

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

        public void KeyDown(string key)
        {
            KeyPressed = key;
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

        public void KeyUp(string key)
        {
            KeyPressed = null;
            ReleaseItem();
        }

        public void BringToFront(ConnectionViewModel connection)
        {
            connection.Z = _connections.Max(i => i.Z) + 1;
        }

        public void SendToBack(ConnectionViewModel connection)
        {
            var min = _connections.Min(i => i.Z);
            if (min <= 1)
            {
                foreach (var i in Items)
                {
                    i.Z++;
                }
            }

            connection.Z = min - 1;
        }

        private DelegateCommand<ItemViewModel>? _deleteItem;

        public DelegateCommand<ItemViewModel> DeleteItem => _deleteItem ??= new DelegateCommand<ItemViewModel>(item =>
        {
            foreach (var connection in _connections.Where(c => c.IsConnectedTo(item)).ToList())
            {
                _connections.Remove(connection);
            }

            _items.Remove(item);
            OnPropertyChanged(nameof(Items));
        });

        private DelegateCommand<(ItemViewModel item1, ItemViewModel item2)>? _connect;

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

        public readonly List<ItemViewModel> SelectedItems = new List<ItemViewModel>();

        public void ClearSelectedItems()
        {
            SelectedItems.Clear();
            UpdateSelectedItems();
        }

        public void UpdateSelectedItems()
        {
            //Remove item highlight
            foreach (var item in Items)
            {
                item.BorderColor = Color.Black;
            }

            //Highlight Selected Items
            foreach (var item in SelectedItems)
            {
                BringToFront(item);
                item.BorderColor = Color.Green;
            }

            //Highlight any connections for a selected item
            foreach (var connection in _connections.Where(connection => SelectedItems.Any(connection.IsConnectedTo)))
            {
                BringToFront(connection);
                connection.BorderColor = Color.Green;
            }

            SelectedItemChanged?.Invoke(this, SelectedItems);
        }

        private ConnectionViewModel? _selectedConnection;

        public ConnectionViewModel? SelectedConnection
        {
            get => _selectedConnection;
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


        public void MouseDown(Vector2 position)
        {
            MouseDownPosition = position;
            if (SelectedConnection == null)
            {
                ClickConnection(position);
            }

            if (SelectedConnection == null)
            {
                ClickItem(position);
            }
        }

        public void MouseMove(Vector2 position)
        {
            if (SelectedItems != null)
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
            if (SelectedItems != null)
            {
                ReleaseItem();
            }

            if (SelectedConnection != null)
            {
                ReleaseConnection(position);
            }
        }

        private Vector2 _lastDrawPoint = Vector2.Zero;

        private void ClickItem(Vector2 position)
        {
            var selectedItem = Items.FirstOrDefault(item => item.CollidesWith(position));

            //If no item was selected
            if(selectedItem == default && string.IsNullOrEmpty(KeyPressed))
            {
                ClearSelectedItems();
                Selection.IsShown = true;
                Selection.Position = position;
                Selection.Size = Vector2.Zero;
                return;
            }
            
            //If the border of the item was selected -> resize the item
            if (selectedItem.CollidesWithEdge(position))
            {
                ClearSelectedItems();
                SelectedItems.Add(selectedItem);

                LastMousePosition = position;
                if (LastMousePosition == selectedItem.TopLeft)
                {
                    ResizingItem = ResizeDirection.NW;
                }                
                else if (LastMousePosition == selectedItem.TopRight)
                {
                    ResizingItem = ResizeDirection.NE;
                }                
                else if (LastMousePosition == selectedItem.BottomLeft)
                {
                    ResizingItem = ResizeDirection.SW;
                }                
                else if (LastMousePosition == selectedItem.BottomRight)
                {
                    ResizingItem = ResizeDirection.SE;
                }                
                else if (LastMousePosition.X == selectedItem.TopLeft.X)
                {
                    ResizingItem = ResizeDirection.W;
                }
                else if(LastMousePosition.X == selectedItem.TopRight.X)
                {
                    ResizingItem = ResizeDirection.E;
                }
                else if(LastMousePosition.Y == selectedItem.TopLeft.Y)
                {
                    ResizingItem = ResizeDirection.N;

                }
                else if(LastMousePosition.Y == selectedItem.BottomLeft.Y)
                {
                    ResizingItem = ResizeDirection.S;
                }
                else
                {
                    ResizingItem = ResizeDirection.None;
                }
            }

            //If the item was not yet selected
            else if (!SelectedItems.Contains(selectedItem))
            {
                SelectedItems.Add(selectedItem);
                if (string.IsNullOrEmpty(KeyPressed))
                {
                    LastMousePosition = position;
                    MovingItem = true;
                }
            }

            //If the center of the item was selected -> Move the selected items
            else
            {
                LastMousePosition = position;
                MovingItem = true;
            }

            UpdateSelectedItems();
        }

        private void MoveItem(ItemViewModel selectedItem, Vector2 delta, HashSet<ItemViewModel> movingItems)
        {
            var unselectedItems = Items.Except(movingItems);
            selectedItem.Position += delta;
            var collidingItems = unselectedItems.Where(i => i.CollidesWith(selectedItem)).ToList();
            movingItems.AddRange(collidingItems);

            foreach (var collidingItem in collidingItems)
            {
                MoveItem(collidingItem, delta, movingItems);
            }
        }

        private void MoveItem(Vector2 position)
        {
            var movingItems = SelectedItems.ToHashSet();
            if (Selection.IsShown)
            {
                Selection.Size = Vector2.Abs(MouseDownPosition - position);

                if (position.X <= MouseDownPosition.X)
                {
                    if(position.Y <= MouseDownPosition.Y)
                    {
                        Selection.Position = position;
                    }
                    else
                    {
                        Selection.Position = new Vector2(position.X, MouseDownPosition.Y);
                    }
                }else
                {
                    if (position.Y <= MouseDownPosition.Y)
                    {
                        Selection.Position = new Vector2(MouseDownPosition.X, position.Y);
                    }
                    else
                    {
                        Selection.Position = MouseDownPosition;
                    }
                }
            }
            else if (MovingItem)
            {
                var delta = position - LastMousePosition;

                LastMousePosition = position;

                foreach (var item in SelectedItems)
                {
                    MoveItem(item, delta, movingItems);
                }

                foreach (var connection in Connections)
                {
                    //If the item is moving or is connected to a moving item
                    if (movingItems.Any(i => connection.IsConnectedTo(i)) ||
                        connection.ConnectionPath.Any(p => movingItems.Any(c => c.CollidesWith(p))))
                    {
                        connection.Redraw();
                    }
                }

            }
            else if (ResizingItem != ResizeDirection.None)
            {

                ResizeItem(position);
            }
        }

        private void ResizeItem(Vector2 position)
        {
            var selectedItem = SelectedItems.FirstOrDefault();

            var delta = position - LastMousePosition;

            var newPosition = selectedItem.Position;
            var newSize = selectedItem.Size;
            switch (ResizingItem)
            {
                case ResizeDirection.NW:
                    newPosition += delta;
                    newSize += delta * new Vector2(-1, -1);
                    break;
                case ResizeDirection.NE:
                    newPosition += delta * new Vector2(0, 1);
                    newSize += delta * new Vector2(1, -1);
                    break;
                case ResizeDirection.SE:
                    newSize += delta;
                    break;
                case ResizeDirection.SW:
                    newPosition += delta * new Vector2(1, 0);
                    newSize += delta * new Vector2(-1, 1);
                    break;
                case ResizeDirection.N:
                    newPosition += delta * new Vector2(0, 1);
                    newSize += delta * new Vector2(0, -1);
                    break;
                case ResizeDirection.S:
                    newSize += delta * new Vector2(0, 1);
                    break;
                case ResizeDirection.E:
                    newSize += delta * new Vector2(1, 0);
                    break;
                case ResizeDirection.W:
                    newPosition += delta * new Vector2(1, 0);
                    newSize += delta * new Vector2(-1, 0);
                    break;
            }

            if (newSize.X>= 2 && newSize.Y >= 2)
            {
                selectedItem.Position = newPosition;
                selectedItem.Size = newSize;
            }
            LastMousePosition = position;

        }

        private void ReleaseItem()
        {
            MovingItem = false;
            ResizingItem = ResizeDirection.None;

            if (Selection.IsShown)
            {
                Selection.IsShown = false;

                SelectedItems.AddRange(Items.Where(i => i.CollidesWith(Selection)));
                UpdateSelectedItems();
                return;
            }

            if (!string.IsNullOrEmpty(KeyPressed))
            {
                return;
            }

            foreach (var connection in Connections)
            {
                connection.BorderColor = Color.Black;
                connection.Redraw();
            }

            ClearSelectedItems();
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

                if (!connection.Item2.CollidesWith(position))
                {
                    continue;
                }

                _connectedItem = connection.Item2;
                SelectedConnection = connection;
                break;
            }

            if(SelectedConnection != null)
            {
                BringToFront(SelectedConnection);
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