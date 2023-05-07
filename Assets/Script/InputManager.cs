using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public bool keyIsSet;
    public KeyCode LeftMoveKey;
    public KeyCode RightMoveKey;
    public KeyCode UpMoveKey;
    public KeyCode DownMoveKey;
    public KeyCode Jump;
    public KeyCode Dash;
    public KeyCode Climb;
    public bool IsWin;
    [HideInInspector]
    public bool ClimbKey { get {return Input.GetKey(Climb); } }
    [HideInInspector]
    public bool JumpKey { get { return Input.GetKey(Jump); } }
    [HideInInspector]
    public bool JumpKeyDown {
        get
        {
            if(Input.GetKeyDown(Jump) || JumpFrame > 0)
            {
				return true;
            }
            return false;
        }
    }
    [HideInInspector]
    public bool JumpKeyUp { get { return Input.GetKeyUp(Jump); } }
    [HideInInspector]
    public bool DashKey { get { return Input.GetKey(Dash); } }
    [HideInInspector]
    public bool DashKeyDown { get { return Input.GetKeyDown(Dash); } }
    [HideInInspector]
    public bool DashKeyUp { get { return Input.GetKeyUp(Dash); } }

    public float v = 0;
    public int MoveDir;

    public int JumpFrame;

    private void Awake()
    {
        KeyInit();
    }
    public void HandleWin()
    {
        IsWin = true;
        this.GetComponent<PlayerCharacter>().HandleWin();
        MoveDir = 0;
        v = 0;
    }
    public void KeyInit()
    {
        if (!keyIsSet)
        {
            Jump = KeyCode.K;
            Dash = KeyCode.J;
            Climb = KeyCode.L;
            LeftMoveKey = KeyCode.A;
            RightMoveKey = KeyCode.D;
            UpMoveKey = KeyCode.W;
            DownMoveKey = KeyCode.S;
        }
    }

    private void FixedUpdate()
    {
        if(JumpFrame >= 0)
        {
            JumpFrame--;
        }
    }

    private void Update()
    {
        if (IsWin) return;
        CheckHorzontalMove();
        CheckVerticalMove();
        if (Input.GetKeyDown(Jump))
        {
            JumpFrame = 3;       
        }
    }

    void CheckVerticalMove()
    {
        if (Input.GetKey(UpMoveKey))
        {
            v = 1;
        }
        else if (Input.GetKey(DownMoveKey))
        {
            v = -1;
        }
        else
        {
            v = 0;
        }
    }

    void CheckHorzontalMove()
    {
		if (Input.GetKeyDown(RightMoveKey) )
		{
				MoveDir = 1;
		}
		else if (Input.GetKeyDown(LeftMoveKey))
		{
				MoveDir = -1;
		}
		else if (Input.GetKeyUp(RightMoveKey) || Input.GetKeyUp(LeftMoveKey))
		{
            MoveDir = Input.GetKey(LeftMoveKey) ? -1 : 0;
            if (MoveDir == 0) MoveDir = Input.GetKey(RightMoveKey) ? 1 : 0;
        }
	}

}
