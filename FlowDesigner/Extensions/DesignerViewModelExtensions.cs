using Aptacode.FlowDesigner.Core.ViewModels;
using Aptacode.FlowDesigner.Core.ViewModels.Components;
using Aptacode.PathFinder.Geometry.Neighbours;
using Aptacode.PathFinder.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;

namespace Aptacode.FlowDesigner.Core.Extensions
{
    public static class DesignerViewModelExtensions
    {
        #region PathFinding

        public static List<Vector2> GetPath(this DesignerViewModel designer, Vector2 startPoint, Vector2 endPoint, INeighbourFinder neighbourFinder = null)
        {
            var points = new List<Vector2>();

            try
            {
                var mapBuilder = new MapBuilder();

                foreach (var item in designer.ConnectedComponents.ToList())
                {
                    mapBuilder.AddObstacle(item.Position - Vector2.One, item.Size + Vector2.One * 2);
                }

                mapBuilder.SetStart(startPoint);
                mapBuilder.SetEnd(endPoint);
                mapBuilder.SetDimensions(designer.Width, designer.Height);
                var mapResult = mapBuilder.Build();
                if (!mapResult.Success)
                {
                    throw new Exception(mapResult.Message);
                }

                var pathFinder =
                    new PathFinder.Algorithm.PathFinder(mapResult.Map, neighbourFinder ?? DefaultNeighbourFinder.Straight(0.5f));

                points.AddRange(pathFinder.FindPath());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return points;
        }

        #endregion

        #region Movement

        public static void Move(
         this DesignerViewModel designer,
         BaseShapeViewModel component,
         Vector2 delta,
         List<BaseShapeViewModel> movingComponents,
         CancellationTokenSource cancellationToken)
        {
            var unselectedItems = designer.ConnectedComponents.Except(movingComponents);
            var newPosition = component.Position + delta;

            if (newPosition.X < 0 || newPosition.Y < 0 || newPosition.X + component.Size.X >= designer.Width ||
                newPosition.Y + component.Size.Y >= designer.Height)
            {
                cancellationToken.Cancel();
                return;
            }

            component.Position = newPosition;

            var collidingItems = unselectedItems
                .Where(i => i.CollidesWith(component.PositionAndMargin, component.SizeAndMargin)).ToList();
            movingComponents.AddRange(collidingItems);

            foreach (var collidingItem in collidingItems)
            {
                Move(designer, collidingItem, delta, movingComponents, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                component.Position -= delta;
            }
        }


        #endregion
    }
}
