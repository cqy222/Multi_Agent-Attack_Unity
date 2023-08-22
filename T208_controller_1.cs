using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;

public class T208_controller_1 : MonoBehaviour
{
     [System.Serializable]
    public class PlayerInfo
    {
        public RLagent Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }

    [Header("Max Environment Steps")] public int MaxEnvironmentSteps = 10000;
    private int m_ResetTimer;
    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();
    private Dictionary<RLagent, PlayerInfo> m_PlayerDict = new Dictionary<RLagent, PlayerInfo>();
    private T208_settings T208_setting;
    private int Blue_RemainingPlayers;
    private int Red_RemainingPlayers;
    private SimpleMultiAgentGroup m_AgentGroup_Blue;
    private SimpleMultiAgentGroup m_AgentGroup_Red;
    public int win_game = 100;
    public int shot_enemy = 2;
    public int kill_enemy = 10;
    public int lose_game = -50;
    // public int hero_trapped = -1

    
    // Start is called before the first frame update
    void Start()
    {
        T208_setting = FindObjectOfType<T208_settings>();
        //Reset Players Remaining
        Blue_RemainingPlayers = 4;
        Red_RemainingPlayers = 3;
        // Initialize TeamManager
        m_AgentGroup_Blue = new SimpleMultiAgentGroup();
        m_AgentGroup_Red = new SimpleMultiAgentGroup();
        Debug.Log("start!!");
        foreach (var item in AgentsList)
        {
            Debug.Log(item.Agent.name);
            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.GetComponent<Rigidbody>();
            // Add to team manager
            if (item.Agent.team == Team.Blue){
                m_AgentGroup_Blue.RegisterAgent(item.Agent);
            }
            else{
                m_AgentGroup_Red.RegisterAgent(item.Agent);
            }
            
        }
        ResetScene();
    }
    void FixedUpdate()
    {
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            m_AgentGroup_Blue.EndGroupEpisode();
            m_AgentGroup_Red.EndGroupEpisode();
            ResetScene();
        }
    }


    public void ResetScene()
    {
        // 开始延迟执行的协程
        StartCoroutine(DelayedReset());
    }

    public IEnumerator DelayedReset()
    {
    // 在重置场景之前等待5秒钟
        yield return new WaitForSeconds(5f);
        //Debug.Log("In ResetScene !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
     // 重置计时
        m_ResetTimer = 0;
     // 重置生存的智能体数量
        Blue_RemainingPlayers = 4;
        Red_RemainingPlayers = 3;
	// 四个方向任意旋转场景，可以防止过拟合在一个位置上
    // var rotation = Random.Range(0, 4);
    // var rotationAngle = rotation * 90f;
    // transform.Rotate(new Vector3(0f, rotationAngle, 0f));
     // 重置列表中的每个智能体
        float start_x,rtx;
        float start_y,rty;
        float start_z,rtz;
        foreach (var item in AgentsList)
            {
         // 如果设定了随机，在场景中随机一个位置，没有就固定位置
         //Debug.Log(item.Agent.name);
            switch(item.Agent.name){
                case "ally1": start_x=1.82f;start_y=-8.59f;start_z = 1.67f;rtx=0f;rty=-90f;rtz=0f; break;
                case "ally2": start_x=2.57f;start_y=-8.581f;start_z = -0.8f;rtx=0f;rty=-90.297f;rtz=0f; break;
                case "all_car": start_x=-14.4f;start_y=-8.597f;start_z = 1.71f;rtx=0;rty=-169.34f;rtz=0; break;
                case "all_UAV": start_x=-11.34f;start_y=-6.266f;start_z = 1.21f;rtx=0;rty=-176.13f;rtz=0; break;

                case "enemy_UAV": start_x=-13.18f ;start_y=-6.41f ;start_z =-14.06f  ;rtx=0 ;rty=0 ;rtz=0 ; break;
                case "enemy1": start_x=-14.92f ;start_y=-8.56f ;start_z = -16.72f ;rtx= 0;rty= 0;rtz= 0; break;
                case "enemy2": start_x=-12.33f ;start_y=-8.566f ;start_z =-16.72f  ;rtx=0f ;rty=0f ;rtz=0f ; break;
                default: start_x=-13.18f ;start_y=-8.566f ;start_z =1.5f  ;rtx=0f ;rty=0f ;rtz=0f ; break;
                
            }
            //var randomPosX = Random.Range(-5f, 5f);
            var newStartPos = new Vector3(start_x, start_y, start_z);
            //Debug.Log(newStartPos);
            var newRot = Quaternion.Euler(rtx, rty, rtz);
            //item.Agent.transform.SetPositionAndRotation(newStartPos, newRot);
            item.Agent.transform.SetPositionAndRotation(item.StartingPos, newRot);

            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;

            item.Agent.gameObject.SetActive(true);
            if (item.Agent.team == Team.Blue){
                m_AgentGroup_Blue.RegisterAgent(item.Agent);
            }
            else{
                m_AgentGroup_Red.RegisterAgent(item.Agent);
            }
        }
        ClearBullet();
    }

    public void ClearBullet()
    {
        string targetTag1 = "allyB";
        GameObject[] objectsToDelete1 = GameObject.FindGameObjectsWithTag(targetTag1);
        // 删除获取到的所有游戏对象
        foreach (GameObject obj in objectsToDelete1)
        {
            Destroy(obj);
        }
        string targetTag2 = "enemyB";
        GameObject[] objectsToDelete2 = GameObject.FindGameObjectsWithTag(targetTag2);
        // 删除获取到的所有游戏对象
        foreach (GameObject obj in objectsToDelete2)
        {
            Destroy(obj);
        }
    }

    public void Whoisshot(RLagent this_agent, Team shotTeam){
        //Debug.Log("in Whoisdead********************************");
        if (shotTeam == Team.Blue)
        {
            m_AgentGroup_Red.AddGroupReward(shot_enemy - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_AgentGroup_Blue.AddGroupReward(-shot_enemy);
        }
        else
        {
            //Red_RemainingPlayers -=1;
            m_AgentGroup_Blue.AddGroupReward(shot_enemy - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_AgentGroup_Red.AddGroupReward(-shot_enemy);
        }
        
    }



    //判断奖励函数
    public void Whoisdead(RLagent this_agent, Team shotTeam){
        //Debug.Log("in Whoisdead********************************");
        if (shotTeam == Team.Blue)
        {
            Blue_RemainingPlayers -=1;
            m_AgentGroup_Red.AddGroupReward(kill_enemy - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_AgentGroup_Blue.AddGroupReward(-kill_enemy);
        }
        else
        {
            Red_RemainingPlayers -=1;
            m_AgentGroup_Blue.AddGroupReward(kill_enemy - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_AgentGroup_Red.AddGroupReward(-kill_enemy);
        }
        
        if (Red_RemainingPlayers == 0){
            m_AgentGroup_Blue.AddGroupReward(win_game);
            m_AgentGroup_Red.AddGroupReward(lose_game);
            m_AgentGroup_Blue.EndGroupEpisode();
            m_AgentGroup_Red.EndGroupEpisode();
            ResetScene();
        }
        else if (Blue_RemainingPlayers == 0){
            m_AgentGroup_Blue.AddGroupReward(lose_game);
            m_AgentGroup_Red.AddGroupReward(win_game);
            m_AgentGroup_Blue.EndGroupEpisode();
            m_AgentGroup_Red.EndGroupEpisode();
            ResetScene();
        }
        else{
            this_agent.gameObject.SetActive(false);
        }
    }

    // //卡住了
    // public void Trapped(RLagent this_agent, Collider other)
    // {
    //     if (Mathf.Approximately(this_agent.Rb.velocity.magnitude, 0f))
    //     {
    //         if (this_agent.team == Team.Blue)
    //         {
    //             m_AgentGroup_Blue.AddGroupReward(hero_trapped);
    //         }
    //         else
    //         {
    //             m_AgentGroup_Red.AddGroupReward(hero_trapped);
    //         }
    //     }
    // }

    
}
