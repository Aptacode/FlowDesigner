using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using Aptacode.AppFramework.Components.Primitives;
using Aptacode.BlazorCanvas;
using Aptacode.Geometry.Collision.Rectangles;
using Aptacode.Geometry.Primitives;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public class ConnectedComponentViewModel : PolygonViewModel
    {
        #region Ctor

        public ConnectedComponentViewModel(Polygon body) : base(body)
        {
            FillColor = Color.White;
            Margin = 1;
            ConnectionPoints = new List<ConnectionPointViewModel>();
        }

        #endregion

        public ConnectionPointViewModel AddConnectionPoint()
        {
            var ellipse = new Ellipse(Body.TopLeft + new Vector2(4, 0), new Vector2(1, 1), 0);
            var connectionPoint = new ConnectionPointViewModel(this, ellipse)
            {
                FillColor = FillColor
            };
            ConnectionPoints.Add(connectionPoint);
            Add(connectionPoint);
            return connectionPoint;
        }

        public override async Task Draw(BlazorCanvasInterop ctx)
        {
            OldBoundingRectangle = BoundingRectangle;
            Invalidated = false;

            if (!IsShown)
            {
                return;
            }

            ctx.FillStyle(FillColorName);

            ctx.StrokeStyle(BorderColorName);

            ctx.LineWidth(BorderThickness);

            foreach (var child in Children)
            {
                await child.Draw(ctx);
            }

            await CustomDraw(ctx);
            await DrawText(ctx);
        }

        #region Prop

        public BoundingRectangle Body => Polygon.BoundingRectangle;
        public List<ConnectionPointViewModel> ConnectionPoints { get; }

        #endregion
    }
}