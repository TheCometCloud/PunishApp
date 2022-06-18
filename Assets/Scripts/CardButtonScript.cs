using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardButtonScript : MonoBehaviour
{
    public int index;
    public GameManager manager;
    public Text text;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => manager.GetComponent<GameManager>().PlayCard(index));
    }

    // Update is called once per frame
    void Update()
    {
        var hand = manager.GetComponent<GameManager>().Hand;
        
        if (index >= hand.Count)
        {
            GetComponent<Button>().interactable = false;
        }
        else 
        {
            GetComponent<Button>().interactable = true;
            text.text = hand[index].Name;
        }
    }
}
