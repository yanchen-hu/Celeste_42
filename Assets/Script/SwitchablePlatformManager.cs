using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchablePlatformManager : MonoBehaviour
{
    public enum PLATFORM_TYPE
    {
        TYPE_A,
        TYPE_B,
    }

    public GameObject[] PlatformAList;
    public GameObject[] PlatformBList;
    public float SecondsBetweenSwitch;
    public bool PlatformAShowFirst;

    private float timer;
    private PLATFORM_TYPE currentType;
    // Start is called before the first frame update
    void Start()
    {
        timer = SecondsBetweenSwitch;
        currentType = PlatformAShowFirst ? PLATFORM_TYPE.TYPE_A : PLATFORM_TYPE.TYPE_B;
        HandlePlatformSwitch(currentType);
    }

    private PLATFORM_TYPE GetTheOtherType(PLATFORM_TYPE type)
    {
        return type == PLATFORM_TYPE.TYPE_A ? PLATFORM_TYPE.TYPE_B : PLATFORM_TYPE.TYPE_A;
    }

    private void HandlePlatformSwitch(PLATFORM_TYPE showType)
    {
        GameObject[] showList = GetPlatformList(showType);
        for(int i = 0;i<showList.Length;i++)
        {
            showList[i].GetComponent<BoxCollider2D>().isTrigger = false;
            Color co = showList[i].GetComponent<SpriteRenderer>().color;
            co.a = 1f;
            showList[i].GetComponent<SpriteRenderer>().color = co;
        }

        PLATFORM_TYPE hideType = GetTheOtherType(showType);
        GameObject[] hideList = GetPlatformList(hideType);
        for (int i = 0; i < hideList.Length; i++)
        {
            hideList[i].GetComponent<BoxCollider2D>().isTrigger = true;
            Color co = hideList[i].GetComponent<SpriteRenderer>().color;
            co.a = 0.3f;
            hideList[i].GetComponent<SpriteRenderer>().color = co;
        }
        currentType = showType;
    }

    private GameObject[] GetPlatformList(PLATFORM_TYPE type)
    {
        switch (type)
        {
            case PLATFORM_TYPE.TYPE_A:
                return PlatformAList;
            case PLATFORM_TYPE.TYPE_B:
                return PlatformBList;
        }
        return null;
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if(timer<=0)
        {
            HandlePlatformSwitch(GetTheOtherType(currentType));
            timer = SecondsBetweenSwitch;
        }
    }
}
