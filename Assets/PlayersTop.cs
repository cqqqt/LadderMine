using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;

public class PlayersTop : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        foreach (var text in GetComponentsInChildren<Text>())
        {
            text.text = "";
        }
    }
    
    public void SetTexts (List<PlayerControls> players)
    {
        PlayerControls[] top = players
            .Where(p => !p.IsDead)
            .OrderByDescending(p => p.Score)
            .Take(5)
            .ToArray();

        for (int i = 0; i < top.Length; i++)
        {
            transform.GetChild(i).GetComponent<Text>().text =
                (i+1) + ". " + top[i].photonView.Owner.NickName + "     " + top[i].Score;
        }
    }   
}
