using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnstablePlatform : MonoBehaviour
{
    public float MaxSecondBeforeDisappear;
    public float SecondReenablePlatform;
    private bool hasPlayerOn;
    private float timer;
    // Start is called before the first frame update
    void Start()
    {
        resetTimer();
        hasPlayerOn = false;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if(hasPlayerOn)
        {
            timer -= Time.fixedDeltaTime;
            if(timer<=0)
            {
                this.GetComponent<SpriteRenderer>().enabled = false;
                this.GetComponent<BoxCollider2D>().enabled = false;
                hasPlayerOn = false;
                StopAllCoroutines();
                StartCoroutine(ReenablePlatform(SecondReenablePlatform));
            }
        }
    }

    IEnumerator ReenablePlatform(float sec)
    {
        yield return new WaitForSeconds(sec);
        this.GetComponent<SpriteRenderer>().enabled = true;
        this.GetComponent<BoxCollider2D>().enabled = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if(!hasPlayerOn) resetTimer();
            hasPlayerOn = true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        //if (collision.gameObject.tag == "Player")
        //{
        //    hasPlayerOn = false;
        //}
    }
    private void resetTimer()
    {
        timer = MaxSecondBeforeDisappear;
    }
}
