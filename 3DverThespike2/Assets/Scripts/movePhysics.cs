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

    
    public Vector3 startPos;
    public Vector3 endPos;

    private float currentTime;
    public float flightTime;
    private float flightMaxHeight;
    private float Height;

    // Start is called before the first frame update
    void Start()
    {
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        //timeFlow
        currentTime += Constants.playSpeed;
        //moveParabola
        moveParabola(startPos , endPos , currentTime);    
    }
    public void initPhysics(){
        setLandY(); // 본인의 캐릭터 크기에 맞게 가장 낮은 위치의 좌표.
        setVector(90.001f,0.0f);
        depthSpeed = 0.0f;
        startParabola();       
    }
    public void setLandY(){
        boxCollider = GetComponent<BoxCollider>();
        Height = boxCollider.bounds.size.y;
        landY = 0.0f + Height/2;
    }
    public void startParabola() {
        startPos = transform.position;

        // 땅 아래에서 시작하는 경우가 있으면 , 물리 연산이 꼬이게 됨.
        if (startPos.y < landY)
            startPos.y = landY;// = new Vector3(transform.position.x , landY , transform.position.z);

        flightMaxHeight = getFlightMaxHeight(verticalSpeed);
        flightTime = getFlightTime();
        endPos = getParabolaEnd( flightTime );
        resetCurrentTime();

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
        resetCurrentTime();
    }
    
    /// <summary>
    /// 움직임 멈춤. 그 자리 그대로 떨어짐.
    /// </summary>
    public void stopParabola() {
        setVector(0f,0f);
        depthSpeed = 0.0f;
        startParabola();
    }
    public void moveParabola(Vector3 start ,Vector3 end , float time) {
        var t = time / flightTime;
        var mid = Vector3.Lerp(start, end ,t);
        mid.y = start.y  + (mid.x - start.x)*(verticalSpeed / horizontalSpeed ) - 0.5f*gravityScale*Mathf.Pow( (mid.x - start.x) / horizontalSpeed, 2);
        transform.position = new Vector3(mid.x , mid.y , mid.z);            
        
    }    
    public Vector3 getParabolaByTime(Vector3 start ,Vector3 end , float time) {
        var t = time / flightTime;
        var mid = Vector3.Lerp(start, end ,t);
        mid.y = start.y + (mid.x - start.x)*(verticalSpeed / horizontalSpeed ) - 0.5f*gravityScale*Mathf.Pow( (mid.x - start.x) / horizontalSpeed, 2);
        return new Vector3(mid.x , mid.y , mid.z);
    }    

    public void resetCurrentTime(){
        currentTime = 0f;
        return;
    }
    public void setVector(float Dir , float Spd) {
        direction = Dir * (Constants.Pi/180);  // 라디안 각 변환
        horizontalSpeed = Spd * Mathf.Cos(direction);
        verticalSpeed   = Spd * Mathf.Sin(direction);


        horizontalSpeed = Mathf.Max(0.001f , Mathf.Abs(horizontalSpeed)) * Mathf.Sign(horizontalSpeed); // 가로속도가 0이면 , 1/  0이되어 ㅈ되는 경우가 많음. 그래서 최솟값으로 설정.
        return;
    }
    public float getVerticalFlippedDirection(){
         if (horizontalSpeed == 0.0f) return 0.0f;
         return Mathf.Atan(-verticalSpeed / horizontalSpeed) * (180 / Constants.Pi);
    } 
    public float getDirection(){
        return direction;
    }
    public float getSpeed(){
        if (horizontalSpeed == 0.0f) return 0.0f;
        var dir = Mathf.Atan(verticalSpeed / horizontalSpeed);
        return verticalSpeed / Mathf.Sin(dir);
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
    public void setVectorByHighestY(float desX, float desY, float dY){
        var highestY = transform.position.y + dY;
        var h = highestY - transform.position.y;
        var H = highestY - desY;
        var dx = desX - transform.position.x;
        float t = Mathf.Sqrt(2*H/gravityScale) + Mathf.Sqrt(2*h/gravityScale);

        if (highestY < desY) {
            Debug.Log("Error on setVectorByHighestY ");
            return;
        }
        verticalSpeed = Mathf.Sqrt(2*gravityScale*h);
        horizontalSpeed = dx /  t ;

        direction = Mathf.Atan(verticalSpeed / horizontalSpeed);
        speed = verticalSpeed / Mathf.Sin(direction);
        
        setVector(direction * 180 / (Constants.Pi),speed);
    }
    public void setVectorForQuickAttack(float desX , float desY , float desZ ,float time)
    {
        //속공을 위해 필요한 스피드 계산
        // 주어진 시간 내로, 주어진 좌표값까지 이동시키는 속도를 지정.
        var dx = desX - transform.position.x;
        var dy =  desY - transform.position.y;
        var dz = desZ - transform.position.z;

        horizontalSpeed = dx/time;
        verticalSpeed = dy/time + gravityScale*time/2;
        depthSpeed = dz/time;
        direction = Mathf.Atan(verticalSpeed / horizontalSpeed);
        horizontalSpeed = Mathf.Max(0.001f , Mathf.Abs(horizontalSpeed)) * Mathf.Sign(horizontalSpeed);

        return;
    }
    public void setVectorByVspeedParabola(float desX, float desY, float vspeed){ 
        // 포물선을 그리는 궤도로 목표 좌표까지 이동시키는 속도.
        // 여기서 포물선을 위한 최고점은, 현재 기준의 좌표 y값에, 주어진 vspeed 속도만큼 더 올라간 후의 최대 높이이다.
        var highestY = transform.position.y + Mathf.Pow(vspeed , 2) / (2 * gravityScale);

        var h = highestY - transform.position.y;
        var H = highestY - desY;
        var dx = desX - transform.position.x;

        if (highestY < desY) {
            // 최대 높이까지 올라갔음에도, 목표 y값보다 작아질 경우, 계산이 불가하다.
            Debug.Log("Error on setVectorByVspeedParabola. ");
            return;
        }
        float t = Mathf.Sqrt(2*H/gravityScale) + Mathf.Sqrt(2*h/gravityScale);

        verticalSpeed = vspeed;
        horizontalSpeed = dx /  t ;
        direction = Mathf.Atan(verticalSpeed / horizontalSpeed);
        speed = verticalSpeed / Mathf.Sin(direction);
        setVector(direction * 180 / (Constants.Pi),speed);
    }
    public void setVectorByVspeedSpike(float desX, float desY , float vspeed){ 
        // 포물선을 안그리고 바로 내려 찍는 궤도
        var verticalSpeed = -vspeed;
        var distance =  desX - transform.position.x;
        var h = transform.position.y - desY;

        var t = (-vspeed  + Mathf.Sqrt(Mathf.Pow(vspeed , 2) + 2 * h * gravityScale)) / gravityScale;
        horizontalSpeed = distance / t;

        direction = Mathf.Atan(verticalSpeed / horizontalSpeed);
        speed = verticalSpeed / Mathf.Sin(direction);
        setVector(direction * 180 / (Constants.Pi),speed);
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
    public float getCurrentTime(){
        return currentTime;
    }
    /// <summary>
    /// 해당 포물선에서 x값일 때의 z값을 반환하는 함수.
    /// x값이 NaN일 경우 , NaN 반환
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public float getFallingPlaceZbyX(float x){
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
        if (verticalSpeed > 0) // 속도가 위를 향할 때
        {
            T = ( verticalSpeed/gravityScale ) + Mathf.Sqrt( 2 * flightMaxHeight / gravityScale ); // 낙하하는 데 걸리는 시간    
        }
        else // 속도가 아래를 향할 때,
        {
            float A,B,C;
            A = gravityScale;
            B = -2*verticalSpeed;
            C = -2*(startPos.y - landY);
            T = -B + Mathf.Sqrt(Mathf.Pow(B,2) - 4*A*C)/(2*A); 
        }

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
        var dis = x - transform.position.x;
        var spd = horizontalSpeed;

        if (spd == 0.0f) {
            if (dis > 0) return INF; 
            else return 0;// 움직일 필요가 없는 시간.
        }
        return  dis / spd;
    }    
    public float getRemainTimeToParabolaZ(float z) {
        if (float.IsNaN(z)) return INF;
        var dis = z - transform.position.z;
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
                //Debug.Log($"Error! : {endPos.x} , secondX : {secondX} , firstX : {firstX} , verticalSpeed : {verticalSpeed} , Direction : {direction}");
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
                Debug.Log(MaxY);
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
        currentTime = flightTime;
        startPos.x += disx;
        startPos.y += disy;
        startPos.z += disz;
        endPos.x   += disx;
        endPos.y   += disy;
        endPos.z   += disz;
    }
    public bool isParabolaEnd(){
        return (currentTime >= flightTime );
    }    
}
