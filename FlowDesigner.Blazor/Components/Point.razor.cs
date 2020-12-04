using System;
using Aptacode.FlowDesigner.Core.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Aptacode.FlowDesigner.Blazor.Components
{
    public class PointBase : ComponentBase
    {
        [Parameter] public PointViewModel Point { get; set; }
    }
}