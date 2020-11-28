using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Aptacode.FlowDesigner.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace FlowDesigner.Blazor.Demo.Pages
{
    public class DiagramBase : ComponentBase
    {
        public DesignerViewModel Designer { get; set; }

        public ItemViewModel? SelectedItem { get; set; }

        private Vector2 MouseDelta { get; set; }

        Vector2 ConnectionMouseDelta;
        ConnectedItem connectedItem;

        protected override async Task OnInitializedAsync()
        {
            Designer = new DesignerViewModel();
            Designer.CreateItem.Execute(("State 1", new Vector2(10, 10), new Vector2(10, 4)));
            Designer.CreateItem.Execute(("State 2", new Vector2(10, 20), new Vector2(10, 4)));
            Designer.CreateItem.Execute(("State 3", new Vector2(25, 30), new Vector2(10, 4)));
            Designer.CreateItem.Execute(("State 4", new Vector2(10, 40), new Vector2(10, 4)));
            Designer.CreateItem.Execute(("State 5", new Vector2(10, 50), new Vector2(10, 4)));
            Designer.CreateItem.Execute(("State 6", new Vector2(25, 60), new Vector2(10, 4)));
            var items = Designer.Items.ToList();
            Designer.Connect.Execute((items[0], items[1]));
            //Designer.Connect.Execute((items[0], items[2]));
            //Designer.Connect.Execute((items[0], items[3]));
            //Designer.Connect.Execute((items[0], items[4]));

            await base.OnInitializedAsync();
        }

        public void MouseDown(MouseEventArgs e)
        {
            if (SelectedItem == null)
            {
                ClickItem(e);
            }

            if (connectedItem == null)
            {
                ClickConnection(e);
            }
        }

        public void MouseUp(MouseEventArgs e)
        {
            if (SelectedItem != null)
            {
                ReleaseItem(e);
            }
            if (connectedItem != null)
            {
                ReleaseConnection(e);
            }

        }

        public void MouseOut(MouseEventArgs e) { }

        Vector2 _lastDrawPoint = Vector2.Zero;
        public void MouseMove(MouseEventArgs e)
        {
            if (SelectedItem != null)
            {
                MoveItem(e);
            }
            if (connectedItem != null)
            {
                MoveConnection(e);
   
            }
        }
        private void ClickItem(MouseEventArgs e)
        {

            var mousePosition = new Vector2((int)(e.OffsetX / 10.0f), (int)(e.OffsetY / 10.0f));

            foreach (var item in Designer.Items)
            {
                if (!item.CollidesWith(mousePosition))
                {
                    continue;
                }

                SelectedItem = item;
                break;
            }

            if (SelectedItem == null)
            {
                return;
            }

            Designer.BringToFront(SelectedItem);
            MouseDelta = mousePosition - SelectedItem.Position;

            Console.WriteLine($"Click {SelectedItem.Label}{SelectedItem.Position}");
        }

        private void MoveItem(MouseEventArgs e)
        {
            var mouseX = (int) (e.OffsetX / 10.0);
            var mouseY = (int)(e.OffsetY / 10.0);
            var newPosition = new Vector2(mouseX, mouseY) - MouseDelta;

            SelectedItem.Position = newPosition;

            if (!((newPosition - _lastDrawPoint).Length() > 2.0f))
            {
                return;
            }

            _lastDrawPoint = newPosition;
            var timer = new Stopwatch();
            timer.Start();

            foreach (var connection in Designer.Connections.Where(c => c.Item1.Item == SelectedItem || c.Item2.Item == SelectedItem))
            {
                connection.Refresh();
            }

            timer.Stop();
            Console.WriteLine($"Total Elapsed: {timer.ElapsedMilliseconds}ms");
        }

        private void ReleaseItem(MouseEventArgs e)
        {
            var mousePosition = new Vector2((int)(e.OffsetX / 10.0), (int)(e.OffsetY / 10.0));
            SelectedItem.Position = mousePosition - MouseDelta;

            Console.WriteLine($"Release {SelectedItem.Label}{SelectedItem.Position}");

            var timer = new Stopwatch();
            timer.Start();

            foreach (var connection in Designer.Connections.Where(c => c.Item1.Item == SelectedItem || c.Item2.Item == SelectedItem))
            {
                connection.Refresh();
            }

            timer.Stop();
            Console.WriteLine($"Total Elapsed: {timer.ElapsedMilliseconds}ms");

            SelectedItem = null;
        }

        private void ClickConnection(MouseEventArgs e)
        {
            var mousePosition = new Vector2((int)(e.OffsetX / 10.0f), (int)(e.OffsetY / 10.0f));

            foreach (var connection in Designer.Connections)
            {
                if (connection.Item1.CollidesWith(mousePosition))
                {
                    connectedItem = connection.Item1;
                    break;
                }
                if (connection.Item2.CollidesWith(mousePosition))
                {
                    connectedItem = connection.Item2;
                    break;
                }

                break;
            }
            if(connectedItem != null)
            {
                ConnectionMouseDelta = mousePosition - connectedItem.ConnectionPoint;
            }
        }

        private void MoveConnection(MouseEventArgs e)
        {
            if (connectedItem == null)
            {
                return;
            }

            var mousePosition = new Vector2((int)(e.OffsetX / 10.0), (int)(e.OffsetY / 10.0));
            var newAnchorPosition = mousePosition - ConnectionMouseDelta;
            connectedItem.AnchorPoint = connectedItem.ToAnchorPoint(newAnchorPosition);
            Console.WriteLine(connectedItem.AnchorPoint);
        }

        private void ReleaseConnection(MouseEventArgs e)
        {
            connectedItem = null;

        }
    }
}