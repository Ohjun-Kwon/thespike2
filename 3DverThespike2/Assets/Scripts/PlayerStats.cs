using UnityEngine;
namespace playerStatsNameSpace{
    public class playerStats
    {
        private float strength;
        private float jump;
        private float speed;
        private float defense;

        // 리시브, 토스, 스윙에 소요 되는 시간
        private float swingTime;
        private float receiveTime;
        private float tossTime;
        private float reactSpeed;
        

        private float Original_swingTime;
        private float Original_receiveTime;
        private float Original_tossTime;
        private float Original_reactSpeed;
        private float Original_strength;
        private float Original_jump;
        private float Original_speed;
        private float Original_defense;

        public playerStats(float strength, float jump , float speed , float defense, float swingTime = 0.1f, float receiveTime = 0.1f, float tossTime = 0.1f,float reactSpeed = 1.0f){
            this.strength = strength;
            this.jump = jump;
            this.speed = speed;
            this.defense = defense;
            this.swingTime = swingTime; 
            this.receiveTime = receiveTime;
            this.tossTime = tossTime;
            this.reactSpeed = reactSpeed;
            
            this.Original_swingTime = swingTime; 
            this.Original_receiveTime = receiveTime;
            this.Original_tossTime = tossTime;
            this.Original_reactSpeed = reactSpeed;

            this.Original_strength = strength;
            this.Original_jump = jump;
            this.Original_speed = speed;
            this.Original_defense = defense;        
        }
        public void playerStatsReset(){
            this.strength = this.Original_strength;
            this.jump = this.Original_jump;
            this.speed = this.Original_speed;
            this.defense = this.Original_defense;
        }
        public float getStrength(){
            return this.strength;
        }
        public void setStrength(float strength){
            this.strength = strength;
        }
        public float getJump(){
            return this.jump;
        }
        public void setjump(float jump){
            this.jump = jump;
        }
        public float getReactSpeed() {
            return reactSpeed;
        }
        public void setReactSpeed(float reactSpeed) {
            this.reactSpeed = reactSpeed;
        }
        public float getSpeed(){
            return this.speed;
        }
        public void setSpeed(float speed){
            this.speed = speed;
        }
        public float getDefense(){
            return this.defense;
        }
        public float getSwingTime() { return this.swingTime; }
        public float getReceiveTime() { return this.receiveTime; }
        public float getTossTime() { return this.tossTime; }
        public void setDefense(float defense){
            this.defense = defense;
        }    
    }
}