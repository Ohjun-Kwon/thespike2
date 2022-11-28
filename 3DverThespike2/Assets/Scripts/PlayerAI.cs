
using UnityEngine;
using static Constants;
using playerStatsNameSpace;

public class PlayerAI : MonoBehaviour
{

    //[SerializeField] public GameObject  lineX;
    [SerializeField] public GameObject SystemObject;
    private MainControl MainControl;
    private MainSetting MainSetting;
    private PlayerSetting PlayerSetting;

    Transform highXTransform;

    private BoxCollider boxCollider;      
    private ObjectGravity gravityControl;
    private movePhysics movePhys;
    
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        
        movePhys = GetComponent<movePhysics>();
        boxCollider = GetComponent<BoxCollider>();
        gravityControl = GetComponent<ObjectGravity>();
        MainControl = SystemObject.GetComponent<MainControl>();
        MainSetting = SystemObject.GetComponent<MainSetting>();
        PlayerSetting = GetComponent<PlayerSetting>();
    }

    public (float x , float z) getPlayerPlace(int touchCount , int rotation){
        
        float zPos , xPos;
        int team = GetComponent<PlayerSetting>().getTeam();
        int position = GetComponent<PlayerSetting>().getPosition();
        int rotationPlace = GetComponent<PlayerSetting>().getRotation();
        int now_rot = (rotationPlace + rotation) % 4;

        bool ourBall = MainControl.isBallOur(team);
        
        (xPos , zPos) = getOriginalPlace(now_rot,team);
        if (MainSetting.getCurrentSituation() == SIT_SERVERGO)
        {
            if (gameObject == MainControl.nowServePlayer) {
                xPos = NET_X + team * SERVERPOS;
                zPos = Z_CENTER;
            }
            return (xPos,zPos);
        }

        bool isServe = MainSetting.getCurrentSituation() == SIT_SERVERGO || MainSetting.getCurrentSituation() == SIT_SERVERTOSS || MainSetting.getCurrentSituation() == SIT_SERVERWAIT;
        bool goBlock = true;
            if (isServe) {
                if (MainControl.getNowServer().GetComponent<PlayerSetting>().getTeam() != team) 
                    goBlock = false;
            }

        if (ourBall) {
            if (MainSetting.getCurrentBallType(team) == BALL_ATTACK && goBlock) // 공격 수비
            {
                if (isFront(now_rot)) (xPos,zPos) = BlockPosition(team, now_rot,xPos,zPos);
            }
            switch(position)
                {
                    case Constants.SETTER: (xPos,zPos) = setterMovePosition(team,now_rot,xPos,zPos); break;
                    case Constants.SPIKER: (xPos,zPos) = spikerMovePosition(team,now_rot,xPos,zPos); break;          
                    case Constants.BLOCKER: (xPos,zPos) = blockerMovePosition(team,now_rot,xPos,zPos); break;          
                }
        }
        else {
            if (goBlock) if (isFront(now_rot)) (xPos,zPos) = BlockPosition(team, now_rot,xPos,zPos);
        }
        //로테이션에 따른 기본 위치 정해주기
  
        //특정 포지션에 따른 기본 위치 정해주기.
        return (xPos,zPos);
    }


    private (float xPos,float zPos) BlockPosition(int team , int now_rot,float originalxPos , float originalzPos){
        float xPos;
        float zPos;
        
        if (PlayerSetting.getBlockFollowZ() == NOBLOCK_Z) { //블록 안해도 될 경우. (상대가 공격을 못 때리는 경우)
            xPos = originalxPos;
            zPos = originalzPos;
        }
        else if (PlayerSetting.getBlockFollowZ() == NOMOVE_Z) { // 블록을 하되 , 아직 움직이지 않는 경우
            xPos = NET_X + team*NEARLIMIT;
            zPos = originalzPos;
        }
        else {    
            xPos = NET_X + team*NEARLIMIT;
            zPos = PlayerSetting.getBlockFollowZ();
        }
        
        return (xPos,zPos);
    }



    /// <summary>
    /// 플레이어의 로테이션이 전위인지 판단하는 함수
    /// </summary>
    /// <param name="now_rot"></param>
    /// <returns></returns>
    private bool isFront(int now_rot) {
        if (now_rot == 1 || now_rot == 2) return true;
        else return false;
    }       
    private (float xPos,float zPos) blockerMovePosition(int team, int now_rot , float x ,float z) {
        float xPos = x;
        float zPos = z;
        
        if (MainControl.getTouchCount(team) >= 1) {
            Vector3 target = PlayerSetting.getTarget();
            xPos = target.x;
            zPos = target.z;
        }  
        
        return (xPos,zPos);
    }    
    private (float xPos,float zPos) setterMovePosition(int team, int now_rot , float x ,float z) {
        float xPos = x;
        float zPos = z;
        
        if (MainControl.getTouchCount(team) == 0) // 우리팀 볼일 때
        {
            xPos = NET_X + team * MIDFRONT;
            zPos = 0.0f;
        }
        return (xPos,zPos);
    }
    private (float xPos,float zPos) spikerMovePosition(int team, int now_rot , float x ,float z) {
        float xPos = x;
        float zPos = z;
        
        if (MainControl.getTouchCount(team) == 1)
        {
            xPos = NET_X + team * MID;
        }
        
        return (xPos,zPos);
    }
    public (float x, float z) getOriginalPlace(int now_rot,int team){
        /*
          3   2 |   1   0
        0   1   | 2   3
        */
        float zPos = 0 , xPos = 0;
        if (now_rot <= 1) zPos = Z_RIGHT;
        else zPos = Z_LEFT;
        
        switch(now_rot)
        {
            case 0: xPos = NET_X + team * FARBACK; break;
            case 1: xPos = xPos = NET_X + team * NEARBACK; break;
            case 2: xPos = xPos = NET_X + team * FARFRONT; break;
            case 3: xPos = xPos = NET_X + team * MIDBACK; break;
        }

        return (xPos,zPos);
    }




    /// <summary>
    /// 세터 역할을 받은 녀석의 세컨볼 움직임에 필요한 값을 계산한다.
    /// jump_type은 점프를 할지 말지, jump_delay는 얼마나 더 일찍 점프를 할지 등을 정한다.
    /// 플레이어의 토스 스탯 등에 의해, 할지 말지가 결정되며
    /// Ball Type에서 이미 해당 세터에게 이 볼이 낮은지 높은지 결정된다.
    /// </summary>
    /// <param name="jump_type"></param>
    /// <param name="jump_delay"></param>
    /// <param name="isLowBall"></param>
    public void getJumpDataOfSecondBall(ref float jump_type,ref float jump_delay, bool isLowBall) {
        int team      = PlayerSetting.getTeam();
        int ball_type = MainSetting.getCurrentBallType(team);
        
        jump_type = (isLowBall) ? JUMP_NO : JUMP_TOSS; 
        jump_delay = UnityEngine.Random.Range(0.0f,0.8f);
    }
    public void DoReceive() {
        Debug.Log("Receive");
    }

    public void DoSpike() {
        Debug.Log("Spike");
    }
}
