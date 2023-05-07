using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEndDoor : MonoBehaviour
{
    public bool IsForPlayer2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if((collision.gameObject.layer == LayerMask.NameToLayer("Player") && !IsForPlayer2)
            ||(collision.gameObject.layer == LayerMask.NameToLayer("Player2") && IsForPlayer2))
        {
            GameManager.Instance.HandlePlayerReachDestination();
            collision.gameObject.GetComponent<InputManager>().HandleWin();
        }
    }
}
