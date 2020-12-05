using System;
using System.Threading.Tasks;
using Aptacode.FlowDesigner.Core.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Aptacode.FlowDesigner.Blazor.Components
{
    public class ConnectionBase : ComponentBase
    {
        [Parameter] public ConnectionViewModel Connection { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }
    }
}