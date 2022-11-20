
using UnityEngine.UI;
using UnityEngine;
using static Constants;
using static PhysCalculate;

using playerStatsNameSpace;
using TMPro;


public class MainControl : MonoBehaviour
{
    [SerializeField] public GameObject[] Players = new GameObject[8];
    
    
    [SerializeField] public GameObject  Ball;

    //[SerializeField] public TextMeshProUGUI  ballText;
    //[SerializeField] public TextMeshProUGUI  playerText;
    //[SerializeField] public GameObject  lineY;
    //[SerializeField] public GameObject  BallXY;

    [SerializeField] public float  DELAY = 0.0f;

    private Transform playerTransform;
    [SerializeField]public float goalX;
    [SerializeField]public float goalZ;
    private GameObject lastTouchPlayer;
    public GameObject followingPlayer;
    private GameObject controlId;
    public GameObject nowServePlayer;
    private MainSetting MainSetting;
    private TimeTrigger TimeTrigger;

    public int LEFT_rotation  = 0;
    public int RIGHT_rotation = 0;
    [SerializeField] public int[] touchCount = new int[2] {0,0};

    Transform lineYTransform;
    Transform BallXYTransform;

    [SerializeField]public float mbY;
    public float quickTime;
    public int currentSituation;

    [SerializeField] public TextMeshProUGUI ballTeamTEXT;
    // Start is called before the first frame update
    void Start()
    {
        //lineYTransform = lineY.GetComponent<Transform>();
        //BallXYTransform = BallXY.GetComponent<Transform>();
        for (int i = 0 ; i < Constants.playerNumber; i ++) {
             if (i <= 3)
                 Players[i].GetComponent<PlayerSetting>().setTeam(Constants.TEAM_LEFT);
             else
                 Players[i].GetComponent<PlayerSetting>().setTeam(Constants.TEAM_RIGHT);
        }

        for (int j = 0 ; j < 2; j ++) 
        {
             Players[0+ j*4].GetComponent<PlayerSetting>().setPosition(Constants.LIBERO);
             Players[0 + j*4].GetComponent<PlayerSetting>().setRotation(0);
             //power , jump , speed , defense
             Players[0+ j*4].GetComponent<PlayerSetting>().playerCreate(3.0f,1.6f,2.9f,3.0f);        
             Players[0+ j*4].GetComponent<PlayerMove>().setStatus();    
             
             Players[1+ j*4].GetComponent<PlayerSetting>().setPosition(Constants.SETTER);
             Players[1 + j*4].GetComponent<PlayerSetting>().setRotation(1);
             Players[1+ j*4].GetComponent<PlayerSetting>().playerCreate(2.0f,1.5f,2.8f,1.0f);
             Players[1+ j*4].GetComponent<PlayerMove>().setStatus();
             
             Players[2+ j*4].GetComponent<PlayerSetting>().setPosition(Constants.BLOCKER);
             Players[2 +j*4].GetComponent<PlayerSetting>().setRotation(2);
             Players[2+ j*4].GetComponent<PlayerSetting>().playerCreate(4.0f,1.6f,2.3f,1.0f);        
             Players[2+ j*4].GetComponent<PlayerMove>().setStatus();        
             
             
             Players[3 + j*4].GetComponent<PlayerSetting>().setPosition(Constants.SPIKER);
             Players[3 + j*4].GetComponent<PlayerSetting>().setRotation(3);
             Players[3+ j*4].GetComponent<PlayerSetting>().playerCreate(1.0f,1.8f,2.7f,1.0f);
             Players[3+ j*4].GetComponent<PlayerMove>().setStatus();

             nowServePlayer = Players[1];
        }
        MainSetting = GetComponent<MainSetting>();
        TimeTrigger = GetComponent<TimeTrigger>();
        //int x = 20;
        //TimeTrigger.addTrigger(10f,()=> {Debug.Log($"{x}");});
        
        //TimeTrigger.addTrigger(5f,()=> {x += 5;});
    }




