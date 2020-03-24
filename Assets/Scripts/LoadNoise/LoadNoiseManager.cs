using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadNoiseManager : MonoBehaviour {

    public int loadNum = 69;
    //マイクロホン間隔
    float micInterval = 0.05f;
    //周波数帯域の下限
    public float freq_range_min = 707;
    //周波数帯域の上限
    public float freq_range_max = 1414;
    //オブジェクトの色情報
    public float lv_min = 65;
    public float lv_max = 105;
    //大気密度[kg/m^3]
    float atmDensity;
    public float atm = 1008.1f;
    public float temp = 28.1f;

    //取得した信号
    double[][] soundSignals;

    //表示用オブジェクト
    GameObject OutputObj;

    int n = 0;
    //インテンシティ保持
    Vector3[] intensities;
    public GameObject prefab;

    // Use this for initialization
    void Start() {
        soundSignals = ReadNoise();
        atmDensity = CalculateAtmDensity(atm, temp);
        intensities = MakeIntensity(soundSignals);
        OutputObj = GameObject.Instantiate(prefab) as GameObject;
        OutputObj.transform.localPosition = Vector3.zero;
        OutputObj.transform.localRotation = Quaternion.LookRotation(10000000000 * intensities[0]);
        OutputObj.transform.localScale = new Vector3(1, 1, 4);
        float intensityLevel = AcousticMathNew.CalcuIntensityLevel(intensities[0]);
        Color vecColor = ColorBar.DefineColor(1, intensityLevel, lv_min, lv_max);

        OutputObj.transform.GetComponent<Renderer>().material.color = vecColor;
        OutputObj.name = "VectorObject";

    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKey(KeyCode.L))
        {
            n++;
            if(n == intensities.Length)
            {
                n = 0;
            }
            OutputObj.transform.localRotation = Quaternion.LookRotation(10000000000 * intensities[n]);
            float intensityLevel = AcousticMathNew.CalcuIntensityLevel(intensities[n]);
            Color vecColor = ColorBar.DefineColor(1, intensityLevel, lv_min, lv_max);

            OutputObj.transform.GetComponent<Renderer>().material.color = vecColor;
            Debug.Log(intensities[n].x + "," + intensities[n].y + "," + intensities[n].z);
            Debug.Log(intensityLevel);
            Debug.Log("Display No." + n);
        }
    }

    public double[][] ReadNoise()
    {
        //byteファイル風読み込み
        
        //TextAsset asset5 = Resources.Load("TSP_20_48k16a", typeof(TextAsset)) as TextAsset;
        TextAsset asset1 = Resources.Load("LoadNoise/ZOOM00" + loadNum + "_Tr1", typeof(TextAsset)) as TextAsset;
        Debug.Log("LoadNoise/ZOOM0069" + loadNum + "_Tr1");
        TextAsset asset2 = Resources.Load("LoadNoise/ZOOM00" + loadNum + "_Tr2", typeof(TextAsset)) as TextAsset;
        TextAsset asset3 = Resources.Load("LoadNoise/ZOOM00" + loadNum + "_Tr3", typeof(TextAsset)) as TextAsset;
        TextAsset asset4 = Resources.Load("LoadNoise/ZOOM00" + loadNum + "_Tr4", typeof(TextAsset)) as TextAsset;
        double[][] signal = new double[4][];
        signal[0] = new double[asset1.bytes.Length / 8];
        signal[1] = new double[asset2.bytes.Length / 8];
        signal[2] = new double[asset3.bytes.Length / 8];
        signal[3] = new double[asset4.bytes.Length / 8];

        signal[0] = readArray.bytes2array(asset1, asset1.bytes.Length / 8);
        signal[1] = readArray.bytes2array(asset2, asset2.bytes.Length / 8);
        signal[2] = readArray.bytes2array(asset3, asset3.bytes.Length / 8);
        signal[3] = readArray.bytes2array(asset4, asset4.bytes.Length / 8);
        return signal;
    }

    public Vector3[] MakeIntensity(double[][] noise)
    {
        double[][] resizeNoise = new double[4][];
        resizeNoise[0] = new double[4096];
        resizeNoise[1] = new double[4096];
        resizeNoise[2] = new double[4096];
        resizeNoise[3] = new double[4096];
        List<Vector3> intensities = new List<Vector3>();
        int i = 0;
        Debug.Log(noise[0].Length);

        while (i+4096 < noise[0].Length)
        {
            for (int j = 0; j < 4096; j++) {
                resizeNoise[0][j] = noise[0][i + j];
                resizeNoise[1][j] = noise[1][i + j];
                resizeNoise[2][j] = noise[2][i + j];
                resizeNoise[3][j] = noise[3][i + j];
            }

            Vector3 intensity = AcousticMathNew.CrossSpectrumMethod(resizeNoise, 44100, 12, freq_range_min, freq_range_max, atmDensity, micInterval);
            intensities.Add(intensity);
            i += 4096;
        }
        return intensities.ToArray();
    }

    float CalculateAtmDensity(float atm, float temp)
    {
        //大気密度の計算法:ρ=P/{R(t+273.15)} ただしRは乾燥空気の気体定数2.87としている
        return atm / (2.87f * (temp + 273.15f));
    }
}

