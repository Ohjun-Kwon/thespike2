using UnityEngine;
namespace playerStatsNameSpace{
    public class playerStats
    {
        private float strength;
        private float jump;
        private float speed;
        private float defense;

        private float Original_strength;
        private float Original_jump;
        private float Original_speed;
        private float Original_defense;

        public playerStats(float strength, float jump , float speed , float defense){
            this.strength = strength;
            this.jump = jump;
            this.speed = speed;
            this.defense = defense;

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
        public float getSpeed(){
            return this.speed;
        }
        public void setSpeed(float speed){
            this.speed = speed;
        }
        public float getDefense(){
            return this.defense;
        }
        public void setDefense(float defense){
            this.defense = defense;
        }    
    }
}