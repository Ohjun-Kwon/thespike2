
using UnityEngine;
using static Constants;
using playerStatsNameSpace;

/// <summary>
/// Player의 상태 등을 저장하는 Component
/// </summary>
public class PlayerSetting : MonoBehaviour
{

    [SerializeField] public GameObject SystemObject;
    private MainControl mainControl;
    private int position;
     [SerializeField] public string positionName;
    private int rotationPlace;
    private int team;
    Transform highXTransform;
    private BoxCollider2D boxCollider;      
    public playerStats Status;
    private Vector3 target;
    [SerializeField] public int playerAction;
    public bool control = false;

    [SerializeField] public Vector3 goal;
    private float goalJump;
    private float goalDelay;
    [SerializeField]private float blockFollowZ; // 블로킹할 때 따라가는 Zㄱ밧.
    
    [SerializeField] public bool checkB; 
    [SerializeField] public float maxH; 
    
    public MainSetting mainSetting;
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        mainControl = SystemObject.GetComponent<MainControl>();
        mainSetting = SystemObject.GetComponent<MainSetting>();
    }
    public void playerCreate(float strength, float jump , float speed , float defense,float swingTime = 0.1f , float receiveTime = 0.1f , float tossTime = 0.1f , float reactSpeed = 0.25f ){
        Status = new playerStats(strength,jump,speed,defense,swingTime,receiveTime,tossTime,UnityEngine.Random.Range(0.2f,0.5f)*0f);
    }
    public playerStats getStatus(){
        return Status;
    }

    /// <summary>
    /// 속공의 x,y,z값을 정한다.
    /// 해당 x,y,z값은 , mainControl isPlayerInTime에서 세터의 점프와 함께 정해진다.
    /// </summary>
    /// <param name="tx">속공의 타점 x</param>
    /// <param name="ty">속공의 타점 y</param>
    /// <param name="tz">속공의 타점 z</param>
    public void setTarget(float tx,float ty,float tz){
        target = new Vector3(tx,ty,tz);
    }
    public void setGoal(float x,float y, float z, float goalJump,float goalDelay = 0.0f) {
        goal = new Vector3(x,y,z);
        this.goalDelay = goalDelay;
        this.goalJump = goalJump;
    }
    public Vector3 getGoal(){
        return goal;
    }
    public float getGoalJump(){
        return goalJump;
    }
    public float getGoalDelay(){
        return goalDelay;
    }
    /// <summary> 선수를 Control 하는지 여부 설정. ( True False ) </summary>
    public GameObject setControl(bool con){
        control = con;
        return gameObject;
    }
    public string getPositionName() {
        return positionName;
    }
    public Vector3 getTarget(){ return target; }
    public void setPosition(int pos) { 
        position = pos; 
        if (team == TEAM_LEFT) positionName = "왼쪽";
        else positionName = "오른쪽";
        switch(position) {
            case LIBERO: positionName += "리베로"; break;
            case SPIKER:positionName += "스파이커"; break;
            case BLOCKER:positionName += "미들블로커"; break;
            case SETTER: positionName += "세터"; break;
        }    
    }
    public void setRotation(int rot) { rotationPlace = rot; }
    public int getRotation() { return rotationPlace; }
    public int getPosition(){ return position; }
    public int getPlayerAction() { return playerAction;}
    public void setPlayerAction(int _playerAction) { playerAction = _playerAction; }
    public void setTeam(int _team) { team = _team; }
    public int getTeam(){ return team; }
    public bool isControl() { return control;}

    public float getBlockFollowZ() { return blockFollowZ;}

    public void resetBlockFollowZ() {
        //도달 안해.
        blockFollowZ = INF;
    }
    public void setBlockFollowZ(float z) {
        if (z != NOBLOCK_Z && z != NOMOVE_Z) {
            if (z < Z_RIGHT*0.5f) z = Z_RIGHT;
            else if (z > Z_LEFT*0.5f) z = Z_LEFT;
            else z = Z_CENTER;
            
        }
        int rotationPlace = mainSetting.getRotation(team);
        int now_rot = getRotation();
        
        now_rot = (now_rot + rotationPlace) %4;
        if (now_rot == 1) { // RIGHT
            PlayerSetting otherBlocker = mainControl.getPlayersByRot(getTeam(),2).GetComponent<PlayerSetting>();
            float otherZ = otherBlocker.getBlockFollowZ();

            if (otherZ == z) {
                if (z == Z_CENTER) blockFollowZ = Z_LEFT;
                else blockFollowZ = Z_CENTER;
            }
            else blockFollowZ = z;
        }
        else{ 
            PlayerSetting otherBlocker = mainControl.getPlayersByRot(getTeam(),1).GetComponent<PlayerSetting>();
            float otherZ = otherBlocker.getBlockFollowZ();

            if (otherZ == z) {
                if (z == Z_CENTER) blockFollowZ = Z_RIGHT;
                else blockFollowZ = Z_CENTER;
            }
            else blockFollowZ = z;
        }
    }
    
        
}