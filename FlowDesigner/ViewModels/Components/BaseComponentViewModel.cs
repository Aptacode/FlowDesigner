﻿using System;
using System.Drawing;
using System.Numerics;
using Aptacode.CSharp.Common.Utilities.Mvvm;
using Aptacode.FlowDesigner.Core.Enums;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public abstract class BaseComponentViewModel : BindableBase
    {
        private readonly Color _fillColor = Color.White;
        private Color _borderColor = Color.Black;
        private float _borderThickness;
        private bool _isShown = true;
        protected bool _isSelected = false;

        protected BaseComponentViewModel(Guid id)
        {
            Id = id;
            IsShown = true;
            _borderThickness = 0.3f;
        }

        public Guid Id { get; set; }

        public bool IsShown
        {
            get => _isShown;
            set => SetProperty(ref _isShown, value);
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


        public abstract void AddTo(DesignerViewModel designer);
        public abstract void RemoveFrom(DesignerViewModel designer);

        public abstract void Move(DesignerViewModel designer, Vector2 delta);
        public abstract void Resize(DesignerViewModel designer, Vector2 delta, ResizeDirection direction);
        public abstract void Resize(DesignerViewModel designer, Vector2 delta);

        public virtual void Deselect(DesignerViewModel designer)
        {
            if (_isSelected)
            {
                _isSelected = false;

                BorderColor = Color.Black;
                designer.BringToFront(this);
            }
        }

        public virtual void Select(DesignerViewModel designer)
        {
            if (!_isSelected)
            {
                _isSelected = true;

                BorderColor = Color.Green;
                designer.BringToFront(this);
            }
        }
    }
}