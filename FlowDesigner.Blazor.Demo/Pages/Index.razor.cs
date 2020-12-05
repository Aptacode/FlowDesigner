using Aptacode.FlowDesigner.Core.Utilities;
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

            var item1 = Designer.CreateConnectedComponent("State 1", new Vector2(10, 8), new Vector2(15, 5));
            var item2 = Designer.CreateConnectedComponent("State 2", new Vector2(10, 20), new Vector2(15, 5));
            var item3 = Designer.CreateConnectedComponent("State 3", new Vector2(40, 40), new Vector2(15, 5));
            var item4 = Designer.CreateConnectedComponent("State 4", new Vector2(40, 60), new Vector2(15, 5));
            var item5 = Designer.CreateConnectedComponent("State 5", new Vector2(40, 70), new Vector2(15, 5));
            var item6 = Designer.CreateConnectedComponent("State 6", new Vector2(70, 70), new Vector2(15, 5));

            var connectionPoint1 = Designer.CreateConnectionPoint(item1);
            var connectionPoint2 = Designer.CreateConnectionPoint(item2);
            var connectionPoint3 = Designer.CreateConnectionPoint(item3);
            var connectionPoint4 = Designer.CreateConnectionPoint(item4);

            var connection = connectionPoint1.Connect(Designer, connectionPoint2);
            var connection2 = connectionPoint1.Connect(Designer, connectionPoint3);

            await base.OnInitializedAsync();
        }
    }
}
