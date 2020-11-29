using System;
using System.Numerics;
using Aptacode.CSharp.Common.Utilities.Mvvm;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class ItemViewModel : BindableBase
    {
        private Vector2 _position;

        private Vector2 _size;

        private int _z;

        public ItemViewModel(Guid id, string label, Vector2 position, Vector2 size)
        {
            Id = id;
            Label = label;
            Position = position;
            Size = size;
            Z = 10;
        }

        public Guid Id { get; set; }
        public string Label { get; set; }

        public int Z
        {
            get => _z;
            set => SetProperty(ref _z, value);
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

        public Vector2 MidPoint => Position + Size / 2;

        public Vector2 TopLeft => Position;
        public Vector2 TopRight => Position + Size * new Vector2(1, 0);
        public Vector2 BottomRight => Position + Size;
        public Vector2 BottomLeft => Position + Size * new Vector2(0, 1);

        public bool CollidesWith(Vector2 position) =>
            position.X >= Position.X && position.X <= Position.X + Size.X &&
            position.Y >= Position.Y && position.Y <= Position.Y + Size.Y;

        public bool CollidesWithEdge(Vector2 position) =>
            CollidesWith(position) &&
            (position.X <= Position.X + 1 || position.X >= Position.X + Size.X - 1)
            &&
            (position.Y <= Position.Y + 1 || position.Y >= Position.Y + Size.Y - 1);
    }
}