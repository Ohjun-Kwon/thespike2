using static Constants;

using UnityEngine;
using System;
using TMPro;

public class BallMovement : MonoBehaviour
{
    private movePhysics movePhys;
    private MainControl MainControl;
    private MainSetting MainSetting;
    [SerializeField] public GameObject SystemObject;
    [SerializeField] public float b_H;

    [SerializeField] public TextMeshProUGUI touchCountTEXT1;
    [SerializeField] public TextMeshProUGUI touchCountTEXT2;    
    // Start is called before the first frame update
    void Start()
    {
        movePhys = GetComponent<movePhysics>();
        movePhys.initPhysics();
        MainControl = SystemObject.GetComponent<MainControl>();
        MainSetting = SystemObject.GetComponent<MainSetting>();
        
    }

    void Update() {   
        
    }
    void FixedUpdate() {
        if (Mathf.Abs(transform.position.y - movePhys.getLandBody_Y()) <= 0.001f && Mathf.Abs(movePhys.verticalSpeed) > 0.08f)
        {
            ballHitFloor();
        }
    }
    public void ballReceive(int team) {
        
        movePhys.setVectorByVspeedParabola(NET_X + team * (0.05f + UnityEngine.Random.Range(0.0f,9.0f)),NET_Y,UnityEngine.Random.Range(1.9f,3.2f));
        movePhys.setZDirection(Constants.CENTER);
        MainSetting.setCurrentSituation(SIT_RALLYPLAYING);
        movePhys.startParabola();
        MainControl.addTouchCount(team);
        MainSetting.setCurrentBallType();
        MainControl.commandPlayerMove();
        DEBUG_Text();        
    }
    public void ballSpike(int team) {
        if (UnityEngine.Random.Range(0,9) < 8) {
            movePhys.setVectorByVspeedSpike(NET_X + team * (- 1.5f - UnityEngine.Random.Range(5.0f,12.0f)),movePhys.getLandBody_Y(),UnityEngine.Random.Range(1.1f,2f));
            movePhys.setZDirection(UnityEngine.Random.Range(0,3));
        }
        else {
            movePhys.setVectorByVspeedParabola(NET_X + team * (- 2.05f - UnityEngine.Random.Range(0.0f,6.0f)),NET_Y,UnityEngine.Random.Range(0.09f,0.2f));
            movePhys.setZDirection(UnityEngine.Random.Range(0,3));
        }
        movePhys.startParabola();
        MainControl.addTouchCount(team);
        MainSetting.setCurrentBallType();
        MainControl.commandPlayerMove();
        DEBUG_Text();        
    }

    public void ballToss(int team) {
        if (UnityEngine.Random.Range(0,6) < 0){
            movePhys.setVectorByVspeedParabola(NET_X + team * (2.5f + UnityEngine.Random.Range(0.0f,2.0f)),NET_Y,2.35f);
            movePhys.setZDirection(Constants.CENTER);
            MainControl.setCurrentSituation(STRATEGY_OPEN);
        }
        else{
            float z_dir = team == TEAM_LEFT ? Z_RIGHT : Z_LEFT;
            movePhys.setVectorForQuickAttack( NET_X + team * NEARFRONT, MainControl.mbY ,z_dir, MainControl.quickTime);
            MainControl.setCurrentSituation(STRATEGY_QUICK);
        }
        MainSetting.setCurrentSituation(SIT_RALLYPLAYING); 
        movePhys.startParabola();
        MainControl.addTouchCount(team);
        MainSetting.setCurrentBallType();
        MainControl.commandPlayerMove();
        DEBUG_Text();        
    }    

    public void ballHitFloor(){
        Debug.Log("Hit Floor");
        movePhys.setVector(movePhys.getVerticalFlippedDirection(),movePhys.getSpeed()*0.8f);
        movePhys.startParabola();
        MainControl.resetTouchCount();
    }
    public void ballHitNet() {
         movePhys.setVector(movePhys.getDirection()+180.0f,movePhys.getSpeed()*0.3f);
         movePhys.startParabola();
        MainSetting.setCurrentBallType();
        MainControl.commandPlayerMove();         
     }
    public void ballInCenter(float dir, float spd) {
            movePhys.setVector(dir,spd);
            movePhys.setZDirection(Constants.CENTER);
            movePhys.startParabola();
            MainControl.setTouchCount(TEAM_LEFT , 0);
            MainControl.setTouchCount(TEAM_RIGHT , 0);
            MainControl.commandPlayerMove();
    }
    public void DEBUG_Text() {
        switch(MainSetting.getCurrentBallType(TEAM_LEFT)) {
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
        }
        switch(MainSetting.getCurrentBallType(TEAM_RIGHT)) {
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
        }       
    }
    
     void OnTriggerEnter(Collider other) {
         if (other.tag == "Net") {
                 ballHitNet();
         }
     }
}
