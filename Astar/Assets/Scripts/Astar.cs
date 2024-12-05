using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Astar
{
    Cell[,] grid;
    
    /// <summary>
    /// TODO: Implement this function so that it returns a list of Vector2Int positions which describes a path from the startPos to the endPos
    /// Note that you will probably need to add some helper functions
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="grid"></param>
    /// <returns></returns>
    public List<Vector2Int> FindPathToTarget(Vector2Int startPos, Vector2Int endPos, Cell[,] grid)
    {
        // setup
        this.grid = grid;
        //HashSet<Node> visited = new HashSet<Node>();
        List<Node> open = new List<Node>();
        Node root = CreateNode(startPos, endPos, null);
        open.AddRange(ExpandNode(root, endPos));

        bool satisfied = false;
        int iterations = 0;
        while (!satisfied)
        {
            // abort search if too many iterations
            iterations++;
            if (iterations > 19999)
            {
                Debug.LogWarning("Aborted path search after " + iterations + " iterations");
                break;
            }
            
            // get the first node, which is always the highest because the list is sorted
            Node highest = open[0];
            //Debug.Log($"Pos:{highest.position} G:{highest.GScore} H:{highest.HScore} F:{highest.FScore}");

            if (highest.position == endPos)
            {
                // finish search if path is found
                satisfied = true; 
                
                // get path
                List<Vector2Int> path = new List<Vector2Int>();
                Node curr = highest;
                
                while (curr != null)
                {
                    path.Add(curr.position);
                    curr = curr.parent;
                }

                path.Reverse();
                
                Debug.Log("Found valid path after " + iterations + " iterations" +
                          "\nPath length: " + path.Count);
                return path;
            }
            else
            {
                // expand the highest node
                open.AddRange(ExpandNode(highest, endPos));
                open.Remove(highest);
                
                // remove duplicates and sort by H score
                // // distinct uses a custom equality comparer that compares the position value
                open = open.OrderBy(p => p.FScore).Distinct(new NodeEqualityComparer()).ToList();
            }
        }
        
        return null;
    }

    List<Node> ExpandNode(Node source, Vector2Int endPos)
    {
        Vector2Int sourcePos = source.position;
        List<Node> expanded = new List<Node>();

        foreach (Cell c in grid[sourcePos.x, sourcePos.y].GetNeighbours(grid))
        {
            // do not add if neighbour has a wall inbetween source node
            if (!(c.HasWall(Wall.RIGHT) && c.gridPosition.x < sourcePos.x)
                && !(c.HasWall(Wall.DOWN) && c.gridPosition.y > sourcePos.y)
                && !(c.HasWall(Wall.LEFT) && c.gridPosition.x > sourcePos.x)
                && !(c.HasWall(Wall.UP) && c.gridPosition.y < sourcePos.y))
            {
                expanded.Add(CreateNode(c.gridPosition, endPos, source));
            }
        }

        return expanded;
    }

    Node CreateNode(Vector2Int pos, Vector2Int endPos, Node parent)
    {
        return new Node(pos, parent, CalculateTravelledDistance(parent),CalculateHeuristic(pos, endPos));
    }

    int CalculateHeuristic(Vector2Int nodePos, Vector2Int endPos)
    {
        // get taxicab distance to node
        //return (endPos.x - nodePos.x + endPos.y - nodePos.y);
        return Mathf.FloorToInt(Vector2Int.Distance(endPos, nodePos));
    }

    int CalculateTravelledDistance(Node parent)
    {
        Node curr = parent;
        int nestings = 0;
        
        // recursively go through all parents to get distance
        while (curr != null)
        {
            nestings++;
            curr = curr.parent;
        }
        
        return nestings;
    }

    /// <summary>
    /// This is the Node class you can use this class to store calculated FScores for the cells of the grid, you can leave this as it is
    /// </summary>
    public class Node
    {
        public Vector2Int position; //Position on the grid
        public Node parent; //Parent Node of this node

        public float FScore { //GScore + HScore
            get { return GScore + HScore; }
        }
        public float GScore; //Current Travelled Distance
        public float HScore; //Distance estimated based on Heuristic

        public Node() { }
        public Node(Vector2Int position, Node parent, int GScore, int HScore)
        {
            this.position = position;
            this.parent = parent;
            this.GScore = GScore;
            this.HScore = HScore;
        }
    }

    public class NodeEqualityComparer : IEqualityComparer<Node>
    {
        public bool Equals(Node x, Node y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.position.Equals(y.position);
        }

        public int GetHashCode(Node obj)
        {
            return obj.position.GetHashCode();
        }
    }
}
