using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class HallwayCollabAgent : HallwayAgent
{
    public HallwayCollabAgent teammate;
    public bool isSpotter = true;
    int m_Message = 0;

    [HideInInspector]
    public int selection = 0;
    public override void OnEpisodeBegin()
    {
        m_Message = -1;

        var agentOffset = 10f;
        if (isSpotter)
        {
            agentOffset = -15;
        }

        if (!isSpotter)
        {
            transform.position = new Vector3(0f + Random.Range(-3f, 3f),
                1f, agentOffset + Random.Range(-5f, 5f))
                + ground.transform.position;
            transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }
        else
        {
            transform.position = new Vector3(0f,
                1f, agentOffset)
                + ground.transform.position;
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        // Remove the randomness

        m_AgentRb.velocity *= 0f;
        if (isSpotter)
        {
            var blockOffset = -9f;
            // Only the Spotter has the correct selection
            selection = Random.Range(0, 2);
            if (selection == 0)
            {
                symbolO.transform.position =
                    new Vector3(0f, 2f, blockOffset)
                    + ground.transform.position;
                symbolX.transform.position =
                    new Vector3(0f, -1000f, blockOffset + Random.Range(-5f, 5f))
                    + ground.transform.position;
            }
            else
            {
                symbolO.transform.position =
                    new Vector3(0f, -1000f, blockOffset + Random.Range(-5f, 5f))
                    + ground.transform.position;
                symbolX.transform.position =
                    new Vector3(0f, 2f, blockOffset)
                    + ground.transform.position;
            }

            var goalPos = Random.Range(0, 2);
            if (goalPos == 0)
            {
                symbolOGoal.transform.position = new Vector3(7f, 0.5f, 22.29f) + area.transform.position;
                symbolXGoal.transform.position = new Vector3(-7f, 0.5f, 22.29f) + area.transform.position;
            }
            else
            {
                symbolXGoal.transform.position = new Vector3(7f, 0.5f, 22.29f) + area.transform.position;
                symbolOGoal.transform.position = new Vector3(-7f, 0.5f, 22.29f) + area.transform.position;
            }
        }
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        if (useVectorObs)
        {
            sensor.AddObservation(StepCount / (float)MaxStep);
        }
        sensor.AddObservation(m_Message);
    }

    public void tellAgent(int message)
    {
        m_Message = message;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        AddReward(-1f / MaxStep);
        if (!isSpotter)
        {
            MoveAgent(actionBuffers.DiscreteActions);
        }

        int comm_act = actionBuffers.DiscreteActions[1];
        teammate.tellAgent(comm_act);
        // if (isSpotter) // Test
        // {
        //     teammate.tellAgent(selection);
        // }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("symbol_O_Goal") || col.gameObject.CompareTag("symbol_X_Goal"))
        {
            if (!isSpotter)
            {
                // Check the ground truth
                if ((teammate.selection == 0 && col.gameObject.CompareTag("symbol_O_Goal")) ||
                    (teammate.selection == 1 && col.gameObject.CompareTag("symbol_X_Goal")))
                {
                    SetReward(1f);
                    teammate.SetReward(1f);
                    StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.goalScoredMaterial, 0.5f));
                }
                else
                {
                    SetReward(-0.1f);
                    teammate.SetReward(-0.1f);
                    StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.failMaterial, 0.5f));
                }
                EndEpisode();
                teammate.EndEpisode();
            }
        }
    }
}
