using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    [SerializeField] public List<Card> myCards;

    // Start is called before the first frame update
    void Start()
    {
        myCards = new List<Card>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
