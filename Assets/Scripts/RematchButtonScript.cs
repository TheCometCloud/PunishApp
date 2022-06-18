using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RematchButtonScript : MonoBehaviour
{
    public bool rematch;
    GameManager manager;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.FindGameObjectsWithTag("Manager")[0].GetComponent<GameManager>();
        GetComponent<Button>().onClick.AddListener(() => manager.DecideRematch(rematch));
    }

    // Update is called once per frame
    void Update()
    {
        manager = GameObject.FindGameObjectsWithTag("Manager")[0].GetComponent<GameManager>();
        var rematch = manager.MyRematch;
        
        if (rematch is true)
        {
            GetComponent<Button>().interactable = false;
        }
        else if (rematch == null)
        {
            GetComponent<Button>().interactable = true;
        }
    }
}
