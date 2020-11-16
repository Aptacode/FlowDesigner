using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aptacode.FlowDesigner.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace FlowDesigner.Blazor.Demo.Pages
{
    public class DiagramBase : ComponentBase
    {
        public DesignerViewModel Designer { get; set; }

        protected override async Task OnInitializedAsync() {

            Designer = new DesignerViewModel();
            Designer.CreateItem.Execute("State 1");
            Designer.CreateItem.Execute("State 2");
            var items = Designer.Items.ToList();
            var item1 = items.First();
            var item2 = items.Last();
            Designer.Connect.Execute((item1, item2));

            await base.OnInitializedAsync();
        }

        public ItemViewModel? SelectedItem { get; set; }
        private int mouseDx { get; set; }
        private int mouseDy { get; set; }

        public void MouseDown(MouseEventArgs e)
        {
            var point = ((int)(e.OffsetX / 10), (int)(e.OffsetY / 10));
            foreach(var item in Designer.Items)
            {
                if (item.CollidesWith(point))
                {
                    SelectedItem = item;
                    Designer.BringToFront(SelectedItem);
                }
            }

            if(SelectedItem == null)
            {
                return;
            }

            var mouseX = ((int)((e.OffsetX) / 10)) * 10;
            var mouseY = ((int)((e.OffsetY) / 10)) * 10;
            mouseDx = mouseX - SelectedItem.X * 10;
            mouseDy = mouseY - SelectedItem.Y * 10;

        }

        public void MouseUp(MouseEventArgs args)
        {
            SelectedItem = null;
        }

        public void MouseOut(MouseEventArgs e)
        {
        }

        public void MouseMove(MouseEventArgs e)
        {
            if(SelectedItem != null)
            {
                SelectedItem.X = (((int)((e.OffsetX) / 10)) * 10 - mouseDx) / 10;
                SelectedItem.Y = (((int)((e.OffsetY) / 10)) * 10 - mouseDy) / 10;
            }
        }
    }
}
