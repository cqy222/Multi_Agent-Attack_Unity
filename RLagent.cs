using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;

public enum Team
{
    Blue = 0,
    Red = 1
}

public class RLagent : Agent
{
    public enum Position
    {
        Enemy,
        Ally,
        Other
    }
    [HideInInspector]
    public static float moveSpeed = 0.1f, turnSpeed = 5f;
    int hp = 3;
    //int carry_bullet = 25;
    int carry_bullet = 50;
    Animator animator;
    public float step_pen = 0.0f;
    public float random_shot = -1;
    public float trapped = -0.5f;
    public GameObject bullet;
    public GameObject onDie;
    public Rigidbody rBody;
    public float forceMultiplier = 10;  
    public Team team;
    private BehaviorParameters m_BehaviorParameters;
    private T208_settings T208_setting;
    private EnvironmentParameters m_ResetParams;
    private T208_controller_1 env_contorller;
    //private void Start() {
    //    animator = GetComponent<Animator>();
    //    rBody = GetComponent<Rigidbody>();
    //}

    public override void Initialize(){
        hp = 3;
        env_contorller = GetComponentInParent<T208_controller_1>();
        animator = GetComponent<Animator>();
        rBody = GetComponent<Rigidbody>();
        team = new Team();
        m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        if (m_BehaviorParameters.TeamId == (int)Team.Blue)
        {
            team = Team.Blue;
        }
        else
        {
            team = Team.Red;
        }
        T208_setting = FindObjectOfType<T208_settings>();
        // 获取配置文件参数
        m_ResetParams = Academy.Instance.EnvironmentParameters;
    }

    public override void OnEpisodeBegin()
    {
       //Debug.Log("OnEpisodeBegin Agent!");
       switch(this.name){
                case "ally1": hp = 1; break;
                case "ally2": hp = 1; break;
                case "all_car": hp = 3; break;
                case "all_UAV": hp = 2; break;

                case "enemy_UAV": hp = 2;break;
                case "enemy1": hp = 6; break;
                case "enemy2": hp = 6; break;
                default: hp = 5; break;
                
            }
       //hp = 3;
       carry_bullet = 50;
       // If the Agent fell, zero its momentum

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(carry_bullet);
        sensor.AddObservation(hp);
    }

    public void MoveAgent(ActionSegment<int> act){
        
        var forwardAxis = act[0];
        var rotateAxis = act[1];
        var shotAxis = act[2];

        if (forwardAxis == 1) {
            if (animator != null)
                animator.Play("Run");
        }
        if (forwardAxis==1 &&!Physics.Raycast(transform.position,transform.forward,moveSpeed*1.5f)) {
            transform.Translate(transform.forward * moveSpeed, Space.World);
        }

        if (rotateAxis==1)
            transform.Rotate(new Vector3(0, -turnSpeed, 0), Space.Self);
        if (rotateAxis==2)
            transform.Rotate(new Vector3(0, turnSpeed, 0), Space.Self);
        
        if (shotAxis == 1 && carry_bullet>0) {
            carry_bullet -=1;
            AddReward(random_shot/(carry_bullet+1));
            if (animator != null)
                animator.Play("Shoot");
            if (this.name.Contains("UAV")  ){
                GameObject bulletInst = Instantiate(bullet, transform.position+ new Vector3(0,0,0), Quaternion.identity);
                bulletInst.GetComponent<Bullet>().direction = transform.forward;
                bulletInst.tag = tag+"B";
            }else{
                GameObject bulletInst = Instantiate(bullet, transform.position+ new Vector3(0,1.6f,0), Quaternion.identity);
                bulletInst.GetComponent<Bullet>().direction = transform.forward;
                bulletInst.tag = tag+"B";
            }
            //GameObject bulletInst = Instantiate(bullet, transform.position+ new Vector3(0,2.0f,0), Quaternion.identity);
            
        }
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        //攻方要尽快攻击
        if (team == Team.Blue)
        {
            //Debug.Log("攻方要尽快攻击");
            AddReward(-step_pen);
        }//守方尽量阻敌
        else if (team == Team.Red)
        {
           // Debug.Log("守方尽量阻敌");
            AddReward(step_pen);
        }
        
        MoveAgent(actionBuffers.DiscreteActions);
        //Debug.DrawRay(transform.position, transform.forward* moveSpeed * 1.3f,Color.red);
    
    }

