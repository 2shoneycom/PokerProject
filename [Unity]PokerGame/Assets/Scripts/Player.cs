using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    [SerializeField] public List<Card> myCards;
    public Transform myCardLeft;
    public Transform myCardRight;

    // Start is called before the first frame update
    void Start()
    {
        myCards = new List<Card>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RemoveCard()
    {
        for (int i = 0; i < myCards.Count; i++)
        {
            var card = myCards[i];
            var cardObject = card.transform.gameObject;
            Destroy(cardObject);
        }
        myCards.Clear();
    }
}
