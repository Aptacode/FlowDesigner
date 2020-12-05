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
using Aptacode.FlowDesigner.Core.Extensions;
using Aptacode.FlowDesigner.Core.Utilities;
using Aptacode.FlowDesigner.Core.ViewModels.Components;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class DesignerViewModel : BindableBase
    {
        public DesignerViewModel(int width, int height)
        {
            Width = width;
            Height = height;
            Selection = new SelectionViewModel(Guid.NewGuid(), Vector2.Zero, Vector2.Zero);
            Path = new PathViewModel();
            ResizeDirection = ResizeDirection.None;
        }
        #region Components
        public SelectionViewModel Selection { get; set; }
        public PathViewModel Path { get; set; }
        public readonly List<ConnectedComponentViewModel> SelectedItems = new List<ConnectedComponentViewModel>();

        public List<ConnectedComponentViewModel> Items { get; set; } = new List<ConnectedComponentViewModel>();
        public List<PointViewModel> Points { get; set; } = new List<PointViewModel>();
        public List<RectangleViewModel> Rectangles { get; set; } = new List<RectangleViewModel>();
        public List<PathViewModel> Paths { get; set; } = new List<PathViewModel>();
        public List<ConnectionViewModel> Connections { get; set; } = new List<ConnectionViewModel>();
        public List<BaseComponentViewModel> Components { get; set; } = new List<BaseComponentViewModel>();

        #endregion

        #region Properties
        public int Width { get; set; }
        public int Height { get; set; }
        private Vector2 LastMousePosition { get; set; }
        private Vector2 MouseDownPosition { get; set; }

        private ResizeDirection _resizingItem;
        public ResizeDirection ResizeDirection
        {
            get => _resizingItem;
            set => SetProperty(ref _resizingItem, value);
        }

        private bool _movingItem;
        public bool MovingItem
        {
            get => _movingItem;
            set => SetProperty(ref _movingItem, value);
        }
        #endregion

        #region Events

        public event EventHandler<IEnumerable<ConnectedComponentViewModel>> SelectedItemChanged;
        public event EventHandler<ConnectionPointViewModel> SelectedConnectionChanged;

        #endregion

        #region Keyboard

        public string? KeyPressed;
        public bool ControlPressed => KeyPressed == "Control";
        public bool IsPressed(string key) => string.Equals(KeyPressed, key, StringComparison.OrdinalIgnoreCase);
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



        #region Commands

        public void Add(BaseComponentViewModel component)
        {
            Components.Add(component);
            OnPropertyChanged(nameof(Components));
        }

        public void Remove(BaseComponentViewModel component)
        {
            Components.Remove(component);
            OnPropertyChanged(nameof(Components));
        }

        #endregion

        #region Mouse

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
                item.BringToFront(this);
                item.BorderColor = Color.Green;
            }

            //Highlight any connections for a selected item
            foreach (var connection in Connections.Where(connection => SelectedItems.Any(connection.IsConnectedTo)))
            {
                connection.Select();
                connection.BringToFront(this);
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
                        connection.Path.BringToFront(this);
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
            if (IsPressed("p"))
            {
                var selectedPoint = Points.FirstOrDefault(p => p.Position == position);
                if (!Points.Remove(selectedPoint))
                {
                    var newPoint = this.CreatePoint(position);
                    newPoint.AddTo(this);
                }

                return;
            }

            //Interact with Connections
            if (_selectedConnectionPoint == null || IsPressed("c"))
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
                    Selection.Position = position.Y <= MouseDownPosition.Y
                        ? position
                        : new Vector2(position.X, MouseDownPosition.Y);
                }
                else
                {
                    Selection.Position = position.Y <= MouseDownPosition.Y
                        ? new Vector2(MouseDownPosition.X, position.Y)
                        : MouseDownPosition;
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

            if (IsPressed("d"))
            {
                selectedItem.RemoveFrom(this);
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

        private void MoveItem(Vector2 position)
        {
            if (MovingItem)
            {
                var delta = position - LastMousePosition;

                LastMousePosition = position;

                var cancellationTokenSource = new CancellationTokenSource();
                var movingItems = SelectedItems.OfType<BaseShapeViewModel>().ToList();
                foreach (var item in SelectedItems)
                {
                    this.Move(item, delta, movingItems, cancellationTokenSource);
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
                                connection.Path.CollidesWith(i.PositionAndMargin, i.SizeAndMargin) || 
                            (
                                i is ConnectedComponentViewModel connectedComponent &&
                            connection.IsConnectedTo(connectedComponent))))
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
            var selectedItem = SelectedItems.FirstOrDefault();
            if(selectedItem != null)
            {
                var delta = position - LastMousePosition;
                selectedItem.Resize(this, delta, ResizeDirection);
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
                connection.Deselect();
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

            if (IsPressed("d"))
            {
                selectedConnection.RemoveFrom(this);
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

            if (IsPressed("c"))
            {
                if (!Items.Any(i => i.CollidesWith(position)))
                {
                    Path.ClearPoints();
                    var startPoint = _selectedConnectionPoint.GetOffset(_selectedConnectionPoint.Item.Margin);
                    var endPoint = position;
                    var path = this.GetPath(startPoint, endPoint);
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
            if (_selectedConnectionPoint == null)
            {
                return;
            }

            if (IsPressed("c"))
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
                    if (collidingItem != null && collidingItem != _selectedConnectionPoint.Item)
                    {

                        selectedConnectionPoint = this.CreateConnectionPoint(collidingItem);
                        selectedConnectionPoint.AddTo(this);
                        selectedConnectionPoint.UpdateAnchorPointDelta(_selectedConnectionPoint.AnchorPoint);
                    }
                }

                if (SelectedConnectionPoint != selectedConnectionPoint && selectedConnectionPoint != null)
                {
                    _selectedConnectionPoint.Connect(this, selectedConnectionPoint);
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

        #region Connections

        public void RedrawConnections()
        {
            foreach (var connection in Connections)
            {
                connection.Redraw();
            }
        }

        #endregion
    }
}