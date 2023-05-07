using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidenWall : MonoBehaviour
{
    public bool IsHidenWallEnterAble;
    // Start is called before the first frame update
    void Start()
    {
        SetHidenWaterEnterable(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            SetHidenWaterEnterable(false);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if(collision.gameObject.GetComponent<PlayerCharacter>().playState ==PlayState.Dash )
            {
                SetHidenWaterEnterable(true);
                print(collision.gameObject.GetComponent<PlayerCharacter>().playState);
            }
        }
    }

    private void SetHidenWaterEnterable(bool enterAble)
    {
        IsHidenWallEnterAble = enterAble;
        if(enterAble)
        {
            this.GetComponent<BoxCollider2D>().isTrigger = true;
            Color co = this.GetComponent<SpriteRenderer>().color;
            co.a = 0.3f;
            this.GetComponent<SpriteRenderer>().color = co;
            AudioSource source = this.GetComponent<AudioSource>();
            if (!source.isPlaying) source.Play();
        }
        else
        {
            this.GetComponent<BoxCollider2D>().isTrigger = false;
            Color co = this.GetComponent<SpriteRenderer>().color;
            co.a = 1f;
            this.GetComponent<SpriteRenderer>().color = co;
            AudioSource source = this.GetComponent<AudioSource>();
            source.Stop();
        }
    }
}
