using System;
using System.Linq;
using System.Numerics;
using Aptacode.FlowDesigner.Core.ViewModels.Components;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public enum CollisionType
    {
        Normal, Edge, Margin
    }
    public interface ICollider
    {
        bool CollisionsAllowed { get; set; }

        bool CollidesWith(CollisionType type, params Vector2[] vertices);
    }

    public static class Collider
    {
        public static bool Collides(Vector2[] vertices, Vector2 point)
        {
            var collision = false;
            var edges = vertices.Zip(vertices.Skip(1), (a, b) => (a, b)).ToList();
            edges.Add((vertices.Last(), vertices.First()));
            foreach (var (a,b) in edges )
            {
                if (((a.Y >= point.Y && b.Y < point.Y) || (a.Y < point.Y && b.Y >= point.Y)) &&
                    (point.X < (b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y) + a.X))
                {
                    collision = !collision;
                }
            }


            return collision;
        }

        public static bool Collides(Vector2[] vertices1, Vector2[] vertices2)
        {
            return vertices1.Any(vertex => Collides(vertices2, vertex));
        }
    }
}