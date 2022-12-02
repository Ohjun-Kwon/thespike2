using static Constants;

using UnityEngine;
using System;
using TMPro;

public class BallMovement : MonoBehaviour
{
    private movePhysics movePhys;
    private MainControl mainControl;
    private MainSetting mainSetting;
    [SerializeField] public GameObject SystemObject;

    [SerializeField] public TextMeshProUGUI touchCountTEXT1;
    [SerializeField] public TextMeshProUGUI touchCountTEXT2;    

    [SerializeField] public float hitNetTime = INF;
    [SerializeField] public float hitFloorTime = INF;
    public Vector3 hitNetVector;

    public TimeTrigger timeTrigger;
    // Start is called before the first frame update
    void Start()
    {
        movePhys = GetComponent<movePhysics>();
        movePhys.initPhysics();
        mainControl = SystemObject.GetComponent<MainControl>();
        mainSetting = SystemObject.GetComponent<MainSetting>();
        timeTrigger = mainControl.GetComponent<TimeTrigger>();
        CheckHit();
    }

    void Update() {   
        
    }
    public void FixedUpdate() {

        if (timeTrigger.getMainTimeFlow() >= hitNetTime) 
        {
            hitNetTime = INF;
            ballHitNet();
        }
        if (timeTrigger.getMainTimeFlow() >= hitFloorTime) 
        {
            //mainControl.showDebug($"hit Floor Time이 몇이길래..? {hitFloorTime}");
            hitFloorTime = INF;
            ballHitFloor();
        }

        if (hitNetTime >= INF / 2) GetComponent<Renderer>().material.SetColor("_Color", Color.white);
        else    GetComponent<Renderer>().material.SetColor("_Color", Color.black);        // 안닿아
    }
    public void CheckHit() {
        hitNetTime = checkHitNet();
        hitFloorTime = movePhys.getFlightTime();

        if (hitNetTime < 0.1f) hitNetTime = INF;
        else hitNetTime += timeTrigger.getMainTimeFlow();

        if (hitFloorTime < 0.3f) hitFloorTime = INF;
        else hitFloorTime += timeTrigger.getMainTimeFlow();

    }
    public float getHitNetTime() {
        return hitNetTime;
    }
    /// <summary>
    /// 볼의 움직임 변화가 있을 때마다 호출하는 함수.
    /// surpriseLevel에 따라 플레이어들의 반응 속도가 결정된다.
    /// </summary>
    /// <param name="surpriseLevel"></param>
    public void ballMovementChange(int surpriseLevel) {
        mainSetting.setCurrentBallType();
        mainControl.allPlayerFreeze(surpriseLevel);
        mainControl.commandPlayerMove();
        CheckHit();
        DEBUG_Text();        
    }
    public void ballReceive(int team) {
        mainControl.showDebug("Do Receive");

        if (mainControl.getTouchCount(team) == 0) {
            movePhys.setVectorByVspeedParabola(NET_X + team * (0.05f + UnityEngine.Random.Range(-0.5f,5.0f)),NET_Y,UnityEngine.Random.Range(2.6f,6.2f));
            movePhys.setZDirection(Constants.CENTER);
        }
        else {
            movePhys.setVectorByVspeedParabola(NET_X - team * (0.05f + UnityEngine.Random.Range(1.0f,5.0f)),NET_Y/2,UnityEngine.Random.Range(2.0f,3.2f));
            movePhys.setZDirection(Constants.CENTER);
        }
        
        mainSetting.setCurrentSituation(SIT_RALLYPLAYING);
        movePhys.startParabola();
        mainControl.addTouchCount(team);
        mainControl.playSound("RECEIVE");
        ballMovementChange(1);
    }
    public void ballSpike(int team) {
        int spike = 3;
        if (UnityEngine.Random.Range(0,35) < 33 || true) {
            movePhys.setVectorByVspeedSpike(NET_X + team * (- 1.5f - UnityEngine.Random.Range(5.0f,12.0f)),movePhys.getLandBody_Y(),UnityEngine.Random.Range(1.1f,3f));
            movePhys.setZDirection(UnityEngine.Random.Range(0,3));
            mainControl.playSound("SPIKE");
            spike = 1;
        }
        else {
            movePhys.setVectorByVspeedParabola(NET_X + team * (- 3.05f),NET_Y,UnityEngine.Random.Range(0.3f,0.8f));
            movePhys.setZDirection(UnityEngine.Random.Range(0,3));
            mainControl.playSound("TOSS");
            
        }
        movePhys.startParabola();
        mainControl.addTouchCount(team);
        ballMovementChange(spike);   
    }
    public void ballBlock(int team) {
        float newDir;
        if (UnityEngine.Random.Range(0,9) < 2) {
            newDir = PhysCalculate.getYFlippedDirection(movePhys.getCurrentDirection());
        }
        else{
            newDir = PhysCalculate.getXFlippedDirection(movePhys.getCurrentDirection());
            newDir += (270 - newDir) /5;
            movePhys.setZDirection(UnityEngine.Random.Range(0,3));
        }
        movePhys.setVector(newDir,movePhys.getCurrentSpeed()*0.95f);
        movePhys.startParabola();
        mainControl.resetTouchCount();
        ballMovementChange(1);   
        mainControl.playSound("BLOCK");
    }
    public void ballToss(int team) {

        int MB_ID = BLOCKER + (mainControl.getLastTouchTeam() == TEAM_LEFT ? 0 : 4);
        PlayerSetting MBSet = mainControl.getPlayersByIndex(MB_ID).GetComponent<PlayerSetting>();
        
        if (MBSet.getPlayerAction() == ACTION_QUICKREADY && UnityEngine.Random.Range(0,7) < 6){
            float z_dir = team == TEAM_LEFT ? Z_RIGHT : Z_LEFT;
            movePhys.setVectorForQuickAttack( NET_X + team * NEARFRONT, mainControl.mbY ,z_dir, mainControl.quickTime);
            mainControl.setCurrentSituation(STRATEGY_QUICK);
        }
        else{ 
            movePhys.setVectorByVspeedParabola(NET_X + team * (-1.5f + UnityEngine.Random.Range(0.0f,7.0f)),NET_Y,0.75f + UnityEngine.Random.Range(1.6f,2.6f));
            movePhys.setZDirection(Constants.CENTER);
            movePhys.setZDirection(UnityEngine.Random.Range(0,3));
            
            mainControl.setCurrentSituation(STRATEGY_OPEN);
        }
        mainSetting.setCurrentSituation(SIT_RALLYPLAYING); 
        movePhys.startParabola();
        mainControl.addTouchCount(team);
        mainControl.playSound("TOSS");
        ballMovementChange(1);
    }    

    
    public void ballServe(float dir, float spd) {
            movePhys.setVector(dir,spd);
            movePhys.setZDirection(Constants.CENTER);
            movePhys.startParabola();
            mainControl.setTouchCount(TEAM_LEFT , 0);
            mainControl.setTouchCount(TEAM_RIGHT , 0);
            mainSetting.setCurrentBallType();
            mainControl.commandPlayerMove();
            CheckHit();
            DEBUG_Text();
            mainControl.playSound("SERVE");
            ballMovementChange(2);
    }
    public void ballHitFloor(){
        mainControl.playSound("LAND");
        float newDir = PhysCalculate.getYFlippedDirection(movePhys.getCurrentDirection());
        //mainControl.showDebug($"newDir : {newDir} originalDirection : {movePhys.getCurrentDirection()} speed : {movePhys.getCurrentSpeed()*0.85}",1);
        
        movePhys.setVector(newDir,movePhys.getCurrentSpeed()*0.4f);
        movePhys.startParabola();
        mainControl.resetTouchCount();
        CheckHit();
    }
    public void ballHitNet() {

//        mainControl.showDebug($"Direction : {movePhys.getCurrentDirection()} , {360f - movePhys.getCurrentDirection()}");

        float ballYOnNet = movePhys.getParabolaYbyX(NET_X);
        transform.position = hitNetVector;
        if (ballYOnNet > NET_Y) // 끝에만 살짝 걸친 정도.
        {
            float newDir = PhysCalculate.getYFlippedDirection(movePhys.getCurrentDirection());
            //Debug.Break();
            //mainControl.showDebug(newDir);
            movePhys.setVector(newDir,movePhys.getCurrentSpeed()*0.3f);
            movePhys.startParabola();
            mainControl.playSound("NET");
        }
        else {
            float newDir = PhysCalculate.getXFlippedDirection(movePhys.getCurrentDirection());
            //Debug.Break();
            //mainControl.showDebug(newDir);
            movePhys.setVector(newDir,movePhys.getCurrentSpeed()*0.3f);
            movePhys.startParabola();
            mainControl.playSound("NET");
        }
        ballMovementChange(2);
    }