    //卡住或者不动了
    // public void Trapped()
    // {
    //     if (Mathf.Approximately(rBody.velocity.magnitude, 0f))
    //     {
    //         if (team == Team.Blue)
    //         {
    //             AddReward(trapped)
    //         }
    //         else
    //         {
    //             AddReward(trapped);
    //         }
    //     }
    // }

    /*
    void OnCollisionEnter(Collision col) {
        //Debug.Log(col.collider.tag);
        //Debug.Log(this.tag);
        //if (col.collider.tag!= this.tag && col.collider.tag == "Bullet") {
        Debug.Log("OnCollisionEnter???");    

        if (this.tag == "enemy"){
            if (col.gameObject.CompareTag("allyB"))
            {
            // 在碰撞到标记为"ally"的游戏对象时执行的操作
                hp--;
                Debug.Log("zhong dan le !!"); 
                Destroy(col.gameObject);
                if (hp == 0){
                    this.gameObject.SetActive(false);
                    Destroy(gameObject);
                    if (this.tag == "ally"){
                        Debug.Log("Team.Blue Dead???");
                        env_contorller.Whoisdead(this,Team.Blue);
                    }
                else{
                    Debug.Log("Team.Red Dead~~~~~~");
                    env_contorller.Whoisdead(this,Team.Red);
                    }
                }
            }
        }  
        if (this.tag == "ally"){
            if (col.gameObject.CompareTag("enemyB"))
            {
            // 在碰撞到标记为"enemy"的游戏对象时执行的操作
                hp--;
                Debug.Log("zhong dan le !!"); 
                Destroy(col.gameObject);
                if (hp == 0){
                    this.gameObject.SetActive(false);
                    Destroy(gameObject);
                    if (this.tag == "ally"){
                        Debug.Log("Team.Blue Dead???");
                        env_contorller.Whoisdead(this,Team.Blue);
                    }
                    else{
                        Debug.Log("Team.Red Dead~~~~~~");
                        env_contorller.Whoisdead(this,Team.Red);
                    }
                }
            }
        } 
        //if(col.transform.tag!= tag && col.transform.tag!= "T_land" && col.transform.tag!= "other"){
            
        if(col.transform.tag.Contains("B") && !(col.transform.tag.Contains(this.tag))){
            hp--;
            Debug.Log("zhong dan le !!"); 
            Destroy(col.gameObject);
            if (hp == 0){
                this.gameObject.SetActive(false);
                Destroy(gameObject);
                if (this.tag == "ally"){
                    Debug.Log("Team.Blue Dead???");
                    env_contorller.Whoisdead(this,Team.Blue);
                }
                else{
                    Debug.Log("Team.Red Dead~~~~~~");
                    env_contorller.Whoisdead(this,Team.Red);
                }
            }
                
        }
        
    }
    */

    void OnTriggerEnter(Collider col){
        //和智能体在同一个父物体下并且智能体为激活状态
        if (this.tag == "enemy"){
            if (col.gameObject.CompareTag("allyB"))
            {
            // 在碰撞到标记为"ally"的游戏对象时执行的操作
                hp--;
                if (this.tag == "ally"){
                        //Debug.Log("Team.Blue Dead???");
                        //Instantiate(onDie,transform.position,Quaternion.identity);
                        env_contorller.Whoisshot(this,Team.Blue);
                    }
                else{
                        //Debug.Log("Team.Red Dead~~~~~~");
                        //Instantiate(onDie,transform.position,Quaternion.identity);
                        env_contorller.Whoisshot(this,Team.Red);
                    }
                //Debug.Log("zhong dan le !!"); 
                Destroy(col.gameObject);
                if (hp == 0){
                    this.gameObject.SetActive(false);
                    //Destroy(gameObject);
                    if (this.tag == "ally"){
                        //Debug.Log("Team.Blue Dead???");
                        Instantiate(onDie,transform.position,Quaternion.identity);
                        env_contorller.Whoisdead(this,Team.Blue);
                    }
                    else{
                    //Debug.Log("Team.Red Dead~~~~~~");
                    Instantiate(onDie,transform.position,Quaternion.identity);
                    env_contorller.Whoisdead(this,Team.Red);
                    }
                }
            }
        }  
        if (this.tag == "ally"){
            if (col.gameObject.CompareTag("enemyB"))
            {
            // 在碰撞到标记为"enemy"的游戏对象时执行的操作
                hp--;
                if (this.tag == "ally"){
                        //Debug.Log("Team.Blue Dead???");
                        //Instantiate(onDie,transform.position,Quaternion.identity);
                        env_contorller.Whoisshot(this,Team.Blue);
                    }
                else{
                        //Debug.Log("Team.Red Dead~~~~~~");
                        //Instantiate(onDie,transform.position,Quaternion.identity);
                        env_contorller.Whoisshot(this,Team.Red);
                    }
                //Debug.Log("zhong dan le !!"); 
                Destroy(col.gameObject);
                if (hp == 0){
                    this.gameObject.SetActive(false);
                    //Destroy(gameObject);
                    if (this.tag == "ally"){
                        //Debug.Log("Team.Blue Dead???");
                        Instantiate(onDie,transform.position,Quaternion.identity);
                        env_contorller.Whoisdead(this,Team.Blue);
                    }
                    else{
                        //Debug.Log("Team.Red Dead~~~~~~");
                        Instantiate(onDie,transform.position,Quaternion.identity);
                        env_contorller.Whoisdead(this,Team.Red);
                    }
                }
            }
        } 
    }

