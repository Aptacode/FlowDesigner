using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        #region Mouse
        public ItemViewModel? SelectedItem { get; set; }
        public ConnectedItem? SelectedConnection { get; set; }

        private Vector2 MouseDelta { get; set; }
        private Vector2 ConnectionMouseDelta { get; set; }

        public void MouseDown(Vector2 position)
        {
            if (SelectedItem == null)
            {
                ClickItem(position);
            }

            if (SelectedConnection == null)
            {
                ClickConnection(position);
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

        Vector2 _lastDrawPoint = Vector2.Zero;
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
            MouseDelta = position - SelectedItem.Position;

            Console.WriteLine($"Click {SelectedItem.Label}{SelectedItem.Position}");
        }

        private void MoveItem(Vector2 position)
        {
            var newPosition = position - MouseDelta;

            SelectedItem.Position = newPosition;

            if (!((newPosition - _lastDrawPoint).Length() > 2.0f))
            {
                return;
            }

            _lastDrawPoint = newPosition;
            var timer = new Stopwatch();
            timer.Start();

            foreach (var connection in Connections.Where(c => c.Item1.Item == SelectedItem || c.Item2.Item == SelectedItem))
            {
                connection.Refresh();
            }

            timer.Stop();
            Console.WriteLine($"Total Elapsed: {timer.ElapsedMilliseconds}ms");
        }

        private void ReleaseItem(Vector2 position)
        {
            SelectedItem.Position = position - MouseDelta;

            Console.WriteLine($"Release {SelectedItem.Label}{SelectedItem.Position}");

            var timer = new Stopwatch();
            timer.Start();

            foreach (var connection in Connections.Where(c => c.Item1.Item == SelectedItem || c.Item2.Item == SelectedItem))
            {
                connection.Refresh();
            }

            timer.Stop();
            Console.WriteLine($"Total Elapsed: {timer.ElapsedMilliseconds}ms");

            SelectedItem = null;
        }

        private void ClickConnection(Vector2 position)
        {
            foreach (var connection in Connections)
            {
                if (connection.Item1.CollidesWith(position))
                {
                    SelectedConnection = connection.Item1;
                    break;
                }
                if (connection.Item2.CollidesWith(position))
                {
                    SelectedConnection = connection.Item2;
                    break;
                }

                break;
            }

            if (SelectedConnection != null)
            {
                ConnectionMouseDelta = position - SelectedConnection.ConnectionPoint;
            }
        }

        private void MoveConnection(Vector2 position)
        {
            if (SelectedConnection == null)
            {
                return;
            }

            var newAnchorPosition = position - ConnectionMouseDelta;
            SelectedConnection.AnchorPoint = SelectedConnection.ToAnchorPoint(newAnchorPosition);
            Console.WriteLine(SelectedConnection.AnchorPoint);
        }

        private void ReleaseConnection(Vector2 position)
        {
            SelectedConnection = null;
        }

        #endregion
    }
}