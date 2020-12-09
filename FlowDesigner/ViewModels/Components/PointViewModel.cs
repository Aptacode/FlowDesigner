using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Aptacode.FlowDesigner.Core.Enums;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public class PointViewModel : ComponentViewModel
    {
        private Vector2 _position;

        public PointViewModel(Guid id, Vector2 position) : base(id)
        {
            _points = new Vector2[1]
            {
                position
            };
            Position = position;
        }

        #region Properties

        public Vector2 Position
        {
            get => _points[0];
            set
            {
                _points[0] = value;
                SetProperty(ref _position, value);
            }
        }

        #endregion

        #region Designer

        #region Selection

        public override void AddTo(DesignerViewModel designer)
        {
            designer.Add(this);
        }

        public override void RemoveFrom(DesignerViewModel designer)
        {
            designer.Remove(this);
        }

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

        public override void Resize(DesignerViewModel designer, Vector2 delta, ResizeDirection direction)
        {
            
        }

        public override void Resize(DesignerViewModel designer, Vector2 delta)
        {

        }

        public override bool CollidesWith(CollisionType type, params Vector2[] vertices)
        {
            return CollisionsAllowed && Collider.Collides(vertices, _position);
        }

        #endregion

        #endregion
    }
}