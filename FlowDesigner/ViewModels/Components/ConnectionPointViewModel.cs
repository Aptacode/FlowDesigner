using Aptacode.FlowDesigner.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public class ConnectionPointViewModel : BaseComponentViewModel
    {
        private Vector2 _anchorPoint;
        private Vector2 _anchorPointDelta;
        private float _connectionPointSize;

        public ConnectionPointViewModel(Guid id, ConnectedComponentViewModel item) : base(id)
        {
            Item = item;
            Item.PropertyChanged += Item_PropertyChanged;
            ConnectionPointSize = 1.0f;
            UpdateAnchorPointDelta(new Vector2(Item.TopRight.X, (float) Math.Floor(Item.MidPoint.Y)));
        }

        public List<ConnectionViewModel> Connections { get; set; } = new List<ConnectionViewModel>();

        public ConnectedComponentViewModel Item { get; set; }

        public float ConnectionPointSize
        {
            get => _connectionPointSize;
            set => SetProperty(ref _connectionPointSize, value);
        }

        public Vector2 AnchorPoint
        {
            get => _anchorPoint;
            set => SetProperty(ref _anchorPoint, value);
        }

        public Vector2 AnchorPointDelta
        {
            get => _anchorPointDelta;
            set => SetProperty(ref _anchorPointDelta, value);
        }

        public ResizeDirection AnchorPointFace { get; set; }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ConnectedComponentViewModel.Position):
                    AnchorPoint = Item.MidPoint - AnchorPointDelta;
                    break;
                case nameof(ConnectedComponentViewModel.Size):
                    var tempAnchorPoint = AnchorPoint;

                    var clampedX = Math.Clamp(AnchorPoint.X, Item.Position.X + 1, Item.Position.X + Item.Size.X -1);
                    var clampedY = Math.Clamp(AnchorPoint.Y, Item.Position.Y + 1, Item.Position.Y + Item.Size.Y -1);

                    switch (AnchorPointFace)
                    {
                        case ResizeDirection.N:
                            tempAnchorPoint = new Vector2(clampedX, Item.Position.Y);
                            break;
                        case ResizeDirection.E:
                            tempAnchorPoint = new Vector2(Item.Position.X + Item.Size.X, clampedY);
                            break;
                        case ResizeDirection.S:
                            tempAnchorPoint = new Vector2(clampedX, Item.Position.Y + Item.Size.Y);
                            break;
                        case ResizeDirection.W:
                            tempAnchorPoint = new Vector2(Item.Position.X, clampedY);
                            break;
                    }

                    AnchorPointDelta = Item.MidPoint - tempAnchorPoint;
                    if(_anchorPoint != tempAnchorPoint)
                    {
                        _anchorPoint = tempAnchorPoint;
                        Redraw();
                        AnchorPoint = tempAnchorPoint;
                    }

                    break;
            }
        }

        public void UpdateAnchorPointDelta(Vector2 mousePosition)
        {
            var tempAnchorPoint = AnchorPoint;

            if (mousePosition.Y <= Item.TopLeft.Y && mousePosition.X >= Item.TopLeft.X &&
                mousePosition.X <= Item.TopRight.X)
            {
                tempAnchorPoint = GetIntersection(Item.TopLeft, Item.TopRight, Item.MidPoint, mousePosition);
                AnchorPointFace = ResizeDirection.N;
            }
            else if (mousePosition.X >= Item.TopRight.X && mousePosition.Y >= Item.TopRight.Y &&
                     mousePosition.Y <= Item.BottomRight.Y)
            {
                AnchorPointFace = ResizeDirection.E;
                tempAnchorPoint = GetIntersection(Item.TopRight, Item.BottomRight, Item.MidPoint, mousePosition);
            }
            else if (mousePosition.Y >= Item.BottomRight.Y && mousePosition.X >= Item.TopLeft.X &&
                     mousePosition.X <= Item.TopRight.X)
            {
                AnchorPointFace = ResizeDirection.S;
                tempAnchorPoint = GetIntersection(Item.BottomRight, Item.BottomLeft, Item.MidPoint, mousePosition);
            }
            else if (mousePosition.X <= Item.TopLeft.X && mousePosition.Y >= Item.TopRight.Y &&
                     mousePosition.Y <= Item.BottomRight.Y)
            {
                AnchorPointFace = ResizeDirection.W;
                tempAnchorPoint = GetIntersection(Item.BottomLeft, Item.TopLeft, Item.MidPoint, mousePosition);
            }

            AnchorPointDelta = Item.MidPoint - tempAnchorPoint;
            if (_anchorPoint != tempAnchorPoint)
            {
                _anchorPoint = tempAnchorPoint;
                Redraw();
                AnchorPoint = tempAnchorPoint;
            }
        }

        public (float m, float c) ToLineEquation(Vector2 start, Vector2 end)
        {
            if (Math.Abs(end.X - start.X) < Constants.Tolerance)
            {
                return (float.PositiveInfinity, start.X);
            }

            var m = (end.Y - start.Y) / (end.X - start.X);
            var c = -m * start.X + start.Y;
            return (m, c);
        }

        public Vector2 GetIntersection(Vector2 edgeStart, Vector2 edgeEnd, Vector2 itemCenter, Vector2 mousePosition)
        {
            var (edgeM, edgeC) = ToLineEquation(edgeStart, edgeEnd);
            var (mouseM, mouseC) = ToLineEquation(itemCenter, mousePosition);

            //Vertical Edge
            var xIntersect = float.IsPositiveInfinity(edgeM) ? edgeC : mousePosition.X;

            //Vertical mouse line
            var yIntersect = float.IsPositiveInfinity(mouseM) ? edgeStart.Y : mousePosition.Y;

            var minX = Item.Position.X;
            var minY = Item.Position.Y;
            var maxX = Item.Position.X + Item.Size.X;
            var maxY = Item.Position.Y + Item.Size.Y;

            if (xIntersect <= minX && yIntersect <= minY)
            {
                xIntersect = minX + 1;
                yIntersect = minY;
            }
            else if (xIntersect >= maxX && yIntersect <= minY)
            {
                xIntersect = maxX;
                yIntersect = minY + 1;
            }
            else if (xIntersect >= maxX && yIntersect >= maxY)
            {
                xIntersect = maxX - 1;
                yIntersect = maxY;
            }
            else if (xIntersect <= minX && yIntersect >= maxY)
            {
                xIntersect = minX;
                yIntersect = maxY - 1;
            }
            else
            {
                xIntersect = Math.Clamp(xIntersect, Item.Position.X, Item.Position.X + Item.Size.X);
                yIntersect = Math.Clamp(yIntersect, Item.Position.Y, Item.Position.Y + Item.Size.Y);
            }

            return new Vector2(xIntersect, yIntersect);
        }

        public bool CollidesWith(Vector2 position) =>
            position.X >= AnchorPoint.X - ConnectionPointSize &&
            position.X <= AnchorPoint.X + ConnectionPointSize &&
            position.Y >= AnchorPoint.Y - ConnectionPointSize && position.Y <= AnchorPoint.Y + ConnectionPointSize;

        public Vector2 GetOffset(float offset)
        {
            if (_anchorPoint == Item.TopLeft)
            {
                return _anchorPoint + new Vector2(-offset, -offset);
            }

            if (_anchorPoint == Item.TopRight)
            {
                return _anchorPoint + new Vector2(offset, -offset);
            }

            if (_anchorPoint == Item.BottomRight)
            {
                return _anchorPoint + new Vector2(offset, offset);
            }

            if (_anchorPoint == Item.BottomLeft)
            {
                return _anchorPoint + new Vector2(-offset, offset);
            }

            if (Math.Abs(_anchorPoint.Y - Item.TopLeft.Y) < Constants.Tolerance)
            {
                return _anchorPoint + new Vector2(0, -offset);
            }

            if (Math.Abs(_anchorPoint.Y - Item.BottomRight.Y) < Constants.Tolerance)
            {
                return _anchorPoint + new Vector2(0, offset);
            }

            if (Math.Abs(_anchorPoint.X - Item.TopLeft.X) < Constants.Tolerance)
            {
                return _anchorPoint + new Vector2(-offset, 0);
            }

            if (Math.Abs(_anchorPoint.X - Item.BottomRight.X) < Constants.Tolerance)
            {
                return _anchorPoint + new Vector2(offset, 0);
            }

            return Vector2.Zero;
        }

        internal void Redraw()
        {
            foreach (var connection in Connections)
            {
                connection.Redraw();
            }
        }
    }
}