    void Update() {
        if (Input.GetKeyDown(KeyCode.D))
        {
            RIGHT_rotation += 1;
            resetTouchCount();
            nowServePlayer = getNowServer(TEAM_RIGHT);
            MainSetting.setCurrentSituation(SIT_SERVERGO);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            LEFT_rotation += 1;
            resetTouchCount();
            nowServePlayer = getNowServer(TEAM_LEFT);
            MainSetting.setCurrentSituation(SIT_SERVERGO);
        }
    }
    void FixedUpdate()
    {   
        float controlDirection = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))       { controlDirection = -INF; } //PlayerMoveStart(Vector3.left , controlId,true); } 
        else if (Input.GetKey(KeyCode.RightArrow)) { controlDirection = INF;  } //PlayerMoveStart(Vector3.right , controlId,true);  
        
        if (Input.GetKey(KeyCode.Space)) { controlId.GetComponent<PlayerMove>().DoJump(JUMP_SPIKE); } 

       // PlayerMoveToX(FloorX, Players[0]);
        //if (touchCount <= 2) {
        

        if (MainSetting.getCurrentSituation() == SIT_SERVERGO)
            serveControl(nowServePlayer);
            ballTeamTEXT.text = $"FLow TIME : {TimeTrigger.getMainTimeFlow() }";

            for (int i = 0 ; i < Constants.playerNumber; i ++) 
            {
                if (followingPlayer == Players[i]) {
                    float left_time = Ball.GetComponent<movePhysics>().getRemainTimeToParabolaX(goalX);
                    
                    bool isSlow = false;
                    if (followingPlayer.GetComponent<PlayerMove>().IsArrivedInTime(goalX, left_time, true,5.0f)) isSlow = true;
                    
                    
                    PlayerMoveTo(goalX,goalZ, followingPlayer , isSlow);
                    continue;
                }
                else {
                    
                    if (nowServePlayer == Players[i]) continue;
                    float xPos , zPos;
                    int team = Players[i].GetComponent<PlayerSetting>().getTeam();
                    int rot = team == TEAM_LEFT ? LEFT_rotation : RIGHT_rotation;
                    (xPos,zPos) = Players[i].GetComponent<PlayerAI>().getPlayerPlace(getTouchCount(team),rot);
                    
                    if ( Players[i].GetComponent<PlayerSetting>().isControl() ) 
                        xPos = Players[i].transform.position.x + controlDirection;
                    
                    bool isSlow = false;
                    if (Players[i].GetComponent<PlayerMove>().IsArrivedInTime(xPos,zPos, true , 0.5f)) isSlow = true;

                    PlayerMoveTo(xPos,zPos, Players[i],isSlow);
                }
            }           
    }

