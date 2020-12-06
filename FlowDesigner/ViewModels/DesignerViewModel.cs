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
        public readonly HashSet<ConnectedComponentViewModel> SelectedItems = new HashSet<ConnectedComponentViewModel>();

        readonly List<ConnectionViewModel> _connections = new List<ConnectionViewModel>();
        readonly List<BaseComponentViewModel> _components = new List<BaseComponentViewModel>();

        public IEnumerable<ConnectedComponentViewModel> Items => GetComponents<ConnectedComponentViewModel>();
        public IEnumerable<PointViewModel> Points  => GetComponents<PointViewModel>();
        public IEnumerable<RectangleViewModel> Rectangles => GetComponents<RectangleViewModel>();
        public IEnumerable<PathViewModel> Paths  => GetComponents<PathViewModel>();
        public IEnumerable<ConnectionViewModel> Connections => _connections;
        public IEnumerable<BaseComponentViewModel> Components => _components;

        public IEnumerable<TType> GetComponents<TType>() where TType : BaseComponentViewModel
        {
            if(ComponentsByType.TryGetValue(typeof(TType), out var components))
            {
                return components.Select(s => (TType)Convert.ChangeType(s, typeof(TType)));
            }
            return new List<TType>();
        }

        public void Add<TType>(TType component) where TType : BaseComponentViewModel
        {
            if (!ComponentsByType.TryGetValue(typeof(TType), out var components))
            {
                components = new List<BaseComponentViewModel>();
            }
            if (!components.Contains(component))
            {
                components.Add(component);
                _components.Add(component);

                ComponentsByType[typeof(TType)] = components;
                OnPropertyChanged(nameof(Components));
            }
        }

        public void Remove<TType>(TType component) where TType : BaseComponentViewModel
        {
            if (ComponentsByType.TryGetValue(typeof(TType), out var components) && components.Remove(component))
            {
                _components.Remove(component);
                ComponentsByType[typeof(TType)] = components;
                OnPropertyChanged(nameof(Components));
            }
        }

        public Dictionary<Type, List<BaseComponentViewModel>> ComponentsByType { get; set; } = new Dictionary<Type, List<BaseComponentViewModel>>();
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

        #region Layering

        public void BringToFront(BaseComponentViewModel component)
        {
            if (_components.Remove(component))
            {
                _components.Insert(0, component);
                OnPropertyChanged(nameof(Components));
            }
        }

        public void SendToBack(BaseComponentViewModel component)
        {
            if (_components.Remove(component))
            {
                _components.Add(component);
                OnPropertyChanged(nameof(Components));
            }
        }

        public void BringForward(BaseComponentViewModel component)
        {
            var index = _components.IndexOf(component);
            if (index == 0)
            {
                return;
            }

            _components.RemoveAt(index);
            _components.Insert(index - 1, component);
            OnPropertyChanged(nameof(Components));
        }

        public void SendBackward(BaseComponentViewModel component)
        {
            var index = _components.IndexOf(component);
            if (index == _components.Count - 1)
            {
                return;
            }

            _components.RemoveAt(index);
            _components.Insert(index + 1, component);
            OnPropertyChanged(nameof(Components));
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

        public void Add(ConnectionViewModel connection)
        {
            if (_connections.Contains(connection))
            {
                return;
            }

            _connections.Add(connection);
            OnPropertyChanged(nameof(Connections));
        }

        public void Remove(ConnectionViewModel connection)
        {
            _connections.Remove(connection);
            OnPropertyChanged(nameof(Connections));
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
                BringToFront(item);
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
            if (IsPressed("p"))
            {
                var selectedPoint = Points.FirstOrDefault(p => p.Position == position);
                if (selectedPoint == null)
                {
                    var newPoint = this.CreatePoint(position);
                    newPoint.AddTo(this);
                }
                else
                {
                    selectedPoint.RemoveFrom(this);
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
                Selection.Adjust(MouseDownPosition, position);
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
                SelectedItems.AddRange(Items.Where(i => i.CollidesWith(Selection.Position, Selection.Size)));
                UpdateSelectedItems();
            }
        }

        private void ClickItem(Vector2 position)
        {
            var selectedItem = Items.FirstOrDefault(item => item.CollidesWith(position));

            //If no item was selected
            if (selectedItem == null)
            {
                ClearSelectedItems();

                if (NothingPressed)
                {
                    Selection.Show(position);
                }
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
            }
            //If the center of the item was selected -> Move the selected items
            else
            {
                MovingItem = true;
            }

            SelectedItems.Add(selectedItem);
            LastMousePosition = position;
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
            ConnectionPointViewModel? selectedConnection = GetComponents<ConnectionPointViewModel>().FirstOrDefault(c => c.CollidesWith(position));

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
                    var path = this.GetPath(startPoint, position);
                    Path.AddPoints(path);
                }
            }
            else
            {
                _selectedConnectionPoint.Update(position);
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
                        selectedConnectionPoint.Update(_selectedConnectionPoint.AnchorPoint);
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