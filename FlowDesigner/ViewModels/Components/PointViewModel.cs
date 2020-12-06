﻿using System;
using System.Numerics;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public class PointViewModel : BaseShapeViewModel
    {
        public PointViewModel(Guid id, Vector2 position) : base(id, position, new Vector2(0, 0))
        {
            CollisionsAllowed = false;
        }

        public override void AddTo(DesignerViewModel designer)
        {
            designer.Add(this);
        }

        public override void RemoveFrom(DesignerViewModel designer)
        {
            designer.Remove(this);
        }
    }
}