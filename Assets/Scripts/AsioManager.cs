using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using AsioCSharpDll;
using System;
using System.Runtime.InteropServices;
using System.IO;

public class AsioManager : MonoBehaviour {

    // From c++ Dll (unmanaged)
    [DllImport("AsioCppDll")]
    public static extern int GetAsioDriverSum();

    [DllImport("AsioCppDll")]
    public static extern IntPtr GetAsioDriverNames(int j);

    [DllImport("AsioCppDll")]
    public static extern void SelectAsioDriver(int k);

    [DllImport("AsioCppDll")]
    public static extern void ConfigSampleRateLength(int sampleRate, int isamplingLength, int osamplingLength);

    [DllImport("AsioCppDll")]
    public static extern bool StartAsioMain2(IntPtr tspMem);

    [DllImport("AsioCppDll")]
    public static extern bool StartAsioMain();

    [DllImport("AsioCppDll")]
    public static extern void startSound();

    [DllImport("AsioCppDll")]
    public static extern void GetFourSoundSignal2(IntPtr tempMem0, IntPtr tempMem1, IntPtr tempMem2, IntPtr tempMem3, IntPtr tspMem);

    [DllImport("AsioCppDll")]
    public static extern void GetFourSoundSignal(IntPtr tempMem0, IntPtr tempMem1, IntPtr tempMem2, IntPtr tempMem3);

    [DllImport("AsioCppDll")]
    public static extern void StopAsioMain();

    static int length_bit;
    static TextAsset csvFile;
    List<double> csvDatas = new List<double>(); // CSVの中身を入れるリスト;
    static System.Numerics.Complex[][] calibdata = new System.Numerics.Complex[4][];
    /// <summary>
    /// 使えるASIOドライバー名を取得(IntPtr型) -> IDと一緒にString型に変換して返す
    /// </summary>
    /// <param name="asioDriverSum">ドライバーの総数</param>
    /// <returns></returns>
    public static string[] GetAsioDriverNames()
    {
        //ASIOドライバーの数を取得
        int asioDriverSum = GetAsioDriverSum();

        Debug.Log(asioDriverSum);
        //ドライバー数の分だけ名前を格納する配列を用意
        string[] outputAsioDrivers = new string[asioDriverSum];

        for (int asioDriverID = 0; asioDriverID < asioDriverSum; asioDriverID++)
        {
            IntPtr ptrAsioName = GetAsioDriverNames(asioDriverID);
            string tempAsioDriverName = Marshal.PtrToStringAnsi(ptrAsioName);
            outputAsioDrivers[asioDriverID] = asioDriverID.ToString() + ": " + tempAsioDriverName;
        }
        return outputAsioDrivers;
    }



    /// <summary>
    /// ASIOを起動する関数
    /// </summary>
    public static void PrepareAsio2(int asioDriverID, int sampleRate, int sampleLength, double[] tspSignal)
    {
        //TSP準備
        // int osampleLength = 1048576;
        int osampleLength = 4096;

        int[] tsp_int = new int[osampleLength];
        for (int i = 0; i < osampleLength; i++)
        {
            tsp_int[i] = (int)(100000000 * tspSignal[i]);
        }/*
        int[] tsp_int = new int[tspSignal.Length];
        for (int i = 0; i < tspSignal.Length; i++)
        {
            tsp_int[i] = (int)(4000 * tspSignal[i]);
        }
        */
        IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * osampleLength);
        Marshal.Copy(tsp_int, 0, ptr, osampleLength);
        /*
        IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * tspSignal.Length);
        Marshal.Copy(tsp_int, 0, ptr, tspSignal.Length);*/
        //Marshal.Copy(tsp_int, 0, ptr, sampleLength);
        //Marshal.Copy(tspSignal, 0, ptr, sampleLength);
        SelectAsioDriver(asioDriverID);
        Debug.Log("leng:" + tspSignal.Length);
        for (int i = 100000; i < 100010; i++)
        {
            Debug.Log(i + ":" + tspSignal[i]);
        }

        //Debug.Log(asioDriverID);

        ConfigSampleRateLength(sampleRate, sampleLength, osampleLength);

        //Debug.Log(sampleRate);
        //Debug.Log(sampleLength);

