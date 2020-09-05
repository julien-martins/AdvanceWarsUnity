using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Video;

public class MapManager : MonoBehaviour
{

    public int Width { get; private set; }
    public int Height { get; private set; }

    public enum TileType
    {
        Tree,
        Mountain,
        Water
    }

    public List<GameObject> units;

    public Dictionary<string, TileType> types;

    public Tilemap backgroundMap;
    public Tilemap objectMap;
    public Tilemap unitMap;

    Tilemap tilemap4;
    Tilemap tilemap5;
    Tilemap tilemap6;

    public static MapManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            Init();

        }
        else
            Destroy(this);
    }

    void Init()
    {
        Width = backgroundMap.cellBounds.xMax - backgroundMap.cellBounds.xMin;
        Height = backgroundMap.cellBounds.yMax - backgroundMap.cellBounds.yMin;

        types = new Dictionary<string, TileType>();
        types.Add("treeTile", TileType.Tree);
        types.Add("mountainTile", TileType.Mountain);
    }

    public Vector3Int MouseToCellPos()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return backgroundMap.WorldToCell(worldPos);
    }

    public void SpawnUnits()
    {
        for (int i = -9; i < 8; i++)
        {
            for (int j = -5; j < 4; j++)
            {
                TileBase tile = unitMap.GetTile(new Vector3Int(i, j, 0));

                if (tile != null)
                {
                    string name = tile.name;

                    if (name == "tankAllyPlacementTile")
                    {
                        GameObject go = Instantiate(units[0], new Vector3(i + 0.5f, j + 0.5f, 0), Quaternion.identity);
                        go.GetComponent<Unit>().SetIndexPos(i, j);
                        go.GetComponent<SpriteRenderer>().color = Color.green;
                        GameManager.Instance.AddUnitToPlayer1(go);
                    }
                    else if (name == "soldierAllyPlacementTile")
                    {
                        GameObject go = Instantiate(units[1], new Vector3(i + 0.5f, j + 0.5f, 0), Quaternion.identity);
                        go.GetComponent<Unit>().SetIndexPos(i, j);
                        go.GetComponent<SpriteRenderer>().color = Color.green;
                        GameManager.Instance.AddUnitToPlayer1(go);
                    }

                    else if(name == "tankEnemyPlacementTile")
                    {
                        GameObject go = Instantiate(units[0], new Vector3(i + 0.5f, j + 0.5f, 0), Quaternion.identity);
                        go.GetComponent<Unit>().SetIndexPos(i, j);
                        go.GetComponent<SpriteRenderer>().color = Color.red;
                        GameManager.Instance.AddUnitToPlayer2(go);
                    }
                    else if(name == "soldierEnemyTile")
                    {
                        GameObject go = Instantiate(units[1], new Vector3(i + 0.5f, j + 0.5f, 0), Quaternion.identity);
                        go.GetComponent<Unit>().SetIndexPos(i, j);
                        go.GetComponent<SpriteRenderer>().color = Color.red;
                        GameManager.Instance.AddUnitToPlayer2(go);
                    }

                }

            }
        }
    }

    public bool HasUnit(int x, int y) => unitMap.GetTile(new Vector3Int(x, y, 0)) != null;

    public String GetObject(int x, int y) => objectMap.GetTile(new Vector3Int(x, y, 0)).name;

    public bool IsObstacle(TileType typeToCheck, int x, int y)
    {
        TileBase tile = objectMap.GetTile(new Vector3Int(x, y, 0));
        if (tile != null)
        {
            TileType type = types[tile.name];
            return type == typeToCheck;
        }
        return false;
    }
    public bool IsObstacles(TileType[] typeToCheck, int x, int y)
    {
        TileBase tile = objectMap.GetTile(new Vector3Int(x, y, 0));
        if (tile != null)
        {
            for (int i = 0; i < typeToCheck.Length; i++)
            {
                if (types[tile.name] == typeToCheck[i])
                    return true;
            }
        }
        return false;
    }

}
