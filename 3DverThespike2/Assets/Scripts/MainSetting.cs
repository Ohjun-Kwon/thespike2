
using UnityEngine.UI;
using UnityEngine;
using static Constants;
using System.Collections.Generic; 
using static PhysCalculate;



public class MainSetting : MonoBehaviour
{


    private int gameCurrentSituation;
    private int[] currentBallType = new int[2] {0,0}; // 현재 공의 상태 
    private GameObject[] currentSetter = new GameObject[2] {null, null}; // 이번 상황에서 누가 세터인지

    private int[] teamScore = new int[2] {0,0};
    private List<int> teamScoreLog = new List<int>();
    [SerializeField]public GameObject Ball;
    
    private MainControl MainControl;

    void Start(){
        gameCurrentSituation = SIT_SERVERGO;
        MainControl = GetComponent<MainControl>();
    }
    void FixedUpdate() {
    }
    public void addTeamScore(int team){
        if (team == TEAM_LEFT){
            teamScore[0] += 1;
        }
        else {
            teamScore[1] += 1;
        }
        addTeamScoreLog(team);
    }
    public void addTeamScoreLog(int team){
        teamScoreLog.Add(team); // 로그에 추가한다.
    }
    public int getLastScoreTeam(){
        if (teamScoreLog.Count == 0) {
            return TEAM_NO;
        }
        return teamScoreLog[teamScoreLog.Count - 1];
    }
    public int getTeamScore(int team) {
        if (team == TEAM_LEFT) return teamScore[0];
        else return teamScore[1];
    }

    /// <summary>
    /// 현재 게임의 Situation을 상수값으로 정함. SIT_* 상수값 참고.
    /// </summary>
    /// <param name="situation"></param>
    public void setCurrentSituation(int situation){
        gameCurrentSituation = situation;
    }

    /// <summary>
    /// 현재 게임의 Situation을 상수값으로 반환. SIT_* 상수값 참고.
    /// 랠리 중인지 , 서브 준비 중인 지 등을 반환. 
    /// </summary>
    /// <returns> int 상수값 (현재 경기의 상황 SIT_*) </returns>
    public int getCurrentSituation(){
        return gameCurrentSituation;
    }
    /// <summary>
    /// 현재 경기가 랠리중인지 아닌지 booltype으로 반환
    /// 서버가 볼을 때린 상황 / 랠리 중인 상황을 제외하고는 false이다.
    /// </summary>
    /// <returns>bool 경기 랠리 여부</returns>
    public bool isGameRallying(){
        if (gameCurrentSituation == SIT_RALLYPLAYING ) return true;
        if (gameCurrentSituation == SIT_SERVERHIT) return true;
        return false;
    }
    /// <summary>
    /// 현재 Ball이 어떤지 각 팀의 입장에서 int형 상수값으로 반환해준다. setCurrentBallType에서 설정해주게 된다.
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public int getCurrentBallType(int team) { 
        if (team == TEAM_LEFT) return currentBallType[0];
        else return currentBallType[1];    
    }


    
    /// <summary>
    /// 현재 Ball의 움직임이 어떤지 각 팀의 입장에서 계산 (checkCurrentBallType)하는 함수를 이용하여
    /// currentBallType에 저장한다.
    /// </summary>
    public void setCurrentBallType() {
        currentBallType[0] = checkBallTypeSetSetter(TEAM_LEFT);
        currentBallType[1] = checkBallTypeSetSetter(TEAM_RIGHT);
    }



