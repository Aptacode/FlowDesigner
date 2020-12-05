using System;
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
        private bool _isShown;

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


        public virtual void AddTo(DesignerViewModel designer)
        {
            designer.Add(this);
        }

        public virtual void RemoveFrom(DesignerViewModel designer)
        {
            designer.Remove(this);
        }

        public abstract void Move(DesignerViewModel designer, Vector2 delta);
        public abstract void Resize(DesignerViewModel designer, Vector2 delta, ResizeDirection direction);


        #region Layering

        public void BringToFront(DesignerViewModel designer)
        {
            if (designer.Components.Remove(this))
            {
                designer.Components.Insert(0, this);
            }
        }

        public void SendToBack(DesignerViewModel designer)
        {
            if (designer.Components.Remove(this))
            {
                designer.Components.Add(this);
            }
        }

        public void BringForward(DesignerViewModel designer)
        {
            var index = designer.Components.IndexOf(this);
            if (index == 0)
            {
                return;
            }

            designer.Components.RemoveAt(index);
            designer.Components.Insert(index - 1, this);
        }

        public void SendBackward(DesignerViewModel designer)
        {
            var index = designer.Components.IndexOf(this);
            if (index == designer.Components.Count - 1)
            {
                return;
            }

            designer.Components.RemoveAt(index);
            designer.Components.Insert(index + 1, this);
        }

        #endregion
    }
}