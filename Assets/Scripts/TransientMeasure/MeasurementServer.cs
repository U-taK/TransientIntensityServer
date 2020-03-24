using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace uOSC
{
    [RequireComponent(typeof(uOscServer))]
    public class MeasurementServer : MonoBehaviour
    {
        //計測データ保存用
        List<string> transformData = new List<string>();
        //受信データの管理番号
        int i = 0;

        double[][] soundSignals;
        //PC版生成用プレハブ
        [SerializeField]
        SettingManager setting;
        //空間基準マーカレプリカ

        
        MeasurementManager measurementManager;

        //座標送信時間,送信された座標情報
        string sendTime;
        DateTime pastTime = DateTime.Now;
        Vector3 sendPos;
        Quaternion sendRot;


        //開始ボタンで計測開始
        public void InitServer()
        {
            //サーバ立ち上げ
            var server = GetComponent<uOscServer>();
            server.onDataReceived.AddListener(OnDataReceived);
            measurementManager = GetComponent<MeasurementManager>();
            Debug.Log("Init setting");
        }
        void OnDataReceived(Message message)
        {
            //計測結果取得
            // address
            var msg = message.address;
            Debug.Log("catch address: " + msg);
            // timestamp
            //msg += "(" + message.timestamp.ToLocalTime() + ") ";
            if (msg == "PositionSender")
            {
                // values
                foreach (var value in message.values)
                {
                    transformData.Add(value.ToString());
                }
                sendTime = transformData[i];
                sendPos = new Vector3(float.Parse(transformData[i + 1]), float.Parse(transformData[i + 2]), float.Parse(transformData[i + 3]));
                sendRot = new Quaternion(float.Parse(transformData[i + 4]), float.Parse(transformData[i + 5]), float.Parse(transformData[i + 6]), float.Parse(transformData[i + 7]));
                measurementManager.PlayARecord(sendPos, sendRot);
                i += 8;
            }
            else if(msg == "InstantEnd"){
                Debug.Log("Instance finish" + message.values.ToString());
                measurementManager.beInstance = true;
                measurementManager.InsEndNum = message.values.ToString();
            }
        }

        
    }
}