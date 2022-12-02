using static Constants;
using static PhysCalculate;

using UnityEngine;
using System;

public class movePhysics : MonoBehaviour
{
    // [SerializeField] public GameObject  lineX;
    // [SerializeField] public GameObject  lineY2;
    // [SerializeField] public GameObject  lineY3;
    
    // [SerializeField] public GameObject  dBall;
    // [SerializeField] public GameObject  mainSystem;
    // Transform lineYTransform;
    // Transform highXTransform;
    // Transform highYTransform;
    // Transform highY2Transform;
    // Transform drawBallTransform;


    private BoxCollider boxCollider;      
    public float verticalSpeed = 0f;
    public float horizontalSpeed = 0f;    
    public float depthSpeed = 0f;

    public float direction; // 라디안각으로 저장함.
    public float speed;
    
    private float landY;

    
    [SerializeField]public Vector3 startPos;
    [SerializeField]public Vector3 endPos;
    [SerializeField]public MainControl mainControl;
     [SerializeField]private float mainFlow;
     [SerializeField]private float startTime;
     [SerializeField]public float flightTime;
    private float flightMaxHeight;
    private float Height;

    /// <summary>
    /// 해당 메서드는, TimeTrigger에서 실행한다.
    /// </summary>
    public void PhysicalFixedUpdate(float time) 
    {
        //timeFlow
        mainFlow = time;
        //moveParabola
        moveParabola(startPos , endPos , getCurrentTime());    
    }
    public void initPhysics(){
        setLandY(); // 본인의 캐릭터 크기에 맞게 가장 낮은 위치의 좌표.
        setVector(90.0f,0.001f);
        depthSpeed = 0.0f;
        startParabola();       
    }
    public void setLandY(){
        boxCollider = GetComponent<BoxCollider>();
        Height = boxCollider.bounds.size.y;
        landY = 0.0f + Height/2;
    }
    public float getCurrentTime() {
        return mainFlow - startTime;
    }
    public void startParabola() {
        startPos = transform.position;

        // 땅 아래에서 시작하는 경우가 있으면 , 물리 연산이 꼬이게 됨.
        if (startPos.y < landY)
            startPos.y = landY;// = new Vector3(transform.position.x , landY , transform.position.z);

        flightMaxHeight = getFlightMaxHeight(verticalSpeed);
        flightTime = getFlightTime();
        endPos = getParabolaEnd( flightTime );
        resetStartTime();

        return;
    }
    public float getLandY() {
        return landY;    
    }
    /// <summary>
    /// 땅 위에 있을 때의 Y값.
    /// </summary>
    /// <returns></returns>
    public float getLandBody_Y() {
        return landY;
    }
    public float getLandHead_Y() {
        return landY + Height / 2;
    }        
    public void resetParabola() {
        startPos = transform.position;
        endPos = transform.position;
        resetStartTime();
    }
    
    /// <summary>
    /// 움직임 멈춤. 그 자리 그대로 떨어짐.
    /// </summary>
    public void stopParabola() {
        setVector(0f,0f);
        depthSpeed = 0.0f;
        startParabola();
    }
    /// <summary>
    /// 시작->끝 , 시간에 따른 좌표를 정해줘.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="time"></param>
    public void moveParabola(Vector3 start ,Vector3 end , float time) {
        var t = time / flightTime;
        var mid = Vector3.Lerp(start, end ,t);
        mid.y = start.y  + (mid.x - start.x)*(verticalSpeed / horizontalSpeed ) - 0.5f*gravityScale*Mathf.Pow( (mid.x - start.x) / horizontalSpeed, 2);
        transform.position = new Vector3(mid.x , mid.y , mid.z);            
    }    
    public float getHorizontalSpeed() {
        return horizontalSpeed;
    }
    public float getVerticalSpeed() {
        return verticalSpeed;
    }
    public float getDepthSpeed(){
        return depthSpeed;
    }
    public Vector3 getCurrentPosition() {
        return getPositionByTime( getCurrentTime());
    }
    public Vector3 getPositionByTime(float time) {

        Vector3 start = startPos;
        Vector3 end = endPos;

        
        var t = time / flightTime;
        var mid = Vector3.Lerp(start, end ,t);
        mid.y = start.y  + (mid.x - start.x)*(verticalSpeed / horizontalSpeed ) - 0.5f*gravityScale*Mathf.Pow( (mid.x - start.x) / horizontalSpeed, 2);
        return new Vector3(mid.x , mid.y , mid.z);                    
    }
    public Vector3 getParabolaByTime( float time) {
        var t = time / flightTime;
        Vector3 start = startPos;
        Vector3 end = endPos;
        var mid = Vector3.Lerp(start, end ,t);
        mid.y = start.y + (mid.x - start.x)*(verticalSpeed / horizontalSpeed ) - 0.5f*gravityScale*Mathf.Pow( (mid.x - start.x) / horizontalSpeed, 2);
        return new Vector3(mid.x , mid.y , mid.z);
    }    

