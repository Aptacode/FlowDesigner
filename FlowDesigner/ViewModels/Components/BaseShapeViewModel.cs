using System;
using System.Linq;
using System.Numerics;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public abstract class BaseShapeViewModel : BaseComponentViewModel, ICollidor
    {
        private readonly float _edgeThickness;
        private float _margin;
        private Vector2 _position;
        private Vector2 _size;

        protected BaseShapeViewModel(Guid id, Vector2 position, Vector2 size) : base(id)
        {
            Position = position;
            Size = size;
            _margin = 2.0f;
            _edgeThickness = 1.0f;
            CollisionsAllowed = true;
        }

        public Vector2 Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public Vector2 Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }

        public float Margin
        {
            get => _margin;
            set => SetProperty(ref _margin, value);
        }

        public Vector2 MidPoint => Position + Size / 2;

        public Vector2 TopLeft => Position;
        public Vector2 TopRight => Position + Size * new Vector2(1, 0);
        public Vector2 BottomRight => Position + Size;
        public Vector2 BottomLeft => Position + Size * new Vector2(0, 1);

        public Vector2 SizeAndMargin => Size + new Vector2(Margin, Margin);
        public Vector2 PositionAndMargin => Position - new Vector2(Margin, Margin);

        public bool CollisionsAllowed { get; set; }

        public virtual bool CollidesWith(Vector2 position) =>
            CollisionsAllowed &&
            position.X >= Position.X && position.X <= Position.X + Size.X &&
            position.Y >= Position.Y && position.Y <= Position.Y + Size.Y;

        public virtual bool CollidesWithEdge(Vector2 position) =>
            CollisionsAllowed &&
            CollidesWith(position) &&
            (position.X < Position.X + _edgeThickness || position.X >= Position.X + Size.X - _edgeThickness) ||
            position.Y < Position.Y + _edgeThickness || position.Y >= Position.Y + Size.Y - _edgeThickness;

        public virtual bool CollidesWith(params Vector2[] points) =>
            CollisionsAllowed &&
            points.Any(CollidesWith);

        public virtual bool CollidesWith(Vector2 point, Vector2 shape) =>
            CollisionsAllowed &&
            Position.X < point.X + shape.X + Margin * 2 &&
            TopRight.X > point.X - Margin * 2 &&
            Position.Y < point.Y + shape.Y + Margin * 2 &&
            BottomRight.Y > point.Y - Margin * 2;
    }
}