using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Node :IComparable
{
	public int G { get; private set; }
	public int H { get; private set; }

	public Cell Cell;

	public Node Parent;

	public Node( Cell cell, Node parent, int g, int h )
	{
		this.Cell = cell;
		this.Parent = parent;
		this.G = g;
		this.H = h;
	}

	public int F { get { return G + H; } }

	public Node Set(Node parent, int g, int h)
	{
		this.Parent = parent;
		this.G = g;
		this.H = h;
		return this;
	}

	public int CompareTo(object other)
	{
 		return F.CompareTo(((Node)other).F);
	}
}

public class Paths {

	// Uses the astar path finding to modify the level nodes, then
	// returns the next node this participant should move to
	public static bool FindNextNode(Node start, Node end, out Node nextNode)
	{
		nextNode = null;
		if (FindAStarPath(start, end))
		{
			Node current  = end;
			//int counter  = 0;
			while ( current.Parent.Cell != start.Cell )
			{
				//if ( counter > 100 )
				//	return false;
				current = current.Parent;
			}
			nextNode = current;
			return true;
		}
		return false;
	}

	// This modifies the nodes in the level manager, so the path can be traced backwards
	public static  bool FindAStarPath( Node start, Node end )
	{
		start.Set( null, 0, FindH( start, end ) );

		List<Node> openList = new List<Node>();
		List<Node> closedList = new List<Node>();

		// Add the starting square (or node) to the open list.
		Node currentNode = start;

		openList.Add(currentNode);

		// Do this until you add the end node to closed list or openList is empty
		//	which means there is no path
		while ( openList.Count > 0  )
		{
			openList.Sort( );

			// Look for the lowest F cost square on the open list. We refer to this as the current square.
			currentNode = openList[0];
			for ( int i = 1; i < openList.Count; i++ )
			{
				if ( openList[i].F < currentNode.F )
					currentNode = openList[i];
			}

			// Switch it to the closed list
			openList.Remove( currentNode );
			closedList.Add( currentNode );
			if ( currentNode == end )
				return true;

			// For each of the 8 squares adjacent to this current square
			//	If it is not walkable or if it is on the closed list, ignore it. Otherwise do the following. 
			List<Node> newNodes = FindAdjacentNodes( currentNode, closedList );

			for ( int i = 0; i < newNodes.Count; i++ )
			{
				if ( newNodes[i].Cell.Status != CellStatus.Open || closedList.Contains( newNodes[i] ) )
				{
					newNodes.Remove( newNodes[i] );
					i--;
				}
			}

			List<Node> alreadyOnOpen = new List<Node>();

			for (int i  = 0; i < newNodes.Count; i++)
			{
				// If it isn’t on the open list, add it to the open list. Make the current square the parent of 
				// this square. Record the F, G, and H costs of the square. 
				if ( !openList.Contains( newNodes[i] ) )
				{
					openList.Add( newNodes[i] );
					newNodes[i].Set( currentNode, FindG( currentNode, newNodes[i] ), FindH( newNodes[i], end ) );
					if ( newNodes[i] == end )
						return true;
				}
				// If it is on the open list already, check to see if this path to that square is better, using 
				// G cost as the measure. A lower G cost means that this is a better path. If so, change the 
				// parent of the square to the current square, and recalculate the G score of the square.
				else
					alreadyOnOpen.Add( newNodes[i] );
			}

			for (int i = 0; i < alreadyOnOpen.Count; i++)
			{
				int newG = FindG(currentNode, alreadyOnOpen[i]); 
				if (newG < alreadyOnOpen[i].G)
					alreadyOnOpen[i].Set(currentNode, newG, FindH(alreadyOnOpen[i], end));
			}
		}
		return false;
	}

	private static List<Node> FindAdjacentNodes( Node centerNode, List<Node> closedNodes )
	{
		List<Node> adjacentNodes = new List<Node>();

		Node[,] floorNodes = LevelManager.Instance.FloorNodes;

		int floorColumns = LevelManager.Instance.FloorColumns;
		int floorRows = LevelManager.Instance.FloorRows;

		// West
		if ( centerNode.Cell.column > 0 )
		{
			adjacentNodes.Add( floorNodes[centerNode.Cell.column - 1, centerNode.Cell.row] );
		}
		// North West
		if ( centerNode.Cell.column > 0 && centerNode.Cell.row <  floorColumns - 1)
		{
			adjacentNodes.Add( floorNodes[centerNode.Cell.column - 1, centerNode.Cell.row + 1] );
		}
		// North
		if ( centerNode.Cell.row < floorRows - 1 )
		{
			adjacentNodes.Add( floorNodes[centerNode.Cell.column, centerNode.Cell.row + 1] );
		}
		// North East
		if ( centerNode.Cell.column < floorColumns - 1 && centerNode.Cell.row < floorRows - 1 )
		{
			adjacentNodes.Add( floorNodes[centerNode.Cell.column + 1, centerNode.Cell.row + 1] );
		}
		// East
		if ( centerNode.Cell.column < floorColumns - 1 )
		{
			adjacentNodes.Add( floorNodes[centerNode.Cell.column + 1, centerNode.Cell.row] );
		}
		// South East
		if ( centerNode.Cell.column < floorColumns - 1 && centerNode.Cell.row > 0 )
		{
			adjacentNodes.Add( floorNodes[centerNode.Cell.column + 1, centerNode.Cell.row - 1] );
		}
		// South
		if ( centerNode.Cell.row > 0 )
		{
			adjacentNodes.Add( floorNodes[centerNode.Cell.column, centerNode.Cell.row - 1] );
		}
		// South West
		if ( centerNode.Cell.column > 0 && centerNode.Cell.row > 0 )
		{
			adjacentNodes.Add( floorNodes[centerNode.Cell.column - 1, centerNode.Cell.row - 1] );
		}

		return adjacentNodes;
	}

	private static int FindG(Node previousNode, Node thisNode)
	{
		if ( previousNode.Cell.column == thisNode.Cell.column || previousNode.Cell.row == thisNode.Cell.row )
			return previousNode.G + 10;
		else
			return previousNode.G + 14;
	}

	/// <summary>
	/// Finds the H value using the Manhattan method
	/// </summary>
	/// <param name="start"></param>
	/// <param name="end"></param>
	/// <returns></returns>
	private static int FindH(Node start, Node end)
	{
		return 10 * (Mathf.Abs( start.Cell.column - end.Cell.column ) + Mathf.Abs( start.Cell.row - end.Cell.row ));
	}

}
