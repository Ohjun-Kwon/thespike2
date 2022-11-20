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

    public Trigger(float t_ , Action runFunc_){
        this.time = t_;
        this.runFunc = runFunc_;
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
    public List<Trigger> TriggerList = new List<Trigger> ();

    void FixedUpdate()
    {
        mainTimeFlow += playSpeed;

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

    }
    public float getMainTimeFlow(){
        return mainTimeFlow;
    }
    public Trigger addTrigger(float t_,Action Func){
        Trigger T = new Trigger(t_ + getMainTimeFlow(),Func);
        TriggerList.Add(T);
        return T;
    }
}
