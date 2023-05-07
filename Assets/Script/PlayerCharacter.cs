using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

/// <summary>
/// 玩家现在的方向
/// </summary>
public enum PlayDir
{
    Right,
    Left,
}
/// <summary>
/// 状态枚举
/// </summary>
public enum PlayState
{
    Normal,
    Jump,
    Climb,
    Dash,
    Fall,
}
public class PlayerCharacter : MonoBehaviour
{
    public Vector3 Velocity;

    public PlayState playState;

    PlayDir playDir;
	PlayDir lastDir;
	[SerializeField]
	public bool IsPlayer2;
    public float MoveSpeed;
	//coyotetime is the time that player could still jump
	//when already get out of the platform and start falling
	public int MaxCoyotetimeFrame;
    public Animator playAnimator;
    bool isIntroJump;					  //is intro jump
    public bool isCanControl = true;      //could control
    public bool isMove = true;            //could move
	bool isAlive;
	Vector2 boxSize;
	Rigidbody2D rig;
	InputManager input;
	int playerLayerMask;               
    RaycastHit2D DownBox;
    RaycastHit2D[] UpBox;
    RaycastHit2D[] RightBox;
    RaycastHit2D[] LeftBox;
    RaycastHit2D[] HorizontalBox;
    float startJumpPos;          
    public float JumpMax;        
    public float JumpMin;        
    public float JumpSpeed;         
    public float ClimbSpeed;

	public AudioClip[] AudioClips;// 0 - dash /1 - jump /2- dead
	private AudioSource audioSource;
	Vector2 DashDir;

	int dashCount = 0;
	int CoyotetimeFrame = 0;

	//speed when horizental move
	float moveH;
	//direction when horizental move
	int introDir;        

    void Start()
    {
		isAlive = true;
		input = this.GetComponent<InputManager>();
		rig = GetComponent<Rigidbody2D>();
		if(IsPlayer2)
        {
			playerLayerMask = LayerMask.GetMask("Player2");
		}
		else
        {
			playerLayerMask = LayerMask.GetMask("Player");
		}
       
        playerLayerMask = ~playerLayerMask;
        boxSize = new Vector2(1, 1.2f);
		audioSource = this.GetComponent<AudioSource>();

	}

    private void FixedUpdate()
    {
		if (!isAlive) return;

		if (CoyotetimeFrame > 0)
		{
			CoyotetimeFrame--;
		}
        HorizontalMove();
        switch (playState)
        {
            case PlayState.Normal:
                Normal();
                break;
            case PlayState.Climb:
                Climb();
                break;
            case PlayState.Fall:
                Fall();
                break;
            case PlayState.Dash:
                CheckDashJump();
                break;
        }
        rig.MovePosition(transform.position + Velocity * Time.fixedDeltaTime);
    }

	public void HandleWin()
    {
		StopAllCoroutines();
		Velocity = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
		if (!isAlive ) return;
        RayCastBox();
        CheckDir();
		CheckDash();
	}


    /// <summary>
    /// HorizontalMove
    /// </summary>
    void HorizontalMove()
    {
		if(input.MoveDir == 0)
        {
			playAnimator.SetBool("IsRun", false);
		}
		if (isCanMove())
        {
			if ((Velocity.x > 0 && input.MoveDir == -1) || (Velocity.x < 0 && input.MoveDir == 1) || input.MoveDir == 0 || Mathf.Abs(Velocity.x) > MoveSpeed)
            {
                introDir = Velocity.x > 0 ? 1 : -1;
                moveH = Mathf.Abs(Velocity.x);
				if(isGround)
				{
					moveH -= MoveSpeed / 3;
				}
				else
				{
					moveH -= MoveSpeed / 6;
				}
				if (moveH < 0.01f)
                {
                    moveH = 0;
                }
                Velocity.x = moveH * introDir;
            }
			else
			{
				if (input.MoveDir == 1 && !(isGround && input.v < 0))
				{
					if (isGround)
					{
						Velocity.x += MoveSpeed / 6;
						playAnimator.SetBool("IsRun",true);
					}
					else
					{
						Velocity.x += MoveSpeed / 15f;
					}
					if (Velocity.x > MoveSpeed) Velocity.x = MoveSpeed;
                }
				else if (input.MoveDir == -1 && !(isGround && input.v < 0))
				{
					if (isGround)
					{
						Velocity.x -= MoveSpeed / 6;
						playAnimator.SetBool("IsRun", true);
					}
					else
					{
						Velocity.x -= MoveSpeed / 12f;
					}
					if (Velocity.x < -MoveSpeed) Velocity.x = -MoveSpeed;
                }
			}
  
        }
    }
	bool CheckUpMove()
	{
		if (UpBox.Length == 1)
		{
			var pointDis = UpBox[0].point.x - transform.position.x;
			if (pointDis > 0.34f)
			{
				var offsetPos = Mathf.Floor(transform.position.x);
				transform.position = new Vector3(offsetPos + 0.48f, transform.position.y, 0);
				return true;
			}
			else if (pointDis < -0.34f)
			{
				var offsetPos = Mathf.Floor(transform.position.x);
				transform.position = new Vector3(offsetPos + 0.52f, transform.position.y, 0);
				return true;
			}
			else
			{
				Velocity.y = 0;
				playState = PlayState.Fall;
				return false;
			}
		}
		return true;
	}