    public void resetStartTime(){
        startTime = mainFlow;
        return;
    }
    public void setVector(float Dir , float Spd) {
        direction = Dir * (Constants.Pi/180);  // 라디안 각 변환

        setHorizontal(Spd * Mathf.Cos(direction));
        verticalSpeed = Spd * Mathf.Sin(direction);
        return;
    }
    public float getCurrentDirection(){
        Vector3 curPos = getPositionByTime(getCurrentTime());
        Vector3 pastPos = getPositionByTime(Mathf.Max( 0f ,getCurrentTime() - playSpeed));

        float hs = (curPos.x - pastPos.x)*100f;
        float vs = (curPos.y - pastPos.y)*100f; // 값이 너무 작으면 Atan에서 인식을 못함. 
        return getDirectionByVSHS(vs,hs);
    }
    public float getDirectionByVSHS(float vs , float hs) {
        hs = Mathf.Max(0.001f , Mathf.Abs(horizontalSpeed)) * Mathf.Sign(horizontalSpeed);        
        float curDir = Mathf.Atan(vs / hs) * (180 / Constants.Pi);

        if (hs < 0 && vs > 0) curDir += 180;
        if (vs < 0 && hs < 0) curDir += 180;
        return curDir = (curDir + 360f)%360f;
    }
    public float getSpeed(){
        //verticalSpeed는 시작 속도이다.
        if (horizontalSpeed == 0.0f) return verticalSpeed;
        var dir = Mathf.Atan(verticalSpeed / horizontalSpeed);
        return Mathf.Abs(verticalSpeed / Mathf.Sin(dir));
    }
    public float getCurrentSpeed(){
        //verticalSpeed는 시작 속도이다.
        Vector3 curPos = getPositionByTime(getCurrentTime());
        Vector3 pastPos = getPositionByTime(getCurrentTime() - playSpeed);

        return Mathf.Sqrt(Mathf.Pow((curPos.x - pastPos.x)/playSpeed , 2) +Mathf.Pow((curPos.y - pastPos.y)/playSpeed , 2));
    }    
    public void setZDirection(int place){
        float nowZ = getZ();
        float targetZ = 0.0f;
        if (place == Constants.LEFT) 
            targetZ = Constants.Z_LEFT;            
        else if (place == Constants.RIGHT)
            targetZ = Constants.Z_RIGHT;
        else if (place == Constants.CENTER) 
            targetZ = 0.0f;
        depthSpeed = (targetZ - nowZ) / getFlightTime();
        return;
    }
    public float getSpeedByVSHS(float vs, float hs) {
        return Mathf.Sqrt(Mathf.Pow(vs,2)+ Mathf.Pow(hs,2));
    }
    public void setVectorByHighestY(float desX, float desY, float dY){
        Vector3 curPos = getCurrentPosition();
        var highestY = curPos.y + dY;
        var h = highestY - curPos.y;
        var H = highestY - desY;
        var dx = desX - curPos.x;
        float t = Mathf.Sqrt(2*H/gravityScale) + Mathf.Sqrt(2*h/gravityScale);

        if (highestY < desY) {
            mainControl.showDebug("Error on setVectorByHighestY ");
            return;
        }
        verticalSpeed = Mathf.Sqrt(2*gravityScale*h);
        setHorizontal(dx/t);
        direction = getDirectionByVSHS(verticalSpeed,horizontalSpeed);
        speed = getSpeedByVSHS(verticalSpeed,horizontalSpeed);
        
        setVector(direction ,speed);
    }
    public void setVectorForQuickAttack(float desX , float desY , float desZ ,float time)
    {
        //속공을 위해 필요한 스피드 계산
        // 주어진 시간 내로, 주어진 좌표값까지 이동시키는 속도를 지정.
        Vector3 curPos = getCurrentPosition();
        var dx = desX - curPos.x;
        var dy =  desY - curPos.y;
        var dz = desZ - curPos.z;

        setHorizontal(dx/time);
        verticalSpeed = dy/time + gravityScale*time/2;
        depthSpeed = dz/time;
        direction = getDirectionByVSHS(verticalSpeed,horizontalSpeed);// Mathf.Atan(verticalSpeed / horizontalSpeed);
        return;
    }
    public void setHorizontal(float spd) {
        horizontalSpeed = spd;
        horizontalSpeed = Mathf.Max(0.001f , Mathf.Abs(horizontalSpeed)) * Mathf.Sign(horizontalSpeed); // 가로속도가 0이면 , 1/  0이되어 ㅈ되는 경우가 많음. 그래서 최솟값으로 설정
    }
    public void setVectorByVspeedParabola(float desX, float desY, float vspeed){ 
        // 포물선을 그리는 궤도로 목표 좌표까지 이동시키는 속도.
        // 여기서 포물선을 위한 최고점은, 현재 기준의 좌표 y값에, 주어진 vspeed 속도만큼 더 올라간 후의 최대 높이이다.

        Vector3 curPos = getCurrentPosition();
        var highestY = curPos.y + Mathf.Pow(vspeed , 2) / (2 * gravityScale);

        var h = highestY - curPos.y;
        var H = highestY - desY;
        var dx = desX - curPos.x;

        if (highestY < desY) {
            // 최대 높이까지 올라갔음에도, 목표 y값보다 작아질 경우, 계산이 불가하다.
            return;
        }
        float t = Mathf.Sqrt(2*H/gravityScale) + Mathf.Sqrt(2*h/gravityScale);

        verticalSpeed = vspeed;
        horizontalSpeed = dx /  t ;
        direction = getDirectionByVSHS(verticalSpeed,horizontalSpeed);
        speed = getSpeedByVSHS(verticalSpeed,horizontalSpeed);
        setVector(direction,speed);
    }
    public void setVectorByVspeedSpike(float desX, float desY , float vspeed){ 
        // 포물선을 안그리고 바로 내려 찍는 궤도
        var verticalSpeed = -vspeed;
        Vector3 curPos = getCurrentPosition();
        var distance =  desX - curPos.x;
        var h = curPos.y - desY;

        var t = (-vspeed  + Mathf.Sqrt(Mathf.Pow(vspeed , 2) + 2 * h * gravityScale)) / gravityScale;
        horizontalSpeed = distance / t;
        Debug.Log($"horizontalSpeed {horizontalSpeed}");
        Debug.Log($"Time{t}");
        direction = getDirectionByVSHS(verticalSpeed,horizontalSpeed);
        speed = getSpeedByVSHS(verticalSpeed,horizontalSpeed);
        Debug.Log($"direction {direction} speed {speed}");
        setVector(direction ,speed);
    }

