using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class Sample1Agent : Agent
{
    private Rigidbody _rigidbody;

    private EnvironmentParameters m_ResetParams;

    [Header("たーげっと")]
    public Transform Target;

    [Header("基準になるステージ")]
    public Transform StageTransform;

    [Header("ステータス表示用テキスト")]
    public UnityEngine.TextMesh StatusText;

    public override void Initialize()
    {
        Debug.Log("Initialize");

        this._rigidbody = base.GetComponent<Rigidbody>();
        this.m_ResetParams = Academy.Instance.EnvironmentParameters;

        this.SetResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Debug.Log($"CollectObservations >>");

        // ターゲットの位置 (ボールからの相対位置)
        var targetPosition = this.Target.position - this.transform.position;

        // ボールの位置 (ステージからの相対位置)
        var ballPosition = base.transform.position - this.StageTransform.position;

        // ボールの速度
        var ballVelocity = this._rigidbody.velocity;

        sensor.AddObservation(targetPosition.x);
        sensor.AddObservation(targetPosition.z);
        sensor.AddObservation(ballPosition.x);
        sensor.AddObservation(ballPosition.z);
        sensor.AddObservation(ballVelocity.x);
        sensor.AddObservation(ballVelocity.z);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        // Debug.Log($"OnActionReceived >>");

        const float power = 20f;
        var actionX = Mathf.Clamp(vectorAction[0], -1f, 1f) * power;
        var actionZ = Mathf.Clamp(vectorAction[1], -1f, 1f) * power;

        // 移動
        this._rigidbody.AddForce(new Vector3(actionX, 0, actionZ));

        // ターゲットの位置 (ボールからの相対位置)
        var targetPosition = this.Target.position - this.transform.position;

        if ((0 < targetPosition.x && 0 < this._rigidbody.velocity.x) ||
            (targetPosition.x < 0 && this._rigidbody.velocity.x < 0))
        {
            // 評価: 報酬を与える
            base.AddReward(+0.01f);
        }
        else
        {
            // 評価: 報酬を減らす
            base.AddReward(-0.01f);
        }

        if ((0 < targetPosition.z && 0 < this._rigidbody.velocity.z) ||
            (targetPosition.z < 0 && this._rigidbody.velocity.z < 0))
        {
            // 評価: 報酬を与える
            base.AddReward(+0.01f);
        }
        else
        {
            // 評価: 報酬を減らす
            base.AddReward(-0.01f);
        }

        // 落下判定
        if (this.transform.position.y < -1.0f)
        {
            // UI更新
            this.StatusText.text = "落下";

            // 評価: 報酬を減らす
            base.AddReward(-1f);

            // リセットして次のエピソードを開始
            base.EndEpisode();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("MY_TARGET"))
        {
            // UI更新
            this.StatusText.text = "クリア";

            // 評価: 報酬を与える
            base.AddReward(1.0f);

            // リセットして次のエピソードを開始
            base.EndEpisode();
        }
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log($"OnEpisodeBegin >> ");

        const float stageSize = 10f;

        // ボールの位置をランダムで指定
        this.transform.position = new Vector3(Random.Range(-stageSize, stageSize), 1f, Random.Range(-stageSize, stageSize)) + this.StageTransform.position;
        base.gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        this._rigidbody.velocity = new Vector3(0f, 0f, 0f);

        // ターゲットの位置をランダムで指定
        this.Target.position = new Vector3(Random.Range(-stageSize, stageSize), 1f, Random.Range(-stageSize, stageSize)) + this.StageTransform.position;

        // Reset the parameters when the Agent is reset.
        this.SetResetParameters();
    }

    public override void Heuristic(float[] actionsOut)
    {
        Debug.Log($"Heuristic >> ");

        actionsOut[0] = -Input.GetAxis("Horizontal");
        actionsOut[1] = +Input.GetAxis("Vertical");
    }

    public void SetBall()
    {
        // Debug.Log($"SetBall >> ");

        // //Set the attributes of the ball by fetching the information from the academy
        // this._rigidbody.mass = this.m_ResetParams.GetWithDefault("mass", 1.0f);

        // var scale = this.m_ResetParams.GetWithDefault("scale", 1.0f);
        // this.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetResetParameters()
    {
        // Debug.Log($"SetResetParameters >> ");

        this.SetBall();
    }
}