    /// <summary>
    /// 현재 움직임이 네트를 부딪히는 지, 부딪힌다면 언제 부딪히는지 반환한다.
    /// </summary>
    /// <returns>네트를 부딪히는 시간 못 부딪히면 INF</returns>
    public float checkHitNet() {
        if (Mathf.Sign(movePhys.startPos.x - NET_X) != Mathf.Sign(movePhys.endPos.x - NET_X)) 
        {
            float size = movePhys.getHeight()/2;

            float targetX;
            if (movePhys.getHorizontalSpeed() > 0) targetX = NET_X - size;
            else targetX = NET_X + size;
            float ballYOnNet = movePhys.getParabolaYbyX(targetX)-movePhys.getHeight()/2;
            
            
            if (ballYOnNet < NET_Y) // hit the net
            {
                float leftTime = movePhys.getRemainTimeToParabolaX(targetX);
                hitNetVector = movePhys.getParabolaByTime(leftTime);
                return leftTime;
                
            }
            else
                return INF;
        }
        else return INF;

    }

    public void DEBUG_Text() {
        switch(mainSetting.getCurrentBallType(TEAM_LEFT)) {
            case BALL_FREEBALL : touchCountTEXT1.text = "TEAM LEFT : Free ball DEFENSE"; break;
            case BALL_FREEBALL_SHORT : touchCountTEXT1.text = "TEAM LEFT : Short Free ball DEFENSE"; break;
            case BALL_FAINT_SHORT : touchCountTEXT1.text = "TEAM LEFT : Short faint DEFENSE"; break;
            case BALL_FAINT_LONG : touchCountTEXT1.text = "TEAM LEFT : Long faint DEFENSE"; break;
            case BALL_ATTACK : touchCountTEXT1.text = "TEAM LEFT : Attack DEFENSE"; break;
            case BALL_BALLWAIT : touchCountTEXT1.text = "TEAM LEFT : WAITFORBALL"; break;
            case BALL_RECEIVE_GOOD : touchCountTEXT1.text = "TEAM LEFT : Good Receive"; break;                        
            case BALL_RECEIVE_SHORT: touchCountTEXT1.text = "TEAM LEFT : Short Receive"; break;                       
            case BALL_RECEIVE_GOOD_LOW : touchCountTEXT1.text = "TEAM LEFT : Low Good Receive"; break;                        
            case BALL_RECEIVE_SHORT_LOW: touchCountTEXT1.text = "TEAM LEFT : Low Short Receive"; break;                                     
            case BALL_RECEIVE_BAD: touchCountTEXT1.text = "TEAM LEFT : Bad Receive"; break;                                                 
            case BALL_RECEIVE_BAD_LOW: touchCountTEXT1.text = "TEAM LEFT : Low Bad Receive"; break;                                                 
            case BALL_RECEIVE_BAD_LONG: touchCountTEXT1.text = "TEAM LEFT : Long Bad Receive"; break;                                                 
            case BALL_TOSS: touchCountTEXT1.text = "TEAM LEFT : TOSS"; break;       
            case BALL_UNAVAILABLE: touchCountTEXT1.text = "TEAM LEFT : UNAVAILABLE"; break;                                                
        }
        switch(mainSetting.getCurrentBallType(TEAM_RIGHT)) {
            case BALL_FREEBALL : touchCountTEXT2.text = "TEAM RIGHT : Free ball Defense"; break;
            case BALL_FREEBALL_SHORT : touchCountTEXT2.text = "TEAM RIGHT : Short Free ball Defense"; break;
            case BALL_FAINT_SHORT : touchCountTEXT2.text = "TEAM RIGHT : Short faint Defense"; break;
            case BALL_FAINT_LONG : touchCountTEXT2.text = "TEAM RIGHT : Long faint Defense"; break;
            case BALL_ATTACK : touchCountTEXT2.text = "TEAM RIGHT : Attack Defense"; break;
            case BALL_BALLWAIT : touchCountTEXT2.text = "TEAM RIGHT : WAITFORBALL"; break;
            case BALL_RECEIVE_GOOD : touchCountTEXT2.text = "TEAM RIGHT : Good Receive"; break;                        
            case BALL_RECEIVE_SHORT: touchCountTEXT2.text = "TEAM RIGHT : Short Receive"; break;                       
            case BALL_RECEIVE_GOOD_LOW : touchCountTEXT2.text = "TEAM RIGHT : Low Good Receive"; break;                        
            case BALL_RECEIVE_SHORT_LOW: touchCountTEXT2.text = "TEAM RIGHT : Low Short Receive"; break;                                     
            case BALL_RECEIVE_BAD: touchCountTEXT2.text = "TEAM RIGHT : Bad Receive"; break;                                                 
            case BALL_RECEIVE_BAD_LOW: touchCountTEXT2.text = "TEAM RIGHT : Low Bad Receive"; break;                                                 
            case BALL_RECEIVE_BAD_LONG: touchCountTEXT2.text = "TEAM RIGHT : Long Bad Receive"; break;                                                 
            case BALL_TOSS: touchCountTEXT2.text = "TEAM RIGHT : TOSS"; break;                 
            case BALL_UNAVAILABLE: touchCountTEXT2.text = "TEAM RIGHT : UNAVAILABLE"; break;                                                                                
        }       
    }

}

