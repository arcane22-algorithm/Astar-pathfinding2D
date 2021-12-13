/*
 * Author : Lee Hong Jun
 * Email : hong3883@naver.com
 * Date : 2017. 10. 13
 * Description
 * A* based Pathfinding2DBehavior script.
 * Pathfinding2DBehavior + Pathfinding2DNode
 * 
 * 
 */

using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace v1.Pathfinding2D
{
    public class Pathfinding2DBehavior : MonoBehaviour
    {
        public float movementSpd;
        public bool isMoving;
        
        private int _pixelsPerUnit = 100;
        private Vector2 _startPoint, _endPoint, _nodeSize, _nodePosGab;
        private Pathfinding2DNode _currentNode;
        private readonly List<Pathfinding2DNode> _path = new List<Pathfinding2DNode>();
        private readonly Dictionary<string, Pathfinding2DNode> _openList = new Dictionary<string, Pathfinding2DNode>();
        private readonly Dictionary<string, Pathfinding2DNode> _closeList = new Dictionary<string, Pathfinding2DNode>();
        private readonly Dictionary<string, Pathfinding2DNode> _obstacles = new Dictionary<string, Pathfinding2DNode>();
        
        
        // Update is called once per frame
        void Update()   
        {
            if (isMoving) MoveAlongPath();
        }

        // Initialize pathfinding data.
        void Init(Vector2 destination)
        {
            Debug.Log("init");
            isMoving = false;
            
            _path.Clear();
            _openList.Clear();
            _closeList.Clear();
            _obstacles.Clear();

            _currentNode = null;
            _startPoint = transform.position;
            _endPoint = destination;

            // Init obstacles
            GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
            foreach(GameObject o in obstacles)
            {
                Pathfinding2DNode obstacleNode = new Pathfinding2DNode(null, o.transform.position, ToWorldPosSize(o));
                _obstacles.Add(obstacleNode.name, obstacleNode);
            }
            
            _nodeSize = ToWorldPosSize(this.gameObject);
            _nodePosGab = _nodeSize;
        }

        // Translate pixel to unity world position size by using Sprite real pixel value, pixel per unit, and transform's scale.(basically, Sprite's 100 pixel = 1unit)
        public Vector2 ToWorldPosSize(GameObject obj)
        {
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            float xSize = spriteRenderer.size.x * obj.transform.lossyScale.x;
            float ySize = spriteRenderer.size.y * obj.transform.lossyScale.y;
            
            return new Vector2(xSize, ySize);
        }

        // Find path to destination. If you want to use this behavior, make Pathfinding2DBehavior field and call this function.
        public bool FindPath(Vector2 destination)
        {
            Init(destination);
            
            if (IsObstacle(new Pathfinding2DNode(null, _endPoint)))
            {
                Debug.Log("Failure");
                return false;
            }

            if (_startPoint == _endPoint)
            {
                Debug.Log("Success");
                return true;
            }

            Pathfinding2DNode startNode = new Pathfinding2DNode(null, _startPoint);
            _openList.Add(startNode.name, startNode);

            for (;;)
            {
                _currentNode = FindLowestFCostNode();
                CalculateNodePosGab(_currentNode);
                Search8Direction(_currentNode);
                _openList.Remove(_currentNode.name);
                _closeList.Add(_currentNode.name, _currentNode);
                if (_openList.Count == 0)
                {
                    Debug.Log("Failure");
                    return false;
                }

                if (IsNodeIn_closeList(new Pathfinding2DNode(null, _endPoint)))
                {
                    Debug.Log("Success");
                    _path.Add(_currentNode);
                    while (_path[_path.Count - 1].parent != null)
                    {
                        _path.Add(_path[_path.Count - 1].parent);
                    }

                    _path.Reverse();
                    for (int j = 0; j < _path.Count; j++)
                    {
                        Color color = Color.red;
                        if (j == 0)
                        {
                            color = Color.yellow;
                        }

                        if (j == _path.Count - 1)
                        {
                            color = Color.green;
                        }

                        Debug.DrawLine(new Vector3(_path[j].position.x - 0.1f, _path[j].position.y - 0.1f, 5),
                            new Vector3(_path[j].position.x + 0.1f, _path[j].position.y + 0.1f, 5), color, 5.0f);
                        Debug.DrawLine(new Vector3(_path[j].position.x - 0.1f, _path[j].position.y + 0.1f, 5),
                            new Vector3(_path[j].position.x + 0.1f, _path[j].position.y - 0.1f, 5), color, 5.0f);
                    }

                    isMoving = true;
                    return true;
                }
            }
        }

        /* Search current node's 8direction. If 8 direction node is in close, ignore it. If 8 direction node is obstacle, also ignore it.
         * If 8 direction is not in open list, add it to open list.
         * If 8 direction node is in open list, compare open list node's g cost and 8 direction node's g cost.
         */
        void Search8Direction(Pathfinding2DNode current)
        {
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    Pathfinding2DNode searchNode = new Pathfinding2DNode(current, new Vector2(current.position.x + i * _nodePosGab.x, current.position.y + j * _nodePosGab.y));
                    CalculatePathCost(searchNode);
                    if (!IsObstacle(searchNode) && !IsNodeIn_closeList(searchNode))
                    {
                        if (IsNodeIn_openList(searchNode))
                        {
                            try
                            {
                                if (_openList[searchNode.name].gcost > searchNode.gcost)
                                {
                                    _openList[searchNode.name].parent = current;
                                    CalculatePathCost(_openList[searchNode.name]);
                                }
                            }
                            catch (KeyNotFoundException e)
                            {
                                Debug.Log(e.Message);
                                Debug.Log(_nodePosGab + "/" + _openList.ContainsKey(searchNode.name) + "/" + i + "/" +
                                          j + "/" + _currentNode.position);
                            }
                        }
                        else
                        {
                            _openList.Add(searchNode.name, searchNode);
                        }
                    }
                }
            }
        }

        // If 'findPath' function find path successfully, object moves along path.
        void MoveAlongPath()
        {
            Vector2 sPos = _path[0].position; //start position
            Vector2 ePos = _path[1].position; //end position
            float dx = ePos.x - sPos.x;
            float dy = ePos.y - sPos.y;
            float dis = Mathf.Sqrt(Mathf.Pow(dx, 2) + Mathf.Pow(dy, 2));
            float dis2 = Mathf.Sqrt(Mathf.Pow(transform.position.x - sPos.x, 2) +
                                    Mathf.Pow(transform.position.y - sPos.y, 2));
            
            if (dis - dis2 > 0) transform.Translate(dx / dis * movementSpd * Time.deltaTime, dy / dis * movementSpd * Time.deltaTime, 0);
            else _path.Remove(_path[0]);
            
            if (_path.Count == 1) isMoving = false;
        }

        // If is node in open list, return true. Compare by key(nodeName)
        bool IsNodeIn_openList(Pathfinding2DNode node)
        {
            if (node == null) return false;

            if (_openList.ContainsKey(node.name)) return true;
            else return false;
        }

        // If is node in close list, return true. Compare by key(nodeName)
        bool IsNodeIn_closeList(Pathfinding2DNode node)
        {
            if (node == null)
            {
                return false;
            }
            else if (_closeList.ContainsKey(node.name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Check that is node obstacle. obstacle node can't be path. Check by box collision.
        bool IsObstacle(Pathfinding2DNode node)
        {
            float nodeX = node.position.x;
            float nodeY = node.position.y;

            foreach (Pathfinding2DNode obs in _obstacles.Values)
            {
                bool condition1 = nodeX + (_nodeSize.x * 0.5f) > obs.position.x - (obs.size.x * 0.5f);
                bool condition2 = nodeX - (_nodeSize.x * 0.5f) < obs.position.x + (obs.size.x * 0.5f);
                bool condition3 = nodeY + (_nodeSize.y * 0.5f) > obs.position.y - (obs.size.y * 0.5f);
                bool condition4 = nodeY - (_nodeSize.y * 0.5f) < obs.position.y + (obs.size.y * 0.5f);

                if (condition1 && condition2 && condition3 && condition4) return true;
            }

            return false;
        }

        // Find lowest f cost node in open list. 
        Pathfinding2DNode FindLowestFCostNode()
        {
            Pathfinding2DNode lowestNode = null;
            foreach (Pathfinding2DNode node in _openList.Values)
            {
                if (lowestNode == null)
                {
                    lowestNode = node;
                }
                else if (lowestNode.fcost > node.fcost)
                {
                    lowestNode = node;
                }
            }

            return lowestNode;
        }

        /*
         * calculateNodePosGab : Calculate node's position gab. Basically, node's position gab is same with node's size. If search node's position get too close to 
                                 destination, node's position gab is allocated dx and dy. (dx, dy is gab that between search node and destination).
         * calculateGcost : Calculate node's g cost. g cost = parent's g cost + g value. (g value is >> Straight : 1.0, (10), diagonal : root(2), 1.414, (14)).
         * calculateHcost : Calculate node's h cost. h cost = dx + dy (dx = l node.x - end.x l / nodeGab.x, dy = l node.y - end.y l / nodeGab.y >> the number of tile between node and destination).
         * calculateFcost : Calculate node's f cost. f cost = g cost +  .
         * calculatePathCost : Call g cost, h cost, f cost function. If any function are fail to calculate cost, it return false.
         */
        void CalculateNodePosGab(Pathfinding2DNode node)
        {
            float dx = Mathf.Abs(node.position.x - _endPoint.x);
            float dy = Mathf.Abs(node.position.y - _endPoint.y);
            if (dx > 0 && dx < _nodeSize.x && dy > 0 && dy < _nodeSize.y)
            {
                _nodePosGab.x = dx;
                _nodePosGab.y = dy;
            }

            if (dy == 0 && dx < _nodeSize.x) _nodePosGab.x = dx;
            
            if (dx == 0 && dy < _nodeSize.y) _nodePosGab.y = dy;
            
        }

        bool CalculateGcost(Pathfinding2DNode node)
        {
            if (node == null || node.parent == null) return false;

            float dx = node.position.x - node.parent.position.x;
            float dy = node.position.y - node.parent.position.y;
            
            if (dx == 0 || dy == 0) node.gcost = node.parent.gcost + 10;
            else node.gcost = node.parent.gcost + 14;

            return true;
        }

        bool CalculateHcost(Pathfinding2DNode node)
        {
            if (node == null) return false;

            float dx = Mathf.Abs(node.position.x - _endPoint.x);
            float dy = Mathf.Abs(node.position.y - _endPoint.y);
            node.hcost = (int)(10 * dx / _nodePosGab.x + 10 * dy / _nodePosGab.y);
            return true;
        }

        bool CalculateFcost(Pathfinding2DNode node)
        {
            if (node == null) return false;

            node.fcost = node.gcost + node.hcost;
            return true;
        }

        bool CalculatePathCost(Pathfinding2DNode node)
        {
            return CalculateGcost(node) && CalculateHcost(node) && CalculateFcost(node);
        }
    }
}