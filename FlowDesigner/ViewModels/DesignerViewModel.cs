#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading;
using Aptacode.CSharp.Common.Utilities.Extensions;
using Aptacode.CSharp.Common.Utilities.Mvvm;
using Aptacode.FlowDesigner.Core.Enums;
using Aptacode.FlowDesigner.Core.ViewModels.Components;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class DesignerViewModel : BindableBase
    {
        private readonly List<ConnectionViewModel> _connections = new List<ConnectionViewModel>();

        private readonly List<ConnectedComponentViewModel> _items = new List<ConnectedComponentViewModel>();
        private readonly List<PointViewModel> _points = new List<PointViewModel>();

        private bool _movingItem;
        private ResizeDirection _resizingItem;

        public DesignerViewModel(int width, int height)
        {
            Width = width;
            Height = height;
            Selection = new SelectionViewModel(Guid.NewGuid(), Vector2.Zero, Vector2.Zero);
            ResizingItem = ResizeDirection.None;
            Components = new List<BaseComponentViewModel>();
        }

        private ConnectionPointViewModel? _connectedItem { get; set; }

        private Vector2 LastMousePosition { get; set; }
        private Vector2 MouseDownPosition { get; set; }

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

        public SelectionViewModel Selection { get; set; }

        public IEnumerable<ConnectedComponentViewModel> Items => _items.OrderBy(i => i.Z);
        public IEnumerable<PointViewModel> Points => _points.OrderBy(i => i.Z);
        public IEnumerable<ConnectionViewModel> Connections => _connections.OrderBy(i => i.Z);

        public List<BaseComponentViewModel> Components { get; set; }


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

        public event EventHandler<IEnumerable<ConnectedComponentViewModel>> SelectedItemChanged;
        public event EventHandler<ConnectionViewModel> SelectedConnectionChanged;

        #endregion

        #region Keyboard

        public string? KeyPressed;
        public bool ControlPressed => KeyPressed == "Control";
        public bool IPressed => string.Equals(KeyPressed, "i", StringComparison.OrdinalIgnoreCase);
        public bool PPressed => string.Equals(KeyPressed, "p", StringComparison.OrdinalIgnoreCase);
        public bool NothingPressed => string.IsNullOrEmpty(KeyPressed);

        public void KeyDown(string key)
        {
            KeyPressed = key;
        }

        public void KeyUp(string key)
        {
            KeyPressed = null;
            ReleaseItem();
        }

        #endregion

        #region Layering

        public void BringToFront(ConnectedComponentViewModel item)
        {
            item.Z = Items.Max(i => i.Z) + 1;
        }

        public void SendToBack(ConnectedComponentViewModel item)
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

        #endregion

        #region Commands

        public ConnectedComponentViewModel AddItem(string name, Vector2 position, Vector2 size)
        {
            var newItem = new ConnectedComponentViewModel(Guid.NewGuid(), name, position, size);
            _items.Add(newItem);
            OnPropertyChanged(nameof(Items));
            return newItem;
        }

        public void RemoveItem(ConnectedComponentViewModel item)
        {
            _items.Remove(item);
            OnPropertyChanged(nameof(Items));
        }

        public PointViewModel AddPoint(Vector2 position)
        {
            var newItem = new PointViewModel(Guid.NewGuid(), position);
            _points.Add(newItem);
            OnPropertyChanged(nameof(Points));
            return newItem;
        }

        public void RemovePoint(Vector2 position)
        {
            var selectedPoint = Points.FirstOrDefault(p => p.Position == position);
            _points.Remove(selectedPoint);
            OnPropertyChanged(nameof(Points));
        }

        public ConnectionPointViewModel AddConnectionPoint(ConnectedComponentViewModel item)
        {
            var newConnectionPoint = new ConnectionPointViewModel(Guid.NewGuid(), item);
            item.ConnectionPoints.Add(newConnectionPoint);
            OnPropertyChanged(nameof(Items));
            return newConnectionPoint;
        }

        public void RemoveConnectionPoint(ConnectionPointViewModel connectionPoint)
        {
            connectionPoint.Item.ConnectionPoints.Remove(connectionPoint);
            foreach (var connection in connectionPoint.Connections.ToArray())
            {
                _connections.Remove(connection);
                connection.Break();
            }

            OnPropertyChanged(nameof(Items));
        }

        private DelegateCommand<(ItemViewModel item1, ItemViewModel item2)>? _connect;

        public DelegateCommand<(ItemViewModel item1, ItemViewModel item2)> Connect => _connect ??=
            new DelegateCommand<(ItemViewModel item1, ItemViewModel item2)>(tuple =>
            {
                var newConnection =
                    new ConnectionViewModel(Guid.NewGuid(), "New Connection", this, tuple.item1, tuple.item2);
                _connections.Add(newConnection);
                OnPropertyChanged(nameof(Connections));
            });

        public void RemoveConnection(ConnectionViewModel connection)
        {
            connection.Break();
            _connections.Add(connection);
            OnPropertyChanged(nameof(Connections));
        }

        #endregion

        #region Mouse

        public readonly List<ConnectedComponentViewModel> SelectedItems = new List<ConnectedComponentViewModel>();

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

            //Interact with Points
            if (PPressed)
            {
                var selectedPoint = Points.FirstOrDefault(p => p.Position == position);
                if (!_points.Remove(selectedPoint))
                {
                    AddPoint(position);
                }

                return;
            }

            //Interact with Connections
            if (SelectedConnection == null)
            {
                ClickConnection(position);
            }

            //Interact with Items
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

        private void ClickItem(Vector2 position)
        {
            var selectedItem = Items.FirstOrDefault(item => item.CollidesWith(position));

            //If no item was selected
            if (selectedItem == default && NothingPressed)
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
                else if (LastMousePosition.X == selectedItem.TopRight.X)
                {
                    ResizingItem = ResizeDirection.E;
                }
                else if (LastMousePosition.Y == selectedItem.TopLeft.Y)
                {
                    ResizingItem = ResizeDirection.N;
                }
                else if (LastMousePosition.Y == selectedItem.BottomLeft.Y)
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
                if (NothingPressed)
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

        private void MoveItem(ConnectedComponentViewModel selectedItem, Vector2 delta,
            HashSet<ConnectedComponentViewModel> movingItems, CancellationTokenSource cancellationToken)
        {
            var unselectedItems = Items.Except(movingItems);
            var newPosition = selectedItem.Position + delta;

            if (newPosition.X < 0 || newPosition.Y < 0 || newPosition.X + selectedItem.Size.X >= Width ||
                newPosition.Y + selectedItem.Size.Y >= Height)
            {
                cancellationToken.Cancel();
                return;
            }

            selectedItem.Position = newPosition;


            var collidingItems = unselectedItems
                .Where(i => i.CollidesWith(selectedItem.PositionAndMargin, selectedItem.SizeAndMargin)).ToList();
            movingItems.AddRange(collidingItems);

            foreach (var collidingItem in collidingItems)
            {
                MoveItem(collidingItem, delta, movingItems, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                selectedItem.Position -= delta;
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
                    Selection.Position = position.Y <= MouseDownPosition.Y ? position : new Vector2(position.X, MouseDownPosition.Y);
                }
                else
                {
                    Selection.Position = position.Y <= MouseDownPosition.Y ? new Vector2(MouseDownPosition.X, position.Y) : MouseDownPosition;
                }
            }
            else if (MovingItem)
            {
                var delta = position - LastMousePosition;

                LastMousePosition = position;

                var cancellationTokenSource = new CancellationTokenSource();
                foreach (var item in SelectedItems)
                {
                    MoveItem(item, delta, movingItems, cancellationTokenSource);
                }

                if (cancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }

                foreach (var connection in Connections)
                {
                    if (movingItems.Contains(connection.Point1.Item) && movingItems.Contains(connection.Point2.Item))
                    {
                        connection.Path.Translate(delta);
                    }
                    else if (movingItems.Any(i =>
                            connection.IsConnectedTo(i) ||
                            connection.Path.CollidesWith(i.PositionAndMargin, i.SizeAndMargin)))
                        //If the item is moving or is connected to a moving item

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
            var originalPosition = selectedItem.Position;
            var originalSize = selectedItem.Size;
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

            if (newSize.X >= 2 && newSize.Y >= 2)
            {
                selectedItem.Position = newPosition;
                selectedItem.Size = newSize;
            }

            if (Items.Count(i => i.CollidesWith(selectedItem.PositionAndMargin, selectedItem.SizeAndMargin)) > 1)
            {
                selectedItem.Position = originalPosition;
                selectedItem.Size = originalSize;
            }

            if(selectedItem.Size.X < originalSize.X || selectedItem.Size.Y < originalSize.Y)
            {
                foreach(var connection in selectedItem.Connections)
                {
                    var s = connection.Item1.Item == selectedItem ? connection.Item1 : connection.Item2;
                    s.UpdateAnchorPointDelta(position);

                    switch (ResizingItem)
                    {
                        case ResizeDirection.NW:
                            if (s.AnchorPoint.X > selectedItem.TopRight.X)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.TopRight.X - 1, selectedItem.TopRight.Y));
                            }
                            if (s.AnchorPoint.Y < selectedItem.TopRight.Y)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.TopRight.X, selectedItem.TopRight.Y + 1));
                            }
                            break;
                        case ResizeDirection.NE:
                            if (s.AnchorPoint.X < selectedItem.TopLeft.X)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.TopLeft.X + 1, selectedItem.TopLeft.Y));
                            }
                            if (s.AnchorPoint.Y < selectedItem.TopLeft.Y)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.TopLeft.X, selectedItem.TopRight.Y + 1));
                            }
                            break;
                        case ResizeDirection.SE:
                            if (s.AnchorPoint.X > selectedItem.BottomRight.X)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.BottomRight.X - 1, selectedItem.BottomRight.Y));
                            }
                            if (s.AnchorPoint.Y > selectedItem.BottomRight.Y)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.BottomRight.X, selectedItem.BottomRight.Y - 1));
                            }
                            break;
                        case ResizeDirection.SW:
                            if (s.AnchorPoint.X < selectedItem.BottomLeft.X)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.BottomLeft.X + 1, selectedItem.BottomLeft.Y));
                            }
                            if (s.AnchorPoint.Y > selectedItem.BottomLeft.Y)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.BottomLeft.X, selectedItem.BottomLeft.Y - 1));
                            }
                            break;
                        case ResizeDirection.N:
                            if (s.AnchorPoint.X == selectedItem.TopLeft.X && s.AnchorPoint.Y < selectedItem.TopLeft.Y)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.TopLeft.X, selectedItem.TopLeft.Y + 1));
                            }
                            if (s.AnchorPoint.X == selectedItem.TopRight.X && s.AnchorPoint.Y < selectedItem.TopRight.Y)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.TopRight.X, selectedItem.TopRight.Y + 1));
                            }
                            break;
                        case ResizeDirection.S:
                            if (s.AnchorPoint.X == selectedItem.BottomLeft.X && s.AnchorPoint.Y > selectedItem.BottomLeft.Y)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.BottomLeft.X, selectedItem.BottomLeft.Y - 1));
                            }
                            if (s.AnchorPoint.X == selectedItem.BottomRight.X && s.AnchorPoint.Y > selectedItem.BottomRight.Y)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.BottomRight.X, selectedItem.BottomRight.Y - 1));
                            }
                            break;
                        case ResizeDirection.E:
                            if (s.AnchorPoint.Y == selectedItem.TopRight.Y && s.AnchorPoint.X > selectedItem.TopRight.X)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.TopRight.X - 1, selectedItem.TopRight.Y));
                            }
                            if (s.AnchorPoint.Y == selectedItem.BottomRight.Y && s.AnchorPoint.X > selectedItem.BottomRight.X)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.BottomRight.X - 1, selectedItem.BottomRight.Y));
                            }
                            break;
                        case ResizeDirection.W:
                            if (s.AnchorPoint.Y == selectedItem.TopLeft.Y && s.AnchorPoint.X < selectedItem.TopLeft.X)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.TopLeft.X + 1, selectedItem.TopLeft.Y));
                            }
                            if (s.AnchorPoint.Y == selectedItem.BottomLeft.Y && s.AnchorPoint.X < selectedItem.BottomLeft.X)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.BottomLeft.X + 1, selectedItem.BottomLeft.Y));
                            }
                            break;
                    }
                }
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

                SelectedItems.AddRange(Items.Where(i =>
                    i.CollidesWith(Selection.PositionAndMargin, Selection.SizeAndMargin)));

                UpdateSelectedItems();
                return;
            }

            if (ControlPressed)
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
                if (connection.Point1.CollidesWith(position))
                {
                    _connectedItem = connection.Point1;
                    SelectedConnection = connection;
                    break;
                }

                if (!connection.Point2.CollidesWith(position))
                {
                    continue;
                }

                _connectedItem = connection.Point2;
                SelectedConnection = connection;
                break;
            }

            if (SelectedConnection != null)
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