	bool CouldChangeToState(PlayState state)
    {
        switch (state)
        {
			case PlayState.Climb:
				return (playState != PlayState.Dash && playState != PlayState.Jump) && BoxCheckCanClimb() && isCanControl && !isIntroJump;
			case PlayState.Fall:
				return playState != PlayState.Dash && playState != PlayState.Jump && playState != PlayState.Climb;
			default:
				return true;
		}
    }

    bool isCanMove()
    {
        return playState != PlayState.Dash && playState != PlayState.Climb && isCanControl && isMove;
    }

    /// <summary>
    /// raycasting four directions for detecting collision
    /// </summary>
    void RayCastBox()
    {
        RightBox = Physics2D.BoxCastAll(CentralPosition, boxSize, 0, Vector3.right, 0.1f, playerLayerMask);
        LeftBox = Physics2D.BoxCastAll(CentralPosition, boxSize, 0, Vector3.left, 0.1f, playerLayerMask);
        UpBox = Physics2D.BoxCastAll(CentralPosition, boxSize, 0, Vector3.up, 0.05f, playerLayerMask);
        DownBox = Physics2D.BoxCast(CentralPosition, boxSize, 0, Vector3.down, 0.05f, playerLayerMask);
    }

    /// <summary>
    /// On Land State
    /// </summary>
    public void Normal()
    {
        if (!isGround)
        {
			CoyotetimeFrame = MaxCoyotetimeFrame;
			playState = PlayState.Fall;
            return;
        }
		Velocity.y = 0;
		dashCount = 1;
		if(input.JumpFrame>0)
		{
			Jump();
			return;
		}
    }

    /// <summary>
    /// falling down
    /// </summary>
    public void Fall()
    {
        if (isGround)
        {
            playState = PlayState.Normal;
            return;
        }
		if(CoyotetimeFrame > 0 && input.JumpKeyDown)
		{
			CoyotetimeFrame = 0;
			Velocity.y = 0;
			Jump();
			return;
		}
		//when falling down, if could climb, but try to jump, then jump on wall.
		if (input.JumpKeyDown && BoxCheckCanClimb() && !input.ClimbKey && !CheckIsClimb())
		{
			Velocity.y = 0;
			Velocity.x = 0;
			Jump(new Vector2(4 * -GetClimpDirInt, 0), new Vector2(24 , 0));
			return;
		}
		if (CouldChangeToState(PlayState.Fall))
        {
            Velocity.y -= 150f * Time.deltaTime;
            Velocity.y = Mathf.Clamp(Velocity.y, -25, Velocity.y);
            if (CouldChangeToState(PlayState.Climb) && (CheckIsClimb() || input.ClimbKey))
            {
                playState = PlayState.Climb;
            }
        }
    }

    /// <summary>
    /// Check if there is a wall that could climb
    /// </summary>
    /// <returns></returns>
    bool BoxCheckCanClimbDash()
    {
		RightBox = Physics2D.BoxCastAll(CentralPosition, boxSize, 0, Vector3.right, 0.4f, playerLayerMask);
		LeftBox = Physics2D.BoxCastAll(CentralPosition, boxSize, 0, Vector3.left, 0.4f, playerLayerMask);
		if (RightBox.Length > 0)
        {
            HorizontalBox = RightBox;
        }
        else if (LeftBox.Length > 0)
        {
			HorizontalBox = LeftBox;
        }
        return RightBox.Length != 0 || LeftBox.Length != 0;
    }

