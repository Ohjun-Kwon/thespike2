
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

     [SerializeField]public Vector3 goal;
    [SerializeField]public GameObject goalBall;


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
             Players[0+ j*4].GetComponent<PlayerSetting>().playerCreate(3.0f,1.6f,1.1f,3.0f);        
             Players[0+ j*4].GetComponent<PlayerMove>().setStatus();    
             
             Players[1+ j*4].GetComponent<PlayerSetting>().setPosition(Constants.SETTER);
             Players[1 + j*4].GetComponent<PlayerSetting>().setRotation(1);
             Players[1+ j*4].GetComponent<PlayerSetting>().playerCreate(2.0f,1.5f,1.2f,1.0f);
             Players[1+ j*4].GetComponent<PlayerMove>().setStatus();
             
             Players[2+ j*4].GetComponent<PlayerSetting>().setPosition(Constants.BLOCKER);
             Players[2 +j*4].GetComponent<PlayerSetting>().setRotation(2);
             Players[2+ j*4].GetComponent<PlayerSetting>().playerCreate(4.0f,1.6f,5.4f,1.0f);        
             Players[2+ j*4].GetComponent<PlayerMove>().setStatus();        
             
             
             Players[3 + j*4].GetComponent<PlayerSetting>().setPosition(Constants.SPIKER);
             Players[3 + j*4].GetComponent<PlayerSetting>().setRotation(3);
             Players[3+ j*4].GetComponent<PlayerSetting>().playerCreate(1.0f,1.8f,5.5f,1.0f);
             Players[3+ j*4].GetComponent<PlayerMove>().setStatus();

             nowServePlayer = Players[1];
        }
        MainSetting = GetComponent<MainSetting>();
        TimeTrigger = GetComponent<TimeTrigger>();
        
        //TimeTrigger.addTrigger(5f,()=> {x += 5;});
    }




    void Update() {
        if (Input.GetKeyDown(KeyCode.D))  // 오른쪽 선수가 서브
        {
            RIGHT_rotation += 1;
            resetTouchCount();
            nowServePlayer = getNowServer(TEAM_RIGHT);
            MainSetting.setCurrentSituation(SIT_SERVERGO);
        }
        else if (Input.GetKeyDown(KeyCode.S)) // 왼쪽 선수가 서브
        {
            LEFT_rotation += 1;
            resetTouchCount();
            nowServePlayer = getNowServer(TEAM_LEFT);
            MainSetting.setCurrentSituation(SIT_SERVERGO);
        }
        if (Input.GetKeyDown(KeyCode.Q))      { playSpeed -= 0.005f; } //시간 느리게 
        else if (Input.GetKeyDown(KeyCode.W)) { playSpeed += 0.005f; } //시간 빠르게
        ballTeamTEXT.text = $"play Speed : {playSpeed}";     
    }
    void FixedUpdate()
    {   
        float controlDirection = 0f;

        //Control선수 조종 용도.
        if (Input.GetKey(KeyCode.LeftArrow))       { controlDirection = -INF; } //PlayerMoveStart(Vector3.left , controlId,true); } 
        else if (Input.GetKey(KeyCode.RightArrow)) { controlDirection = INF;  } //PlayerMoveStart(Vector3.right , controlId,true);  
        
        if (Input.GetKey(KeyCode.Space)) { getPlayerByPosition(LIBERO,TEAM_LEFT).GetComponent<PlayerMove>().DoSlide(); } 
        //controlId.GetComponent<PlayerMove>().DoJump(JUMP_SPIKE);
        
       // PlayerMoveToX(FloorX, Players[0]);
        //if (touchCount <= 2) {


        
            
      
        for (int i = 0 ; i < playerNumber; i ++) //플레이어들 전부 순회 플레이어 모든 움직임
        {
            movePhysics ballPhys = Ball.GetComponent<movePhysics>(); 
            float goalX = goal.x;
            float goalZ = goal.z;
            if (nowServePlayer == Players[i]){
                if (MainSetting.getCurrentSituation() == SIT_SERVERGO) serveControl(nowServePlayer);
                continue; // 현재 서브하는 선수는 자체적으로 움직임.
            }

            if (followingPlayer == Players[i]) {  // 현재 볼을 따라가는 선수. 
                //시간 계산해보니까 여유로워
                //어? 충분히 시간 안에 가는데?
                float left_time = Mathf.Max(ballPhys.getRemainTimeToParabolaX(goalX),ballPhys.getRemainTimeToParabolaZ(goalZ));
                bool isSlow = false;
                if (followingPlayer.GetComponent<PlayerMove>().IsArrivedInTime(goalX,goalZ, left_time, true, 5.0f )) isSlow = true;
                //isSlow를 켜주면 애가 걸어.
                
                PlayerMoveTo(goalX,goalZ, followingPlayer , isSlow); //이동시키는거고. 
                continue;
            }
            else { // 그 외의 선수.
                
                float xPos , zPos;
                int team = Players[i].GetComponent<PlayerSetting>().getTeam();
                int rot = team == TEAM_LEFT ? LEFT_rotation : RIGHT_rotation;
                (xPos,zPos) = Players[i].GetComponent<PlayerAI>().getPlayerPlace(getTouchCount(team),rot); //얘는 지금 뭐해야 할 지 알아와.

                
                if ( Players[i].GetComponent<PlayerSetting>().isControl() )  //조종하는 선수일 경우.
                    xPos = Players[i].transform.position.x + controlDirection; //조종방향으로 이동.
                
                bool isSlow = false;
                if (Players[i].GetComponent<PlayerMove>().IsArrivedInTime(xPos,zPos, 4.0f  ,true , 0.5f )) isSlow = true;

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
                
                Ball.GetComponent<BallMovement>().ballServe(90 + team * UnityEngine.Random.Range(40,60), 4f);
            });
            return;
        } 
    }

    private bool PlayerMoveTo( float x ,float z , GameObject controlPlayer, bool isSlow = false) {        
        //플레이어를 특정 좌표로 움직이게 하는 함수.
        Transform playerTransform = controlPlayer.GetComponent<Transform>();

        Vector3 Direction = new Vector3(0f,0f,0f);
        bool    isthereMove1 = false;
        bool    isthereMove2 = false;

        isthereMove1 = PlayerMoveToX( x , controlPlayer , ref Direction,isSlow);
        isthereMove2 = PlayerMoveToZ( z , controlPlayer ,ref Direction,isSlow);

        controlPlayer.GetComponent<PlayerMove>().goal = new Vector3(x,0.0f,z);
        
        if (!isthereMove1 && !isthereMove2) {
            controlPlayer.GetComponent<PlayerMove>().isMove = false;
            return false;
        }
        
        controlPlayer.GetComponent<PlayerMove>().isSlow = isSlow;
        PlayerMoveStart(Direction , controlPlayer);
        return true;
        
    }
    private bool PlayerMoveToX(float x , GameObject controlPlayer,ref Vector3 Direction, bool isSlow)
    {
        Transform playerTransform = controlPlayer.GetComponent<Transform>();
        movePhysics playerPhys = controlPlayer.GetComponent<movePhysics>();
        bool isthereMove = false;
        float slowSpeed = isSlow ? SLOW_SPEED : 1;
        if (float.IsNaN(x)) {
             return false; // x가 NaN값일 경우.
        }

        if (controlPlayer.GetComponent<PlayerSetting>().getTeam() == TEAM_LEFT) x = Mathf.Min(LEFT_LIMIT ,x);
        else x = Mathf.Max(RIGHT_LIMIT,x);

        
        if ( Mathf.Abs(x - playerTransform.position.x) <= controlPlayer.GetComponent<PlayerSetting>().Status.getSpeed()*slowSpeed *Constants.playSpeed *1.05f ) 
        {   
            playerPhys.changePositionX(x);
        }
        else{
            if (x > playerTransform.position.x) Direction += new Vector3(1.0f,0.0f,0.0f);
            else Direction += new Vector3(-1.0f,0.0f,0.0f);    
            isthereMove = true;
        }
        return isthereMove;
    }
    private bool PlayerMoveToZ(float z , GameObject controlPlayer,ref Vector3 Direction , bool isSlow){
        
        bool isthereMove = false;
        Transform playerTransform = controlPlayer.GetComponent<Transform>();
        movePhysics playerPhys = controlPlayer.GetComponent<movePhysics>();
        float slowSpeed = isSlow ? SLOW_SPEED : 1;
        if (float.IsNaN(z)) {
             return false; // x가 NaN값일 경우.
        }
        if ( Mathf.Abs(z - playerTransform.position.z) <= controlPlayer.GetComponent<PlayerSetting>().Status.getSpeed()* Constants.playSpeed*slowSpeed*1.05f ){
            playerPhys.changePositionZ(z);
        }
        else {
            if (z > playerTransform.position.z) Direction += new Vector3(0.0f,0.0f,1.0f);
            else Direction += new Vector3(0.0f,0.0f,-1.0f);    
            isthereMove = true;
        }
        return isthereMove;
    }



    /// <summary>
    /// 중요한 함수
    /// followingPlayer를 정해주고, followingPlayer가 어디로 갈 지도 정해준다.
    /// 볼의 궤도가 변경될 때마다 실행된다. (ex . 리시브 , 토스 ,스파이크 , 네트 부딪힘.)
    /// </summary>
    public void commandPlayerMove() 
    {
        
        if (!MainSetting.isGameRallying()) return;
            //Players[i].GetComponent<PlayerAI>()
        
        followingPlayer = getNearestPlayer(); // 저 getNearestPlayer는 , 볼을 누가받아야할 지 score 계산해서 정해주는거야.

        if (followingPlayer == null) return; // 받을 수 있는 놈이 없다. (ex : 네트 걸림 등. )


        movePhysics ballPhys = Ball.GetComponent<movePhysics>();
        movePhysics playerPhys = followingPlayer.GetComponent<movePhysics>();        
        PlayerSetting playerSet = followingPlayer.GetComponent<PlayerSetting>();
        PlayerMove playerMove = followingPlayer.GetComponent<PlayerMove>();
        PlayerAI playerAI = followingPlayer.GetComponent<PlayerAI>();
        playerStats playerStat = playerSet.Status;
        
        float BallType = MainSetting.getCurrentBallType(TEAM_LEFT);
        int team = playerSet.getTeam();
        bool isLowBall = ( BallType == BALL_RECEIVE_GOOD_LOW || BallType == BALL_RECEIVE_BAD_LOW || BallType == BALL_RECEIVE_SHORT_LOW );

        if (getTouchCount(team) == 0) { //리시브 상황
            
            goal = playerSet.getGoal();           
            goalBall.transform.position = playerSet.getGoal();                     
            float costTime = ballPhys.getRemainTimeToParabolaX(goal.x) - playerStat.getReceiveTime();

            if (playerMove.IsArrivedInTime( goal.x , goal.z , costTime)) { // 리시브는 점프를 안해요.
                playerSet.setPlayerAction(ACTION_RECEIVEREADY);
                playerMove.setActionTime(ballPhys.getRemainTimeToParabolaX(goal.x)-playerStat.getReceiveTime());       
            } 
        }
        else if (getTouchCount(team) == 1) {    // 토스 상황
            float jump_type = 0.0f;
            float delay = playerSet.getGoalDelay();

            jump_type =  playerSet.getGoalJump();
            goal = playerSet.getGoal();
            goalBall.transform.position = playerSet.getGoal();


            float pTime = getFlightTimeBySpeed(playerSet.Status.getJump() * jump_type) + getTimeByHeight(delay);

            playerMove.setJumpTime(ballPhys.getRemainTimeToParabolaX(goal.x) - pTime , jump_type);  
            float costTime = ballPhys.getRemainTimeToParabolaX(goal.x);

            //Debug.Log($" x : {x} , z : {z} getJump : {playerSet.getGoalJump()} , costTime {costTime}");

            if (playerMove.IsArrivedInTime( goal.x , goal.z , costTime,false,0.0f,true) ) { //세터는 점프토스가 안되면 알아서 안한다.
                if (jump_type == JUMP_TOSS) playerSet.setPlayerAction(ACTION_JUMPTOSSREADY); // 점프토스인지 아닌지 구별!
                else playerSet.setPlayerAction(ACTION_TOSSREADY);             
                playerMove.setActionTime(ballPhys.getRemainTimeToParabolaX(goal.x)- playerStat.getTossTime());     
            }

            // 속공 점프
            GameObject MB = getPlayerByPosition( BLOCKER , getLastTouchTeam() );
            
            movePhysics MBPhys = MB.GetComponent<movePhysics>();   
            PlayerSetting MBSet = MB.GetComponent<PlayerSetting>();
            PlayerMove MBMove = MB.GetComponent<PlayerMove>();
            playerStats MBStat = MBSet.Status;

            float MBdelay = UnityEngine.Random.Range(0.0f,1.2f); // MB가 얼마나 끌어 때릴 지.
            
            var ty = MBPhys.getLandHead_Y()+ getMaxHeightBySpeed(MBSet.Status.getJump() * JUMP_SPIKE) - MBdelay;
            var tz = getLastTouchTeam() == TEAM_LEFT ? Z_RIGHT : Z_LEFT;
            var tx = NET_X + team * NEARFRONT;
            float mbTime = getFlightTimeBySpeed(MBSet.Status.getJump()) + getTimeByHeight(MBdelay);
            costTime = ballPhys.getRemainTimeToParabolaX(goal.x) + quickTime - mbTime;
            quickTime = 1.8f;

            if ( MBMove.IsAvailableToQuick(tx,ty,tz, costTime,true,MBdelay)) {
                mbY = MBPhys.getLandHead_Y() + getMaxHeightBySpeed(MBSet.Status.getJump()) - MBdelay;
                if (!MBSet.isControl())
                    MBMove.setJumpTime(ballPhys.getRemainTimeToParabolaX(goal.x) + quickTime - mbTime , JUMP_SPIKE);            
                
                MBSet.setPlayerAction(ACTION_QUICKREADY);
                MBMove.setActionTime(ballPhys.getRemainTimeToParabolaX(goal.x) + quickTime - MBStat.getSwingTime() );       
                MBSet.setTarget( tx , ty , tz);               
            }
        }
        else {
            if (playerSet.getPlayerAction() == ACTION_QUICKREADY) {
                Vector3 _target = playerSet.getTarget();
                goalBall.transform.position = new Vector3 (_target.x,_target.y,_target.z); 
                goal = new Vector3(_target.x,_target.y,_target.z);                     
            }
            else {   
                float delay = playerSet.getGoalDelay();
                goal = playerSet.getGoal();
                goalBall.transform.position = getGoal();
                float jump_type = playerSet.getGoalJump();

                float pTime = getFlightTimeBySpeed(playerSet.Status.getJump()) + getTimeByHeight(delay);
                float left_time = ballPhys.getRemainTimeToParabolaX(goal.x) - pTime;

                if (jump_type == JUMP_SPIKE) { // 도달 가능하면 공격을 하세요.
                    float swingSpeed = playerSet.Status.getSwingTime();
                    playerSet.setPlayerAction(ACTION_SPIKEREADY);
                    playerMove.setJumpTime(ballPhys.getRemainTimeToParabolaX(goal.x) - pTime , JUMP_SPIKE);
                    playerMove.setActionTime(ballPhys.getRemainTimeToParabolaX(goal.x) - swingSpeed);     
                }
                else {
                    float costTime = ballPhys.getRemainTimeToParabolaX(goal.x) - playerStat.getReceiveTime();

                    if (playerMove.IsArrivedInTime( goal.x , goal.z , costTime)) {
                        playerSet.setPlayerAction(ACTION_RECEIVEREADY);
                        playerMove.setActionTime(ballPhys.getRemainTimeToParabolaX(goal.x)-playerStat.getReceiveTime());       
                    }
                }         
            }
        }
    
        
    }

    /// <summary>
    /// 볼에게 일어날 다음 이벤트의 좌표를 나타낸다. (스파이크 위치 , 리시브 위치) 등등...
    /// </summary>
    /// <returns></returns>
    public Vector3 getGoal() {
        return goal;
    }
    private GameObject getPlayerByPosition(int position , int team) {
        return Players[position + (team == TEAM_LEFT ? 0 : 4)];
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
        return cplayerMove.getArrivedTime(x,z);// Mathf.Max(cplayerMove.getTakingTimeToLinearX(x) , cplayerMove.getTakingTimeToLinearZ(z)) + cplayerMove.getMoveDelay();
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


/// <summary>
/// playerTime과 ballTime을 계산하고 score을 계산한다.
/// score이 givenScore보다 높을 경우, 해당 플레이어의 goalX , goalZ를 설정해준다.
/// </summary>
/// <param name="Player">플레이어</param>
/// <param name="TouchCount">터치횟수</param>
/// <param name="playerTime">player 소요 참조변수</param>
/// <param name="ballTime">ball 소요 참조변수 </param>
/// <param name="isFall">true일 경우, 포물선이 내려가는 기준에서 만나는 점의 x를 기준으로 계산 , false일 경우 올라가는 점 기준</param>
/// <param name="given_score">주어진 점수, 이보다 클 경우 goal을 설정한다.</param>
/// <returns>계산한 score을 반환한다.</returns>
    public float setTimeAndGoal(GameObject Player , int TouchCount,ref float playerTime ,ref float ballTime ,bool isFall , float given_score) {
        PlayerSetting ps = Player.GetComponent<PlayerSetting>();
        PlayerMove playerMove = Player.GetComponent<PlayerMove>();
        movePhysics ballPhys = Ball.GetComponent<movePhysics>();
        movePhysics playerPhys = Player.GetComponent<movePhysics>();
        PlayerSetting playerSet = Player.GetComponent<PlayerSetting>();        
        int team = playerSet.getTeam();

        float x,y,z, jump_type = JUMP_NO;
        float delay = 0.0f;
        switch(TouchCount) {
            default:
                    x = playerMove.getFallingPlaceXbyPlayer(JUMP_NO,isFall,0.0f);
                    y = playerPhys.getLandBody_Y();      
                    z = ballPhys.getParabolaZbyX(x);
                    ballTime = getBallTime(x,z);
                    break;
                    
            case 1: 
                    delay = UnityEngine.Random.Range(0.0f,1.2f);
                    x = playerMove.getFallingPlaceXbyPlayer(JUMP_TOSS,isFall,delay); // 점프 토스로 계산
                    z = ballPhys.getParabolaZbyX(x);
                    ballTime = getBallTime(x,z);
                    jump_type = JUMP_TOSS;
                    if (!playerMove.IsArrivedInTime(x,z,ballTime) ) {
                        jump_type = JUMP_NO;
                        x = playerMove.getFallingPlaceXbyPlayer(JUMP_NO,isFall,0.0f); // 점프 토스 안되면..
                        z = ballPhys.getParabolaZbyX(x);
                        ballTime = getBallTime(x,z);
                    }
                    y = playerPhys.getLandHead_Y()+ getMaxHeightBySpeed(playerSet.Status.getJump() * jump_type) - delay; 
                    break;
            case 2: 
                    delay = UnityEngine.Random.Range(0.0f,1.2f);
                    jump_type = JUMP_SPIKE;
                    x = playerMove.getFallingPlaceXbyPlayer(JUMP_SPIKE,isFall,delay); // 공격
                    z = ballPhys.getParabolaZbyX(x);
                    y = playerPhys.getLandHead_Y()+ getMaxHeightBySpeed(playerSet.Status.getJump() * jump_type) - delay;
                    ballTime = getBallTime(x,z);

                    float pTime = getFlightTimeBySpeed(playerSet.Status.getJump()) + getTimeByHeight(delay);
                    float left_time = ballPhys.getRemainTimeToParabolaX(x) - pTime;
                    
                    if (!playerMove.IsAvailableToAttack(x,y,z,left_time,true,delay)) { // 도달 가능하면 공격을 하세요.
                        x = playerMove.getFallingPlaceXbyPlayer(JUMP_NO,isFall,0.0f); // 공격 안되면 리시브..
                        y = playerPhys.getLandBody_Y();      
                        z = ballPhys.getParabolaZbyX(x);
                        ballTime = getBallTime(x,z);
                        jump_type = JUMP_NO;
                    }
                    break;
        }
        if (team == TEAM_LEFT) {
            if (x > NET_X) return -INF;
        }
        else if (team == TEAM_RIGHT) {
            if (x < NET_X) return -INF;
        }
        playerTime = getPlayerTime(Player , x , z) + playerSet.Status.getReactSpeed();
        float score = ballTime - playerTime;
        Debug.Log($"x : {x} , z : {z} , score {score} / given_score {given_score}");
        if (score > given_score) 
            playerSet.setGoal(x,y,z,jump_type,delay);

        return score;
        
    }
    private float calculateScore(GameObject Player , int TouchCount) {
        //어떤 선수가 followingPlayer가 될 지 우선순위를 구하기 위해 플레이어별 거리,포지션으로 스코어 계산
        
        PlayerSetting ps = Player.GetComponent<PlayerSetting>();
        PlayerMove playerMove = Player.GetComponent<PlayerMove>();
        movePhysics ballPhys = Ball.GetComponent<movePhysics>();
        movePhysics playerPhys = Player.GetComponent<movePhysics>();
        PlayerSetting playerSet = Player.GetComponent<PlayerSetting>();        
 
        int team = ps.getTeam();
        int position = ps.getPosition();
        float score = -INF;


        if (playerSet.isControl()) { 
            return score;
            //-INF
        }
        if (lastTouchPlayer == Player) {
            //-INF 투터치가 안되도록.
            return score;
        }
        float jump_type = JUMP_NO;
        float playerTime = 0.0f,ballTime = 0.0f;

        
        bool isAvailableToReach;

        
        // 포물선에서 올라가는 볼을 계산
        // 이 때 올라가는 볼 좌표까지 도달하는데 걸리는 시간을 통하여 score을 계산한다.
        // 두번 째 계산은 내려갈 때 볼 계산 해당 
        // 이게 더 score가 높으면 이걸로 간다!!!

        score = setTimeAndGoal(Player, TouchCount ,ref playerTime , ref ballTime , false, score);
        score = setTimeAndGoal(Player, TouchCount ,ref playerTime , ref ballTime , true , score);
        Debug.Log("--------------------------");

        isAvailableToReach = score > 0;

        int AbsTouchCount = Mathf.Abs(getTouchCount(team));

        //(능력치 기준으로 score 배수를 바꿀 생각중)

        if (AbsTouchCount == 0) { // 리시브 
            if (isAvailableToReach) {
                if (position == LIBERO) { score = score * 5.0f; } //리베로가 받을 확률 더 높게.
                else if (position == SETTER) { score = score / 2; } //세터의 경우 스코어 절반
            }
        }
        else if (AbsTouchCount == 1) {
            if (isAvailableToReach) {
                if (position == SETTER)     score = score * 10.0f; //세터가 올리는게 제일 좋아. 
                if (position == LIBERO)     score = score * 3.0f;  //세터가 아니면, 리베로가 올리는게 제일 좋아 
            }
        }
        else if (AbsTouchCount == 2 ) {
            jump_type = JUMP_SPIKE;
            if (isAvailableToReach) if (position == LIBERO) { score = score / 10.0f; } // 리베로는 가능하면 마지막 터치는 안함.

            if ( getCurrentSituation() == STRATEGY_OPEN ) { // 오픈 토스일 경우, 반드시 줄 필요는 없다.
                if (position == SPIKER) { score = score * 4.5f; }
                if (ps.getPlayerAction() == ACTION_QUICKREADY) { score = -INF; }
            }
            if ( getCurrentSituation() == STRATEGY_QUICK ) { // 속공의 경우 반드시 속공수한테 준다.
                if (ps.getPlayerAction() == ACTION_QUICKREADY) {
                    score = INF;
                }
            }
        }
        else 
            score = -INF;


        bool checkB = checkMyBall( playerPhys.getLandBody_Y() + getMaxHeightBySpeed(playerSet.Status.getJump() * jump_type) + playerPhys.getHeight()*0.5f, team,AbsTouchCount);
        // 어느 팀의 볼인지 체크 ( ex : 넘어가는 볼인지 , 넘어간다 하더라도 본인이 점프하면 닿을 수 있는 볼인지 판단)
        playerSet.checkB = checkB;
        if (!checkB) score = -INF; //볼이 우리꺼가 아닐 경우 안받는다
        

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
    public GameObject getPlayersByIndex(int index) {
        if (index >= 0 && index < playerNumber) return Players[index];
        return null;
    }
    /// <summary>
    /// 공의 낙하지점에 가장 적합한 Player을 상황에 맞추어 계산한다.
    /// </summary>
    /// <returns></returns>
    public GameObject getNearestPlayer() {
        //get player who has the Highest score.
        float maxVal = -10.0f;
        GameObject maxPlayer = null;
        movePhysics ballPhys = Ball.GetComponent<movePhysics>();

        for (int i = 0; i < Constants.playerNumber; i ++)
        {
            int player_team = Players[i].GetComponent<PlayerSetting>().getTeam();
            PlayerMove playerMove =  Players[i].GetComponent<PlayerMove>();
            float score;

            score = calculateScore(Players[i] , getTouchCount(player_team));
            
            Players[i].GetComponent<PlayerMove>().score = score; 

            if (score > maxVal) { // score가 0보다는 커야 함.
                maxVal = score;
                maxPlayer = Players[i];
            }
        }
        
        return maxPlayer;
    }
    public bool checkMyBall(float playerJumpHeight , int playerTeam, int touchCount){
        movePhysics ballPhys = Ball.GetComponent<movePhysics>();
        
        // 볼이 네트를 지나갈 경우
        if (Mathf.Sign(ballPhys.startPos.x - NET_X) != Mathf.Sign(ballPhys.endPos.x - NET_X))
        {
            float ballHeightOnNet , ballHeightOnPlayetLimitX;
            bool IsLeftToRight = (ballPhys.startPos.x < NET_X);
            int myteam = IsLeftToRight ? TEAM_LEFT : TEAM_RIGHT;
            float limit_X = IsLeftToRight ? LEFT_LIMIT : RIGHT_LIMIT;
                // //포물선이 네트를 넘길 때, 해당 포물선이 플레이어의 타점을 지나가는지 여부를 통해 자신의 볼인지 아닌지 결정한다.
                // float Reach1 =  ballPhys.getParabolaXbyY(playerJumpHeight, false); // 올라가는 궤도에서의 만나는 지점
                // float Reach2 =  ballPhys.getParabolaXbyY(playerJumpHeight , true); // 내려가는 궤도에서 만나는 지점

                // if (IsLeftToRight) { //값이 닿는다고 하더라도 , 한계를 넘어버리면 못 닿는거로 취급
                //     Reach1 = (Reach1 > limit_X ? float.NaN : Reach1); 
                //     Reach2 = (Reach2 > limit_X ? float.NaN : Reach2);
                // }
                // else {
                //     Reach1 = (Reach1 < limit_X ? float.NaN : Reach1); 
                //     Reach2 = (Reach2 < limit_X ? float.NaN : Reach2);
                // }
                
                
                //ballHeightOnPlayetLimitX = ballPhys.getParabolaYbyX(limit_X)-ballPhys.getHeight()/2;
                ballHeightOnNet = ballPhys.getParabolaYbyX(NET_X)-ballPhys.getHeight()/2;
                if (playerTeam == myteam) // 우리 팀 입장에서
                {
                    if (getCurrentSituation() == SIT_SERVERHIT) // 서브 볼은 기본적으로 무조건 우리꺼 아님.
                        return false;

                    return true;
                    // if ( !float.IsNaN(Reach1) || !float.IsNaN(Reach2))  return true; //하나라도 닿는게 있으면 우리꺼
                    // else return false; // 아~ 둘다 못닿으면 어쩔 수 없죠~

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

