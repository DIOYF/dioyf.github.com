using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class paowuxian : MonoBehaviour
{
    private Vector2 mouse_Pos;
    public LineRenderer line1;
    public int line1Num = 10;
    Vector3[] points1;

    public LineRenderer line2;
    public float maxForce = 2;
    int line2Num = 2;
    Vector3[] points2;

    Rigidbody2D rb2d;
    public float releaseForce;

    Vector2 release_Velocity; // 初速度
    float S; // 最大水平距离
    float t; // 水平飞行时间
    float g = 9.8f; // 重力加速度
    public Transform ground; // 地面对象
    float height; // 小球离地高度
    float xUnit = 0.1f; // X轴绘制间隔

    // 地面图层掩码
    public LayerMask groundLayer;
    // 拖拽示意物体
    public GameObject dragPoint;
    // 轨迹线七点颜色
    Vector4 fadeLine = new Vector4(1, 1, 1, 1);

    enum STATE
    {
        NONE = -1,
        IDLE = 0,
        GRAB,
        DRAG, // 
        RELEASE, //松手
        LAND, //落地
        NUM,
    }

    private STATE state = STATE.IDLE;
    private STATE next_state = STATE.NONE;

    private void Start()
    {


        // 设置线段数量
        line1.positionCount = line1Num;
        line2.positionCount = line2Num;
        // 设置线段数量
        // 获取线段数量
        points1 = new Vector3[line1Num];
        points2 = new Vector3[line2Num];
        // 获取线段数量






        rb2d = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        mouse_Pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        switch (state)
        {
            case STATE.RELEASE:
                if (rb2d.IsTouchingLayers(groundLayer) && rb2d.velocity.y == 0)
                    next_state = STATE.LAND;
                break;

            case STATE.LAND:
                if (rb2d.velocity == Vector2.zero)
                    next_state = STATE.IDLE;
                break;
        }


        if (next_state != STATE.NONE)
        {
            state = next_state;
            next_state = STATE.NONE;

            switch (state)
            {
                case STATE.GRAB:
                    rb2d.gravityScale = 0;
                    height = transform.position.y - ground.position.y;

                    line1.startColor = new Vector4(1, 1, 1, 1);
                    line2.enabled = true;
                    dragPoint.SetActive(true);
                    next_state = STATE.DRAG;
                    break;
                case STATE.RELEASE:
                    rb2d.drag = 0;
                    rb2d.gravityScale = 1;
                    rb2d.velocity = release_Velocity;

                    line2.enabled = false;
                    dragPoint.SetActive(false);
                    break;
                case STATE.LAND:
                    rb2d.drag = 0.8f;
                    break;
            }
        }


        switch (state)
        {
            // TODO: 主要的图像绘制代码
            case STATE.DRAG:
                points2[0] = transform.position;
                points2[1] = mouse_Pos;

                if (Vector3.Distance(points2[0], points2[1]) > maxForce)
                {
                    points2[1] = points2[0] + (points2[1] - points2[0]).normalized * maxForce;
                }
                line2.SetPositions(points2);

                // 末端位置
                dragPoint.transform.position = points2[1];


                release_Velocity = (points2[0] - points2[1]) * releaseForce;

                Debug.Log(release_Velocity);
                S = release_Velocity.x * (release_Velocity.y / g 
                    + Mathf.Sqrt((release_Velocity.y * release_Velocity.y / g / g) + 2 * height / g));

                xUnit = S / line1Num;

                for (int i = 0; i < line1Num; i++)
                {
                    points1[i].x = transform.position.x + i * xUnit;
                    points1[i].y = GetFuncPathY(points1[i].x);
                }
                line1.SetPositions(points1);
                break;


            case STATE.RELEASE:
                fadeLine.w -= Time.deltaTime * 2;
                Mathf.Clamp01(fadeLine.w);
                line1.startColor = fadeLine;
                break;
        }
    }


    private void OnMouseDown()
    {
        next_state = STATE.GRAB;
    }

    private void OnMouseUp()
    {
        next_state = STATE.RELEASE;
    }

    float GetFuncPathY(float x)
    {
        float y;
        y = (release_Velocity.y / release_Velocity.x) * (x - transform.position.x)
            - (g * (x - transform.position.x) * (x - transform.position.x)) / (2 * release_Velocity.x * release_Velocity.x)
            + transform.position.y;

        return y;
    }
                
                
                
}