	bool BoxCheckCanClimb()
	{
		if (RightBox == null || LeftBox == null) return false;
		if (RightBox.Length > 0)
		{
			HorizontalBox = RightBox;
		}
		else if (LeftBox.Length > 0)
		{
			HorizontalBox = LeftBox;
		}
		return RightBox.Length > 0 || LeftBox.Length > 0;
	}

	/// <summary>
	/// Check if is climbing on the wall
	/// </summary>
	/// <returns></returns>
	bool CheckIsClimb()
    {
        return (input.MoveDir < 0 && LeftBox.Length > 0) || (input.MoveDir > 0 && RightBox.Length > 0);
    }

    /// <summary>
    /// climb
    /// </summary>
    void Climb()
    {
		bool CheckBox = BoxCheckCanClimb();
		if (!input.ClimbKey || !CheckBox)
        {
            if(isGround)
            {
                playState = PlayState.Normal;
                return;
            }
            if (!CheckIsClimb())
            {
                playState = PlayState.Fall;
                return;
            }
        }
        Velocity.x = 0;
		playDir = HorizontalBox == RightBox ? PlayDir.Right : PlayDir.Left;

		if (CouldChangeToState(PlayState.Climb))
        {
            //if you are about to get to the top of the wall, either fall a distance or jump on the wall
            if (input.v <= 0 && transform.position.y - HorizontalBox[0].point.y > 0.7f || !input.ClimbKey)
            {
                Velocity.y = -ClimbSpeed;
            }
            else if (transform.position.y - HorizontalBox[0].point.y <= 0.7f || input.ClimbKey)
            {
                Velocity.y = input.v * ClimbSpeed;
            }
        }
		//wall jump
		if(input.JumpKeyDown)
		{
			if(input.ClimbKey)
			{
				if((input.MoveDir > 0 && GetDirInt < 0) || (input.MoveDir < 0 && GetDirInt > 0))
				{
					Jump(new Vector2(8 * -GetDirInt, 0), new Vector2(24 , 0));
				}
				else
				{
					Jump();
				}
			}
			else
			{
				Jump(new Vector2(8 * -GetDirInt, 0), new Vector2(24 , 0));
			}
		}

    }


    /// <summary>
    /// Jump functions
    /// </summary>
    void Jump(Vector2 vel, Vector2 maxVel)
    {
        playAnimator.SetTrigger("IsJump");
		audioSource.clip = AudioClips[1];
		audioSource.Play();
		playState = PlayState.Jump;
		startJumpPos = transform.position.y;
		isIntroJump = true;
		if (vel.y >= 0)
			Velocity.y = vel.y;
		StartCoroutine(IntroJump(vel, maxVel));
	}
    void Jump()
    {
		Jump(Vector2.zero, Vector2.zero);
	}
	
	bool CheckForWallJump()
    {
		if (input.JumpKeyDown && BoxCheckCanClimb() && !input.ClimbKey && !CheckIsClimb())
		{
			Velocity.y = 0;
			isIntroJump = false;
			Jump(new Vector2(4 * -GetDirInt, 0), new Vector2(24, 0));
			return true;
		}
		return false;
	}

	bool CheckForHitWall()
    {
		if (!CheckUpMove())
		{
			Velocity.y = 0;
			isIntroJump = false;
			isMove = true;
			return true;
		}
		return false;
	}