        if (StartAsioMain2(ptr))
        {
            Debug.Log("Asio start");
        }
        else Debug.Log("Asio Startできなかった");
    }



    /// <summary>
    /// ASIOを起動する関数
    /// </summary>
    public static string PrepareAsio(int asioDriverID, int sampleRate, int sampleLength)
    {
        SelectAsioDriver(asioDriverID);
        ConfigSampleRateLength(sampleRate, sampleLength, 4096);
        if (StartAsioMain())
        {
            Debug.Log("Asio start");
            return "Asio start";
        }
        else Debug.Log("Asio Startできなかった");
        return "Asio Startできなかった";
    }

    /// <summary>
    /// ASIOを起動する関数
    /// </summary>
    public static void PrepareAsio2(int asioDriverID, int sampleRate, int isampleLength, int osampleLength, double[] signal)
    {

        int[] sig_int = new int[osampleLength];
        for (int i = 0; i < osampleLength; i++)
        {
            sig_int[i] = (int)(100000000 * signal[i]);
        }

        IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * osampleLength);
        Marshal.Copy(sig_int, 0, ptr, osampleLength);
        /*
        IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * tspSignal.Length);
        Marshal.Copy(tsp_int, 0, ptr, tspSignal.Length);*/
        //Marshal.Copy(tsp_int, 0, ptr, sampleLength);
        //Marshal.Copy(tspSignal, 0, ptr, sampleLength);
        SelectAsioDriver(asioDriverID);
        Debug.Log("leng:" + signal.Length);
        /* for (int i = 100000; i < 100010; i++)
         {
             Debug.Log(i + ":" + tspSignal[i]);
         }*/

        //Debug.Log(asioDriverID);

        ConfigSampleRateLength(sampleRate, isampleLength, osampleLength);

        if (StartAsioMain2(ptr))
        {
            Debug.Log("Asio start");

            //asioが起動しているならキャリブレーションの値を読み込む
            ReadCalib();

        }
        else Debug.Log("Asio Startできなかった");
    }

    public static void StopAsio()
    {
        StopAsioMain();
    }

    /// <summary>
    /// Asioから音圧信号(IntPtr型)を取得してdouble配列に変換して返す
    /// TSPも
    /// </summary>
    /// <param name="sampleLength">サンプル長</param>
    /// <returns>音圧信号のジャグ配列 hoge["マイクのID番号"]["サンプル"]</returns>
    public static double[][] GetAsioSoundSignals2(int sampleLength, double[] tspSignal)
    {
        //Asioから取得してくるIntPtr型の音圧信号配列
        IntPtr[] ptrSoundSignals = new IntPtr[4];
        for (int micID = 0; micID < 4; micID++)
        {
            ptrSoundSignals[micID] = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * sampleLength);
        }
        IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * sampleLength);
        Marshal.Copy(tspSignal, 0, ptr, sampleLength);
        //AsioからIntPtr型を取得
        GetFourSoundSignal2(ptrSoundSignals[0], ptrSoundSignals[1], ptrSoundSignals[2], ptrSoundSignals[3], ptr);

        //一時的に保管するint型の音圧信号配列
        int[][] tempSoundSignals = new int[4][];
        //出力するdouble型の音圧信号配列
        double[][] outSoundSignals = new double[4][];

        for (int micID = 0; micID < 4; micID++)
        {
            tempSoundSignals[micID] = new int[sampleLength];
            outSoundSignals[micID] = new double[sampleLength];

            //IntPtr -> int
            Marshal.Copy(ptrSoundSignals[micID], tempSoundSignals[micID], 0, sampleLength);

            for (int sample = 0; sample < sampleLength; sample++)
            {
                //outputSoundSignals[micID][sample] = (double)calibValue[0] * (double)tempIntMic0[sample] / (double)Mathf.Pow(10, 8);
                outSoundSignals[micID][sample] = (double)tempSoundSignals[micID][sample] / (double)Mathf.Pow(10, 8);
            }
        }
        //アンマネージ配列のメモリ解放
        Marshal.FreeCoTaskMem(ptr);
        return outSoundSignals;
    }

    /// <summary>
    /// Asioから音圧信号(IntPtr型)を取得してdouble配列に変換して返す
    /// こっちを使用
    /// </summary>
    /// <param name="sampleLength">サンプル長</param>
    /// <returns>音圧信号のジャグ配列 hoge["マイクのID番号"]["サンプル"]</returns>
    public static double[][] GetAsioSoundSignals(int sampleLength)
    {
        // サンプル数の2の乗数を計算
        length_bit = (int)(Mathf.Log(sampleLength, 2f));
        startSound();
        //Asioから取得してくるIntPtr型の音圧信号配列
        IntPtr[] ptrSoundSignals = new IntPtr[4];
        for (int micID = 0; micID < 4; micID++)
        {
            ptrSoundSignals[micID] = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * sampleLength);
        }
        //AsioからIntPtr型を取得
        GetFourSoundSignal(ptrSoundSignals[0], ptrSoundSignals[1], ptrSoundSignals[2], ptrSoundSignals[3]);

        //一時的に保管するint型の音圧信号配列
        int[][] tempSoundSignals = new int[4][];
        //出力するdouble型の音圧信号配列
        double[][] outSoundSignals = new double[4][];
        System.Numerics.Complex[] temp = new System.Numerics.Complex[sampleLength];
        System.Numerics.Complex[] calib = new System.Numerics.Complex[sampleLength];
        double[] outIFFT = new double[sampleLength];
        for (int micID = 0; micID < 4; micID++)
        {
            tempSoundSignals[micID] = new int[sampleLength];
            outSoundSignals[micID] = new double[sampleLength];

            //IntPtr -> int
            Marshal.Copy(ptrSoundSignals[micID], tempSoundSignals[micID], 0, sampleLength);

            for (int sample = 0; sample < sampleLength; sample++)
            {
                outIFFT[sample] = (double)tempSoundSignals[micID][sample] / (double)Mathf.Pow(10, 8);
                //temp[sample] = new System.Numerics.Complex((double)tempSoundSignals[micID][sample] / (double)Mathf.Pow(10, 8), 0);
            }
            //キャリブレーション

            /*AcousticMathNew.FFT(length_bit, temp, out calib);
            for (int sample = 0; sample < sampleLength; sample++)
            {
                //キャリブレーション
                temp[sample] = System.Numerics.Complex.Divide(calib[sample], calibdata[micID][sample]);
                //バンドパスフィルタ
            }
            //出力信号に戻す
            AcousticMathNew.IFFT(length_bit, temp, out outIFFT);*/
            //バンドパスフィルタ
            AcousticMathNew.BPFilter(outIFFT, out outSoundSignals[micID], sampleLength);
        }
        return outSoundSignals;
    }

    /// <summary>
    /// キャリブレーションのデータをresourceから読み込む
    /// </summary>
    public static void ReadCalib()
    {
        int sampleLength = 4096;

        csvFile = Resources.Load("calibdata") as TextAsset;
        StringReader reader = new StringReader(csvFile.text);
        // , で分割しつつ一行ずつ読み込み
        // リストに追加していく
        int loop = 0;
        while (reader.Peek() != -1) // reader.Peaekが-1になるまで
        {
            calibdata[loop] = new System.Numerics.Complex[sampleLength];
            string[] lineR = reader.ReadLine().Split(','); // 実部と虚部をそれぞれ読み込み
            string[] lineI = reader.ReadLine().Split(',');
            for (int sample = 0; sample < sampleLength; sample++)
            {
                calibdata[loop][sample] = new System.Numerics.Complex(double.Parse(lineR[sample]), double.Parse(lineI[sample]));
            }
            loop++;
        }
    }
    /*
    #region ForDebugReasion
    private void Start()
    {
        string[] asioDrivers = GetAsioDriverNames();
        foreach(string asio in asioDrivers)
        {
            Debug.Log(asio);
        }
        PrepareAsio2(1, 44100, 512);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            try
            {
                double[][] test = GetAsioSoundSignals(512);
                Debug.Log(test[0][0]);
                Debug.Log("OK");
            }
            catch
            {
                Debug.Log("だめや");
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            StopAsioMain();
        }
    }
    #endregion*/
}

//ToDo : AsioDll編集すればこのクラス自体必要なくなりそう