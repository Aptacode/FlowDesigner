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

        public int AnchorPointCount => (int) (2 * (Size.X + Size.Y));

        public bool CollidesWith(Vector2 position) =>
            position.X >= Position.X && position.X <= Position.X + Size.X &&
            position.Y >= Position.Y && position.Y <= Position.Y + Size.Y;
    }
}