using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace uOSC
{
    public class CalcTestManager : MonoBehaviour
    {

        //データ読み込み時に使用
        int sampleLength = 4096;
        TextAsset csvFile;
        double[][] signal = new double[4][];

        //瞬時音響インテンシティの計算条件
        float atmD = 1.1923f;
        float micD = 0.05f;

        //PC版生成用プレハブ
        [SerializeField]
        GameObject Cone;
        //空間基準マーカレプリカ
        [SerializeField]
        GameObject copyStandard;

        // Use this for initialization
        void Start()
        {
            //データの読み込み
            ReadTestData();

            //瞬時音響インテンシティ計算
            var intensityDirection = AcousticSI.DirectMethod(signal, atmD, micD);

            //直接法計算
            var sumIntensity = AcousticSI.SumIntensity(intensityDirection);
            float[] intensityLv = new float[intensityDirection.Length];
            for (int i = 0; i < intensityDirection.Length; i++)
            {
                intensityLv[i] = AcousticMathNew.CalcuIntensityLevel(intensityDirection[i]);
            }
            Color color = ColorBar.DefineColor(2, AcousticMathNew.CalcuIntensityLevel(sumIntensity), 65, 105);
            GameObject instant = CreateInstantObj(sumIntensity, color, intensityDirection, intensityLv);
            
        }

        // Update is called once per frame
        void Update()
        {
            //瞬時音響インテンシティパラメータの切り替え

        }



        void ReadTestData()
        {

            //byteファイル読み込み

            TextAsset asset1 = Resources.Load("TestData/mic1", typeof(TextAsset)) as TextAsset;
            TextAsset asset2 = Resources.Load("TestData/mic2", typeof(TextAsset)) as TextAsset;
            TextAsset asset3 = Resources.Load("TestData/mic3", typeof(TextAsset)) as TextAsset;
            TextAsset asset4 = Resources.Load("TestData/mic4", typeof(TextAsset)) as TextAsset;
            signal[0] = new double[asset1.bytes.Length / 8];
            signal[1] = new double[asset2.bytes.Length / 8];
            signal[2] = new double[asset3.bytes.Length / 8];
            signal[3] = new double[asset4.bytes.Length / 8];

            signal[0] = readArray.bytes2array(asset1, asset1.bytes.Length / 8);
            signal[1] = readArray.bytes2array(asset2, asset2.bytes.Length / 8);
            signal[2] = readArray.bytes2array(asset3, asset3.bytes.Length / 8);
            signal[3] = readArray.bytes2array(asset4, asset4.bytes.Length / 8);
        }

        //オブジェクト生成
        public GameObject CreateInstantObj(Vector3 intensity, Color vecColor, Vector3[] iintensities, float[] levels)
        {
            GameObject msPoint = new GameObject("measurepoint");
            msPoint.transform.parent = copyStandard.transform;
            msPoint.transform.localPosition = Vector3.zero;
            msPoint.transform.localRotation = Quaternion.identity;
            GameObject VectorObj = Instantiate(Cone) as GameObject;
            VectorObj.transform.localScale = new Vector3(0.05f, 0.05f, 0.2f);
            VectorObj.transform.parent = msPoint.transform;
            VectorObj.transform.localPosition = Vector3.zero;
            VectorObj.transform.localRotation = Quaternion.LookRotation(10000000000 * intensity);
            VectorObj.transform.GetComponent<Renderer>().material.color = vecColor;
            VectorObj.name = "IntensityObject";
            var parameter = VectorObj.AddComponent<ParameterStorage>();
            parameter.PutIntensity(iintensities, levels);
            return msPoint;
        }
    }
}