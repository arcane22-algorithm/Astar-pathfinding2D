using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace v1.Pathfinding2D
{
    // Pathfinding2D node class
    public class Pathfinding2DNode
    {
        public string name;
        public int fcost, gcost, hcost;
        public Vector2 position, size;
        public Pathfinding2DNode parent;

        public Pathfinding2DNode()
        {
        }
        
        public Pathfinding2DNode(Pathfinding2DNode parent, Vector2 position)
        {
            this.parent = parent;
            this.position = position;
            this.name = position.ToString();
        }
        
        public Pathfinding2DNode(Pathfinding2DNode parent, Vector2 position, Vector2 size)
        {
            this.parent = parent;
            this.position = position;
            this.size = size;
            this.name = position.ToString();
        }

        public override bool Equals(object node)
        {
            if (node == null) return false;
            else return ((Pathfinding2DNode)node).name == this.name;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