    /// <summary>
    /// 현재 Ball의 움직임이 어떤지 각 팀의 입장에서 int형 BALL_ 상수값으로 반환해준다.
    /// 해당 함수에서 (touchCount==1) 일 경우, 이번 상황에서 어떤 Player가 세터 역할을 할지도 정해준다.(currentSetter)
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public int checkBallTypeSetSetter(int team) {

        movePhysics ballPhys = Ball.GetComponent<movePhysics>();
        int touchCount = MainControl.getTouchCount(team); // 터치 카운트

        bool isPassingCourt = Mathf.Sign(ballPhys.startPos.x - NET_X) != Mathf.Sign(ballPhys.endPos.x - NET_X);
        bool isOverNet = false;
        bool isOurBall = false;
        int  whoseBall    = ballPhys.endPos.x < NET_X ? TEAM_LEFT : TEAM_RIGHT;
        float ballMaxY = ballPhys.getFlightMaxY(ballPhys.verticalSpeed);
        float ballFlightTime = ballPhys.getFlightTime();
        float verticalSpeed = ballPhys.verticalSpeed;
        float setterY , setterJumpY , ballXOnSetterY , ballXOnSetterJumpY;
        float limit_x = (team == TEAM_LEFT) ? LEFT_LIMIT : RIGHT_LIMIT;
        int setter_id = (team == TEAM_LEFT) ? 1  : 5 ;
        int teamId = (team == TEAM_LEFT) ? 0 : 1;

        movePhysics setterPhys = MainControl.Players[setter_id].GetComponent<movePhysics>();
        PlayerSetting setterSet = MainControl.Players[setter_id].GetComponent<PlayerSetting>();

        setterY = setterPhys.getLandBody_Y() + setterPhys.getHeight()*0.5f; // 세터 높이
        setterJumpY = setterY + getMaxHeightBySpeed(setterSet.Status.getJump() * JUMP_TOSS); // 세터 점프 높이
        ballXOnSetterY = ballPhys.getParabolaXbyY(setterY,true); // 세터 높이를 지나는 볼의 x좌표
        ballXOnSetterJumpY = ballPhys.getParabolaXbyY(setterJumpY,true); // 세터 점프 높이를 지나는 볼의 x좌표

        if (isPassingCourt) { // 궤도가 코트를 가로지르는 궤도
            isOverNet =  (ballPhys.getParabolaYbyX(NET_X)-ballPhys.getHeight()/2 > NET_Y); // 네트 높이보다 볼이 지나가는 높이가 더 높을 경우. (즉 코트 넘어로 넘어가는 볼)
            if (isOverNet && (team == whoseBall)) isOurBall = true; 
            //코트를 넘어 우리 쪽 볼에 떨어져도 우리꺼일 경우 우리꺼.
        }   
        else{ // 한 코트 내에서 오르고 떨어지는 코트
            if (team == whoseBall) // 우리 코트에 떨어지면
                isOurBall = true; // 우리꺼 맞아요.
        }
        if (touchCount == 0)
        {
            if ( !isOurBall ) return BALL_BALLWAIT;
            if ( isPassingCourt && !isOverNet ) return BALL_BALLWAIT;
            if (ballFlightTime > BALL_FREEBALL_TIME * playSpeed) // 체공 시간이 길 경우
            {
                if ( Mathf.Abs(ballXOnSetterY - NET_X) > FARFRONT ) // 거리가 먼 경우
                    return BALL_FREEBALL;
                else
                    return BALL_FREEBALL_SHORT;
            }
            else // 체공 시간이 짧을 경우
            {
                if (verticalSpeed > 0) // 볼 초기 속도가 위를 향할 경우
                {
                    if ( Mathf.Abs(ballXOnSetterY - NET_X) > FARFRONT ) // 거리가 먼 경우
                        return BALL_FAINT_LONG;
                    else
                        return BALL_FAINT_SHORT;
                }
                else
                    return BALL_ATTACK; // 공격으로 인식
            }
        }
        else if (touchCount == 1)
        {
            float setterMaxY = 0.0f; // 세터의 높이가 될 기준점. (점프를 못 할 경우. 할 경우에 따라 달라짐.)
            bool  isLowBall = false; // 낮은 볼인지 판단.
            if (!checkSetterAvailable(ref setterMaxY , ref isLowBall , MainControl.Players[setter_id]))
            {
                Debug.Log("세터는 못가");
                //만약 도달 못하면..
                currentSetter[teamId] = MainControl.getNearestPlayer();
                if (!checkSetterAvailable(ref setterMaxY , ref isLowBall , currentSetter[teamId])) // 낙하지점에서 가장 근처인 선수 데려와서 재계산. 얘는 안되어도 어쩔 수 없음.
                    Debug.Log("세터 아닌 놈도 못가");
            }
            else 
                currentSetter[teamId] = MainControl.Players[setter_id];
                //currentSetter 필요 없을 경우 추후 제거.
            
            ballXOnSetterY = ballPhys.getParabolaXbyY(setterMaxY,true); // 세터 높이를 지나는 볼의 x좌표

            if ((ballPhys.getParabolaYbyX(limit_x)-ballPhys.getHeight()/2 > setterMaxY) && isPassingCourt) // 네트를 지나며 볼높이가 세터 점프보다도 높을 경우.
                return BALL_RECEIVE_BAD_LONG;

            if (isLowBall) // LOW가 붙은 리시브들은 세터가 점프토스 불가.
            {
                if (Mathf.Abs( ballXOnSetterY - NET_X ) < FARFRONT) //위치가 좋으면
                    return BALL_RECEIVE_GOOD_LOW;
                else if (Mathf.Abs( ballXOnSetterY - NET_X ) < MID) 
                    return BALL_RECEIVE_SHORT_LOW;
                else    
                    return BALL_RECEIVE_BAD_LOW;
            }
            else
            {
                if (Mathf.Abs( ballXOnSetterY - NET_X ) < FARFRONT) //위치가 좋으면
                    return BALL_RECEIVE_GOOD;
                else if (Mathf.Abs( ballXOnSetterY - NET_X ) < MID)
                    return BALL_RECEIVE_SHORT;
                else
                    return BALL_RECEIVE_BAD;
            }
        }
        else
            return BALL_TOSS;
        
    }



