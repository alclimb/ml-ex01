using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class Ball3DAgent : Agent
{
    [Header("Specific to Ball3D")]
    public GameObject ball;

    private Rigidbody m_BallRb;

    private EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        this.m_BallRb = this.ball.GetComponent<Rigidbody>();
        this.m_ResetParams = Academy.Instance.EnvironmentParameters;

        this.SetResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(base.gameObject.transform.rotation.z);
        sensor.AddObservation(base.gameObject.transform.rotation.x);
        sensor.AddObservation(this.ball.transform.position - base.gameObject.transform.position);
        sensor.AddObservation(this.m_BallRb.velocity);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        var actionZ = 2f * Mathf.Clamp(vectorAction[0], -1f, 1f);
        var actionX = 2f * Mathf.Clamp(vectorAction[1], -1f, 1f);

        // Z軸回転
        if ((base.gameObject.transform.rotation.z < 0.25f && actionZ > 0f) ||
            (base.gameObject.transform.rotation.z > -0.25f && actionZ < 0f))
        {
            base.gameObject.transform.Rotate(new Vector3(0, 0, 1), actionZ);
        }

        // X軸回転
        if ((base.gameObject.transform.rotation.x < 0.25f && actionX > 0f) ||
            (base.gameObject.transform.rotation.x > -0.25f && actionX < 0f))
        {
            base.gameObject.transform.Rotate(new Vector3(1, 0, 0), actionX);
        }

        // 落下判定
        if ((this.ball.transform.position.y - base.gameObject.transform.position.y) < -2f ||
            Mathf.Abs(this.ball.transform.position.x - base.gameObject.transform.position.x) > 3f ||
            Mathf.Abs(this.ball.transform.position.z - base.gameObject.transform.position.z) > 3f)
        {
            // 評価: 報酬を減らす
            base.SetReward(-1f);

            // リセットして次のエピソードを開始
            base.EndEpisode();
        }
        else
        {
            // 評価: 報酬を与える
            base.SetReward(0.1f);
        }
    }

    public override void OnEpisodeBegin()
    {
        base.gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        base.gameObject.transform.Rotate(new Vector3(1, 0, 0), Random.Range(-10f, 10f));
        base.gameObject.transform.Rotate(new Vector3(0, 0, 1), Random.Range(-10f, 10f));
        
        this.m_BallRb.velocity = new Vector3(0f, 0f, 0f);
        this.ball.transform.position = new Vector3(Random.Range(-1.5f, 1.5f), 4f, Random.Range(-1.5f, 1.5f))
            + base.gameObject.transform.position;

        // Reset the parameters when the Agent is reset.
        this.SetResetParameters();
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = -Input.GetAxis("Horizontal");
        actionsOut[1] = +Input.GetAxis("Vertical");
    }

    public void SetBall()
    {
        //Set the attributes of the ball by fetching the information from the academy
        this.m_BallRb.mass = this.m_ResetParams.GetWithDefault("mass", 1.0f);

        var scale = this.m_ResetParams.GetWithDefault("scale", 1.0f);
        this.ball.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetResetParameters()
    {
        this.SetBall();
    }
}
