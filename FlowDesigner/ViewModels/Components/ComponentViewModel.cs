using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Aptacode.CSharp.Common.Utilities.Mvvm;
using Aptacode.FlowDesigner.Core.Enums;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public abstract class ComponentViewModel : BindableBase, ICollider
    {
        private readonly Color _fillColor = Color.White;
        private Color _borderColor = Color.Black;
        private float _borderThickness;
        private bool _isShown = true;
        protected bool IsSelected = false;
        private float _margin;
        protected Vector2[] _points = new Vector2[0];

        public IEnumerable<Vector2[]> PointsWithMargin => _points.Select(p => new Vector2[4]
        {
            p - new Vector2(Margin, Margin),
            p + new Vector2(Margin, -Margin),
            p + new Vector2(Margin, Margin),
            p + new Vector2(-Margin, Margin)
        });

        protected ComponentViewModel(Guid id)
        {
            Id = id;
            IsShown = true;
            _borderThickness = 0.3f;
            _margin = 2.0f;
            CollisionsAllowed = true;
        }

        public Vector2[] Points
        {
            get => _points;
            set => SetProperty(ref _points, value);
        }

        #region Properties

        public Guid Id { get; set; }

        public float Margin
        {
            get => _margin;
            set => SetProperty(ref _margin, value);
        }

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

        #endregion

        #region Designer

        public abstract void AddTo(DesignerViewModel designer);
        public abstract void RemoveFrom(DesignerViewModel designer);

        #region Selection

        public virtual void Deselect(DesignerViewModel designer)
        {
            if (!IsSelected)
            {
                return;
            }

            IsSelected = false;

            BorderColor = Color.Black;
            designer.BringToFront(this);
        }

        public virtual void Select(DesignerViewModel designer)
        {
            if (IsSelected)
            {
                return;
            }

            IsSelected = true;

            BorderColor = Color.Green;
            designer.BringToFront(this);
        }

        #endregion

        #endregion

        #region Transformations
        public virtual void Move(DesignerViewModel designer, Vector2 delta)
        {
            for (var i = 0; i < _points.Count(); i++)
            {
                _points[i] += delta;
            }
        }

        public abstract void Resize(DesignerViewModel designer, Vector2 delta, ResizeDirection direction);
        public abstract void Resize(DesignerViewModel designer, Vector2 delta);

        #endregion

        #region  Collisions

        public bool CollisionsAllowed { get; set; }

        public abstract bool CollidesWith(CollisionType type, params Vector2[] vertices);

        #endregion
    }
}