    IEnumerator IntroJump(Vector2 vel, Vector2 maxVel)
    {
        float dis = 0;
		// move up
		float curJumpMin = JumpMin * (vel.y + JumpSpeed) / JumpSpeed;
		float curJumpMax = JumpMax * (vel.y + JumpSpeed) / JumpSpeed;
		float curJumpSpeed = JumpSpeed + vel.y;
		while (playState == PlayState.Jump && dis <= curJumpMin && Velocity.y < curJumpSpeed)
        {
			if (vel.x != 0 && Mathf.Abs(Velocity.x) < maxVel.x)
			{
				isMove = false;
				Velocity.x += vel.x;
				if(Mathf.Abs(Velocity.x) > maxVel.x)
				{
					Velocity.x = maxVel.x * GetDirInt;
				}
			}
			if(CheckForHitWall()) yield break;

			dis = transform.position.y - startJumpPos;
			if (vel.y <= 0)
			{
				Velocity.y += 240 * Time.fixedDeltaTime;
			}
			yield return new WaitForFixedUpdate();
        }
		Velocity.y = curJumpSpeed;
		isMove = true;
		while (playState == PlayState.Jump && input.JumpKey && dis < curJumpMax)
        {
			if (CheckForHitWall()) yield break;
			if (CheckForWallJump())yield break;

			dis = transform.position.y - startJumpPos;
            Velocity.y = curJumpSpeed;
            yield return new WaitForFixedUpdate();
        }
		// slow down
		while (playState == PlayState.Jump && Velocity.y > 0 )
        {
			if (CheckForHitWall()) yield break;
			if (CheckForWallJump()) yield break;

			if (dis > JumpMax)
            {
                Velocity.y -= 100 * Time.fixedDeltaTime;
            }
            else
            {
                Velocity.y -= 200 * Time.fixedDeltaTime;
            }
            yield return new WaitForFixedUpdate();
        }
        // fall down
        Velocity.y = 0;
        yield return 0.1f;
        isIntroJump = false;
        playState = PlayState.Fall;
    }

	void Dash()
    {
        Velocity = Vector2.zero;
		//we never allow more than 1 dash count
		dashCount = 0;
		playState = PlayState.Dash;
		StopAllCoroutines();
		audioSource.clip = AudioClips[0];
		audioSource.Play();
		StartCoroutine(IntroDash());
    }

    IEnumerator IntroDash()
    {
		//Get direction
		float verticalDir;
		if(isGround && input.v < 0)  //if player on the ground and press down, ignore the input
		{
			verticalDir = 0;
		}
		else
		{
			verticalDir = input.v;
		}
		//normalize the dashing direction
		DashDir = new Vector2(input.MoveDir, verticalDir).normalized;
        if(DashDir == Vector2.zero)
        {
			DashDir = Vector3.right * GetDirInt;
        }
        int i = 0;
        isCanControl = false;
        while (i < 9)
        {
			if(playState == PlayState.Dash)
			{
				Velocity = DashDir * 30f;
			}
			i++;
			yield return new WaitForFixedUpdate();
        }
        isCanControl = true;
	}

	void CheckDashJump()
	{
		if (input.JumpKeyDown)
		{
			if (DashDir == Vector2.up && BoxCheckCanClimbDash())
			{
				Jump(new Vector2(4 * -GetClimpDirInt, 24 - JumpSpeed + 6), new Vector2(24, 0));
			}
			else if (isGround)
			{
				Velocity.y = 0;
				Jump();
			}
		}
		else if(isCanControl)
		{
			if (DashDir.y > 0)
			{
				Velocity.y = 24;
			}
			if (isGround)
				playState = PlayState.Normal;
			else
				playState = PlayState.Fall;
		}
	}

	void CheckDash()
    {
		if (input.DashKeyDown && dashCount > 0 && !input.IsWin)
		{
			Dash();
			playAnimator.SetTrigger("IsDash");
		}
	}

    void CheckDir()
    {
        if (playState == PlayState.Climb || playState == PlayState.Dash || input.MoveDir == 0)
            return;
        lastDir = playDir;
		playDir = input.MoveDir > 0 ? PlayDir.Right : PlayDir.Left;

        if(lastDir != playDir)
        {
            transform.localScale = new Vector3(GetDirInt, transform.localScale.y, transform.localScale.z);
        }
    }

    //facing value: 1 for right -1 for left.
    int GetDirInt { get { return playDir == PlayDir.Right ? 1 : -1; } }

	//wall climp jump direction should be on the opposite direction of wall
	int GetClimpDirInt{ get { return HorizontalBox == RightBox ? 1 : -1; } }

    //the central of the player
    Vector2 CentralPosition { get { return transform.position - new Vector3(0, 0.4f); } }

    //check if player is on the ground
    bool isGround { get { return DownBox.collider != null ? true : false; } }

	void OnTriggerEnter2D(Collider2D Collider)
    {
		if(Collider.tag == "Trap" &&isAlive)
        {
			isAlive = false;
			playAnimator.SetBool("IsRun", false);
			playAnimator.SetBool("IsDead", true);
			audioSource.clip = AudioClips[2];
			audioSource.Play();
			GameManager.Instance.ReloadScene(1.3f);
		}
    }
}
