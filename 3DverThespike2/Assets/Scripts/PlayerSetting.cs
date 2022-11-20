
using UnityEngine;
using static Constants;
using playerStatsNameSpace;

/// <summary>
/// Player의 상태 등을 저장하는 Component
/// </summary>
public class PlayerSetting : MonoBehaviour
{

    [SerializeField] public GameObject SystemObject;
    private MainControl MainControl;
    private int position;
     [SerializeField] public string positionName;
    private int rotationPlace;
    private int team;
    Transform highXTransform;
    private BoxCollider2D boxCollider;      
    public playerStats Status;
    private Vector3 target;
    private int playerAction;
    public bool control = false;

    
    [SerializeField] public bool checkB; 
    [SerializeField] public float maxH; 
    
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        MainControl = SystemObject.GetComponent<MainControl>();
    }
    public void playerCreate(float strength, float jump , float speed , float defense){
        Status = new playerStats(strength,jump,speed,defense);
    }
    public playerStats getStatus(){
        return Status;
    }

    /// <summary>
    /// 속공의 x,y,z값을 정한다.
    /// 해당 x,y,z값은 , MainControl isPlayerInTime에서 세터의 점프와 함께 정해진다.
    /// </summary>
    /// <param name="tx">속공의 타점 x</param>
    /// <param name="ty">속공의 타점 y</param>
    /// <param name="tz">속공의 타점 z</param>
    public void setTarget(float tx,float ty,float tz){
        target = new Vector3(tx,ty,tz);
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


    
        
}