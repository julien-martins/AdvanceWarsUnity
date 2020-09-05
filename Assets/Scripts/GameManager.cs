using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    public Image splashcreenTurn;
    public Text nbTour;

    private bool splashcreenTimerIsRunning;
    private float splashcreenCoutdown;

    public int NbTurn { get; protected set; }
    public bool Player1Turn { get; protected set; }
    public bool Player2Turn { get; protected set; }

    [SerializeField]
    private Player Player1;

    [SerializeField]
    private Player Player2;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            //Initialize variable
            Init();

        }
        else
            Destroy(this);
    }

    private void Init()
    {
        NbTurn = 1;
        Player1Turn = true;
        Player2Turn = false;

        Player1 = new Player(Player.Team.Blue);
        Player2 = new Player(Player.Team.Red);

        MapManager.Instance.SpawnUnits();

        splashcreenCoutdown = 1;
        splashcreenTimerIsRunning = false;
        splashcreenTurn.enabled = false;
        nbTour.text = "Tour 1";

    }

    private void Update()
    {
        Debug.Log(splashcreenTimerIsRunning);
        if (splashcreenTimerIsRunning)
        {
            splashcreenCoutdown -= Time.deltaTime;
            Debug.Log(splashcreenCoutdown);
            if(splashcreenCoutdown <= 0)
            {
                splashcreenCoutdown = 1;
                splashcreenTimerIsRunning = false;

                CallbackChangeTurn();

            }
        }
    }

    public List<GameObject> GetAllUnits()
    {
        List<GameObject> result = new List<GameObject>();
        result.AddRange(Player1.GetUnits());
        result.AddRange(Player2.GetUnits());
        return result;
    }

    public void AddUnitToPlayer1(GameObject go) => Player1.AddUnit(go);
    public void AddUnitToPlayer2(GameObject go) => Player2.AddUnit(go);


    public void KillUnit(GameObject go)
    {
        Destroy(go);

        if (Player1.FindUnit(go))
            Player1.RemoveUnit(go);
        else if (Player2.FindUnit(go))
            Player2.RemoveUnit(go);

    }

    public void ChangeTurn()
    {
        if (splashcreenTimerIsRunning) return;
        splashcreenTimerIsRunning = true;
        splashcreenTurn.enabled = true;
        if (Player1Turn) splashcreenTurn.sprite = Resources.Load<Sprite>("Splashcreen/EnemyTurn"); 
        else splashcreenTurn.sprite = Resources.Load<Sprite>("Splashcreen/AllyTurn");
    }

    private void CallbackChangeTurn()
    {
        splashcreenTurn.enabled = false;

        Player1Turn = !Player1Turn;
        Player2Turn = !Player2Turn;
        
        if (Player1Turn) NbTurn++;
        nbTour.text = "Tour " + NbTurn;
        GetPlayerTurn().UnlockAllUnit();
        GetOtherPlayer().UnlockAllUnit();
    }

    public Player GetPlayerTurn() => (Player1Turn) ? Player1 : Player2;

    public Player GetOtherPlayer() => (Player1Turn) ? Player2 : Player1;

    public bool ChangingTurn => splashcreenTimerIsRunning;

}
