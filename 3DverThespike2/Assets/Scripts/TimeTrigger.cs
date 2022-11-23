using UnityEngine.UI;
using UnityEngine;
using static Constants;
using System.Collections.Generic; 
using System; 

public class Trigger{
    private float time;
    private bool isnext = false; // 다음 트리거가 존재하는지.
    private Action runFunc;
    private Trigger nextTrigger;

    public Trigger() {
        this.time = INF;
        this.runFunc = ()=>{};
    }
    public Trigger(float t_ , Action runFunc_){
        this.time = t_;
        this.runFunc = runFunc_;
    }

    public void resetTrigger() {
        this.time = INF;
        this.runFunc = ()=>{};
    }
    public float getTime() { return this.time; }
    public Action getFunc() { return this.runFunc; }
    public Trigger getNext() { return nextTrigger; }
    public bool isNext() { return isnext; }


    public Trigger next(float t_,Action Func) {
        Trigger T = new Trigger(t_,Func);
        this.nextTrigger = T;
        this.isnext = true;
        return T;
    }
    public Trigger addTime(float t_)
    {
        this.time += t_;
        return nextTrigger;
    }

}
    
public class TimeTrigger : MonoBehaviour
{
    private float mainTimeFlow = 0f;
    private movePhysics[] PlayerPhys = new movePhysics[playerNumber];
    private movePhysics ballPhys;


    [SerializeField]public List<Trigger> TriggerList = new List<Trigger> ();
    Trigger ballTrigger = new Trigger(); // ball 움직임 트리거 -> Ball 움직임 트리거는 오직 하나밖에 존재하지 못한다.

    void Start() {
        for (int i = 0; i < playerNumber ; i ++)
            PlayerPhys[i] = GetComponent<MainControl>().getPlayersByIndex(i).GetComponent<movePhysics>(); // 해당 플레이어의 movePhysics들을 모두 가져온다.
            ballPhys = GetComponent<MainControl>().Ball.GetComponent<movePhysics>(); // 볼의 movePhysics도 가져온다.
        
    }
    void FixedUpdate()
    {
        mainTimeFlow += playSpeed;
        for (int i = 0; i < playerNumber ; i ++)
            PlayerPhys[i].PhysicalFixedUpdate(mainTimeFlow);
        ballPhys.PhysicalFixedUpdate(mainTimeFlow);

        

        for (int i = TriggerList.Count - 1; i >= 0 ; i --) 
        {
            Trigger elem = TriggerList[i];
            if (elem.getTime() <= mainTimeFlow)
            {
                elem.getFunc()();
                if (elem.isNext()) // 다음이 있으면 다음 elem을 TriggerList에 넣는다.
                {
                    elem.getNext().addTime(getMainTimeFlow());
                    TriggerList.Add( elem.getNext() );
                }
                TriggerList.Remove(elem);
            }
        }

        if (ballTrigger.getTime() <= mainTimeFlow) {
            ballTrigger.getFunc()();
            ballTrigger.resetTrigger();
        }

    }
    public float getMainTimeFlow(){
        return mainTimeFlow;
    }
    public Trigger addTrigger(float t_,Action Func){
        Trigger T = new Trigger(t_ + getMainTimeFlow(),Func);
        TriggerList.Add(T);
        TriggerList.Sort(Sort); // 시간이 적은게 우선 실행 되도록 정렬 해준다.
        return T;
    }


    int Sort(Trigger t1 , Trigger t2) {
        if (t1.getTime() < t2.getTime()) return 1;
        else return -1;
    }
}

