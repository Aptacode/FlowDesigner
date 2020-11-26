using Aptacode.CSharp.Common.Utilities.Mvvm;
using Aptacode.PathFinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aptacode.FlowDesigner.Core.ViewModels
{

    public class ConnectedItem
    {
        public ConnectedItem(ItemViewModel item, ConnectionMode mode, int anchorPoint)
        {
            Item = item;
            AnchorPoint = anchorPoint;
            Mode = mode;
        }

        public ItemViewModel Item { get; set; }
        public int AnchorPoint { get; set; }
        public ConnectionMode Mode { get; set; }

        public Vector2 GetConnectionPoint()
        {
            if(AnchorPoint <= Item.Size.X)
            {
                return new Vector2(Item.Position.X + AnchorPoint, Item.Position.Y);
            }

            if (AnchorPoint <= Item.Size.X + Item.Size.Y)
            {
                return new Vector2(Item.Position.X + Item.Size.X, Item.Position.Y + AnchorPoint - Item.Size.X);
            }

            if (AnchorPoint <= Item.Size.X * 2 + Item.Size.Y)
            {
                var xOffset = AnchorPoint - 2 * Item.Size.X - Item.Size.Y;
                return new Vector2(Item.Position.X + Item.Size.X - xOffset, Item.Position.Y);
            }

            var yOffset = AnchorPoint - 2 * Item.Size.X - 2 * Item.Size.Y;
            return new Vector2(Item.Position.X, Item.Position.Y + yOffset);
        }
    }
}
