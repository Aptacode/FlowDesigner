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
using Aptacode.PathFinder.Geometry.Neighbours;
using Aptacode.PathFinder.Maps;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class DesignerViewModel : BindableBase
    {

        private bool _movingItem;
        private ResizeDirection _resizingItem;

        public DesignerViewModel(int width, int height)
        {
            Width = width;
            Height = height;
            Selection = new SelectionViewModel(Guid.NewGuid(), Vector2.Zero, Vector2.Zero);
            Path = new PathViewModel();
            ResizeDirection = ResizeDirection.None;
        }

        private Vector2 LastMousePosition { get; set; }
        private Vector2 MouseDownPosition { get; set; }

        public ResizeDirection ResizeDirection
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
        public PathViewModel Path { get; set; }

        public List<ConnectedComponentViewModel> Items { get; set; } = new List<ConnectedComponentViewModel>();
        public List<PointViewModel> Points { get; set; } = new List<PointViewModel>();
        public List<ConnectionViewModel> Connections { get; set; } = new List<ConnectionViewModel>();
        public List<BaseComponentViewModel> Components { get; set; } = new List<BaseComponentViewModel>();

        public int Width { get; set; }
        public int Height { get; set; }

        public void RedrawConnections()
        {
            foreach (var connection in Connections)
            {
                connection.Redraw();
            }
        }

        #region Events

        public event EventHandler<IEnumerable<ConnectedComponentViewModel>> SelectedItemChanged;
        public event EventHandler<ConnectionPointViewModel> SelectedConnectionChanged;

        #endregion

        #region Keyboard

        public string? KeyPressed;
        public bool ControlPressed => KeyPressed == "Control";
        public bool IPressed => string.Equals(KeyPressed, "i", StringComparison.OrdinalIgnoreCase);
        public bool CPressed => string.Equals(KeyPressed, "c", StringComparison.OrdinalIgnoreCase);
        public bool PPressed => string.Equals(KeyPressed, "p", StringComparison.OrdinalIgnoreCase);
        public bool DPressed => string.Equals(KeyPressed, "d", StringComparison.OrdinalIgnoreCase);
        public bool NothingPressed => string.IsNullOrEmpty(KeyPressed);

        public void KeyDown(string key)
        {
            KeyPressed = key;
        }

        public void KeyUp(string key)
        {
            if (ControlPressed)
            {
                KeyPressed = null;
                ReleaseItem();
            }
            KeyPressed = null;
        }

        #endregion

        #region Layering
        public void BringToFront(BaseComponentViewModel component)
        {
            if (Components.Remove(component))
            {
                Components.Insert(0, component);
            }
        }

        public void SendToBack(BaseComponentViewModel component)
        {
            if (Components.Remove(component))
            {
                Components.Add(component);
            }
        }

        public void BringForward(BaseComponentViewModel component)
        {
            var index = Components.IndexOf(component);
            if(index == 0)
            {
                return;
            }

            Components.RemoveAt(index);
            Components.Insert(index - 1, component);
        }

        public void SendBackward(BaseComponentViewModel component)
        {
            var index = Components.IndexOf(component);
            if (index == Components.Count -1)
            {
                return;
            }

            Components.RemoveAt(index);
            Components.Insert(index +1, component);
        }

        #endregion

        #region Commands

        public ConnectedComponentViewModel AddItem(string name, Vector2 position, Vector2 size)
        {
            var newItem = new ConnectedComponentViewModel(Guid.NewGuid(), name, position, size);
            Items.Add(newItem);
            Components.Add(newItem);
            OnPropertyChanged(nameof(Items));
            return newItem;
        }

        public void RemoveItem(ConnectedComponentViewModel item)
        {
            foreach(var connectionPoint in item.ConnectionPoints.ToArray())
            {
                foreach(var connection in connectionPoint.Connections.ToArray())
                {
                    connection.Break(); 
                    Connections.Remove(connection);
                    Components.Remove(connection);
                }
                Components.Remove(connectionPoint);
            }
            Items.Remove(item);
            Components.Remove(item);

            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(Connections));
        }

        public PointViewModel AddPoint(Vector2 position)
        {
            var newItem = new PointViewModel(Guid.NewGuid(), position);
            Points.Add(newItem);
            Components.Add(newItem);
            OnPropertyChanged(nameof(Points));
            return newItem;
        }

        public void RemovePoint(Vector2 position)
        {
            var selectedPoint = Points.FirstOrDefault(p => p.Position == position);
            Points.Remove(selectedPoint);
            Components.Remove(selectedPoint);
            OnPropertyChanged(nameof(Points));
        }

        public ConnectionPointViewModel AddConnectionPoint(ConnectedComponentViewModel item)
        {
            var newConnectionPoint = new ConnectionPointViewModel(Guid.NewGuid(), item);
            item.ConnectionPoints.Add(newConnectionPoint);
            Components.Add(newConnectionPoint);
            OnPropertyChanged(nameof(Items));
            return newConnectionPoint;
        }

        public void RemoveConnectionPoint(ConnectionPointViewModel connectionPoint)
        {
            connectionPoint.Item.ConnectionPoints.Remove(connectionPoint);
            foreach (var connection in connectionPoint.Connections.ToArray())
            {
                Connections.Remove(connection);
                Components.Remove(connection);
                connection.Break();
            }

            OnPropertyChanged(nameof(Items));
        }

        public ConnectionViewModel AddConnection(ConnectionPointViewModel point1, ConnectionPointViewModel point2)
        {
            var newConnection = new ConnectionViewModel(Guid.NewGuid(), this, point1, point2);
            Connections.Add(newConnection);
            point1.Connections.Add(newConnection);
            point2.Connections.Add(newConnection);
            Components.Add(newConnection);
            OnPropertyChanged(nameof(Connections));
            return newConnection;
        }


        public void RemoveConnection(ConnectionViewModel connection)
        {
            connection.Break();
            Connections.Remove(connection);
            Components.Remove(connection);
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
            foreach (var connection in  Connections.Where(connection => SelectedItems.Any(connection.IsConnectedTo)))
            {
                BringToFront(connection);
                connection.BorderColor = Color.Green;
            }

            SelectedItemChanged?.Invoke(this, SelectedItems);
        }
        private ConnectionPointViewModel? _selectedConnectionPoint;

        public ConnectionPointViewModel? SelectedConnectionPoint
        {
            get => _selectedConnectionPoint;
            set
            {
                if (value != null)
                {
                    foreach (var connection in value.Connections)
                    {
                        connection.Select();
                        BringToFront(connection.Path);
                    }
                }

                SetProperty(ref _selectedConnectionPoint, value);
                SelectedConnectionChanged?.Invoke(this, _selectedConnectionPoint);
            }
        }


        public void MouseDown(Vector2 position)
        {
            MouseDownPosition = position;

            //Interact with Points
            if (PPressed)
            {
                var selectedPoint = Points.FirstOrDefault(p => p.Position == position);
                if (!Points.Remove(selectedPoint))
                {
                    AddPoint(position);
                }

                return;
            }

            //Interact with Connections
            if (_selectedConnectionPoint == null || CPressed)
            {
                ClickConnection(position);
            }

            //Interact with Items
            if (_selectedConnectionPoint == null)
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

            if (_selectedConnectionPoint != null)
            {
                MoveConnection(position);
            }

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
        }

        public void MouseUp(Vector2 position)
        {
            if (SelectedItems.Count > 0)
            {
                ReleaseItem();
            }

            if (_selectedConnectionPoint != null)
            {
                ReleaseConnection(position);
            }

            if (Selection.IsShown)
            {
                Selection.IsShown = false;

                SelectedItems.AddRange(Items.Where(i =>
                    i.CollidesWith(Selection.PositionAndMargin, Selection.SizeAndMargin)));

                UpdateSelectedItems();
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

            if (selectedItem == null)
            {
                return;
            }

            if (DPressed)
            {
                RemoveItem(selectedItem);
                KeyPressed = null;
                return;
            }

            //If the border of the item was selected -> resize the item
            ResizeDirection = selectedItem.GetCollidingEdge(position);
            if (ResizeDirection != ResizeDirection.None)
            {
                ClearSelectedItems();
                SelectedItems.Add(selectedItem);
                LastMousePosition = position;
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
            if (MovingItem)
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
            else if (ResizeDirection != ResizeDirection.None)
            {
                ResizeItem(position);
            }
        }

        private void ResizeItem(Vector2 position)
        {
            //Move to BaseShapeViewModel
            var selectedItem = SelectedItems.FirstOrDefault();

            var delta = position - LastMousePosition;
            var originalPosition = selectedItem.Position;
            var originalSize = selectedItem.Size;
            var newPosition = selectedItem.Position;
            var newSize = selectedItem.Size;
            switch (ResizeDirection)
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
                case ResizeDirection.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
                foreach(var connection in selectedItem.ConnectionPoints)
                {
                    var s = connection;
                    connection.UpdateAnchorPointDelta(position);

                    switch (ResizeDirection)
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
                            if (Math.Abs(s.AnchorPoint.X - selectedItem.TopLeft.X) < Constants.Tolerance && s.AnchorPoint.Y < selectedItem.TopLeft.Y)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.TopLeft.X, selectedItem.TopLeft.Y + 1));
                            }
                            if (Math.Abs(s.AnchorPoint.X - selectedItem.TopRight.X) < Constants.Tolerance && s.AnchorPoint.Y < selectedItem.TopRight.Y)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.TopRight.X, selectedItem.TopRight.Y + 1));
                            }
                            break;
                        case ResizeDirection.S:
                            if (Math.Abs(s.AnchorPoint.X - selectedItem.BottomLeft.X) < Constants.Tolerance && s.AnchorPoint.Y > selectedItem.BottomLeft.Y)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.BottomLeft.X, selectedItem.BottomLeft.Y - 1));
                            }
                            if (Math.Abs(s.AnchorPoint.X - selectedItem.BottomRight.X) < Constants.Tolerance && s.AnchorPoint.Y > selectedItem.BottomRight.Y)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.BottomRight.X, selectedItem.BottomRight.Y - 1));
                            }
                            break;
                        case ResizeDirection.E:
                            if (Math.Abs(s.AnchorPoint.Y - selectedItem.TopRight.Y) < Constants.Tolerance && s.AnchorPoint.X > selectedItem.TopRight.X)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.TopRight.X - 1, selectedItem.TopRight.Y));
                            }
                            if (Math.Abs(s.AnchorPoint.Y - selectedItem.BottomRight.Y) < Constants.Tolerance && s.AnchorPoint.X > selectedItem.BottomRight.X)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.BottomRight.X - 1, selectedItem.BottomRight.Y));
                            }
                            break;
                        case ResizeDirection.W:
                            if (Math.Abs(s.AnchorPoint.Y - selectedItem.TopLeft.Y) < Constants.Tolerance && s.AnchorPoint.X < selectedItem.TopLeft.X)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.TopLeft.X + 1, selectedItem.TopLeft.Y));
                            }
                            if (Math.Abs(s.AnchorPoint.Y - selectedItem.BottomLeft.Y) < Constants.Tolerance && s.AnchorPoint.X < selectedItem.BottomLeft.X)
                            {
                                s.UpdateAnchorPointDelta(new Vector2(selectedItem.BottomLeft.X + 1, selectedItem.BottomLeft.Y));
                            }
                            break;
                        case ResizeDirection.None:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }    

            LastMousePosition = position;
        }

        private void ReleaseItem()
        {
            MovingItem = false;
            ResizeDirection = ResizeDirection.None;

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
            ConnectionPointViewModel? selectedConnection = null;
            foreach (var connection in Connections)
            {
                if (connection.Point1.CollidesWith(position))
                {
                    selectedConnection = connection.Point1;
                    break;
                }

                if (!connection.Point2.CollidesWith(position))
                {
                    continue;
                }

                selectedConnection = connection.Point2;
                break;
            }

            if (selectedConnection == null)
            {
                return;
            }

            if (DPressed)
            {
                RemoveConnectionPoint(selectedConnection);
            }
            else
            {
                SelectedConnectionPoint = selectedConnection;
            }
        }

        private void MoveConnection(Vector2 position)
        {
            if (_selectedConnectionPoint == null)
            {
                return;
            }
            if (_selectedConnectionPoint.Item.CollidesWith(position))
            {
                return;
            }

            if (CPressed)
            {
                if(!Items.Any(i => i.CollidesWith(position)))
                {
                    Path.ClearPoints();
                    var startPoint = SelectedConnectionPoint.GetOffset(SelectedConnectionPoint.Item.Margin);
                    var endPoint = position;
                    var path = GetPath(startPoint, endPoint);
                    Path.AddPoints(path);
                }
            }
            else
            {
                _selectedConnectionPoint.UpdateAnchorPointDelta(position);

                _selectedConnectionPoint.Redraw();
            }
        }

        private void ReleaseConnection(Vector2 position)
        {
            if(_selectedConnectionPoint == null)
            {
                return;
            }

            if (CPressed)
            {
                ConnectionPointViewModel? selectedConnectionPoint = null;
                foreach (var connection in Connections)
                {
                    if (connection.Point1.CollidesWith(position))
                    {
                        selectedConnectionPoint = connection.Point1;
                        break;
                    }

                    if (!connection.Point2.CollidesWith(position))
                    {
                        continue;
                    }

                    selectedConnectionPoint = connection.Point2;
                    break;
                }

                if (selectedConnectionPoint == null)
                {
                    var collidingItem = Items.FirstOrDefault(c => c.CollidesWith(position));
                    if (collidingItem != null && collidingItem != SelectedConnectionPoint.Item)
                    {
                        selectedConnectionPoint = AddConnectionPoint(collidingItem);
                        selectedConnectionPoint.UpdateAnchorPointDelta(SelectedConnectionPoint.AnchorPoint);
                    }
                }

                if (SelectedConnectionPoint != selectedConnectionPoint && selectedConnectionPoint != null)
                {
                    AddConnection(SelectedConnectionPoint, selectedConnectionPoint);
                }

                Path.ClearPoints();
            }


            foreach (var connection in _selectedConnectionPoint.Connections)
            {
                connection.Deselect();
            }
            _selectedConnectionPoint = null;


        }

        #endregion

        #region PathFinding

        public List<Vector2> GetPath(Vector2 startPoint, Vector2 endPoint)
        {
            var points = new List<Vector2>();

            try
            {
                var mapBuilder = new MapBuilder();

                foreach (var item in Items.ToList())
                {
                    mapBuilder.AddObstacle(item.Position - Vector2.One, item.Size + (Vector2.One * 2));
                }

                mapBuilder.SetStart(startPoint);
                mapBuilder.SetEnd(endPoint);
                mapBuilder.SetDimensions(Width, Height);
                var mapResult = mapBuilder.Build();
                if (!mapResult.Success)
                {
                    throw new Exception(mapResult.Message);
                }

                var pathFinder =
                    new PathFinder.Algorithm.PathFinder(mapResult.Map, DefaultNeighbourFinder.Straight(0.5f));

                points.AddRange(pathFinder.FindPath());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return points;
        }

        #endregion
    }
}