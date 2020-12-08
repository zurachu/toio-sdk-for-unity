using UnityEngine;
using UnityEngine.UI;
using toio;
using System.Collections.Generic;

public class Sample_Test : MonoBehaviour
{
    Cube cube;

    UnityEngine.UI.Text textRes;
    UnityEngine.UI.Text textPositionID;
    UnityEngine.UI.Text textAngle;
    UnityEngine.UI.Text textSpeed;

    async void Start()
    {
        // UI の取得
        this.textRes = GameObject.Find("TextRes").GetComponent<Text>();
        this.textPositionID = GameObject.Find("TextPositionID").GetComponent<Text>();
        this.textAngle = GameObject.Find("TextAngle").GetComponent<Text>();
        this.textSpeed = GameObject.Find("TextSpeed").GetComponent<Text>();

        // Cube の接続
        var peripheral = await new NearestScanner().Scan();
        cube = await new CubeConnecter().Connect(peripheral);
        // モーター速度の読み取りをオンにする
        await cube.ConfigMotorRead(true);
        // コールバック登録
        cube.idCallback.AddListener("Sample_Sensor", OnUpdateID);                  // 座標角度イベント
        cube.idMissedCallback.AddListener("Sample_Sensor", OnMissedID);            // 座標角度 missedイベント
        cube.motorSpeedCallback.AddListener("Sample_Sensor", OnSpeed);             //
        cube.targetMoveCallback.AddListener("Sample_Sensor", OnTargetRespond);
        cube.multiTargetMoveCallback.AddListener("Sample_Sensor", OnTargetRespond);
    }
    public void test_reset() {cube.MultiTest();}
/*
    public void test_reset() {
        this.textRes.text =　"";
            cube.TargetMove(250,250,270,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);}
*/
    public void test_Move() {
        this.textRes.text =　"";
        cube.Move(60, 60, durationMs:2500, order:Cube.ORDER_TYPE.Strong); }

    public void test_Target() {
        this.textRes.text =　"";
            cube.TargetMove(200,200,270,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);}

    public void test_Mulit2() {
        this.textRes.text =　"";
        int[] xl = new int[]{250,200};
        int[] yl = new int[]{200,250};
        int[] al = new int[]{200,300};
        Cube.TargetRotationType[] tl = new Cube.TargetRotationType[]{
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.AbsoluteClockwise};

        cube.MultiTargetMove(xl,yl,al,tl,0,20,
                            Cube.TargetMoveType.RotatingMove,30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.MultiWriteType.Write,
                            Cube.ORDER_TYPE.Strong);}

    public void test_Mulit3() {
        this.textRes.text =　"";
        int[] xl = new int[]{250,200,200};
        int[] yl = new int[]{200,250,200};
        int[] al = new int[]{200,300,200};
        Cube.TargetRotationType[] tl = new Cube.TargetRotationType[]{
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.AbsoluteClockwise};

        cube.MultiTargetMove(xl,yl,al,tl,0,20,
                            Cube.TargetMoveType.RotatingMove,30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.MultiWriteType.Write,
                            Cube.ORDER_TYPE.Strong);}


    public void test_Acc() {cube.AccelerationMove(20,2,100,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);}


    public void Update(){}

    public void OnTargetRespond(Cube c, int configID, Cube.TargetMoveRespondType Res)
    {
        this.textRes.text = "c:" +configID.ToString() + " r:" +Res.ToString();
    }

    public void OnUpdateID(Cube c)
    {
        this.textPositionID.text = "PositionID:" + " X=" + c.pos.x.ToString() + " Y=" + c.pos.y.ToString();
        this.textAngle.text = " Angle: " + c.angle.ToString();
    }

    public void OnMissedID(Cube c)
    {
        this.textPositionID.text = "PositionID Missed";
        this.textAngle.text = "Angle Missed";
    }

    public void OnSpeed(Cube c)
    {
        this.textSpeed.text = "Speed:" + " L=" + c.leftSpeed.ToString() + " R=" + c.rightSpeed.ToString();
    }
}
