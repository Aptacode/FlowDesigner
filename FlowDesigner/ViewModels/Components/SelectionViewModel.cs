﻿using System;
using System.Numerics;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public class SelectionViewModel : BaseShapeViewModel
    {
        public SelectionViewModel(Guid id, Vector2 position, Vector2 size) : base(id, position, size)
        {
            Margin = 0.0f;
            IsShown = false;
        }
    }
}