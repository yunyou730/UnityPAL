
using System;
using UnityEditor.TextCore.Text;
using UnityEngine;
using UnityEngine.Windows;

public class MKFLoader
{
    private string _filePath;
    private byte[] _bytes;

    private static bool sIsWin95Ver = false;

    public MKFLoader(string filePath)
    {
        _filePath = filePath;
        Debug.Log($"[MKFLoader]is system little endian:{BitConverter.IsLittleEndian}");
    }

    public void Load()
    {
        _bytes = File.ReadAllBytes(_filePath);
        int chunkCount = GetChunkCount();
        Debug.Log($"chunkCount:{chunkCount}");
        for (int i = 0;i < chunkCount;i++)
        {
            int chunkSize = GetChunkSize(i);
            int decompSize = GetDecompressedSize(i);
            Debug.Log($"index:{i},size:{chunkSize},decom_Size:{decompSize}");
        }
    }
    
    /*
    Purpose:Get the number of chunks in an MKF archive.
    Parameters:[IN]  fp - pointer to an fopen'ed MKF file.
    Return value:Integer value which indicates the number of chunks in the specified MKF file.
    */
    public int GetChunkCount()
    {
        int chunkCount = BitConverter.ToInt32(_bytes, 0);   // 4字节,32位
        chunkCount = (chunkCount - 4) >> 2;
        return chunkCount;
    }

    /*++
      Purpose:Get the size of a chunk in an MKF archive.

      Parameters:
        [IN]  uiChunkNum - the number of the chunk in the MKF archive.
        [IN]  fp - pointer to the fopen'ed MKF file.

      Return value:
        Integer value which indicates the size of the chunk.
        -1 if the chunk does not exist.
    --*/
    public int GetChunkSize(int chunkIndex)
    {
        int chunkCount = GetChunkCount();
        if (chunkIndex >= chunkCount)
        {
            return -1;
        }
        uint offset = BitConverter.ToUInt32(_bytes, 4 * chunkIndex);      // 当前chunk数据在文件系统里的 offset
        uint nextOffset = BitConverter.ToUInt32(_bytes, 4 * chunkIndex + 4);  // 下一个chunk数据在文件系统里的 offset
        return (int)(nextOffset - offset);  // 因为内存紧凑排列,因此 下一个chunk的offset - 当前chunk的offset,就是当前chunk 的 size
    }

    /*++
    Purpose:Read a chunk from an MKF archive into lpBuffer.
    Parameters:
        [OUT] lpBuffer - pointer to the destination buffer.
        [IN]  uiBufferSize - size of the destination buffer.
        [IN]  uiChunkNum - the number of the chunk in the MKF archive to read.
        [IN]  fp - pointer to the fopen'ed MKF file.
    Return value:
        Integer value which indicates the size of the chunk.
        -1 if there are error in parameters.
        -2 if buffer size is not enough.
    --*/
    public byte[] ReadChunk(int chunkIndex)
    {
        int chunkSize = GetChunkSize(chunkIndex);
        if (chunkSize == -1 || chunkSize <= 0)
        {
            return null;
        }
        byte[] chunkData = new byte[chunkSize];
        int offset = BitConverter.ToInt32(_bytes, 4 * chunkIndex);
        Array.Copy(_bytes, offset, chunkData, 0, chunkSize);
        return chunkData;
    }
    
    /*++
      Purpose:Get the decompressed size of a compressed chunk in an MKF archive.
      Parameters:
        [IN]  uiChunkNum - the number of the chunk in the MKF archive.
        [IN]  fp - pointer to the fopen'ed MKF file.
      Return value:
        Integer value which indicates the size of the chunk.
        -1 if the chunk does not exist.
    --*/
    int GetDecompressedSize(int chunkIndex)
    {
        int chunkCount = GetChunkCount();
        if (chunkIndex >= chunkCount)
        {
            return -1;
        }
        
        int offset = BitConverter.ToInt32(_bytes, 4 * chunkIndex);

        Debug.Assert(!sIsWin95Ver,"temp not support win95 version PAL");
        
        uint[] buf = new uint[2];
        buf[0] = BitConverter.ToUInt32(_bytes, offset); 
        buf[1] = BitConverter.ToUInt32(_bytes, offset + 4);
        if (buf[0] != 0x315f4a59)
            return -1;
        return (int)(buf[1]);
    }
    
    /*++
    Purpose:Decompress a compressed chunk from an MKF archive into lpBuffer.
    Parameters:
        [OUT] lpBuffer - pointer to the destination buffer.
        [IN]  uiBufferSize - size of the destination buffer.
        [IN]  uiChunkNum - the number of the chunk in the MKF archive to read.
        [IN]  fp - pointer to the fopen'ed MKF file.
    Return value:
        Integer value which indicates the size of the chunk.
        -1 if there are error in parameters, or buffer size is not enough.
        -3 if cannot allocate memory for decompression.
     --*/
    int DecompressChunk()
    {
        return 0;
    }
}