    /// <summary>
    /// 세터가 도달 가능한지 불가능한지 판단하는 함수.
    /// 단순한 시간 뿐만 아니라 , 볼의 높이를 보고 세터가 점프토스를 하거나 스탠딩 토스를 하는 등의
    /// 방법을 쓰더라도 도달 가능한지 없는지를 판단한다.
    /// 레퍼런스 변수에 setter의 최고 도달점과 Ball이 해당 세터에게 높은지 낮은지 여부도 계산 된다.
    /// </summary>
    /// <param name="setterMaxY">세터의 점프 후 최고 도달점 레퍼런스 변수</param>
    /// <param name="isLowBall">세터에게 볼의 높이가 높은지 낮은지 레퍼런스 변수</param>
    /// <param name="setter">세터의 GameObject id </param>
    /// <returns></returns>
    public bool checkSetterAvailable(ref float setterMaxY, ref bool isLowBall , GameObject setter) {
        // 세터가 점프해야 할 지, 그냥 가도 되는 지 확인하는 용도의 함수
        movePhysics ballPhys = Ball.GetComponent<movePhysics>();
        movePhysics setterPhys = setter.GetComponent<movePhysics>();
        PlayerSetting setterSet = setter.GetComponent<PlayerSetting>();
        PlayerMove setterMove = setter.GetComponent<PlayerMove>();

        if (MainControl.getLastTouchPlayer() == setter) return false; // 자신이 마지막으로 터치한 사람일 경우 불가능.

        float setterY = setterPhys.getLandBody_Y() + setterPhys.getHeight()*0.5f + ballPhys.getHeight()*0.5f; // 세터 높이
        float setterJumpY = setterY + getMaxHeightBySpeed(setterSet.Status.getJump() * JUMP_TOSS); // 세터 점프 높이
        float ballXOnSetterY = ballPhys.getParabolaXbyY(setterY,true); // 세터 높이를 지나는 볼의 x좌표
        float ballXOnSetterJumpY = ballPhys.getParabolaXbyY(setterJumpY,true); // 세터 점프 높이를 지나는 볼의 x좌표
        float ballMaxY = ballPhys.getFlightMaxY(ballPhys.verticalSpeed);

        if (ballMaxY >= setterJumpY)  // 세터 점프 토스 높이보다 볼이 높게 올라 갈 경우.
        {
            //IsArrivedInTime
            if (setterMove.IsArrivedInTime( ballXOnSetterJumpY , ballPhys.getRemainTimeToParabolaX(ballXOnSetterJumpY) )) // 점프 한 높이까지 도달할 수 있는지
            {
                setterMaxY = setterJumpY; // 도달할 수 있으니 문제 없음.
                isLowBall = false;
            }
            else if (setterMove.IsArrivedInTime( ballXOnSetterY , ballPhys.getRemainTimeToParabolaX(ballXOnSetterY) ) ) // 점프 안한 높이의 좌표까지는 도달할 수 있는 지. 
            {
                setterMaxY = setterY;
                isLowBall = true; 
            }
            else {
                return false;
            }
        }
        else{
            isLowBall = true;
            setterMaxY = setterY;
            if (!setterMove.IsArrivedInTime(ballXOnSetterY , ballPhys.getRemainTimeToParabolaX(ballXOnSetterY)))
                return false; // 도달 불가.
        }
        return true;
    }
}
