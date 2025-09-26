using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ayy.pal
{
    // 结构体定义（对应C中的结构体）
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct YJ1_TreeNode
    {
        public byte value;
        public byte leaf;
        public ushort level;
        public uint weight;
        public YJ1_TreeNode* parent;
        public YJ1_TreeNode* left;
        public YJ1_TreeNode* right;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct YJ1_TreeNodeList
    {
        public YJ1_TreeNode* node;
        public YJ1_TreeNodeList* next;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct YJ_1_FILEHEADER
    {
        public uint Signature;          // 'YJ_1'
        public uint UncompressedLength; // 压缩前大小
        public uint CompressedLength;   // 压缩后大小
        public ushort BlockCount;       // 块数量
        public byte Unknown;
        public byte HuffmanTreeLength;  // 哈夫曼树长度
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)] // Pack=1 确保内存紧凑（与C结构体对齐）
    public unsafe struct YJ_1_BLOCKHEADER
    {
        public ushort UncompressedLength; // 最大0x4000
        public ushort CompressedLength;   // 包含头部

        // 固定大小缓冲区（非托管，内存嵌入结构体）
        public fixed ushort LZSSRepeatTable[4];          // 替代 ushort[]
        public fixed byte LZSSOffsetCodeLengthTable[4];  // 替代 byte[]
        public fixed byte LZSSRepeatCodeLengthTable[3];  // 替代 byte[]
        public fixed byte CodeCountCodeLengthTable[3];   // 替代 byte[]
        public fixed byte CodeCountTable[2];             // 替代 byte[]
    }

    public static unsafe class Yj1Decompressor
    {
        // // 字节序转换（模拟SDL_SwapLE16）
        // private static ushort SDL_SwapLE16(ushort value)
        // {
        //     return (ushort)(((value & 0xFF) << 8) | ((value >> 8) & 0xFF));
        // }
        //
        // // 字节序转换（模拟SDL_SwapLE32）
        // private static uint SDL_SwapLE32(uint value)
        // {
        //     return ((value & 0xFF) << 24) | ((value & 0xFF00) << 8) |
        //            ((value & 0xFF0000) >> 8) | ((value & 0xFF000000) >> 24);
        // }

        // 位读取函数（unsafe版本）
        private static uint yj1_get_bits(byte* src, ref uint bitptr, uint count)
        {
            byte* temp = src + ((bitptr >> 4) << 1);
            uint bptr = bitptr & 0xF;
            bitptr += count;

            if (count > 16 - bptr)
            {
                uint adjustedCount = count + bptr - 16;
                ushort mask = (ushort)(0xFFFF >> (int)bptr);
                ushort firstPart = (ushort)(temp[0] | (temp[1] << 8));
                ushort secondPart = (ushort)(temp[2] | (temp[3] << 8));
                return (uint)(((firstPart & mask) << (int)adjustedCount) | (secondPart >> (16 - (int)adjustedCount)));
            }
            else
            {
                ushort value = (ushort)(temp[0] | (temp[1] << 8));
                value = (ushort)(value << (int)bptr);
                value = (ushort)(value >> (int)(16 - count));
                return value;
                //return (uint)((value << (int)bptr) >> (16 - (int)count));
            }
        }

        // 循环获取函数
        private static ushort yj1_get_loop(byte* src, ref uint bitptr, YJ_1_BLOCKHEADER* header)
        {
            if (yj1_get_bits(src, ref bitptr, 1) != 0)
                return header->CodeCountTable[0];
            else
            {
                uint temp = yj1_get_bits(src, ref bitptr, 2);
                if (temp != 0)
                    return (ushort)yj1_get_bits(src, ref bitptr, header->CodeCountCodeLengthTable[temp - 1]);
                else
                    return header->CodeCountTable[1];
            }
        }

        // 计数获取函数
        private static ushort yj1_get_count(byte* src, ref uint bitptr, YJ_1_BLOCKHEADER* header)
        {
            ushort temp = (ushort)yj1_get_bits(src, ref bitptr, 2);
            if (temp != 0)
            {
                if (yj1_get_bits(src, ref bitptr, 1) != 0)
                    return (ushort)yj1_get_bits(src, ref bitptr, header->LZSSRepeatCodeLengthTable[temp - 1]);
                else
                    //return SDL_SwapLE16(header->LZSSRepeatTable[temp]);
                    return header->LZSSRepeatTable[temp];
            }
            else
            {
                //return SDL_SwapLE16(header->LZSSRepeatTable[0]);
                return header->LZSSRepeatTable[0];
            }
        }

        // 主解压缩函数
        public static int YJ1_Decompress(byte* Source, byte* Destination, int DestSize)
        {
            YJ_1_FILEHEADER* hdr = (YJ_1_FILEHEADER*)Source;
            byte* src = Source;
            byte* dest;
            uint i;
            YJ1_TreeNode* root = null;
            YJ1_TreeNode* node = null;

            if (Source == null)
                return -1;

            // 验证签名（'YJ_1'的十六进制值）
            //if (SDL_SwapLE32(hdr->Signature) != 0x315F4A59)
            if (hdr->Signature != 0x315F4A59)
                return -1;

            // 验证解压后大小是否在目标缓冲区范围内
            //if (SDL_SwapLE32(hdr->UncompressedLength) > (uint)DestSize)
            if (hdr->UncompressedLength > (uint)DestSize)
                return -1;

            // 构建哈夫曼树
            do
            {
                ushort tree_len = (ushort)(hdr->HuffmanTreeLength * 2);
                uint bitptr = 0;
                byte* flag = src + 16 + tree_len;

                // 分配哈夫曼树节点内存
                int treeSize = sizeof(YJ1_TreeNode) * (tree_len + 1);
                root = (YJ1_TreeNode*)Marshal.AllocHGlobal(treeSize);
                if (root == null)
                    return -1;

                // 初始化根节点
                root[0].leaf = 0;
                root[0].value = 0;
                root[0].left = root + 1;
                root[0].right = root + 2;

                // 构建哈夫曼树
                for (i = 1; i <= tree_len; i++)
                {
                    root[i].leaf = (byte)(yj1_get_bits(flag, ref bitptr, 1) == 0 ? 1 : 0);
                    root[i].value = src[15 + i];

                    if (root[i].leaf != 0)
                    {
                        root[i].left = null;
                        root[i].right = null;
                    }
                    else
                    {
                        root[i].left = root + (root[i].value << 1) + 1;
                        root[i].right = root[i].left + 1;
                    }
                }

                // 移动源指针到数据部分
                uint flagBytes = ((tree_len & 0xF) != 0) ? (uint)((tree_len >> 4) + 1) : (uint)(tree_len >> 4);
                src += 16 + tree_len + (flagBytes << 1);
            } while (false);

            dest = Destination;
            //ushort blockCount = SDL_SwapLE16(hdr->BlockCount);
            ushort blockCount = hdr->BlockCount;

            // 处理每个块
            for (i = 0; i < blockCount; i++)
            {
                uint bitptr;
                
                YJ_1_BLOCKHEADER* header = (YJ_1_BLOCKHEADER*)src;
                src += 4; // 移动过头部

                //ushort compressedLength = SDL_SwapLE16(header->CompressedLength);
                ushort compressedLength = header->CompressedLength;
                if (compressedLength == 0)
                {
                    // 未压缩数据，直接复制
                    //ushort uncompressedLength = SDL_SwapLE16(header->UncompressedLength);
                    ushort uncompressedLength = header->UncompressedLength;
                    while (uncompressedLength-- > 0)
                    {
                        *dest++ = *src++;
                    }
                    continue;
                }

                // 移动过块头部
                src += 20;
                bitptr = 0;

                // 处理块数据
                for (;;)
                {
                    ushort loop = yj1_get_loop(src, ref bitptr, header);
                    if (loop == 0)
                        break;

                    // 处理哈夫曼编码数据
                    while (loop-- > 0)
                    {
                        node = root;
                        while (node->leaf == 0)
                        {
                            if (yj1_get_bits(src, ref bitptr, 1) != 0)
                                node = node->right;
                            else
                                node = node->left;
                        }

                        *dest++ = node->value;
                    }

                    // 处理重复数据(LZSS)
                    loop = yj1_get_loop(src, ref bitptr, header);
                    if (loop == 0)
                        break;

                    while (loop-- > 0)
                    {
                        uint count = yj1_get_count(src, ref bitptr, header);
                        uint pos = yj1_get_bits(src, ref bitptr, 2);
                        pos = yj1_get_bits(src, ref bitptr, header->LZSSOffsetCodeLengthTable[pos]);

                        // 复制重复数据
                        while (count-- > 0)
                        {
                            *dest = *(dest - pos);
                            dest++;
                        }
                    }
                }

                // 移动到下一个块
                src = (byte*)header + compressedLength;
            }

            // 释放哈夫曼树内存
            if (root != null)
                Marshal.FreeHGlobal((IntPtr)root);

            // 返回解压后的大小
            //return (int)SDL_SwapLE32(hdr->UncompressedLength);
            return (int)(hdr->UncompressedLength);
        }

        // 公开的包装方法，方便从托管代码调用
        public static int Decompress(byte[] source, byte[] destination)
        {
            if (source == null || destination == null)
                return -1;

            fixed (byte* pSource = source, pDestination = destination)
            {
                return YJ1_Decompress(pSource, pDestination, destination.Length);
            }
        }
    }
}
