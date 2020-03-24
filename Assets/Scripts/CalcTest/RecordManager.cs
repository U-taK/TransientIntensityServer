///<summary>
///処理したデータを都度確認するためのツール
/// 
/// </summary>

using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Numerics;

class RecordManager
{
    static CreateHeader createHeader;

    public static void Dump2File(double[] signal)
    {
        createHeader = new CreateHeader();
        string FileName = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\test.wav";

        using (FileStream filStream = new FileStream(FileName, FileMode.Create, FileAccess.Write))
        using (BinaryWriter binWriter = new BinaryWriter(filStream))
        {
            createHeader.FormatChunkSize = 16;
            createHeader.FormatID = 1;
            createHeader.Channel = 1;
            createHeader.SampleRate = 44100;
            createHeader.BitPerSample = 16;

            int NumberOfBytePerSample = ((ushort)(Math.Ceiling((double)createHeader.BitPerSample / 8)));
            createHeader.BlockSize = (short)(NumberOfBytePerSample * createHeader.Channel);
            createHeader.BytePerSec = createHeader.SampleRate * createHeader.Channel * NumberOfBytePerSample;
            int DataLength = signal.Length;
            createHeader.DataChunkSize = createHeader.BlockSize * DataLength;
            createHeader.FileSize = createHeader.DataChunkSize + 44;
            
            binWriter.Write(headerBytes());

            for (UInt32 cnt = 0; cnt < DataLength; cnt++)
            {
                double Radian = (double)cnt / createHeader.SampleRate;
                Radian *= 2 * Math.PI;

                short Data = (short)(signal[cnt] * 30000);

                binWriter.Write(BitConverter.GetBytes(Data));
            }
        }
    }

    private static byte[] headerBytes()
    {
        byte[] Datas = new byte[44];

        Array.Copy(Encoding.ASCII.GetBytes("RIFF"), 0, Datas, 0, 4);
        Array.Copy(BitConverter.GetBytes((UInt32)(createHeader.FileSize - 8)), 0, Datas, 4, 4);
        Array.Copy(Encoding.ASCII.GetBytes("WAVE"), 0, Datas, 8, 4);
        Array.Copy(Encoding.ASCII.GetBytes("fmt "), 0, Datas, 12, 4);
        Array.Copy(BitConverter.GetBytes((UInt32)(createHeader.FormatChunkSize)), 0, Datas, 16, 4);
        Array.Copy(BitConverter.GetBytes((UInt16)(createHeader.FormatID)), 0, Datas, 20, 2);
        Array.Copy(BitConverter.GetBytes((UInt16)(createHeader.Channel)), 0, Datas, 22, 2);
        Array.Copy(BitConverter.GetBytes((UInt32)(createHeader.SampleRate)), 0, Datas, 24, 4);
        Array.Copy(BitConverter.GetBytes((UInt32)(createHeader.BytePerSec)), 0, Datas, 28, 4);
        Array.Copy(BitConverter.GetBytes((UInt16)(createHeader.BlockSize)), 0, Datas, 32, 2);
        Array.Copy(BitConverter.GetBytes((UInt16)(createHeader.BitPerSample)), 0, Datas, 34, 2);
        Array.Copy(Encoding.ASCII.GetBytes("data"), 0, Datas, 36, 4);
        Array.Copy(BitConverter.GetBytes((UInt32)(createHeader.DataChunkSize)), 0, Datas, 40, 4);

        return (Datas);
    }
}


class CreateHeader
{
    public int FormatChunkSize;
    public int FormatID = 1;
    public int Channel = 1;
    public int SampleRate = 44100;
    public int BitPerSample = 16;
    public short BlockSize;
    public int BytePerSec;
    public int DataChunkSize;
    public int FileSize;
}