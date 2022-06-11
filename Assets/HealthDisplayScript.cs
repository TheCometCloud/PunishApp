using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplayScript : MonoBehaviour
{
    public Text Text;
    public GameManager manager;

    // Update is called once per frame
    void Update()
    {
        Text.text = $"I Am MasterClient: {manager.IAmMaster}\nYour Health: {manager.MyHealth}\nTheir Health: {manager.TheirHealth}\nTheir Hand: {String.Join(", ", manager.TheirHand.Select(c => c.Name))}";
    }
}
