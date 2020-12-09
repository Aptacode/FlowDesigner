using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public abstract class PolygonViewModel : ComponentViewModel
    {
        private Vector2 _position;

        protected PolygonViewModel(Guid id, Vector2 position) : base(id)
        {
            Position = position;
        }

        #region Properties

        public Vector2 Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        #endregion

        #region Designer

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

        public override bool CollidesWith(CollisionType type, params Vector2[] vertices)
        {
            if (!CollisionsAllowed)
            {
                return false;
            }

            if (type == CollisionType.Margin)
            {
                return PointsWithMargin.ToArray().Any(p => Collider.Collides(vertices, p));
            }
            else
            {
                return Collider.Collides(vertices, Points.ToArray());
            }
        }

        public override void Move(DesignerViewModel designer, Vector2 delta)
        {
            for (var i = 0; i < _points.Count; i++)
            {
                _points[i] += delta;
            }

            Position = _points[0];
        }
    }
}