    private void OnDestroy() {
        if (onDie != null)
            Instantiate(onDie,transform.position,Quaternion.identity);
    }
    //}

    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        switch(this.name){
            case "ally2": 
            //forward
            if (Input.GetKey(KeyCode.W))discreteActionsOut[0] = 1;
            //rotate
            if (Input.GetKey(KeyCode.A))discreteActionsOut[1] = 1;
            if (Input.GetKey(KeyCode.D))discreteActionsOut[1] = 2;
            //fight
            if (Input.GetKey(KeyCode.S))discreteActionsOut[2] = 1; break;
            
            case "ally1": //forward
            if (Input.GetKey(KeyCode.T))discreteActionsOut[0] = 1;
            //rotate
            if (Input.GetKey(KeyCode.F))discreteActionsOut[1] = 1;
            if (Input.GetKey(KeyCode.H))discreteActionsOut[1] = 2;
            //fight
            if (Input.GetKey(KeyCode.G))discreteActionsOut[2] = 1;  break;
            
            case "all_car": //forward
            if (Input.GetKey(KeyCode.P))discreteActionsOut[0] = 1;
            //rotate
            if (Input.GetKey(KeyCode.P))discreteActionsOut[1] = 1;
            if (Input.GetKey(KeyCode.P))discreteActionsOut[1] = 2;
            //fight
            if (Input.GetKey(KeyCode.S))discreteActionsOut[2] = 1; break;
            
            case "all_UAV": //forward
            if (Input.GetKey(KeyCode.P))discreteActionsOut[0] = 1;
            //rotate
            if (Input.GetKey(KeyCode.P))discreteActionsOut[1] = 1;
            if (Input.GetKey(KeyCode.P))discreteActionsOut[1] = 2;
            //fight
            if (Input.GetKey(KeyCode.G))discreteActionsOut[2] = 1; break;

            case "enemy_UAV": //forward
            if (Input.GetKey(KeyCode.P))discreteActionsOut[0] = 1;
            //rotate
            if (Input.GetKey(KeyCode.P))discreteActionsOut[1] = 1;
            if (Input.GetKey(KeyCode.P))discreteActionsOut[1] = 2;
            //fight
            if (Input.GetKey(KeyCode.P))discreteActionsOut[2] = 1; break;
            
            case "enemy1": //forward
            if (Input.GetKey(KeyCode.P))discreteActionsOut[0] = 1;
            //rotate
            if (Input.GetKey(KeyCode.P))discreteActionsOut[1] = 1;
            if (Input.GetKey(KeyCode.P))discreteActionsOut[1] = 2;
            //fight
            if (Input.GetKey(KeyCode.P))discreteActionsOut[2] = 1; break;
            
            case "enemy2": //forward
            if (Input.GetKey(KeyCode.I))discreteActionsOut[0] = 1;
            //rotate
            if (Input.GetKey(KeyCode.J))discreteActionsOut[1] = 1;
            if (Input.GetKey(KeyCode.L))discreteActionsOut[1] = 2;
            //fight
            if (Input.GetKey(KeyCode.P))discreteActionsOut[2] = 1; break;
            
            default: //forward
            if (Input.GetKey(KeyCode.W))discreteActionsOut[0] = 1;
            //rotate
            if (Input.GetKey(KeyCode.A))discreteActionsOut[1] = 1;
            if (Input.GetKey(KeyCode.D))discreteActionsOut[1] = 2;
            //fight
            if (Input.GetKey(KeyCode.S))discreteActionsOut[2] = 1; break;

        }
        
    }


}
