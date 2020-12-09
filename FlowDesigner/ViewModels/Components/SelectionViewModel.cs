using System;
using System.Numerics;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public class SelectionViewModel : RectangleViewModel
    {
        public SelectionViewModel(Guid id, Vector2 position, Vector2 size) : base(id, position, size)
        {
            Margin = 0.0f;
            IsShown = false;
        }

        public override void AddTo(DesignerViewModel designer)
        {
            designer.Selection = this;
            designer.Add(this);
        }

        public override void RemoveFrom(DesignerViewModel designer)
        {
            designer.Selection = null;
            designer.Remove(this);
        }

        public void Adjust(Vector2 startPosition, Vector2 currentPosition)
        {
            Size = Vector2.Abs(startPosition - currentPosition);

            if (currentPosition.X <= startPosition.X)
            {
                Position = currentPosition.Y <= startPosition.Y
                    ? currentPosition
                    : new Vector2(currentPosition.X, startPosition.Y);
            }
            else
            {
                Position = currentPosition.Y <= startPosition.Y
                    ? new Vector2(startPosition.X, currentPosition.Y)
                    : startPosition;
            }

            _points = new Vector2[]
            {
                TopLeft,
                 TopRight,
                 BottomRight,
                 BottomLeft
            };
        }

        public void Show(Vector2 position)
        {
            IsShown = true;
            Position = position;
            Size = Vector2.Zero;
        }
    }
}