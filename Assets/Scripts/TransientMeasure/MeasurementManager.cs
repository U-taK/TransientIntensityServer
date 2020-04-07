using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace uOSC {
    public class MeasurementManager : MonoBehaviour {
        //計測時間サンプル数
        int sNum = 0;
        //送信データの管理番号
        int j = 0;
        List<DataStorage> dataStorages = new List<DataStorage>();

        public List<float> sintensities = new List<float>();
        //設定値
        int sampleRate = 44100;
        //入力側lengthbit
        int isampleLength = 4096;
        //lengthbit
        int length_bit;
        //出力側lengthbit(2のべき乗)
        //int osampleLength = 2048;
        int osampleLength = 32768;
        //録音された音圧信号 [マイクID][サンプル
        double[][] soundSignals;

        //計測番号
        int Num = 0;
        uOscClient client;

        //計測番号と計測結果の連想配列
        Dictionary<int, GameObject> intensities = new Dictionary<int, GameObject>();
        [SerializeField]
        SettingManager setting;

        //PC版生成用プレハブ
        [SerializeField]
        GameObject Cone;
        //空間基準マーカレプリカ
        [SerializeField]
        GameObject copyStandard;

        //送信番号
        int sendNum;
        //送信オブジェクト
        GameObject sendInstant;
        //送信用floatリスト
        List<object> sendStorage = new List<object>();

        //インスタンスオブジェクト生成可能かチェック
        public bool beInstance = true;
        //生成済み番号
        public string InsEndNum;
        /// <summary>
        /// 出力信号読込
        /// </summary>
        /// <returns></returns>
        public double[] readTsp()
        {
            //byteファイル風読み込み
            //TextAsset asset = Resources.Load("whitenoise", typeof(TextAsset)) as TextAsset;
            //TextAsset asset = Resources.Load("SIN1kHz", typeof(TextAsset)) as TextAsset;
            TextAsset asset = Resources.Load("SIN1kHzsMin", typeof(TextAsset)) as TextAsset;
            //読込
            Debug.Log("音源長さ:" + asset.bytes.Length / 2);
            Debug.Log("sampleLength:" + osampleLength);

            double[] tspSignal = new double[asset.bytes.Length / 8];
            tspSignal = readArray.bytes2array(asset, asset.bytes.Length / 8);
            return tspSignal;
        }

        private void Awake()
        {
            length_bit = (int)(Mathf.Log(isampleLength, 2f));
            //ドライバー名デバッグ
            string[] asioDriverIDNames = AsioManager.GetAsioDriverNames();
            foreach (string asioDriverIDName in asioDriverIDNames)
            {
                Debug.Log(asioDriverIDName);
            }
            //tsp読込
            double[] oSignal = readTsp();
            Debug.Log("音源長さ:" + oSignal.Length);
            Debug.Log("sampleLength:" + osampleLength);

            //ASIOスタート
            AsioManager.PrepareAsio2(3, sampleRate, isampleLength, osampleLength, oSignal);
        }

        //計測準備完了(開始ボタンに付与)
        public void InitSetting()
        {
            setting.InitParam4trans();
            client = this.gameObject.GetComponent<uOscClient>();
            client.Address();
            //計測用HoloからユニキャストでデータをもらうとマルチキャストでHolo(観測側)に基本設定を送信し計測を始める
            client.Send("SettingSender", SettingManager.colormapID, SettingManager.lvMin, SettingManager.lvMax, SettingManager.objSize);
            Debug.Log("Init setting in client");
        }

        public void ResendSetting()
        {
            setting.InitParam4trans();
            //計測用HoloからユニキャストでデータをもらうとマルチキャストでHolo(観測側)に基本設定を送信し計測を始める
            client.Send("SettingSender", SettingManager.colormapID, SettingManager.lvMin, SettingManager.lvMax, SettingManager.objSize);

        }


        //計測開始
        public void PlayARecord(Vector3 sendPos, Quaternion sendRot)
        {
            StartCoroutine(RecordSignal(sendPos, sendRot));
        }

        private IEnumerator RecordSignal(Vector3 sendPos, Quaternion sendRot)
        {
            //音声再生
            //AsioManager.startSound();
            //録音のlengthbit分待つ
            yield return new WaitForSeconds(4096f / 44100f);
            //録音開始
            soundSignals = AsioManager.GetAsioSoundSignals(isampleLength);

            //瞬時音響インテンシティ計算
            var intensityDirection = AcousticSI.DirectMethod(soundSignals, SettingManager.AtmDensity, SettingManager.micInterval);

            //直接法計算
            var sumIntensity = AcousticSI.SumIntensity(intensityDirection);
            sintensities.Add(AcousticMathNew.CalcuIntensityLevel(sumIntensity));
            //データ送信
            client.Send("ResultSend", sendPos.x, sendPos.y, sendPos.z, sendRot.x, sendRot.y, sendRot.z, sendRot.w, sumIntensity.x, sumIntensity.y, sumIntensity.z, Num);
            yield return null;

            //PCがわ表示
            float[] intensityLv = new float[intensityDirection.Length];
            for (int i = 0; i < intensityDirection.Length; i++)
            {
                intensityLv[i] = AcousticMathNew.CalcuIntensityLevel(intensityDirection[i]);
            }
            Color color = ColorBar.DefineColor(SettingManager.colormapID, AcousticMathNew.CalcuIntensityLevel(sumIntensity), SettingManager.lvMin, SettingManager.lvMax);
            GameObject instant = CreateInstantObj(Num, sendPos, sendRot, sumIntensity, color, intensityDirection, intensityLv);
            DataStorage data = new DataStorage(Num, sendPos, sendRot, soundSignals, sumIntensity);
            dataStorages.Add(data);
            Num++;
            yield return null;
        }

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                AsioManager.startSound();
                Debug.Log("再生したよ");
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                AsioManager.GetAsioSoundSignals(4096);
                Debug.Log("再生したよ");
            }

        }

        private void OnApplicationQuit()
        {
            AsioManager.StopAsioMain();
        }
        //オブジェクト生成
        public GameObject CreateInstantObj(int No, Vector3 micPos, Quaternion micRot, Vector3 intensity, Color vecColor, Vector3[] iintensities, float[] levels)
        {
            GameObject msPoint = new GameObject("measurepoint" + No);
            msPoint.transform.parent = copyStandard.transform;
            msPoint.transform.localPosition = micPos;
            msPoint.transform.localRotation = micRot;
            intensities.Add(No, msPoint);
            GameObject VectorObj = Instantiate(Cone) as GameObject;
            VectorObj.transform.localScale = new Vector3(SettingManager.objSize, SettingManager.objSize, SettingManager.objSize * 4f);
            VectorObj.transform.parent = msPoint.transform;
            VectorObj.transform.localPosition = Vector3.zero;
            VectorObj.transform.localRotation = Quaternion.LookRotation(10000000000 * intensity);
            VectorObj.transform.GetComponent<Renderer>().material.color = vecColor;
            VectorObj.name = "IntensityObject";
            var parameter = VectorObj.AddComponent<ParameterStorage>();
            parameter.PutIntensity(iintensities, levels);
            return msPoint;
        }

        //瞬時音響インテンシティの再送信
        public void SendInstantIntensity()
        {
            beInstance = true;
            StartCoroutine("Resend");
        }

           /*   private IEnumerator Resend()
              {
                  //1オブジェクトずつデータを送信
                  foreach(KeyValuePair<int,GameObject> val in intensities)
                  {
                      beInstance = false;
                      sendNum = val.Key;
                      sendInstant = val.Value;
                      ParameterStorage param = sendInstant.GetComponentInChildren<ParameterStorage>();
                      sendStorage = new List<object>();
                      Vector3[] intensity = param.PushIntensity();
                      //1回の送信量を減らすために64個ずつ送信
                      //sendStorage.Add((float)sendNum);
                      sendStorage.Add((float)sendNum);
                      sendStorage.Add(0);

                      for (int i = 0; i < intensity.Length / 64; i++)
                      {
                          Vector3 sum = Vector3.zero;
                          for (int j = 0; j < 64; j++)
                          {
                              sum += intensity[i * 64 + j];
                          }
                          sum /= 64f;
                          sendStorage.Add(sum.x);
                          sendStorage.Add(sum.y);
                          sendStorage.Add(sum.z);
                      }
                          object[] sender = sendStorage.ToArray();
                          client.Send("InstanceSend", sender);
                          Debug.Log("Send No." + sendNum);
                          sendStorage.Clear();
                          yield return new WaitForSeconds(0.2f);

                      while(beInstance == false)
                      {
                          yield return new WaitForSeconds(1f);
                          Debug.Log("Writing Num:" + sendNum.ToString());
                          beInstance = true;
                      }
                      client.Send("InsSend finish");
                  }
                  //送信が終わればデータ通信が終了したことを伝える
                  client.Send("SendEnd");
              }*/

        private IEnumerator Resend()
        {
            //1オブジェクトずつデータを送信
            foreach (KeyValuePair<int, GameObject> val in intensities)
            {
                beInstance = false;
                sendNum = val.Key;
                sendInstant = val.Value;
                ParameterStorage param = sendInstant.GetComponentInChildren<ParameterStorage>();
                sendStorage = new List<object>();
                Vector3[] intensity = param.PushIntensity();
                //1回の送信量を減らすために64個ずつ送信
                //sendStorage.Add((float)sendNum);

                int k = 0;
                for (int i = 0; i < 2048 / 64; i++)
                {
                    sendStorage.Add((float)sendNum);
                    sendStorage.Add(k);
                    for (int j = 0; j < 64; j++)
                    {
                        sendStorage.Add(intensity[i * 64 + j].x);
                        sendStorage.Add(intensity[i * 64 + j].y);
                        sendStorage.Add(intensity[i * 64 + j].z);
                    }
                    object[] sender = sendStorage.ToArray();
                    client.Send("InstanceSend", sender);
                    sendStorage.Clear();
                    k++;
                    yield return new WaitForSeconds(0.2f);
                }
                
                Debug.Log("Send No." + sendNum);
                sendStorage.Clear();
                yield return new WaitForSeconds(0.2f);

                while (beInstance == false)
                {
                    yield return new WaitForSeconds(1f);
                    Debug.Log("Writing Num:" + sendNum.ToString());
                    beInstance = true;
                }
                client.Send("InsSend finish");
            }
            //送信が終わればデータ通信が終了したことを伝える
            client.Send("SendEnd");
        }

        /// <summary>
        /// バイナリデータセーブ
        /// </summary>
        public void SaveBinaryData()
        {
            Debug.Log(SettingManager.saveDirPath);
            //ディレクトリなかったら作成
            SafeCreateDirectory(SettingManager.saveDirPath);

            //録音＆マイク位置バイナリファイル保存
            for (int dataIndex = 0; dataIndex < dataStorages.Count; dataIndex++)
            {
                string Filteredname = SettingManager.saveDirPath + @"\measurepoint_" + (dataIndex + 1).ToString() + ".bytes";
                FileStream fs = new FileStream(Filteredname, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);

                for (int micID = 0; micID < 4; micID++)
                {
                    for (int sample = 0; sample < dataStorages[dataIndex].soundSignal[micID].Length; sample++)
                    {
                        bw.Write(dataStorages[dataIndex].soundSignal[micID][sample]);
                    }
                }

                bw.Write((double)dataStorages[dataIndex].micLocalPos.x);
                bw.Write((double)dataStorages[dataIndex].micLocalPos.y);
                bw.Write((double)dataStorages[dataIndex].micLocalPos.z);

                bw.Write((double)dataStorages[dataIndex].micLocalRot.x);
                bw.Write((double)dataStorages[dataIndex].micLocalRot.y);
                bw.Write((double)dataStorages[dataIndex].micLocalRot.z);
                bw.Write((double)dataStorages[dataIndex].micLocalRot.w);

                bw.Close();
                fs.Close();
            }
            //SettingManager.plotNumber = dataStorages.Count;
            setting.SettingSave();
        }

        /// <summary>
        /// 1サンプルごとのデータをバイナリファイルに取得していく
        /// </summary>
        public void SaveInstanceBinaryData(Vector3 sendPos, Quaternion sendRot)
        {
            //ディレクトリなかったら作成
            SafeCreateDirectory(SettingManager.saveDirPath);
            //もう一つディレクトリを作成
            var savePath = SettingManager.saveDirPath + @"\instance";
            SafeCreateDirectory(savePath);

            //録音＆マイク位置バイナリファイル保存
            string Filteredname = savePath + @"\instancepoint_" + (sNum++).ToString() + ".bytes";
            FileStream fs = new FileStream(Filteredname, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);


            var soundSignal = AsioManager.GetAsioSoundSignals(256);

            for (int micID = 0; micID < 4; micID++)
            {
                for (int sample = 0; sample < 4096; sample++)
                {
                    bw.Write(soundSignal[micID][sample]);
                }
            }

            bw.Write((double)sendPos.x);
            bw.Write((double)sendPos.y);
            bw.Write((double)sendPos.z);

            bw.Write((double)sendRot.x);
            bw.Write((double)sendRot.y);
            bw.Write((double)sendRot.z);
            bw.Write((double)sendRot.w);

            bw.Close();
            fs.Close();
        }



        /// <summary>
        /// 指定したパスにディレクトリが存在しない場合
        /// すべてのディレクトリとサブディレクトリを作成します
        /// </summary>
        public static DirectoryInfo SafeCreateDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                return null;
            }
            return Directory.CreateDirectory(path);
        }
    }
    }