using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

namespace uOSC
{
    public class SettingManager : MonoBehaviour
    {

        uOscClient oscClient;
        IntensityClient intensityClient;

        //Setting
        //IPアドレス
        // public InputField iPaddress;
        //ポート番号
        // public InputField port;
        //マイクロホン間隔
        public InputField MicInterval;
        int samplingRate;
        //分析周波数
        public InputField FreqMax;
        public InputField FreqMin;
        //気温
        public InputField Temperature;
        //大気圧
        public InputField Atm;
        //オブジェクトサイズ
        public InputField oSize;
        //保存ディレクトリ
        public InputField SaveDirectory;

        //最低レベル
        public InputField LevelMin;

        public Text animNum;
        //最大レベル
        public InputField LevelMax;
        public static float micInterval;
        public static float freqMin;
        public static float freqMax;
        public static float temperature;
        public static float atm;
        public static float objInterval = 0.01f;
        public static string saveDirPath;
        public static int colormapID = 2;
        public static float lvMin = 60;
        public static float lvMax = 105;
        public static float objSize = 0.05f;
        public static float AtmDensity = 1.1f;

        int t = 0;
        // Use this for initialization
        void Start()
        {
           
        }

       /* public void InitParam()
        {
            oscClient = gameObject.GetComponent<uOscClient>();
            intensityClient = gameObject.GetComponent<IntensityClient>();
            //IPアドレスとポート番号指定
           // oscClient.GetIP(iPaddress.text);
           // oscClient.GetPort(port.text);
           
            intensityClient.InitSetting(MicInterval.text, FreqMax.text, FreqMin.text, Temperature.text, Atm.text, Span.text);
            return;
        }*/

        public void InitParam4Sharing(out float mInterval, out float fMax, out float fMin, out float atmDensity)
        {
            saveDirPath = SaveDirectory.text;
            if(!float.TryParse(MicInterval.text, out micInterval))
                micInterval = 0.05f;
            if (!float.TryParse(FreqMax.text, out freqMax))
                freqMax = 1414f;
            if (!float.TryParse(FreqMin.text, out freqMin))
                freqMin = 707f;
            
            temperature = float.Parse(Temperature.text);
            atm = float.Parse(Atm.text);
            atmDensity = CalculateAtmDensity(atm, temperature);
            mInterval = micInterval;
            fMax = freqMax;
            fMin = freqMin;
            return;
        }

        public void InitParam4trans()
        {
            saveDirPath = SaveDirectory.text;
            if (!float.TryParse(MicInterval.text, out micInterval))
                micInterval = 0.05f;
            if (!float.TryParse(FreqMax.text, out freqMax))
                freqMax = 1414f;
            if (!float.TryParse(FreqMin.text, out freqMin))
                freqMin = 707f;
            if (!float.TryParse(oSize.text, out objSize))
                objSize = 0.05f;
            temperature = float.Parse(Temperature.text);
            atm = float.Parse(Atm.text);
            AtmDensity = CalculateAtmDensity(atm, temperature);
            
                colormapID = 2;
            if (!float.TryParse(LevelMin.text, out lvMin))
                lvMin = 65;
            if (!float.TryParse(LevelMax.text, out lvMax))
                lvMax = 105;
            return;
        }
        public void ServerSetting(out float oInterval)
        {
           // if (!float.TryParse(Span.text, out objInterval))
                objInterval = 0.5f;
            oInterval = objInterval;
        }
        float CalculateAtmDensity(float atm, float temp)
        {
            //大気密度の計算法:ρ=P/{R(t+273.15)} ただしRは乾燥空気の気体定数2.87としている
            return atm / (2.87f * (temp + 273.15f));
        }

        public void SettingSave()
        {
            //設定値メモ保存
                string settingTxtPath = saveDirPath + @"\setting.txt";
                StreamWriter settingSW = new StreamWriter(settingTxtPath, false, System.Text.Encoding.GetEncoding("shift_jis"));
            //    settingSW.WriteLine("MeasurePointNum : " + dataStorages.Count.ToString());
                settingSW.WriteLine("sampleRate : " + IPportGetter.SampleRate);
                settingSW.WriteLine("sampleLength : " + IPportGetter.SampleLength);
                settingSW.WriteLine("freqRange : " + FreqMin.text + " - " + FreqMax.text);
                settingSW.WriteLine("Mic size : " +MicInterval.text);
                settingSW.WriteLine("atmPressure : " + Atm.text);
                settingSW.WriteLine("temperature : " + Temperature.text);
                settingSW.Write("Measure point interval : " + objInterval.ToString());
                settingSW.Close();
            return;
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.O))
            {
                animNum.text = t.ToString();
                if (t < 4096)
                    t++;
                else
                    t = 0;
            }
            if (Input.GetKey(KeyCode.P))
            {
                animNum.text = t.ToString();
                if (t > 0)
                    t--;
                else
                    t = 4096;
            }

        }
    }
}
