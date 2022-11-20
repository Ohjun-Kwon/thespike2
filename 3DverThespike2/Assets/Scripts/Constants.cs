using UnityEngine;


public static class Constants
{
    

    public const float Pi = 3.141592f;
    public const float NET_X = 0.0f;
    public const float NET_Y = 3.0f;
    public const int playerNumber = 8;

    public const int SPIKER = 3;
    public const int SETTER = 1;
    public const int LIBERO = 0;
    public const int BLOCKER = 2;
    
    public const int TEAM_RIGHT = 1;
    public const int TEAM_LEFT = -1;
    public const int TEAM_NO = 0;

    public const int NOONE = -1;


    public const int ZERO = 0;
 
    public const float SERVERPOS = 18.5f;
    public const float FARBACK = 9.5f;
    public const float MIDBACK = 7.5f;
    public const float NEARBACK = 6.0f;
    public const float MID = 5.0f;
    public const float FARFRONT = 4.0f;
    public const float MIDFRONT = 3.0f;
    public const float NEARFRONT = 1.0f;

    public const float LEFT_LIMIT = NET_X - 0.5f;
    public const float RIGHT_LIMIT = NET_X + 0.5f;

    public const int STRATEGY_QUICK = 0;
    public const int STRATEGY_OPEN = 1;
    

    public const int LEFT = 0;
    public const int CENTER = 1;
    public const int RIGHT = 2;

    public const float Z_RIGHT = -0.8f;
    public const float Z_CENTER = 0.0f;
    public const float Z_LEFT = 0.8f;


    public const float INF = 999999f;


    public const int SIT_RALLYPLAYING = 107;
    public const int SIT_RALLYEND     = 108;
    public const int SIT_SERVERGO = 109;
    public const int SIT_SERVERWAIT = 110;
    public const int SIT_SERVERTOSS = 111;
    public const int SIT_SERVERHIT = 112;


    //BALL_ 현재 배구공 움직임의 상황을 나타내는 상수들
    public const int BALL_FREEBALL = 1;
    public const int BALL_FREEBALL_SHORT = 2;
    public const int BALL_FAINT_SHORT = 3;
    public const int BALL_FAINT_LONG = 4;
    public const int BALL_ATTACK = 5;
    public const int BALL_FREEBALL_TIME = 90;
    public const int BALL_BALLWAIT = 6;
    public const int BALL_RECEIVE_GOOD = 7;
    public const int BALL_RECEIVE_SHORT = 8;
    public const int BALL_RECEIVE_GOOD_LOW = 9;
    public const int BALL_RECEIVE_SHORT_LOW = 10;
    public const int BALL_RECEIVE_BAD = 11;
    public const int BALL_RECEIVE_BAD_LOW = 12;
    public const int BALL_RECEIVE_BAD_LONG = 13;
    public const int BALL_TOSS = 14;


    //JUMP의 종류를 나타내는 상수
    public const float JUMP_TOSS = 0.8f;
    public const float JUMP_BLOCK = 0.95f;
    public const float JUMP_SPIKE = 1.0f;
    public const float JUMP_NO = 0.0f;

    public const float SLOW_SPEED = 0.3f;



    //PlayerAction의 종류를 나타내는 상수

    public const int ACTION_RECEIVE = 255;
    public const int ACTION_RECEIVEDONE = 256;
    public const int ACTION_SPIKEREADY = 257;
    public const int ACTION_SPIKESWING = 258;
    public const int ACTION_SPIKEDONE = 259;
    public const int ACTION_BLOCK = 260;
    public const int ACTION_TOSS = 261;
    public const int ACTION_JUMPTOSS = 262;
    public const int ACTION_TOSSDONE = 263;
    public const int ACTION_QUICKREADY = 264;
    public const int ACTION_SERVEWALK = 265;
    public const int ACTION_SERVEREADY = 266;
    public const int ACTION_SERVETOSS = 267;
    public const int ACTION_SERVEDONE = 268;
    

    public static float playSpeed = 0.1f;
    public const float NOVALUE = -90158115.4142f;


    public const float gravityScale = 0.5f;
}