using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Xml;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Tilemaps;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.XR.WSA.Input;

public class Selection : MonoBehaviour
{
    struct Node
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Cout { get; set; }
        public int Heuristique { get; set; }

        public Node(Vector2Int coord, int cout)
        {
            X = coord.x;
            Y = coord.y;
            Cout = cout;
            Heuristique = 0;
        }

        public Node(int x, int y, int cout, int heuristique)
        {
            X = x;
            Y = y;
            Cout = cout;
            Heuristique = heuristique;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode();
    }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else
            {
                Node c = (Node)obj;
                return c.X.Equals(X) && c.Y.Equals(Y);
            }
        }

    }
    
    public Vector2Int Coord { get; private set; }

    public Tilemap selectionMap;
    public Tile selectionTile;

    public Tilemap displacementMap;
    public Tile displacementTile;
    
    public Tilemap pathMap;
    public Tile pathTile;

    public Tile attackTile;

    List<GameObject> units;

    public Tilemap unitMap;
    public Tilemap floorMap;

    private List<Vector2Int> pathFinding;
    private List<Vector2Int> possibleAttackMove;

    GameObject goSelectioned = null;

    enum SelectionMode
    {
        WaitingResponse,
        Normal,
        Movement
    }
    SelectionMode selectionMode;

    void Start()
    {
        Coord = new Vector2Int();
        pathFinding = new List<Vector2Int>();
        possibleAttackMove = new List<Vector2Int>();
        selectionMode = SelectionMode.Normal;
        
    }

    bool InBounds(Vector2Int newPos)
    {
        Vector2Int a = Coord + newPos;
        return (a.x < -9) || (a.x > 8) || (a.y > 4) || (a.y < -5);
    }

    public void Move(Vector2Int newPos) 
    {
        if (!InBounds(newPos))
            Coord += newPos;
    }

    public bool UnitSelectioned() => goSelectioned != null;
    public GameObject GetSelectionedObject() => goSelectioned;

    public Vector3Int GetCoord() => new Vector3Int(Coord.x, Coord.y, 0);

    bool HasUnit(int x, int y)
    {
        //GameManager.Instance.GetPlayerTurn().GetUnits()
        foreach (GameObject go in GameManager.Instance.GetAllUnits())
        {
            Unit unit = go.GetComponent<Unit>();
            Vector2Int index = unit.GetIndexPos();
            if (index.x == x && index.y == y)
                return true;
        }
        return false;
    }

    private void Update()
    {
        if (GameManager.Instance.ChangingTurn) return;
        if (Input.GetKeyDown(KeyCode.Y))
        {
            GameManager.Instance.ChangeTurn();
            selectionMode = SelectionMode.Normal;
            goSelectioned = null;
        }

        if (selectionMode != SelectionMode.WaitingResponse)
        {
            /* Keyboard Selection
            selectionMap.SetTile(GetCoord(), null);
            if (Input.GetKeyDown(KeyCode.W)) Move(new Vector2Int(0, 1));
            if (Input.GetKeyDown(KeyCode.S)) Move(new Vector2Int(0, -1));
            if (Input.GetKeyDown(KeyCode.A)) Move(new Vector2Int(-1, 0));
            if (Input.GetKeyDown(KeyCode.D)) Move(new Vector2Int(1, 0));
            selectionMap.SetTile(GetCoord(), selectionTile);
            */
            selectionMap.SetTile(GetCoord(), null);
            Vector3Int cellPos = MapManager.Instance.MouseToCellPos();
            Coord = new Vector2Int(cellPos.x, cellPos.y);
            selectionMap.SetTile(GetCoord(), selectionTile);
        }

        if (Input.GetMouseButton(1))
        {
            selectionMode = SelectionMode.Normal;
            ClearAllCases(displacementMap);
            ClearAllCases(pathMap);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if(selectionMode == SelectionMode.Normal)
                FindUnit();
            else if(selectionMode == SelectionMode.Movement)
            {
                Unit unit = goSelectioned.GetComponent<Unit>();
                if (Coord.x != unit.GetIndexPos().x || Coord.y != unit.GetIndexPos().y)
                {
                    bool canMove = false;
                    bool canAttack = false;

                    Vector2Int attackCoord = new Vector2Int();

                    foreach (Vector2Int coord in possibleAttackMove)
                    {
                        if (coord.x == Coord.x && Coord.y == coord.y)
                        {
                            canAttack = true;
                            attackCoord = new Vector2Int(coord.x, coord.y);
                            break;
                        }
                    }


                    if (!canAttack)
                    {
                        //Check if the mouse is on pathfinding
                        foreach (Vector2Int coord in pathFinding)
                        {
                            if (Coord.x == coord.x && Coord.y == coord.y)
                            {
                                canMove = true;
                                break;
                            }
                        }
                    }

                    //Move Unit to pos
                    if(canMove)
                        MoveUnit(pathFinding);
                    else if (canAttack)
                        AttackUnit(attackCoord);
                }
                else
                {
                    //Show Interaction Menu
                }

            }

        }

        if (goSelectioned)
        {
            if (selectionMode == SelectionMode.Movement)
                DrawPathFinding();

            DrawPossibleRange();
            DrawPossibleAttack();
        }
        else
        {
            ClearAllCases(displacementMap);
            ClearAllCases(pathMap);
        }
    }

    void AttackUnit(Vector2Int coord)
    {
        GameObject go = FindEnemyUnit(coord.x, coord.y);

        string allyName = goSelectioned.name;
        string enemyName = go.name;

        Unit unit = goSelectioned.GetComponent<Unit>();
        go.GetComponent<Unit>().ApplyDamage(unit.Attack);
        unit.Locked();
        selectionMode = SelectionMode.Normal;
        goSelectioned = null;

        Debug.Log(allyName + " has attacked " + enemyName);

    }

    void MoveUnit(List<Vector2Int> path)
    {
        goSelectioned.transform.position = new Vector3(path[0].x + 0.5f, path[0].y + 0.5f, 0);
        goSelectioned.GetComponent<Unit>().SetIndexPos(path[0].x, path[0].y);
        goSelectioned.GetComponent<Unit>().Locked();
        selectionMode = SelectionMode.Normal;
        ClearAllCases(pathMap);
    }

    GameObject FindEnemyUnit(int x, int y)
    {
        List<GameObject> units = GameManager.Instance.GetOtherPlayer().GetUnits();
        foreach(GameObject go in units)
        {
            Vector2Int coord = go.GetComponent<Unit>().GetIndexPos();
            if (coord.x == x && coord.y == y)
                return go;
        }
        return null;
    }

    void FindUnit()
    {
        bool find = false;
        foreach (GameObject go in GameManager.Instance.GetPlayerTurn().GetUnits())
        {
            Unit unit = go.GetComponent<Unit>();
            Vector2Int indexPos = unit.GetIndexPos();
            if (indexPos.x == Coord.x && indexPos.y == Coord.y)
            {
                find = true;
                //displacement Mode
                if (goSelectioned == go && !unit.HasMoved)
                {
                    selectionMode = (selectionMode == SelectionMode.Movement) ? SelectionMode.Normal : SelectionMode.Movement;
                }
                else
                {
                    selectionMode = SelectionMode.Normal;
                    goSelectioned = go;
                }
                break;
            }
        }
        if (!find)
        {
            selectionMode = SelectionMode.Normal;
            goSelectioned = null;
        }
    }

    void DrawPossibleAttack()
    {
        possibleAttackMove.Clear();
        Unit unit = goSelectioned.GetComponent<Unit>();
        if (unit.HasAttacked) return;
        List<Vector2Int> coords = SearchAllCasesInPossibleRange(unit.GetIndexPos(), 
                                                                unit.AttackRange, 
                                                                AttackRegionCondition);
        List<GameObject> units = GameManager.Instance.GetOtherPlayer().GetUnits();
        foreach (GameObject go in units)
        {
            Vector2Int pos = go.GetComponent<Unit>().GetIndexPos();
            foreach (Vector2Int coord in coords)
                if (pos.x == coord.x && pos.y == coord.y) possibleAttackMove.Add(coord);
        }

        DrawAllCases(displacementMap, attackTile, possibleAttackMove);

    }

    bool AttackRegionCondition(int x, int y)
    {
        List<GameObject> units = GameManager.Instance.GetPlayerTurn().GetUnits();
        
        foreach (GameObject go in units)
        {
            Vector2Int coord = go.GetComponent<Unit>().GetIndexPos();
            if (x == coord.x && y == coord.y)
                return true;
        }

        return false;
    }

    #region Algorithm A*

    void DrawPathFinding()
    {
        ClearAllCases(pathMap);

        pathFinding = ShorterPath(new Node(goSelectioned.GetComponent<Unit>().GetIndexPos(), 0), new Node(Coord, 0));
        
        DrawAllCases(pathMap, pathFinding);
    }

    List<Vector2Int> ShorterPath(Node depart, Node arrive)
    {
        Vector2Int[] dirToCheck = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, -1), new Vector2Int(0, 1) };

        Dictionary<Node, Node> parents = new Dictionary<Node, Node>();

        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();

        openList.Add(depart);

        while (openList.Count != 0)
        {
            Node u = LessHeuristic(openList);
            openList.Remove(u);
            if (u.X == arrive.X && u.Y == arrive.Y)
            {
                return RecoverPath(parents, u);
            }
            for (int i = 0; i < 4; i++)
            {
                Node v = new Node(u.X + dirToCheck[i].x, u.Y + dirToCheck[i].y, 0, 0);

                if (!openList.Contains(v) && !closedList.Contains(v) && displacementMap.GetTile(new Vector3Int(v.X, v.Y, 0)) != null &&
                    floorMap.GetTile(new Vector3Int(v.X, v.Y, 0)) != null )
                {
                    parents.Add(v, u);
                    v.Cout = u.Cout + 1;
                    v.Heuristique = v.Cout + Distance(v, arrive);
                    openList.Add(v);
                }

            }
            closedList.Add(u);
        }

        return new List<Vector2Int>();
    }

    int Distance(Node node1, Node node2) => Math.Abs(node1.X - node2.X) + Math.Abs(node1.Y - node2.Y);

    Node LessHeuristic(List<Node> list)
    {
        Node result = list[0];

        for (int i = 1; i < list.Count; i++)
        {
            if (result.Heuristique > list[i].Heuristique)
                result = list[i];
        }
        return result;
    }

    List<Vector2Int> RecoverPath(Dictionary<Node, Node> parents, Node end)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        Node current = end;
        result.Add(new Vector2Int(current.X, current.Y));
        while(parents.ContainsKey(current))
        {
            current = parents[current];
            Vector2Int index = goSelectioned.GetComponent<Unit>().GetIndexPos();
            result.Add(new Vector2Int(current.X, current.Y));
        }
        return result;
    }

    #endregion

    #region SelectionRegion
        void DrawPossibleRange()
        {
            ClearAllCases(displacementMap);
            //Calculate possile tile where the unit can move
            Unit unit = goSelectioned.GetComponent<Unit>();
            List<Vector2Int> coords = SearchAllCasesInPossibleRange(unit.GetIndexPos(), 
                                                                    unit.PM, 
                                                                    SelectionRegionCondition);

            DrawAllCases(displacementMap, coords);
            
        }

        bool SelectionRegionCondition(int x, int y)
        {
        return MapManager.Instance.IsObstacles(goSelectioned.GetComponent<Unit>().GetDeniedAccess(), x, y)
                || HasUnit(x, y);
        }

        List<Vector2Int> SearchAllCasesInPossibleRange(Vector2Int startPos, int range, Func<int, int, bool> condition)
        {
            Vector2Int[] dirToCheck = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, -1), new Vector2Int(0, 1) };

            List<Node> openList = new List<Node>();
            List<Node> closedList = new List<Node>();

            List<Vector2Int> result = new List<Vector2Int>();

            openList.Add(new Node(startPos, 0));

            while(openList.Count != 0)
            {
                Node tile = openList[0];
                openList.RemoveAt(0);
                for (int i = 0; i < 4; i++)
                {

                    Vector2Int newPos = new Vector2Int(tile.X + dirToCheck[i].x, tile.Y + dirToCheck[i].y);
                    int newF = tile.Cout + 1;
                    Node neighbors = new Node(newPos, newF);
                    
                    if (newF <= range && !openList.Contains(neighbors) && !closedList.Contains(neighbors) 
                        && !condition(newPos.x, newPos.y))
                    {
                        openList.Add(neighbors);
                    }

                }
                closedList.Add(tile);
            }

            foreach (Node n in closedList)
            {
                result.Add(new Vector2Int(n.X, n.Y));
            }

            return result;
        }
        void DrawAllCases(Tilemap tilemap, Tile tile, List<Vector2Int> coords)
        {
            foreach (Vector2Int coord in coords)
            {
                tilemap.SetTile(new Vector3Int(coord.x, coord.y, 0), tile);
            }
        }
        void DrawAllCases(Tilemap tilemap, List<Vector2Int> coords)
        {
            DrawAllCases(tilemap, displacementTile, coords);
        }
        void ClearAllCases(Tilemap tilemap)
        {
            tilemap.ClearAllTiles();
        }
    #endregion

}
