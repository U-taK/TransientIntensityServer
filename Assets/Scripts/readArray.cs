/*
 読み込みとか保存関係
 */

using UnityEngine;
using System.IO;
using System;

class readArray{

    /// <summary>
    /// bytesファイルをdouble型配列に
    /// </summary>
    /// <param name="asset">bytesファイル</param>
    /// <param name="leng">読み込み長さ</param>
    public static double[] bytes2array(TextAsset asset, int leng)
    {
        double[] sound = new double[leng];
        using (Stream fs = new MemoryStream(asset.bytes))
        {    
            using (BinaryReader br = new BinaryReader(fs))
            {
                int i = 0;
                while (i < leng)
                {
                    //符号付2byte読み込み
                    //sound[i] = (double)br.ReadInt16();
                    sound[i] = (double)br.ReadDouble();
                    i++;
                }
            }
        }
        return sound;
    }

    /// <summary>
    /// bytesファイルをshort型配列に
    /// </summary>
    /// <param name="asset">bytesファイル</param>
    /// <param name="leng">読み込み長さ</param>
    public static short[] bytes2array_sh(TextAsset asset, int leng)
    {
        short[] sound = new short[leng];
        using (Stream fs = new MemoryStream(asset.bytes))
        {
            using (BinaryReader br = new BinaryReader(fs))
            {
                int i = 0;
                while (i < leng)
                {
                    sound[i] = br.ReadInt16();
                    i++;
                }
            }
        }
        return sound;
    }

    /// <summary>
    /// バイナリデータセーブdouble
    /// </summary>
    /// <param name="soundSignals">保存するデータ</param>
    /// <param name="saveDirPath">保存場所</param>
    /// <param name="data_name">保存名</param>
    /// <param name="leng">保存する長さ</param>
    public static void SaveBinaryData(double[] soundSignals, string saveDirPath, string data_name, string extention)
    {
        //ディレクトリなかったら作成
        SafeCreateDirectory(saveDirPath);

        //録音＆マイク位置バイナリファイル保存
        string Filteredname = saveDirPath + @"\" + data_name + extention;
        using (FileStream fs = new FileStream(Filteredname, FileMode.Create))
        {
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                for (int i = 0; i < soundSignals.Length; i++)
                {
                    bw.Write(soundSignals[i]);
                }
            }
        }
    }

    /// <summary>
    /// バイナリデータセーブshort
    /// </summary>
    /// <param name="soundSignals">保存するデータ</param>
    /// <param name="saveDirPath">保存場所</param>
    /// <param name="data_name">保存名</param>
    /// <param name="extention">拡張子</param>
    public static void SaveBinaryData_sh(short[] soundSignals, string saveDirPath, string data_name, string extention)
    {
        //ディレクトリなかったら作成
        SafeCreateDirectory(saveDirPath);

        //録音＆マイク位置バイナリファイル保存
        string Filteredname = saveDirPath + @"\" + data_name + extention;
        using (FileStream fs = new FileStream(Filteredname, FileMode.Create))
        {
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                for (int i = 0; i < soundSignals.Length; i++)
                {
                    bw.Write(soundSignals[i]);
                }
            }
        }
    }

    /// <summary>
    /// バイナリデータセーブfloat
    /// </summary>
    /// <param name="soundSignals">保存するデータ</param>
    /// <param name="saveDirPath">保存場所</param>
    /// <param name="data_name">保存名</param>
    /// <param name="leng">保存する長さ</param>
    public static void SaveBinaryData_fl(float[] soundSignals, string saveDirPath, string data_name, string extention)
    {
        //ディレクトリなかったら作成
        SafeCreateDirectory(saveDirPath);

        //録音＆マイク位置バイナリファイル保存
        string Filteredname = saveDirPath + @"\" + data_name + extention;
        using (FileStream fs = new FileStream(Filteredname, FileMode.Create))
        {
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                for (int i = 0; i < soundSignals.Length; i++)
                {
                    bw.Write(soundSignals[i]);
                }
            }
        }
    }

    /// <summary>
    /// 指定したパスにディレクトリが存在しない場合
    /// すべてのディレクトリとサブディレクトリを作成
    /// </summary>
    public static DirectoryInfo SafeCreateDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            return null;
        }
        return Directory.CreateDirectory(path);
    }

    /// <summary>
    /// バイナリデータロード
    /// </summary>
    /// <param name="loadDirPath">読みだすファイル名</param>
    /// <param name="data_name">読みだすデータ名</param>
    /// <param name="num">読みだす長さ</param>
    public static double[] LoadBinaryData(string loadDirPath, string data_name, int num)
    {
        double[] soundSignal = new double[num];

        string Filteredname = loadDirPath + @"\" + data_name + ".dat";
        /*
        FileStream fs = new FileStream(Filteredname, FileMode.Open);
        BinaryReader br = new BinaryReader(fs);
        //fs.Position = 0;
        //fs.Seek(0, SeekOrigin.Begin);
        soundSignal[0] = br.ReadInt16();
        soundSignal[1] = br.ReadInt16();
        for (int i = 0; i < num; i++)
        {
            soundSignal[i] = br.ReadInt16();
        }
        br.Close();
        fs.Close();
        */
        
        using (FileStream fs = new FileStream(Filteredname, FileMode.Open))
        {
            using (BinaryReader br = new BinaryReader(fs))
            {
                //fs.Seek(0, SeekOrigin.Begin);

                int i = 0;
                while (i < num)
                {
                    //符号付2byte読み込み
                    soundSignal[i] = br.ReadInt32();
                    i++;
                }
            }
        }
        return soundSignal;
    }

    /// <summary>
    /// バイナリデータロード
    /// </summary>
    /// <param name="loadDirPath">読みだすファイル名</param>
    /// <param name="data_name">読みだすデータ名</param>
    /// <param name="num">読みだす長さ</param>
    public static byte[] LoadBinaryData2(string loadDirPath, string data_name, int num)
    {
        double[] soundSignal = new double[num];
        
        string Filteredname = loadDirPath + @"\" + data_name + ".dat";
        /*
        FileStream fs = new FileStream(Filteredname, FileMode.Open);
        BinaryReader br = new BinaryReader(fs);
        //fs.Position = 0;
        //fs.Seek(0, SeekOrigin.Begin);
        soundSignal[0] = br.ReadInt16();
        soundSignal[1] = br.ReadInt16();
        for (int i = 0; i < num; i++)
        {
            soundSignal[i] = br.ReadInt16();
        }
        br.Close();
        fs.Close();
        */
        byte[] buf;
        using (FileStream fs = new FileStream(Filteredname, FileMode.Open, FileAccess.Read))
        {
            /*
            using (BinaryReader br = new BinaryReader(fs))
            {
                //fs.Seek(0, SeekOrigin.Begin);

                int i = 0;
                while (i < num)
                {
                    //符号付2byte読み込み
                    soundSignal[i] = br.ReadInt32();
                    i++;
                }
            }
            */
            int fileSize = (int)fs.Length;
            buf = new byte[fileSize];
            int readSize;
            int remain = fileSize;
            int bufPos = 0;
            while (remain > 0)
            {
                readSize = fs.Read(buf, 0, fileSize);
                bufPos += readSize;
                remain -= readSize;
            }
        }

        //return soundSignal;
        return buf;
    }
}
