using Aptacode.FlowDesigner.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace FlowDesigner.Blazor.Demo.Pages
{
    public class IndexBase : ComponentBase
    {

        public DesignerViewModel Designer { get; set; }

        protected async override Task OnInitializedAsync()
        {
            Designer = new DesignerViewModel(200,100);

            var item1 = Designer.AddItem("State 1", new Vector2(10, 8), new Vector2(15, 5));
            var item2 = Designer.AddItem("State 2", new Vector2(10, 20), new Vector2(15, 5));
            var item3 = Designer.AddItem("State 3", new Vector2(40, 40), new Vector2(15, 5));
            var item4 = Designer.AddItem("State 4", new Vector2(40, 60), new Vector2(15, 5));
            var item5 = Designer.AddItem("State 5", new Vector2(40, 70), new Vector2(15, 5));
            var item6 = Designer.AddItem("State 6", new Vector2(70, 70), new Vector2(15, 5));

            var connectionPoint1 = Designer.AddConnectionPoint(item1);
            var connectionPoint2 = Designer.AddConnectionPoint(item2);
            var connectionPoint3 = Designer.AddConnectionPoint(item3);
            var connectionPoint4 = Designer.AddConnectionPoint(item4);

            Designer.AddConnection(connectionPoint1, connectionPoint2);
            Designer.AddConnection(connectionPoint1, connectionPoint3);
            Designer.AddConnection(connectionPoint1, connectionPoint4);
            Designer.AddConnection(connectionPoint2, connectionPoint4);
            Designer.AddConnection(connectionPoint2, connectionPoint3);

            await base.OnInitializedAsync();
        }
    }
}
