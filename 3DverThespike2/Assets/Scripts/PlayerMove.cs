
using UnityEngine;
using static Constants;
using playerStatsNameSpace;
using static PhysCalculate;

public class PlayerMove : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 movingDir = Vector3.left;
    public bool isMove = false; // 움직이는 지 안움직이는 지   
    public bool isSlow = false; // 느리게 움직이는지
    public float landY;
    public float Height;
    private float jumpTime;
    [SerializeField] private float spikeTime;
    private float jumpType;
    [SerializeField]public float score;

    //[SerializeField] public GameObject  lineX;
    [SerializeField] public GameObject SystemObject;
    [SerializeField] public GameObject Ball;
    private MainControl mainControl;
    private PlayerSetting playerSetting;
    private playerStats Status;

    Transform highXTransform;

    [SerializeField]public float movingSpeed;
    private BoxCollider boxCollider;      
    private ObjectGravity gravityControl;
    private movePhysics movePhys;
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
        playerSetting = GetComponent<PlayerSetting>();
        timeTrigger = SystemObject.GetComponent<TimeTrigger>();
        //highXTransform = lineX.gameObject.GetComponent<Transform>();

        movePhys.initPhysics();
    }
    public void setStatus(){
        Status = GetComponent<PlayerSetting>().getStatus();       
    }

    public void setJumpTime(float x,float _jump_type) {
        
        if (_jump_type == JUMP_NO) return; // 점프가 아니면 CUT!
        
        jumpTime = timeTrigger.getMainTimeFlow() + x;
        jumpType = _jump_type;
    }
    public void setSpikeTime(float x) {
    
       spikeTime = timeTrigger.getMainTimeFlow() + x;
    }
    void FixedUpdate()
    {
        checkMoveDelay();
        if (isMove) {
            float speed = isSlow ? SLOW_SPEED : 1.0f; // Slow면 0.3배로 간다.
            if (isMoveDelayFree()) 
                movePhys.moveLinear(movingDir , Status.getSpeed() * speed);
        }
        if (timeTrigger.getMainTimeFlow() >= jumpTime) { DoJump(jumpType); jumpTime = INF;}
        if (timeTrigger.getMainTimeFlow() >= spikeTime){ 
            spikeTime = INF;
            if ((playerSetting.getPlayerAction() == ACTION_SPIKEREADY) || (playerSetting.getPlayerAction() == ACTION_QUICKREADY)) DoSpike(); 
            else if (playerSetting.getPlayerAction() == ACTION_SPIKESWING) { 
                playerSetting.setPlayerAction(ACTION_SPIKEDONE); 
                GetComponent<Renderer>().material.SetColor("_Color", Color.white);
            }     
        }
    }
    

    public void setMoveDelay(float md){
        moveDelay = md;
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


    /// <summary> 본인의 위치에서 주어진 x값까지 본인의 속도로, 주어진 시간 내로 도달 할 수 있는지 여부 반환 </summary>
    /// <param name="x">도달할 거리</param>
    /// <param name="left_time">도달하는 데의 시간</param>
    /// <param name = "isSlow"> 느리게 가는지의 시간</param>
    /// <param name = "slack_time"></param>
    /// <returns>도달 가능 여부</returns>
    public bool IsArrivedInTime(float x , float left_time,bool isSlow = false,float slack_time = 0.0f){
        float slowSpeed = isSlow ? SLOW_SPEED : 1.0f;
        if (Mathf.Abs(x - transform.position.x)/( Status.getSpeed() * slowSpeed) < (left_time - slack_time)) // 시간 내에 도달 할 수 있으면 true
            return true;
        else
            return false;
    }
    /// <summary>
    /// 해당 Player에게 Ball이 어디에 떨어질 지. 계산하는 함수. 
    /// (Player의 점프력 , 키에 따라 x가 달라지기 때문에 각각에게 낙하지점은 다르다.)
    /// 만약 해당 Player의 y값에 Ball이 떨어지지 못 한다면, NaN값을 반환.
    /// </summary>
    /// <param name="controlPlayer"></param>
    /// <param name="jump">어떤 종류의 점프인지</param>
    /// <param name="isTop">플레이어의 머리</param>
    /// <returns></returns>
    public float getFallingPlaceXbyPlayer(float _jump_type,bool isTop,float DELAY){

        movePhysics playerPhys = GetComponent<movePhysics>();
        PlayerSetting playerSet = GetComponent<PlayerSetting>();
        movePhysics ballPhys = Ball.GetComponent<movePhysics>();        
        int team  = playerSet.getTeam();

        float add_playerHeight = isTop ? playerPhys.getHeight() / 2 :  0.0f; // + ballPhys.getHeight() / 2 원래 볼 높이도 더해줘야 하는데 , 볼 높이까지는 안더해줌. 그 정도 여유는 줘야 함.
        float maxY = 0.0f; 
        return ballPhys.getParabolaXbyMaxY(playerPhys.getLandBody_Y() + getMaxHeightBySpeed(playerSet.Status.getJump() * _jump_type) - 0.05f - DELAY + add_playerHeight, true , ref maxY);    
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
            movePhys.setVector(90f,Status.getJump() * _jump_type);
            movePhys.startParabola();
            //highXTransform.position = new Vector3(0.0f,movePhys.getFlightMaxY(Status.getJump()) + Height / 2  , 0.0f);
        }
    }
    public void DoSpike() {
        float swing_time = 1.0f; // 스윙 속도는 사람마다 다를 예정.
        playerSetting.setPlayerAction(ACTION_SPIKESWING); // 스파이크 스윙을 합니다.
        spikeTime = timeTrigger.getMainTimeFlow() + swing_time * 2;
        GetComponent<Renderer>().material.SetColor("_Color", Color.black);
    }
    void OnTriggerEnter(Collider other) {
        if (other.tag == "Ball") {
            if (gameObject == mainControl.followingPlayer) {
                mainControl.setLastTouch(gameObject);

                if (playerSetting.getPlayerAction() == ACTION_RECEIVE) { 
                    mainControl.Ball.GetComponent<BallMovement>().ballReceive(playerSetting.getTeam());
                    playerSetting.setPlayerAction(ACTION_RECEIVEDONE);
                }
                else if (playerSetting.getPlayerAction() == ACTION_TOSS|| playerSetting.getPlayerAction() == ACTION_JUMPTOSS) { 
                    mainControl.Ball.GetComponent<BallMovement>().ballToss(playerSetting.getTeam());
                    playerSetting.setPlayerAction(ACTION_TOSSDONE);                    
                }
                else if (playerSetting.getPlayerAction() == ACTION_SPIKESWING || playerSetting.getPlayerAction() == ACTION_SPIKEREADY || playerSetting.getPlayerAction() == ACTION_QUICKREADY){
                    mainControl.Ball.GetComponent<BallMovement>().ballSpike(playerSetting.getTeam());
                    playerSetting.setPlayerAction(ACTION_SPIKEDONE);                    
                }
                else {
                    Debug.Log($"what? {playerSetting.getPlayerAction()}");

                }
                setMoveDelay(10.0f);
            }
        }
    }

}
