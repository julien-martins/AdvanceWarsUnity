using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour {

    public enum UnitType
    {
        Soldier,
        Tank
    }

    public Text LifeHUD;

    public UnitType Type;
    public string Name;
    public int Life;
    public int Attack;
    public int PM;
    public int AttackRange;

    public List<MapManager.TileType> deniedAccess;

    private int indexX;
    private int indexY;

    public bool HasMoved { get; private set; }
    public bool HasAttacked { get; private set; }

    void Start() {
        HasMoved = false;
        HasAttacked = false;
    }

    public void Locked()
    {
        HasMoved = true;
        HasAttacked = true;
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    public void Unlock(Player.Team team)
    {
        HasMoved = false;
        HasAttacked = false;
        Color color = (team == Player.Team.Blue) ? Color.green : Color.red;
        GetComponent<SpriteRenderer>().color = color;
    }

    public void ApplyDamage(int value)
    {
        Life -= value;
        LifeHUD.text = Life.ToString();
        if (Life <= 0)
            Die();
    }

    public void Die() => GameManager.Instance.KillUnit(this.gameObject);

    public void AddDeniedTile (MapManager.TileType type) => deniedAccess.Add (type);
    public MapManager.TileType[] GetDeniedAccess () => deniedAccess.ToArray ();

    public void SetIndexPos(int x, int y) { indexX = x; indexY = y; }
    public Vector2Int GetIndexPos() => new Vector2Int(indexX, indexY);
}