    public void moveLinear(Vector3 Direction , float Speed ) {
        startPos += Direction * Speed * Constants.playSpeed;
        endPos += Direction * Speed * Constants.playSpeed;
    }
    public void setZ(float z){
        startPos.z = z;
        endPos.z = z;
    }
    public float getZ(){
        return transform.position.z;
    }    
    /// <summary>
    /// 해당 포물선에서 x값일 때의 z값을 반환하는 함수.
    /// x값이 NaN일 경우 , NaN 반환
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public float getParabolaZbyX(float x){
        if (float.IsNaN(x)) return float.NaN;
        var dis = x - startPos.x;
        var totalDis = endPos.x - startPos.x;
        return  startPos.z + (endPos.z - startPos.z)* (dis / totalDis);
    }
    public float getFlightMaxHeight(float verticalSpeed){
        return flightMaxHeight = (startPos.y - landY) + Mathf.Pow(verticalSpeed,2) / (2 * gravityScale); // 최대 높이
    }
    public float getFlightMaxY(float verticalSpeed){
        return getFlightMaxHeight(verticalSpeed) + landY;
    }
    public float getFlightTime(){        
        float T;
        T = ( verticalSpeed/gravityScale ) + Mathf.Sqrt( 2 * flightMaxHeight / gravityScale ); // 낙하하는 데 걸리는 시간    
        
        return T;
    }
    public float getHighestFlightTime(){
        return ( verticalSpeed/gravityScale );
    }
    public float getHeight() {
        return Height;
    }

    
    public Vector3 getParabolaEnd(float T) {
        float endPosX = startPos.x + T * horizontalSpeed;
        float endPosY = landY;
        float endPosZ = startPos.z + T * depthSpeed;
        return new Vector3 ( endPosX , endPosY, endPosZ );
    }
    public float getRemainTimeToParabolaX(float x) {
        if (float.IsNaN(x)) return INF;

        Vector3 curPos = getCurrentPosition();
        var dis = x - curPos.x;
        var spd = horizontalSpeed;

        if (spd == 0.0f) {
            if (dis > 0) return INF; 
            else return 0;// 움직일 필요가 없는 시간.
        }
        return  dis / spd;
    }    
    public float getRemainTimeToParabolaZ(float z) {
        if (float.IsNaN(z)) return INF;
        Vector3 curPos = getCurrentPosition();
        var dis = z - curPos.z;
        var spd = depthSpeed;

        if (spd == 0.0f) {
            if (dis > 0) return INF; 
            else return 0;// 움직일 필요가 없는 시간.
        }

        return  dis / spd;
    }        
    public float getFloorX() {
        return endPos.x;
    }

