using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Inst { get; private set; }
    void Awake() => Inst = this;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartRound()
    {
        StartCoroutine(TurnManager.Inst.StartTurnCo());
    }
}
