using Packages.Rider.Editor.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Player
{
    public enum Team
    {
        Red,
        Blue
    }

    [SerializeField]
    private Team team;

    List<GameObject> units;

    public Player(Team team)
    {
        units = new List<GameObject>();
        this.team = team;
    }
    
    public void UnlockAllUnit()
    {
        foreach(GameObject go in units)
        {
            Unit unit = go.GetComponent<Unit>();
            unit.Unlock(team);
        }
    }

    public List<GameObject> GetUnits() => units;

    public void RemoveUnit(GameObject go) => units.Remove(go);

    public bool FindUnit(GameObject go)
    {
        foreach (GameObject unit in units)
        {
            if (unit.Equals(go))
                return true;
        }
        return false;
    }

    public void AddUnit(GameObject go) => units.Add(go);

    public override bool Equals(object obj)
    {
        Player p = (Player)obj;
        return p.team.Equals(team);
    }

}
