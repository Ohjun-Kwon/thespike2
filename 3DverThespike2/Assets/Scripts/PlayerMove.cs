
using UnityEngine;
using static Constants;
using playerStatsNameSpace;
using static PhysCalculate;

public class slideVar
{
    public float slideSpeedX;
    public float slideSpeedZ;
    public float slideTime;
    public float slideSpeedXFriction;
    public float slideSpeedZFriction;   


    public void setSlide(float slideSpeedX , float slideSpeedZ , float slideTime){
        this.slideSpeedX = slideSpeedX;
        this.slideSpeedZ = slideSpeedZ;
        slideSpeedXFriction = slideSpeedX*playSpeed / slideTime;
        slideSpeedZFriction = slideSpeedZ*playSpeed / slideTime;
    }
}

public class PlayerMove : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 movingDir = Vector3.left;
    [SerializeField] public bool isMove = false; // 움직이는 지 안움직이는 지   
    public bool isSlow = false; // 느리게 움직이는지
    [SerializeField]public bool isSlide = false;
    public float landY;
    public float Height;
    private float jumpTime;
    [SerializeField] private float actionTime;
    private float jumpType;
    [SerializeField]public float score;

    //[SerializeField] public GameObject  lineX;
    [SerializeField] public GameObject SystemObject;
    [SerializeField] public GameObject Ball;
    private MainControl mainControl;
    private PlayerSetting playerSetting;
    private playerStats Status;
    [SerializeField]public Vector3 goal;
 
    Transform highXTransform;

    [SerializeField]public float movingSpeed;
    private BoxCollider boxCollider;      
    private ObjectGravity gravityControl;
    private movePhysics movePhys;
    private slideVar slideVar;
    private TimeTrigger timeTrigger;
    private float moveDelay = 0.0f;
    
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        Height = boxCollider.bounds.size.y;
        
        landY = 0.0f + Height/2;
        
        movePhys = GetComponent<movePhysics>();
        boxCollider = GetComponent<BoxCollider>();
        gravityControl = GetComponent<ObjectGravity>();
        mainControl = SystemObject.GetComponent<MainControl>();
        movePhys = GetComponent<movePhysics>();
        playerSetting = GetComponent<PlayerSetting>();
        timeTrigger = SystemObject.GetComponent<TimeTrigger>();
        slideVar = new slideVar();
        //highXTransform = lineX.gameObject.GetComponent<Transform>();

        movePhys.initPhysics();
    }
    public void setStatus(){
        Status = GetComponent<PlayerSetting>().getStatus();       
    }

    public bool DoSlide() {
        if (isSlide) return false;
        if (!isMoveDelayFree()) return false;

        
        isSlide = true;
        movePhys.setVector(90 - playerSetting.getTeam() *70 ,1.7f);
        movePhys.startParabola();
        float slideTime = 3.0f;
        slideVar.setSlide(movePhys.getHorizontalSpeed() , movePhys.getDepthSpeed() , slideTime);        
        //슬라이드 시간 이후에 Slide를 끈다.
        timeTrigger.addTrigger(movePhys.getFlightTime() + slideTime,()=> {
            isSlide = false;
            setMoveDelay(5.0f);
        });
        return true;
    }
    public void setJumpTime(float x,float _jump_type) {
        
        if (_jump_type == JUMP_NO) return; // 점프가 아니면 CUT!
        
        jumpTime = timeTrigger.getMainTimeFlow() + x;
        jumpType = _jump_type;
    }
    public void setActionTime(float x) {
       actionTime = timeTrigger.getMainTimeFlow() + x;
    }

    public bool isAvailableToMove() {
        if (isSlide) return false;
        if (!isMoveDelayFree()) return false;
        return (isMove);
    }
    public void PlayerFixedUpdate()
    {
        checkMoveDelay();
        if (isAvailableToMove()) {
            float speed = isSlow ? SLOW_SPEED : 1.0f; // Slow면 0.3배로 간다.
                movePhys.moveLinear(movingDir , Status.getSpeed() * speed);
        }
        if (isSlide) {
            if (movePhys.isParabolaEnd()){
                movePhys.moveLinear( new Vector3 (slideVar.slideSpeedX,0f,slideVar.slideSpeedZ), 1f);
                slideVar.slideSpeedX -= slideVar.slideSpeedXFriction;
                slideVar.slideSpeedZ -= slideVar.slideSpeedZFriction;
            }
        }
        if (timeTrigger.getMainTimeFlow() >= jumpTime) { DoJump(jumpType); jumpTime = INF;}

    }   
    

    public void setMoveDelay(float md){
        moveDelay += md;
    }
    public float getMoveDelay(){
        return moveDelay;
    }    
    public void checkMoveDelay(){
        if (moveDelay > 0.0f) moveDelay -= Constants.playSpeed;
        else moveDelay = 0.0f;
    }
    public bool isMoveDelayFree(){
        return moveDelay == 0.0f;
    }


    /// <summary> 본인의 위치에서 주어진 x,z값까지 본인의 속도와 상태로, 주어진 시간 내로 도달 할 수 있는지 여부 반환 </summary>
    /// <param name="x">도달할 x위치</param>
    /// <param name="z">도달할 z위치</param>
    /// <param name="left_time">도달하는 데의 시간</param>
    /// <param name = "isSlow"> 느리게 가는지의 시간</param>
    /// <param name = "slack_time">얼마나 더 여유 시간을 줄 지</param>
    /// <returns>도달 가능 여부</returns>
    public bool IsArrivedInTime(float x , float z,float left_time,bool isSlow = false,float slack_time = 0.0f, bool isOkay = false){
        
        float slowSpeed = isSlow ? SLOW_SPEED : 1.0f;   
        float distance = Mathf.Max(Mathf.Abs(x - transform.position.x) , Mathf.Abs(z - transform.position.z));
        float costTime = distance/( Status.getSpeed() * slowSpeed) + getMoveDelay(); // moveDelay도 추가해준다.


        if (!movePhys.isParabolaEnd())
            costTime +=  movePhys.getFlightTime() - movePhys.getCurrentTime();
        if (costTime < (left_time - slack_time)) // 시간 내에 도달 할 수 있으면 true
            return true;
        else
            return false;
    }

    public float getArrivedTime(float x , float z,bool isSlow = false){
        float slowSpeed = isSlow ? SLOW_SPEED : 1.0f;
        float distance = Mathf.Max(Mathf.Abs(x - transform.position.x) , Mathf.Abs(z - transform.position.z));
        float costTime = distance/( Status.getSpeed() * slowSpeed) + getMoveDelay(); // moveDelay도 추가해준다.

        if (!movePhys.isParabolaEnd()) {
            Debug.Log($"{playerSetting.getTeam()}, {playerSetting.getPositionName()} {movePhys.getFlightTime() - movePhys.getCurrentTime()} ");
            costTime += movePhys.getFlightTime() - movePhys.getCurrentTime(); // 점프 끝나는데 걸리는 시간 중 더 큰건? 
        }
        return costTime;
    }

    public bool IsAvailableToQuick(float x , float y, float z, float left_time , bool isTop , float delay) {
        return (mainControl.getFollowingPlayer() != gameObject && IsArrivedInTime(x,z,left_time)  && (left_time > 0));
    }
    public bool IsAvailableToAttack(float x , float y, float z, float left_time ,bool isTop , float delay) {
        
        return ( IsArrivedInTime(x,z,left_time) && (left_time > 0));
    }
    /// <summary>
    /// 해당 Player에게 Ball이 어디에 떨어질 지. 계산하는 함수. 
    /// (Player의 점프력 , 키에 따라 x가 달라지기 때문에 각각에게 낙하지점은 다르다.)
    /// 만약 해당 Player의 y값에 Ball이 떨어지지 못 한다면, NaN값을 반환.
    /// </summary>
    /// <param name="controlPlayer"></param>
    /// <param name="jump">어떤 종류의 점프인지</param>
    /// <param name="isTop">플레이어의 머리</param>
    /// <param name ="isFall">내려가고 있는 상태의 볼인지</param>
    /// <returns></returns>
    public float getFallingPlaceXbyPlayer(float _jump_type,bool isTop,float DELAY){
        movePhysics playerPhys = GetComponent<movePhysics>();
        PlayerSetting playerSet = GetComponent<PlayerSetting>();
        movePhysics ballPhys = Ball.GetComponent<movePhysics>();        
        int team  = playerSet.getTeam();

        float add_playerHeight = (isTop ? playerPhys.getHeight() / 2 :  0.0f);
        float maxY = 0.0f; 
        float x = ballPhys.getParabolaXbyMaxY(playerPhys.getLandBody_Y() + getMaxHeightBySpeed(playerSet.Status.getJump() * _jump_type) - DELAY + add_playerHeight, isTop , ref maxY);
        
        return x;
    }


    /// <summary>
    /// 해당 x좌표까지 도달하는데 플레이어가 걸리는 시간
    /// 좌표가 NaN값이면 , 도달하는데 걸리는 시간은 INF가 된다.
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public float getTakingTimeToLinearX(float x) {
        if ( float.IsNaN(x) ) return INF;
        var dis = Mathf.Abs(x - transform.position.x);
        var spd = Status.getSpeed();
        return  dis / spd;       
    }

    /// <summary>
    /// 해당 z좌표까지 도달하는데 플레이어가 걸리는 시간
    /// 좌표가 NaN값이면 , 도달하는데 걸리는 시간은 INF가 된다.
    /// </summary>
    /// <param name="z"></param>
    /// <returns></returns>
    public float getTakingTimeToLinearZ(float z) {
        if ( float.IsNaN(z) ) return INF;
        var dis = Mathf.Abs(z - transform.position.z);
        var spd = Status.getSpeed();
        return  dis / spd;       
    }
    public void DoJump(float _jump_type) {
        if (movePhys.isParabolaEnd()) {
            mainControl.playSound("FOOT");
            movePhys.setVector(90f,Status.getJump() * _jump_type);
            movePhys.startParabola();
            //highXTransform.position = new Vector3(0.0f,movePhys.getFlightMaxY(Status.getJump()) + Height / 2  , 0.0f);
        }
        
    }
    public void DoSpike(Vector3 goal) {
        if (goal != mainControl.getGoal()) return;
        if (playerSetting.getPlayerAction() == ACTION_SPIKEREADY || playerSetting.getPlayerAction() == ACTION_QUICKREADY)
        {
            float swing_time = GetComponent<PlayerSetting>().getStatus().getSwingTime(); // 스윙 속도는 사람마다 다를 예정.
            playerSetting.setPlayerAction(ACTION_SPIKESWING); // 스파이크 스윙을 합니다.
            timeTrigger.addTrigger(swing_time,()=> { HitSpike(goal); });
        }
    } 
    public void DoToss(Vector3 goal) {
        if (goal != mainControl.getGoal()) return;
        if (playerSetting.getPlayerAction() == ACTION_JUMPTOSSREADY || playerSetting.getPlayerAction() == ACTION_TOSSREADY)
        {
            playerSetting.setPlayerAction(ACTION_TOSS); 
            float toss_time = GetComponent<PlayerSetting>().getStatus().getTossTime();
            timeTrigger.addTrigger(toss_time,()=> { HitToss(goal); });
        }
    } 
    public void DoReceive(Vector3 goal) {
        if (playerSetting.getPlayerAction() != ACTION_RECEIVEREADY) return;
        if (goal != mainControl.getGoal()) return;

        float receive_time = GetComponent<PlayerSetting>().getStatus().getReceiveTime();
        playerSetting.setPlayerAction(ACTION_RECEIVE);
        timeTrigger.addTrigger(receive_time,()=> {
            HitReceive(goal);
        });
    }     

    public void HitToss(Vector3 goal) {
        if (playerSetting.getPlayerAction() != ACTION_TOSS) return;
        if (goal != mainControl.getGoal()) return;
        bool isCollider = isInBox(goal, Ball.GetComponent<movePhysics>().getHeight());
        if (!isCollider) return;

        setMoveDelay(5.0f);
        playerSetting.setPlayerAction(ACTION_TOSSDONE);                    
        mainControl.setLastTouch(gameObject);
        Ball.transform.position = goal;
        mainControl.Ball.GetComponent<BallMovement>().ballToss(playerSetting.getTeam());

    } 
    public void HitReceive(Vector3 goal) {
        if (playerSetting.getPlayerAction() != ACTION_RECEIVE) return;
        if (goal != mainControl.getGoal()) return;
        bool isCollider = isInBox(goal , Ball.GetComponent<movePhysics>().getHeight());
        if (!isCollider) return;
        
        
        setMoveDelay(5.0f);
        playerSetting.setPlayerAction(ACTION_RECEIVEDONE);
        mainControl.setLastTouch(gameObject);
        Ball.transform.position = goal;
        mainControl.Ball.GetComponent<BallMovement>().ballReceive(playerSetting.getTeam());
        
    }     
    // 스파이크를 때리는 것
    public void HitSpike(Vector3 goal) {
        if (playerSetting.getPlayerAction() != ACTION_SPIKESWING) return;
        if (goal != mainControl.getGoal()) return;

        mainControl.showDebug("Do SPike~",1);
        bool isCollider = isInBox(goal , Ball.GetComponent<movePhysics>().getHeight());
        if (!isCollider) return;
        mainControl.showDebug("Hello~~",1);
        setMoveDelay(5.0f); // 움직임 딜레이 
        playerSetting.setPlayerAction(ACTION_SPIKEDONE);   
        mainControl.setLastTouch(gameObject);        
        Ball.transform.position = goal;
        mainControl.Ball.GetComponent<BallMovement>().ballSpike(playerSetting.getTeam());
        mainControl.showDebug("Spike!");
        
    }
    public void HitBlock(Vector3 ballPos , Vector3 goal) {
        if (goal != mainControl.getGoal()) return; // 볼의 움직임이 만약 달라졌다면.
        if (playerSetting.getPlayerAction() != ACTION_BLOCKJUMP) return; // 블로킹 상태가 아니라면.

        bool isCollider = isInBox(ballPos,Ball.GetComponent<movePhysics>().getHeight());

        if (isCollider) {
            setMoveDelay(10.0f); // 움직임 딜레이 
            playerSetting.setPlayerAction(ACTION_BLOCKDONE);   
            mainControl.setLastTouch(gameObject);        
            mainControl.Ball.GetComponent<BallMovement>().ballBlock(playerSetting.getTeam());
            mainControl.showDebug("Block!!");
        }      
    }

    /// <summary>
    /// 현재 좌표가 자기의 범위에 들어오는지 판단하는 함수.
    /// </summary>
    /// <returns></returns>
    public bool isInBox(Vector3 pos ,float size) {
        Vector3 myPos = transform.position;
        Vector3 mySize = boxCollider.bounds.size;
        if (pos.x - size/2 <= myPos.x + mySize.x / 2)
        if (pos.x + size/2 >= myPos.x - mySize.x / 2)
        if (pos.y - size /2 <= myPos.y + mySize.y / 2)
        if (pos.y + size /2 >= myPos.y - mySize.y / 2)
        if (pos.z - size /2 <= myPos.z + mySize.z / 2)
        if (pos.z + size /2 >= myPos.z - mySize.z / 2)
            return true;
        return false;
    }
    /// <summary>
    /// 정해준 특정 좌표에 있을 때 내 범위에 있는지 판단하는 함수.
    /// </summary>
    /// <param name="myPos"></param>
    /// <param name="pos"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public bool isInBoxOnPos(Vector3 myPos,Vector3 pos ,float size) {
        Vector3 mySize = boxCollider.bounds.size;
        if (pos.x - size/2 <= myPos.x + mySize.x / 2)
        if (pos.x + size/2 >= myPos.x - mySize.x / 2)
        if (pos.y - size /2 <= myPos.y + mySize.y / 2)
        if (pos.y + size /2 >= myPos.y - mySize.y / 2)
        if (pos.z - size /2 <= myPos.z + mySize.z / 2)
        if (pos.z + size /2 >= myPos.z - mySize.z / 2)
            return true;
        return false;
    }
}

