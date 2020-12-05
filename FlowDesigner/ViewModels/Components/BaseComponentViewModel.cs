using System;
using System.Drawing;
using Aptacode.CSharp.Common.Utilities.Mvvm;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public abstract class BaseComponentViewModel : BindableBase
    {
        private Color _borderColor = Color.Black;
        private float _borderThickness;
        private readonly Color _fillColor = Color.White;
        private bool _isShown;
        private int _z;

        protected BaseComponentViewModel(Guid id)
        {
            Id = id;
            Z = 10;
            IsShown = true;
            _borderThickness = 0.3f;
        }

        public Guid Id { get; set; }

        public bool IsShown
        {
            get => _isShown;
            set => SetProperty(ref _isShown, value);
        }

        public int Z
        {
            get => _z;
            set => SetProperty(ref _z, value);
        }

        public Color BorderColor
        {
            get => _borderColor;
            set => SetProperty(ref _borderColor, value);
        }

        public Color FillColor
        {
            get => _fillColor;
            set => SetProperty(ref _borderColor, value);
        }

        public float BorderThickness
        {
            get => _borderThickness;
            set => SetProperty(ref _borderThickness, value);
        }
    }
}