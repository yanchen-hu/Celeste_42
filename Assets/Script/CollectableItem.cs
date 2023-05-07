using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableItem : MonoBehaviour
{
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
        if(collision.tag == "Player")
        {
            this.GetComponent<AudioSource>().Play();
            GameManager.Instance.HandleCollectItems();
            Destroy(this.GetComponent<SpriteRenderer>());
            Destroy(this);
        }
    }
}