    /// <summary>
    /// ball의 포물선에서 특정 y값을 지나는 x값을 반환하는 함수.
    /// isSecondValue가 True면, 2개의 x값중 더 큰 값의 x값을 반환
    /// 만약 포물선이 해당 y좌표를 지나지 않는다면 float.NaN을 반환.
    /// </summary>
    /// <param name="y"></param>
    /// <param name="isSecondValue"></param>
    /// <returns></returns>
    public float getParabolaXbyY(float y , bool isSecondValue) {
            Vector3 start = startPos;
            Vector3 end = endPos;
            var A = 0.5f * gravityScale * Mathf.Pow((1 / horizontalSpeed) , 2);
            var B = -(verticalSpeed / horizontalSpeed);
            var C = y - start.y;
            float firstX = (Mathf.Sqrt( Mathf.Pow(B, 2) -4*A*C) - B)/(2.0f*A);
            float secondX = (-Mathf.Sqrt( Mathf.Pow(B, 2) -4*A*C) - B)/(2.0f*A) ;
        
            var val = Mathf.Pow(B, 2) -4*A*C;
            if ( float.IsNaN(secondX) ||  float.IsNaN(firstX)) {
                //mainControl.showDebug($"Error! : {endPos.x} , secondX : {secondX} , firstX : {firstX} , verticalSpeed : {verticalSpeed} , Direction : {direction}");
                return float.NaN;
            }
            var returnValue = 0.0f;
            if (isSecondValue) // 내려올 때의 x좌표를 구한다면
            {
                if (horizontalSpeed > 0.0f) returnValue = start.x + Mathf.Max(firstX , secondX); // 방향이 오른 쪽이면 더 큰 값이 return 값
                else returnValue = start.x + Mathf.Min(firstX , secondX);// 방향이 왼 쪽이면 더 작은 값이 return 값
            }
            else
            {
                if (horizontalSpeed > 0.0f) returnValue = start.x + Mathf.Min(firstX , secondX); 
                else returnValue = start.x + Mathf.Max(firstX , secondX);
            }
            return  returnValue;
    }


    public float getParabolaXbyMaxY(float y , bool isSecondValue,ref float MaxY) {
            Vector3 start = startPos;
            Vector3 end = endPos;
            var A = 0.5f * gravityScale * Mathf.Pow((1 / horizontalSpeed) , 2);
            var B = -(verticalSpeed / horizontalSpeed);
            var C = y - start.y;
            float firstX = (Mathf.Sqrt( Mathf.Pow(B, 2) -4*A*C) - B)/(2.0f*A);
            float secondX = (-Mathf.Sqrt( Mathf.Pow(B, 2) -4*A*C) - B)/(2.0f*A) ;

            if ( Mathf.Pow(B, 2) -4*A*C < 0 ) { // 닿지 않는다면..
                firstX = -B/(2.0f*A);
                secondX = -B/(2.0f*A);
                MaxY = (1/4) * Mathf.Pow(B,2) / A + start.y;
            }
            var returnValue = 0.0f;
            if (isSecondValue) // 내려올 때의 x좌표를 구한다면
            {
                if (horizontalSpeed > 0.0f) returnValue = start.x + Mathf.Max(firstX , secondX); // 방향이 오른 쪽이면 더 큰 값이 return 값
                else returnValue = start.x + Mathf.Min(firstX , secondX);// 방향이 왼 쪽이면 더 작은 값이 return 값
            }
            else
            {
                if (horizontalSpeed > 0.0f) returnValue = start.x + Mathf.Min(firstX , secondX); 
                else returnValue = start.x + Mathf.Max(firstX , secondX);
            }
            return  returnValue;
    }
    

    public float getParabolaYbyX(float x){
        return startPos.y  + (x - startPos.x)*(verticalSpeed / horizontalSpeed ) - 0.5f*gravityScale*Mathf.Pow( (x - startPos.x) / horizontalSpeed, 2);        
    }
    public void movePosition(float x, float y, float z) {
        float disx = x - startPos.x;
        float disy = y - startPos.y;
        float disz = z - startPos.z;
        //startTime = mainFlow - flightTime;
        startPos.x += disx;
        startPos.y += disy;
        startPos.z += disz;
        endPos.x   += disx;
        endPos.y   += disy;
        endPos.z   += disz;
    }
    public void changePositionByTime(float time) {
        moveParabola(startPos,endPos,time-startTime);
    }
    public void changePositionX(float x) {
        float disx = x - getCurrentPosition().x;
        startPos.x += disx;
        endPos.x += disx;
    }
    public void changePositionZ(float z) {
        float disz = z - getCurrentPosition().z;
        startPos.z += disz;
        endPos.z += disz;
    }
    public bool isParabolaEnd(){
        return ( getCurrentTime() >= flightTime );
    }    
}