/// <summary>
/// 전달 받은 팀의 서브 순서의 Player Id 반환 
/// </summary>
/// <param name="team">어떤 팀의 서버를 구할 지 팀의 여부  (TEAM_LEFT , TEAM_RIGHT) </param>
/// <returns></returns>
    private GameObject getNowServer(int team){
            int last_team = team;
            int rot = last_team == TEAM_LEFT ? LEFT_rotation : RIGHT_rotation;
            
            for (int i = 0 ; i < Constants.playerNumber; i ++) 
            {
                if (last_team == Players[i].GetComponent<PlayerSetting>().getTeam()) {
                    int rotationPlace = Players[i].GetComponent<PlayerSetting>().getRotation();
                    int now_rot = (rotationPlace + rot) % 4;
                    if (now_rot == 0)
                        return Players[i];
                }
            }

            return Players[0];       
    }
    private void serveControl(GameObject controlPlayer) {
        PlayerSetting cPset = controlPlayer.GetComponent<PlayerSetting>();
        movePhysics ballPhys = Ball.GetComponent<movePhysics>();
        int team = cPset.getTeam();
        
        controlPlayer.GetComponent<PlayerSetting>().setPlayerAction(ACTION_SERVEWALK);

        if (!PlayerMoveTo(NET_X + SERVERPOS * team , Z_CENTER, controlPlayer)) {
            MainSetting.setCurrentSituation(SIT_SERVERWAIT);
            controlPlayer.GetComponent<PlayerSetting>().setPlayerAction(ACTION_SERVEREADY);         
            Vector3 CPT = controlPlayer.transform.position;

            ballPhys.stopParabola();
            ballPhys.movePosition(CPT.x - 0.6f*team,CPT.y,Z_CENTER); 
            
            TimeTrigger.addTrigger(9f , () => 
            {
                controlPlayer.GetComponent<PlayerSetting>().setPlayerAction(ACTION_SERVETOSS);                        
                MainSetting.setCurrentSituation(SIT_SERVERTOSS);

                ballPhys.setVector(90 - team*5f,1.6f);
                ballPhys.setZDirection(Constants.CENTER);
                ballPhys.startParabola();
            }).next(6f , () => {
                controlPlayer.GetComponent<PlayerSetting>().setPlayerAction(ACTION_SERVEDONE);                        
                MainSetting.setCurrentSituation(SIT_SERVERHIT);
                setLastTouch(nowServePlayer);
                nowServePlayer = null;
                
                Ball.GetComponent<BallMovement>().ballInCenter(90 + team * UnityEngine.Random.Range(40,60), 4f);
            });
            return;
        } 
    }

    private bool PlayerMoveTo( float x ,float z , GameObject controlPlayer, bool isSlow = false) {        
        //플레이어를 특정 좌표로 움직이게 하는 함수.
        playerTransform = controlPlayer.GetComponent<Transform>();
        Vector3 Direction = new Vector3(0f,0f,0f);
        bool    isthereMove1 = false;
        bool    isthereMove2 = false;

        isthereMove1 = PlayerMoveToX( x , controlPlayer , ref Direction);
        isthereMove2 = PlayerMoveToZ( z , controlPlayer ,ref Direction);


        if (!isthereMove1 && !isthereMove2) {
            controlPlayer.GetComponent<PlayerMove>().isMove = false;
            return false;
        }
        
        controlPlayer.GetComponent<PlayerMove>().isSlow = isSlow;
        PlayerMoveStart(Direction , controlPlayer);
        return true;
        
    }
    private bool PlayerMoveToX(float x , GameObject controlPlayer,ref Vector3 Direction)
    {
        playerTransform = controlPlayer.GetComponent<Transform>();
        bool isthereMove = false;

        if (float.IsNaN(x)) {
             return false; // x가 NaN값일 경우.
        }

        if (controlPlayer.GetComponent<PlayerSetting>().getTeam() == TEAM_LEFT) x = Mathf.Min(LEFT_LIMIT ,x);
        else x = Mathf.Max(RIGHT_LIMIT,x);

        
        if ( Mathf.Abs(x - playerTransform.position.x) <= controlPlayer.GetComponent<PlayerSetting>().Status.getSpeed()* Constants.playSpeed *1.05f ) 
        {   
            playerTransform.position = new Vector3(x, playerTransform.position.y, playerTransform.position.z);
        }
        else{
            if (x > playerTransform.position.x) Direction += new Vector3(1.0f,0.0f,0.0f);
            else Direction += new Vector3(-1.0f,0.0f,0.0f);    
            isthereMove = true;
        }
        return isthereMove;
    }
    private bool PlayerMoveToZ(float z , GameObject controlPlayer,ref Vector3 Direction){
        
        bool isthereMove = false;
        playerTransform = controlPlayer.GetComponent<Transform>();

        if (float.IsNaN(z)) {
             return false; // x가 NaN값일 경우.
        }
        if ( Mathf.Abs(z - playerTransform.position.z) <= controlPlayer.GetComponent<PlayerSetting>().Status.getSpeed()* Constants.playSpeed *1.05f ){
            playerTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, z);
        }
        else {
            if (z > playerTransform.position.z) Direction += new Vector3(0.0f,0.0f,1.0f);
            else Direction += new Vector3(0.0f,0.0f,-1.0f);    
            isthereMove = true;
        }
        return isthereMove;
    }

    public void commandPlayerMove() 
    {
        
        if (!MainSetting.isGameRallying()) return;
            //Players[i].GetComponent<PlayerAI>()
        
        followingPlayer = getNearestPlayer(); // 세터 

        if (followingPlayer == null) return; // 받을 수 있는 놈이 없다. (ex : 네트 걸림 등. )


        movePhysics ballPhys = Ball.GetComponent<movePhysics>();
        movePhysics playerPhys = followingPlayer.GetComponent<movePhysics>();        
        PlayerSetting playerSet = followingPlayer.GetComponent<PlayerSetting>();
        PlayerMove playerMove = followingPlayer.GetComponent<PlayerMove>();
        PlayerAI playerAI = followingPlayer.GetComponent<PlayerAI>();

        float BallType = MainSetting.getCurrentBallType(TEAM_LEFT);
        int team = playerSet.getTeam();
        bool isLowBall = ( BallType == BALL_RECEIVE_GOOD_LOW || BallType == BALL_RECEIVE_BAD_LOW || BallType == BALL_RECEIVE_SHORT_LOW );
        

        float x , y ,z;
        if (getTouchCount(team) == 0) {
            x = playerMove.getFallingPlaceXbyPlayer(JUMP_NO,false,0.0f); // 점프 안하고 리시브.                 
            y = playerPhys.getLandBody_Y();        
            playerSet.setPlayerAction(ACTION_RECEIVE);
        }
        else if (getTouchCount(team) == 1) {
            float jump_type = 0.0f,delay = 0.0f;

            playerAI.getJumpDataOfSecondBall(ref jump_type,ref delay,isLowBall);
            
            if (jump_type == JUMP_TOSS) playerSet.setPlayerAction(ACTION_JUMPTOSS); // 점프토스인지 아닌지 구별!
            else playerSet.setPlayerAction(ACTION_TOSS);

            x =  playerMove.getFallingPlaceXbyPlayer(jump_type,true,delay);                 
            y = playerPhys.getLandHead_Y();//+ ballPhys.getHeight() / 2 ;    

            var pTime = getFlightTimeBySpeed(playerSet.Status.getJump() * jump_type) + getTimeByHeight(delay);
            playerMove.setJumpTime(ballPhys.getRemainTimeToParabolaX(x) - pTime , jump_type);                        
            
            y = y + getMaxHeightBySpeed(playerSet.Status.getJump()) - delay;


            // 속공 점프
            var MB_ID = BLOCKER + (getLastTouchTeam() == TEAM_LEFT ? 0 : 4);
            movePhysics MBPhys = Players[MB_ID].GetComponent<movePhysics>();   
            PlayerSetting MBSet = Players[MB_ID].GetComponent<PlayerSetting>();
            PlayerMove MBMove = Players[MB_ID].GetComponent<PlayerMove>();
            var mbTime = getFlightTimeBySpeed(MBSet.Status.getJump()) + getTimeByHeight(delay);
            quickTime = 2.3f;
            mbY = MBPhys.getLandHead_Y() + getMaxHeightBySpeed(MBSet.Status.getJump());
            if (!MBSet.isControl())
                MBMove.setJumpTime(ballPhys.getRemainTimeToParabolaX(x) + getTimeByHeight(delay) + quickTime - mbTime , JUMP_SPIKE);            

            float swingSpeed = 0.1f;

            MBSet.setPlayerAction(ACTION_QUICKREADY);
            MBMove.setSpikeTime(ballPhys.getRemainTimeToParabolaX(x) + getTimeByHeight(delay) + quickTime - swingSpeed  );
            var ty = MBPhys.getLandHead_Y() + getMaxHeightBySpeed(MBSet.Status.getJump());
            var tz = getLastTouchTeam() == TEAM_LEFT ? Z_RIGHT : Z_LEFT;
            var tx = NET_X + team * NEARFRONT;
            MBSet.setTarget( tx , ty , tz);               
            
        }
        else {
            var delay = 0.05f;
            playerSet.setPlayerAction(ACTION_SPIKEREADY);
            x = playerMove.getFallingPlaceXbyPlayer(JUMP_SPIKE,true,delay); 
            y = playerPhys.getLandHead_Y();//+ ballPhys.getHeight() / 2 ;        
            var pTime = getFlightTimeBySpeed(playerSet.Status.getJump()) + getTimeByHeight(delay);
            
            playerMove.setJumpTime(ballPhys.getRemainTimeToParabolaX(x) - pTime , JUMP_SPIKE);

            float swingSpeed = 0.1f;

            playerMove.setSpikeTime(ballPhys.getRemainTimeToParabolaX(x) - swingSpeed);     

            y = y + getMaxHeightBySpeed(playerSet.Status.getJump()) - delay;
        }
        z = ballPhys.getFallingPlaceZbyX(x);
        goalX = x;
        goalZ = z;
        
        /*
        
        if (getTouchCount(team) == 1) {
            //세터 점프
            var pTime = getFlightTimeBySpeed(playerSet.Status.getJump()) + getTimeByHeight(DELAY);
            controlPlayer.GetComponent<PlayerMove>().setJumpTime(ballPhys.getRemainTimeToParabolaX(x) - pTime);            
            y = playerPhys.getLandY() + getMaxHeightBySpeed(playerSet.Status.getJump()) - DELAY + playerPhys.getHeight() / 2 + ballPhys.getHeight() / 2;
            //미들블로커 점프
            var MB_ID = BLOCKER + (getLastTouchTeam() == TEAM_LEFT ? 0 : 4);
                movePhysics MBPhys = Players[MB_ID].GetComponent<movePhysics>();   
                PlayerSetting MBSet = Players[MB_ID].GetComponent<PlayerSetting>();
                var mbTime = getFlightTimeBySpeed(MBSet.Status.getJump()) + getTimeByHeight(DELAY);
                quickTime = 1.6f;
                mbY = MBPhys.getLandY() + getMaxHeightBySpeed(MBSet.Status.getJump()) + MBPhys.getHeight() *0.5f;
                if (!MBSet.isControl())
                    Players[MB_ID].GetComponent<PlayerMove>().setJumpTime(ballPhys.getRemainTimeToParabolaX(x) + getTimeByHeight(DELAY) + 1.6f - mbTime);            
                var ty = MBPhys.getLandY() + getMaxHeightBySpeed(MBSet.Status.getJump()) + MBPhys.getHeight() / 2 + ballPhys.getHeight() / 2;
                var tz = getLastTouchTeam() == TEAM_LEFT ? Z_RIGHT : Z_LEFT;
                var tx = NET_X + team * NEARFRONT;
                MBSet.setTarget( tx , ty , tz);   
        }
        else if (Mathf.Abs(getTouchCount(team)) == 2)
        {
            //공격수 점프
            var pTime = getFlightTimeBySpeed(playerSet.Status.getJump()) + getTimeByHeight(DELAY);
            if (!playerSet.isControl()) controlPlayer.GetComponent<PlayerMove>().setJumpTime(ballPhys.getRemainTimeToParabolaX(x) - pTime);            
            y = playerPhys.getLandY() + getMaxHeightBySpeed(playerSet.Status.getJump()) - DELAY + playerPhys.getHeight() / 2 + ballPhys.getHeight() / 2;
        }*/
    }



    private void PlayerMoveStart(Vector3 direction,GameObject controlPlayer,bool isAdd = false) {
        controlPlayer.GetComponent<PlayerMove>().isMove = true;
        if (isAdd)
            controlPlayer.GetComponent<PlayerMove>().movingDir += direction;
        else
            controlPlayer.GetComponent<PlayerMove>().movingDir = direction;
    }
    public float getPlayerTime(GameObject Player,float x, float z) {
        // 플레이어가 x값에 도달하는데 걸리는 시간 , z 값에 도달하는 데 걸리는 시간 그 중 더 큰 값 반환.
        PlayerMove cplayerMove = Player.GetComponent<PlayerMove>();
        return Mathf.Max(cplayerMove.getTakingTimeToLinearX(x) , cplayerMove.getTakingTimeToLinearZ(z)) + cplayerMove.getMoveDelay();
    }
    public void setCurrentSituation(int st){
        currentSituation = st;
    }
    public int getCurrentSituation(){
        return currentSituation;
    }
    public float getBallTime(float x , float z){
        movePhysics ballPhys = Ball.GetComponent<movePhysics>();
        return Mathf.Max(ballPhys.getRemainTimeToParabolaX(x) , ballPhys.getRemainTimeToParabolaZ(z));  
    }
    private float calculateScore(GameObject Player , float x) {
        //어떤 선수가 followingPlayer가 될 지 우선순위를 구하기 위해 플레이어별 거리,포지션으로 스코어 계산
        PlayerSetting ps = Player.GetComponent<PlayerSetting>();
        int team = ps.getTeam();
        int position = ps.getPosition();

        
        float z = Ball.GetComponent<movePhysics>().getFallingPlaceZbyX(x);
        float playerTime = getPlayerTime(Player , x , z);
        float ballTime = getBallTime( x , z);
        float score = 0.0f;
        int AbsTouchCount = Mathf.Abs(getTouchCount(team));

        float jump_type = JUMP_NO;

        if (AbsTouchCount == 0) { // 리시브 
            score = Mathf.Max(0.0f,ballTime - playerTime);
            if (position == LIBERO) { score = score * 5.0f; } //리베로가 받을 확률 더 높게.
            else if (position == SETTER) { score = score / 2; } //세터의 경우 스코어 절반
        }
        else if (AbsTouchCount == 1) {
            jump_type = JUMP_TOSS;
            score = Mathf.Max(0.0f,ballTime - playerTime);
            if (position == SETTER) {    score = score * 10.0f; } 
            if (position == SPIKER) {    score = score * 3.0f;  } 
            if (position == LIBERO) {    score = score * 2.0f;  }
        }
        else if (AbsTouchCount == 2 ) {
            jump_type = JUMP_SPIKE;
            score = Mathf.Max(0.0f,ballTime - playerTime);
            if (position == LIBERO) { score = score * 0.0f; }
            
            if ( getCurrentSituation() == STRATEGY_OPEN ) {
                if (position == SPIKER) { score = score * 4.5f; }
            }
            if ( getCurrentSituation() == STRATEGY_QUICK ) {
                if (position == BLOCKER) { score = score * 4.5f; }
            }
        }
        else {
            score = -INF;
        }
        movePhysics playerPhys = Player.GetComponent<movePhysics>();
        PlayerSetting playerSet = Player.GetComponent<PlayerSetting>();

        bool checkB = checkMyBall( playerPhys.getLandBody_Y() + getMaxHeightBySpeed(playerSet.Status.getJump() * jump_type) + playerPhys.getHeight()*0.5f, team);
        // 어느 팀의 볼인지 체크 ( ex : 넘어가는 볼인지 , 넘어간다 하더라도 본인이 점프하면 닿을 수 있는 볼인지 판단)
        playerSet.checkB = checkB;
        if (!checkB) score = -INF; //볼이 우리꺼가 아닐 경우 안받는다
        if (playerSet.isControl()) score = -INF;
        if (lastTouchPlayer == Player) score = -INF; // 투터치가 안되도록.

        return score;

    }
    public void setLastTouch(GameObject player){
        lastTouchPlayer = player;
    }
    public GameObject getLastTouchPlayer(){
        return lastTouchPlayer;
    }
    public GameObject getFollowingPlayer(){
        return followingPlayer;
    }

    /// <summary>
    /// 공의 낙하지점에 가장 적합한 Player을 상황에 맞추어 계산한다.
    /// </summary>
    /// <returns></returns>
    public GameObject getNearestPlayer() {
        //get player who has the Highest score.
        float maxVal = -10.0f;
        GameObject maxPlayer = null;

        for (int i = 0; i < Constants.playerNumber; i ++)
        {
            int player_team = Players[i].GetComponent<PlayerSetting>().getTeam();
            PlayerMove playerMove =  Players[i].GetComponent<PlayerMove>();
            float score;

            if (getTouchCount(player_team) == 2) { // 공격하는 상황
                score = calculateScore(Players[i] , playerMove.getFallingPlaceXbyPlayer(JUMP_SPIKE, true,0.0f)); // 스파이크 타점 기준 도달하는데 걸리는 시간.
                if (score <= 0.0f) score = calculateScore(Players[i] , playerMove.getFallingPlaceXbyPlayer(JUMP_NO,true,0.0f));
            }
            else if (getTouchCount(player_team) == 1) { // 토스하는 상황의 경우.
                score = calculateScore(Players[i] , playerMove.getFallingPlaceXbyPlayer(JUMP_TOSS , true,0.0f)); // 점프토스 타점 기준 도달하는데 걸리는 score 계산 후 이것이 0일 경우. 재계산
                if (score <= 0.0f) score = calculateScore(Players[i] , playerMove.getFallingPlaceXbyPlayer(JUMP_NO,true,0.0f));
            }
            else 
                score = calculateScore(Players[i] , playerMove.getFallingPlaceXbyPlayer(JUMP_NO,false,0.0f));
            
            Players[i].GetComponent<PlayerMove>().score = score; 

            if (score > maxVal) { // score가 0보다는 커야 함.
                maxVal = score;
                maxPlayer = Players[i];
            }
        }
        
        return maxPlayer;
    }
    public bool checkMyBall(float playerJumpHeight , int playerTeam){
        movePhysics ballPhys = Ball.GetComponent<movePhysics>();
        
        // 볼이 네트를 지나갈 경우
        if (Mathf.Sign(ballPhys.startPos.x - NET_X) != Mathf.Sign(ballPhys.endPos.x - NET_X))
        {
            float ballHeightOnNet , ballHeightOnPlayetLimitX;
            bool IsLeftToRight = (ballPhys.startPos.x < NET_X);
            int myteam = IsLeftToRight ? TEAM_LEFT : TEAM_RIGHT;
            float limit_X = IsLeftToRight ? LEFT_LIMIT : RIGHT_LIMIT;
                ballHeightOnPlayetLimitX = ballPhys.getParabolaYbyX(limit_X)-ballPhys.getHeight()/2;
                ballHeightOnNet = ballPhys.getParabolaYbyX(NET_X)-ballPhys.getHeight()/2;
                Ball.GetComponent<BallMovement>().b_H = ballPhys.getParabolaYbyX(limit_X)-ballPhys.getHeight()/2;
                if (playerTeam == myteam) // 우리 팀 입장에서
                {
                    if (getCurrentSituation() == SIT_SERVERHIT) // 서브 볼은 기본적으로 무조건 우리꺼 아님.
                        return false;

                    if ( ballHeightOnPlayetLimitX > Mathf.Max( NET_Y , playerJumpHeight) ) // 플레이어 점프 높이 넘어가버리면 우리꺼 아님..
                        return false;         
                    else 
                        return true;
                }
                else // 반대편 입장에서
                {
                    if (ballHeightOnNet < NET_Y) // 반대 편 입장에서, 
                        return false;
                    else 
                        return true;
                }                
        }
        else
        {
            bool IsLeft = (ballPhys.endPos.x < NET_X);
            
            if (IsLeft) {
                if (playerTeam== TEAM_LEFT) return true;
                else return false;
            } 
            else {
                if (playerTeam== TEAM_RIGHT) return true;
                else return false;
            }
            
        }
        
    }
    public int getTouchCount(int team) {
        if (team == -1) //TEAM_LEFT
            return touchCount[0];
        else // 1 : TEAM_RIGHT
            return touchCount[1];
    }
    public void setTouchCount(int team , int value) {
        int team_id = team == TEAM_LEFT ? 0 : 1;
        touchCount[team_id] = value;
    }
    public void resetTouchCount() {
        touchCount[0] = 0;
        touchCount[1] = 0;
    }
    public void addTouchCount(int team) {

        if (team == TEAM_LEFT){
            touchCount[0] += 1;
            if (getTouchCount(TEAM_RIGHT) > 0) touchCount[1] = 0; // 반대편 꺼주기.
        }
        else{
            touchCount[1] += 1;
            if (getTouchCount(TEAM_LEFT) > 0) touchCount[0] = 0;
        }
        
    }
    public int getLastTouchTeam() {
        if (getTouchCount(TEAM_LEFT) > 0) return TEAM_LEFT; // 반대편 꺼주기.
        if (getTouchCount(TEAM_RIGHT) > 0) return TEAM_RIGHT;

        return 0;
    }
}

