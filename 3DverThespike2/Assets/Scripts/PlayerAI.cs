
using UnityEngine;
using static Constants;
using playerStatsNameSpace;

public class PlayerAI : MonoBehaviour
{

    //[SerializeField] public GameObject  lineX;
    [SerializeField] public GameObject SystemObject;
    private MainControl mainControl;
    private MainSetting mainSetting;
    private PlayerSetting playerSetting;

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
        mainControl = SystemObject.GetComponent<MainControl>();
        mainSetting = SystemObject.GetComponent<MainSetting>();
        playerSetting = GetComponent<PlayerSetting>();
    }

    public (float x , float z) getPlayerPlace(int touchCount , int rotation){
        
        float zPos , xPos;
        int team = GetComponent<PlayerSetting>().getTeam();
        int position = GetComponent<PlayerSetting>().getPosition();
        int rotationPlace = GetComponent<PlayerSetting>().getRotation();
        int now_rot = (rotationPlace + rotation) % 4;

        bool ourBall = mainControl.isBallOur(team);
        
        (xPos , zPos) = getOriginalPlace(now_rot,team);
        if (mainSetting.getCurrentSituation() == SIT_SERVERGO)
        {
            if (gameObject == mainControl.nowServePlayer) {
                xPos = NET_X + team * SERVERPOS;
                zPos = Z_CENTER;
            }
            return (xPos,zPos);
        }

        bool isServe = mainSetting.isNowServe();
        bool goBlock = true;
        if (isServe) {
            if (mainControl.getNowServer().GetComponent<PlayerSetting>().getTeam() != team) 
                goBlock = false;
        }
        if (!isFront(now_rot)) goBlock = false;
        
        if (ourBall) {
            if (goBlock || playerSetting.getPlayerAction() == ACTION_BLOCKJUMP) // ?????? ??????
            {
                (xPos,zPos) = BlockPosition(team, now_rot,xPos,zPos);
            }
            else {
                switch(position)
                {
                    case Constants.SETTER: (xPos,zPos) = setterMovePosition(team,now_rot,xPos,zPos); break;
                    case Constants.SPIKER: (xPos,zPos) = spikerMovePosition(team,now_rot,xPos,zPos); break;          
                    case Constants.BLOCKER: (xPos,zPos) = blockerMovePosition(team,now_rot,xPos,zPos); break;          
                }
            }
            
        }
        else {
            if (goBlock || playerSetting.getPlayerAction() == ACTION_BLOCKJUMP) if (isFront(now_rot)) (xPos,zPos) = BlockPosition(team, now_rot,xPos,zPos);
        }
        //??????????????? ?????? ?????? ?????? ????????????
  
        //?????? ???????????? ?????? ?????? ?????? ????????????.
        return (xPos,zPos);
    }


    private (float xPos,float zPos) BlockPosition(int team , int now_rot,float originalxPos , float originalzPos){
        float xPos;
        float zPos;
        
        if (playerSetting.getBlockFollowZ() == NOBLOCK_Z) { //?????? ????????? ??? ??????. (????????? ????????? ??? ????????? ??????)
            xPos = originalxPos;
            zPos = originalzPos;
        }
        else if (playerSetting.getBlockFollowZ() == NOMOVE_Z) { // ????????? ?????? , ?????? ???????????? ?????? ??????
            xPos = NET_X + team*NEARLIMIT;
            zPos = originalzPos;
        }
        else {    
            xPos = NET_X + team*NEARLIMIT;
            zPos = playerSetting.getBlockFollowZ();
        }
        
        return (xPos,zPos);
    }



    /// <summary>
    /// ??????????????? ??????????????? ???????????? ???????????? ??????
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
        
        if (mainControl.getTouchCount(team) >= 1) {
            Vector3 target = playerSetting.getTarget();
            xPos = target.x;
            zPos = target.z;
        }  
        
        return (xPos,zPos);
    }    
    private (float xPos,float zPos) setterMovePosition(int team, int now_rot , float x ,float z) {
        float xPos = x;
        float zPos = z;
        
        if (mainControl.getTouchCount(team) == 0) // ????????? ?????? ???
        {
            xPos = NET_X + team * MIDFRONT;
            zPos = 0.0f;
        }
        return (xPos,zPos);
    }
    private (float xPos,float zPos) spikerMovePosition(int team, int now_rot , float x ,float z) {
        float xPos = x;
        float zPos = z;
        
        if (mainControl.getTouchCount(team) == 1)
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

        if (team == TEAM_LEFT) {
            if (now_rot <= 1) zPos = Z_RIGHT;
            else zPos = Z_LEFT;
        }else {
            if (now_rot <= 1) zPos = Z_LEFT;
            else zPos = Z_RIGHT;            
        }
        
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
    /// ?????? ????????? ?????? ????????? ????????? ???????????? ????????? ?????? ????????????.
    /// jump_type??? ????????? ?????? ??????, jump_delay??? ????????? ??? ?????? ????????? ?????? ?????? ?????????.
    /// ??????????????? ?????? ?????? ?????? ??????, ?????? ????????? ????????????
    /// Ball Type?????? ?????? ?????? ???????????? ??? ?????? ????????? ????????? ????????????.
    /// </summary>
    /// <param name="jump_type"></param>
    /// <param name="jump_delay"></param>
    /// <param name="isLowBall"></param>
    public void getJumpDataOfSecondBall(ref float jump_type,ref float jump_delay, bool isLowBall) {
        int team      = playerSetting.getTeam();
        int ball_type = mainSetting.getCurrentBallType(team);
        
        jump_type = (isLowBall) ? JUMP_NO : JUMP_TOSS; 
        jump_delay = UnityEngine.Random.Range(0.0f,0.8f);
    }
}
