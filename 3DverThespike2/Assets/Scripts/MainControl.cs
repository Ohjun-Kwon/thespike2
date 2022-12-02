
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

    [SerializeField]public int showSpecialDebug = 2;
    private GameObject lastTouchPlayer;
    public GameObject followingPlayer;
    private GameObject controlId;
    public GameObject nowServePlayer;
    private MainSetting MainSetting;
    private TimeTrigger TimeTrigger;

    [SerializeField] public int[] touchCount = new int[2] {0,0};

    Transform lineYTransform;
    Transform BallXYTransform;

    [SerializeField]public float mbY;
    public float quickTime;
    public int currentSituation;

    public bool dontmove = false;


    [SerializeField] public TextMeshProUGUI ballTeamTEXT;
    [SerializeField] public TextMeshProUGUI blockWhoTEXT;
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
             Players[0+ j*4].GetComponent<PlayerSetting>().playerCreate(3.0f,2.6f,3.8f,3.0f);        
             Players[0+ j*4].GetComponent<PlayerMove>().setStatus();    
             
             Players[1+ j*4].GetComponent<PlayerSetting>().setPosition(Constants.SETTER);
             Players[1 + j*4].GetComponent<PlayerSetting>().setRotation(1);
             Players[1+ j*4].GetComponent<PlayerSetting>().playerCreate(2.0f,2.5f,3.7f,1.0f);
             Players[1+ j*4].GetComponent<PlayerMove>().setStatus();
             
             Players[2+ j*4].GetComponent<PlayerSetting>().setPosition(Constants.BLOCKER);
             Players[2 +j*4].GetComponent<PlayerSetting>().setRotation(2);
             Players[2+ j*4].GetComponent<PlayerSetting>().playerCreate(4.0f,2.3f,3.8f,1.0f);        
             Players[2+ j*4].GetComponent<PlayerMove>().setStatus();        
             
             
             Players[3 + j*4].GetComponent<PlayerSetting>().setPosition(Constants.SPIKER);
             Players[3 + j*4].GetComponent<PlayerSetting>().setRotation(3);
             Players[3+ j*4].GetComponent<PlayerSetting>().playerCreate(1.0f,2.8f,3.7f,1.0f);
             Players[3+ j*4].GetComponent<PlayerMove>().setStatus();

             nowServePlayer = Players[1];
        }
        MainSetting = GetComponent<MainSetting>();
        TimeTrigger = GetComponent<TimeTrigger>();
    }


    public void showDebug(string text,int x = 0) {
        if (x == -1) {
            Debug.Log(text);
            return;
        }
        if (x == showSpecialDebug)
            Debug.Log(text);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.D))  // 오른쪽 선수가 서브
        {
            MainSetting.addRotation(TEAM_RIGHT, 1);
            resetTouchCount();
            nowServePlayer = getNowServer(TEAM_RIGHT);
            MainSetting.setCurrentSituation(SIT_SERVERGO);
        }
        else if (Input.GetKeyDown(KeyCode.S)) // 왼쪽 선수가 서브
        {
            
            MainSetting.addRotation(TEAM_LEFT, 1);
            resetTouchCount();
            nowServePlayer = getNowServer(TEAM_LEFT);
            MainSetting.setCurrentSituation(SIT_SERVERGO);
        }
        if (Input.GetKeyDown(KeyCode.Q))      { playSpeed -= 0.005f; } //시간 느리게 
        else if (Input.GetKeyDown(KeyCode.W)) { playSpeed += 0.005f; } //시간 빠르게
        if (Input.GetKeyDown(KeyCode.E))      { dontmove = dontmove ? false : true; } //시간 느리게
        ballTeamTEXT.text = $"play Speed : {playSpeed}";     
        //blockWhoTEXT.text = $"TEAM LEFT  {MainSetting.getBlockStrategy(TEAM_LEFT,1)} {MainSetting.getBlockStrategy(TEAM_LEFT,2)} TEAM_RIGHT {MainSetting.getBlockStrategy(TEAM_RIGHT,1)} {MainSetting.getBlockStrategy(TEAM_RIGHT,2)}";
        int dir = (int)Ball.GetComponent<movePhysics>().getCurrentDirection();
        blockWhoTEXT.text = $"BALL DIrection {dir}";
    }
    void FixedUpdate()
    {   
        float controlDirection = 0f;

        //Control선수 조종 용도.
        if (Input.GetKey(KeyCode.LeftArrow))       { controlDirection = -INF; } //PlayerMoveStart(Vector3.left , controlId,true); } 
        else if (Input.GetKey(KeyCode.RightArrow)) { controlDirection = INF;  } //PlayerMoveStart(Vector3.right , controlId,true);  
        
        if (Input.GetKey(KeyCode.Space)) { getPlayerByPosition(LIBERO,TEAM_LEFT).GetComponent<PlayerMove>().DoSlide(); } 

            
      
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
                if (dontmove) continue;
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
                int rot = MainSetting.getRotation(team);
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
            int rot = MainSetting.getRotation(team);
            
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
    public GameObject getNowServer() {
        return nowServePlayer;
    }
    public void playSound(string sound) {
        GetComponent<SoundControl>().PlaySound(sound);
    }
    public void allPlayerFreeze(int surpriseLevel) {
        for (int i = 0;i < playerNumber ; i ++) {
            PlayerMove pm = Players[i].GetComponent<PlayerMove>();
            PlayerSetting ps = Players[i].GetComponent<PlayerSetting>();
            pm.setMoveDelay(ps.Status.getReactSpeed()*surpriseLevel);
        }
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
            
            TimeTrigger.addTrigger(1f , () => 
            {
                controlPlayer.GetComponent<PlayerSetting>().setPlayerAction(ACTION_SERVETOSS);                        
                MainSetting.setCurrentSituation(SIT_SERVERTOSS);

                ballPhys.setVector(90 - team*5f,2.1f);
                ballPhys.setZDirection(Constants.CENTER);
                ballPhys.startParabola();
            }).next(3f , () => {
                controlPlayer.GetComponent<PlayerSetting>().setPlayerAction(ACTION_SERVEDONE);                        
                MainSetting.setCurrentSituation(SIT_SERVERHIT);
                setLastTouch(nowServePlayer);
                nowServePlayer = null;
                
                Ball.GetComponent<BallMovement>().ballServe(90 + team * UnityEngine.Random.Range(60,70), 7.5f);
                //SERVE HIT POINT
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

        if (followingPlayer == null ) 
        {
            
            goal = Ball.GetComponent<movePhysics>().endPos;
            goalBall.transform.position = goal;
            return; // 받을 수 있는 놈이 없다. (ex : 네트 걸림 등. )
        }


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
                TimeTrigger.addTrigger( costTime ,()=>{
                    playerMove.DoReceive(goal);    
                });             
            } 
            getPlayersByRot(-team,1).GetComponent<PlayerSetting>().setBlockFollowZ(NOMOVE_Z);
            getPlayersByRot(-team,2).GetComponent<PlayerSetting>().setBlockFollowZ(NOMOVE_Z);
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
                TimeTrigger.addTrigger(ballPhys.getRemainTimeToParabolaX(goal.x)- playerStat.getTossTime(),  ()=> { playerMove.DoToss(goal);});
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
            quickTime = 1.3f;

            if ( MBMove.IsAvailableToQuick(tx,ty,tz, costTime,true,MBdelay)) {
                mbY = MBPhys.getLandHead_Y() + getMaxHeightBySpeed(MBSet.Status.getJump()) - MBdelay;
                if (!MBSet.isControl())
                    MBMove.setJumpTime(ballPhys.getRemainTimeToParabolaX(goal.x) + quickTime - mbTime , JUMP_SPIKE);            
                
                MBSet.setPlayerAction(ACTION_QUICKREADY);
                TimeTrigger.addTrigger(ballPhys.getRemainTimeToParabolaX(goal.x) + quickTime - MBStat.getSwingTime(),  ()=> { MBMove.DoSpike(goal); });
                MBSet.setTarget( tx , ty , tz);   //블로커 블로킹 정해짐.
            }
            
            setTeamBlockBallType(-team ,MainSetting.getCurrentBallType(team));
        }
        else { 

            if (playerSet.getPlayerAction() == ACTION_QUICKREADY) {
                Vector3 _target = playerSet.getTarget();
                goalBall.transform.position = new Vector3 (_target.x,_target.y,_target.z); 
                goal = new Vector3(_target.x,_target.y,_target.z);

                
                setTeamBlockFollow(-team,STRATEGY_QUICK,_target.z,ballPhys.getRemainTimeToParabolaX(_target.x));                     
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

                    
                    TimeTrigger.addTrigger(ballPhys.getRemainTimeToParabolaX(goal.x) - swingSpeed ,()=>{
                                playerMove.DoSpike(goal);    
                    });  


                    setTeamBlockFollow(-team,STRATEGY_OPEN,goal.z,ballPhys.getRemainTimeToParabolaX(goal.x));          
                }
                else {
                    float costTime = ballPhys.getRemainTimeToParabolaX(goal.x) - playerStat.getReceiveTime();

                    if (playerMove.IsArrivedInTime( goal.x , goal.z , costTime)) {
                        playerSet.setPlayerAction(ACTION_RECEIVEREADY);
                        TimeTrigger.addTrigger( ballPhys.getRemainTimeToParabolaX(goal.x)-playerStat.getReceiveTime() ,()=>{
                            playerMove.DoReceive(goal);
                        });     
                    }
                    setTeamBlockFollow(-team,STRATEGY_OPEN,goal.z,0f,true);          
                }         
            }
        }

        
        if (getTouchCount(-team) == 3)
        {
            if (!checkTeamBlockHit(team,1)) // 2명이 동시에 막으면 이상해지므로, 한명이 못 받았을 때에만 나머지 블로커를 체크한다.
                checkTeamBlockHit(team,2);
        }

    
    }


    /// <summary>
    /// 스파이크 친 공이 블로킹에 맞는지 계산하고, 맞는 다면 HitBlock을 타임 트리거에 건다.
    /// </summary>
    /// <param name="team"></param>
    /// <param name="rot">1,2가 전위 선수</param>
    public bool checkTeamBlockHit(int team,int rot) {
        // 전위 선수중에 블록 뛴 선수를 본다.
        // 골의 z값이 같아야 한다.
        // 자신의 x값까지 도달하는데 걸리는 시간 때 y값 비교.
        // z값 비교.
        // 충돌 시간 비교해서 플레이어에게 hit 
        
        GameObject blockPlayer = getPlayersByRot(team, rot);        
        PlayerSetting playerSet = blockPlayer.GetComponent<PlayerSetting>();
        PlayerMove playerMove = blockPlayer.GetComponent<PlayerMove>();         
        movePhysics playerPhys = blockPlayer.GetComponent<movePhysics>();         
        movePhysics ballPhys = Ball.GetComponent<movePhysics>();
        if (playerSet.getPlayerAction() == ACTION_BLOCKJUMP) { // 블로킹 점프상태입니다~
            float addX = Ball.GetComponent<BoxCollider>().bounds.size.x / 2 + blockPlayer.GetComponent<BoxCollider>().bounds.size.x / 2;
            if (team ==TEAM_RIGHT) addX = -addX;
            float leftTime = ballPhys.getRemainTimeToParabolaX(blockPlayer.transform.position.x + addX);
            Vector3 ballPos = ballPhys.getParabolaByTime(ballPhys.getCurrentTime() + leftTime);
            Vector3 playerPos = playerPhys.getParabolaByTime(playerPhys.getCurrentTime() + leftTime);

            if (playerMove.isInBoxOnPos(playerPos,ballPos,ballPhys.getHeight())) {
                TimeTrigger.addTrigger(leftTime,()=> {
                    playerMove.HitBlock(ballPos,goal);
                });
                return true;
            }
        }
        return false;
    }

    public void setTeamBlockFollow(int team,int strategy,float z,float time,bool chance = false) {
        if (chance) {
            getPlayersByRot(team,1).GetComponent<PlayerSetting>().setBlockFollowZ(NOBLOCK_Z);
            getPlayersByRot(team,2).GetComponent<PlayerSetting>().setBlockFollowZ(NOBLOCK_Z);
        }
        else {
            if (MainSetting.getBlockStrategy(team,1) == strategy) {
                GameObject blockPlayer = getPlayersByRot(team,1);
                PlayerSetting playerSet = blockPlayer.GetComponent<PlayerSetting>();
                PlayerMove playerMove = blockPlayer.GetComponent<PlayerMove>();
                blockPlayer.GetComponent<PlayerSetting>().setBlockFollowZ(z);
                float pTime = getFlightTimeBySpeed(playerSet.Status.getJump()*JUMP_BLOCK);
                playerMove.setJumpTime(time - pTime , JUMP_BLOCK);
                playerSet.setPlayerAction(ACTION_BLOCKJUMP);
                TimeTrigger.addTrigger( pTime + time ,()=>{
                    if (playerSet.getPlayerAction() == ACTION_BLOCKJUMP)
                        playerSet.setPlayerAction(ACTION_BLOCKDONE);  
                });                
            }

            if (MainSetting.getBlockStrategy(team,2) == strategy) {
                GameObject blockPlayer = getPlayersByRot(team,2);
                PlayerSetting playerSet = blockPlayer.GetComponent<PlayerSetting>();
                PlayerMove playerMove = blockPlayer.GetComponent<PlayerMove>();
                blockPlayer.GetComponent<PlayerSetting>().setBlockFollowZ(z);
                float pTime = getFlightTimeBySpeed(playerSet.Status.getJump()*JUMP_BLOCK);
                playerMove.setJumpTime(time - pTime , JUMP_BLOCK);
                playerSet.setPlayerAction(ACTION_BLOCKJUMP);
                
                TimeTrigger.addTrigger( pTime + time ,()=>{
                    if (playerSet.getPlayerAction() == ACTION_BLOCKJUMP)
                        playerSet.setPlayerAction(ACTION_BLOCKDONE);  
                });                
            }
        }
    }
    /// <summary>
    /// 리시브 형태에 따라, 블로킹을 누구를 타케팅 할 지 결정한다.
    /// reset이 true이면, 기존에 블록 대기하는거 초기화.
    /// </summary>
    /// <param name="team"></param>
    /// <param name="ballType"></param>
    public void setTeamBlockBallType( int team , int ballType) {
        switch(ballType) {
            case BALL_RECEIVE_GOOD: case BALL_RECEIVE_GOOD_LOW:
                MainSetting.setBlockStrategy(team,1,STRATEGY_QUICK);
                MainSetting.setBlockStrategy(team,2,STRATEGY_OPEN);  
            break;

            default : 
                MainSetting.setBlockStrategy(team,1,STRATEGY_OPEN);
                MainSetting.setBlockStrategy(team,2,STRATEGY_OPEN);  
            break;
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
                    delay = UnityEngine.Random.Range(0.0f,0.7f);
                    x = playerMove.getFallingPlaceXbyPlayer(JUMP_TOSS,isFall,delay); // 점프 토스로 계산
                    z = ballPhys.getParabolaZbyX(x);
                    ballTime = getBallTime(x,z);
                    jump_type = JUMP_TOSS;
                    if (!playerMove.IsArrivedInTime(x,z,ballTime)) {
                        jump_type = JUMP_NO;
                        x = playerMove.getFallingPlaceXbyPlayer(JUMP_NO,isFall,0.0f); // 점프 토스 안되면..
                        z = ballPhys.getParabolaZbyX(x);
                        ballTime = getBallTime(x,z);
                    }
                    y = playerPhys.getLandHead_Y()+ getMaxHeightBySpeed(playerSet.Status.getJump() * jump_type) - delay; 
                    break;
            case 2: 
                    delay = UnityEngine.Random.Range(0.0f,0.5f);
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
            if (x < NET_X) {
                return -INF;
            }
        }
        playerTime = getPlayerTime(Player , x , z);
        float score = ballTime - playerTime;
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
        else{
            score = -INF;
        }



        bool checkB = checkMyBall( playerPhys.getLandBody_Y() + getMaxHeightBySpeed(playerSet.Status.getJump() * jump_type) + playerPhys.getHeight()*0.5f, team,AbsTouchCount);
        // 어느 팀의 볼인지 체크 ( ex : 넘어가는 볼인지 , 넘어간다 하더라도 본인이 점프하면 닿을 수 있는 볼인지 판단)
        playerSet.checkB = checkB;
        if (!checkB) {
            score = -INF; //볼이 우리꺼가 아닐 경우 안받는다
        } 
        

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
    public bool isBallOur(int team) {
       
        GameObject fp = getFollowingPlayer();
         if (fp != null) {
            if (fp.GetComponent<PlayerSetting>().getTeam() == team)
                return true;
         } 
         return false;
            
    }
    public GameObject getPlayersByIndex(int index) {
        if (index >= 0 && index < playerNumber) return Players[index];
        return null;
    }

    public GameObject getPlayersByRot(int team , int rot) {

        int starti = 0;
        int endi = 4;
        if (team == TEAM_RIGHT) {
            starti = 4;
            endi = 8;
        }
        int rotationPlace = MainSetting.getRotation(team);
        for (int i = starti; i < endi; i ++) {
            if ( (Players[i].GetComponent<PlayerSetting>().getRotation() + rotationPlace) % 4 == rot) return Players[i];
        }
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
            float ballHeightOnNet;
            bool IsLeftToRight = (ballPhys.startPos.x < NET_X);
            int myteam = IsLeftToRight ? TEAM_LEFT : TEAM_RIGHT;
            float limit_X = IsLeftToRight ? LEFT_LIMIT : RIGHT_LIMIT;
            BallMovement ballMove = Ball.GetComponent<BallMovement>();
            //Debug.Log($"PASS NET {ballPhys.startPos.x} /  {ballPhys.endPos.x}");
                ballHeightOnNet = ballPhys.getParabolaYbyX(NET_X)-ballPhys.getHeight()/2;
                if (playerTeam == myteam) // 우리 팀 입장에서
                {
                    if (getCurrentSituation() == SIT_SERVERHIT) // 서브 볼은 기본적으로 무조건 우리꺼 아님.
                        return false;
                    // Debug.Log($"PASS NET {ballPhys.startPos.x} /  {ballPhys.endPos.x} TRUE");
                    return true;
                }
                else // 반대편 입장에서
                {
                    //
                    if (ballHeightOnNet < NET_Y) // 반대 편 입장에서, // 네트에 안맞으면
                    {
                        //Debug.Log($"PASS NET {ballPhys.startPos.x} /  {ballPhys.endPos.x} OPPOSITE  FALSE");
                        return false;
                    }
                        
                    else {
                        //Debug.Log($"PASS NET {ballPhys.startPos.x} /  {ballPhys.endPos.x} OPPOSITE FALSE");                        
                        return true;
                    }
                        
                }                
        }
        else
        {
            bool IsLeft = (ballPhys.endPos.x < NET_X);
            
            if (IsLeft) {
                if (playerTeam== TEAM_LEFT) return true;
                else{
                    return false;
                } 
            } 
            else {
                if (playerTeam== TEAM_RIGHT) return true;
                else {
                    return false;
                }
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

