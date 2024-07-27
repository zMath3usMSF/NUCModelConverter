using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUCModelConverter
{
    internal class Block
    {
        int vertexType = 0;
        bool init = false;
        public int currentModel = 0;
        public int modelCount = 0;
        bool uvSkip = true;
        bool isChildModel = false;
        int initIndex = 0;
        int lastMainModel = 0;
        public List<List<List<float>>> vertexList = new List<List<List<float>>>();
        public List<List<float>> vertexNormalsList = new List<List<float>>();
        public List<List<float>> uvsList = new List<List<float>>();
        public List<List<int>> facesIndicesList = new List<List<int>>();
        public List<List<int>> normalsIndicesList = new List<List<int>>();
        public List<List<int>> uvsIndicesList = new List<List<int>>();
        public List<List<int>> teste = new List<List<int>>();
        public void CleanList()
        {
            vertexList.Clear();
            vertexNormalsList.Clear();
            uvsList.Clear();
            facesIndicesList.Clear();
            normalsIndicesList.Clear();
            uvsIndicesList.Clear();
            uvSkip = true;
        }
        public void InitLists()
        {
            vertexList.Add(new List<List<float>>());
            vertexNormalsList.Add(new List<float>());
            uvsList.Add(new List<float>());
            facesIndicesList.Add(new List<int>());
            normalsIndicesList.Add(new List<int>());
            uvsIndicesList.Add(new List<int>());
        }
        public void ReadChunck(MemoryStream ms, byte[] buffer)
        {
            int initt = BitConverter.ToInt32(buffer, 0);
            if(initt == 0x6E01C000)
            {
                init = true;
            }
            if(init == true)
            {
                int type = buffer[3];
                switch (type)
                {
                    case 0x62: //IDCount Int8
                        ReadListIndicesInt8(ms, buffer);
                        break;
                    case 0x66: //IDCount Int16
                        ReadListIndicesInt16(ms, buffer);
                        break;
                    case 0x68: //UVs
                        ReadUVsChunck(ms, buffer);
                        break;
                    case 0x6A: //Indexes
                        ReadIndexes(ms, buffer);
                        break;
                    case 0x6C: //Vertex
                        ReadVertexType(ms, buffer);
                        break;
                    case 0x6E: //Init Model
                        InitModel(ms, buffer);
                        break;
                    default:
                        break;
                }
            }
        }
        private void InitModel(MemoryStream ms, byte[] buffer)
        {
            ms.Read(buffer, 0, 4);
            if (buffer[0] == 0x4 && buffer[1] == 0x10)
            {
                ms.Read(buffer, 0, 4);
                if (buffer[0] == 0x2E)
                {
                    lastMainModel = vertexList.Count - 1;
                    isChildModel = false;
                    uvSkip = true;
                }
            }
        }
        private void ReadListIndicesInt16(MemoryStream ms, byte[] buffer)
        {
            int unk = buffer[0];
            for (int i = 0; i < unk; i++)
            {
                ms.Read(buffer, 0, 1);
                ms.Read(buffer, 0, 1);
            }
        }
        private void ReadListIndicesInt8(MemoryStream ms, byte[] buffer)
        {
            teste.Clear();
            teste.Add(new List<int>());
            int unk = buffer[0];
            int indexesLength = buffer[2];
            int indexesCount = 0;
            for (long j = 0; j < indexesLength; j++)
            {
                ms.Read(buffer, 0, 1);
                if(j == 0)
                {
                    indexesCount = buffer[0];
                    initIndex = buffer[0];
                }
                teste[0].Add(buffer[0]);
                if (j == indexesLength - 1)
                {
                    for (j = ms.Position; j % 4 != 0; j++)
                    {
                        ms.Read(buffer, 0, 1);
                    }
                }
            }
            vertexType = unk;
        }
        private void ReadVertexType(MemoryStream ms, byte[] buffer)
        {
            switch (vertexType)
            {
                case 0:
                    break;
                case 1:
                    ReadVertexNormalsChunck(ms, buffer);
                    break;
                case 2:
                    ReadVertexChunck(ms, buffer);
                    break;
                default:
                    break;
            }
        }
        private void ReadVertexChunck(MemoryStream ms, byte[] buffer)
        {
            if(uvSkip == true)
            {
                InitLists();
                currentModel++;
            }
            byte[] vertexBuffer = new byte[0xC];
            int vertexLength = buffer[2];
            if (vertexList[currentModel - 1].Count == 0)
            {
                for (int i = 0; i < teste[0][0] + 1; i++)
                {
                    vertexList[currentModel - 1].Add(new List<float>());
                }
            }

            for (int i = 0; i < teste[0].Count; i++)
            {
                ms.Read(vertexBuffer, 0, 0xC);
                vertexList[currentModel - 1][teste[0][i]].Add(BitConverter.ToSingle(vertexBuffer, 0));
                vertexList[currentModel - 1][teste[0][i]].Add(BitConverter.ToSingle(vertexBuffer, 4));
                vertexList[currentModel - 1][teste[0][i]].Add(BitConverter.ToSingle(vertexBuffer, 8));
                ms.Seek(4, SeekOrigin.Current);
            }
            if(isChildModel == true)
            {
                for(int i = 0; i < vertexList[currentModel - 1].Count; i++)
                {
                    if (vertexList[currentModel - 1][i].Count == 0)
                    {
                        vertexList[currentModel - 1][i].Add(vertexList[lastMainModel][i][0]);
                        vertexList[currentModel - 1][i].Add(vertexList[lastMainModel][i][1]);
                        vertexList[currentModel - 1][i].Add(vertexList[lastMainModel][i][2]);
                    }
                }
            }
            if (vertexList[currentModel - 1].Count != 0)
            {
                uvSkip = false;
            }
        }
        private void ReadVertexNormalsChunck(MemoryStream ms, byte[] buffer)
        {
            byte[] vertexBuffer = new byte[0xC];
            int vertexLength = buffer[2];
            for (int i = 0; i < vertexLength; i++)
            {
                ms.Read(vertexBuffer, 0, 0xC);
                vertexNormalsList[currentModel - 1].Insert(0, BitConverter.ToSingle(vertexBuffer, 8));
                vertexNormalsList[currentModel - 1].Insert(0, BitConverter.ToSingle(vertexBuffer, 4));
                vertexNormalsList[currentModel - 1].Insert(0, BitConverter.ToSingle(vertexBuffer, 0));
                ms.Seek(4, SeekOrigin.Current);
            }
        }
        private void ReadUVsChunck(MemoryStream ms, byte[] buffer)
        {
            int uvsCount = buffer[2];
            for (int i = 0; i < uvsCount; i++)
            {
                ms.Read(buffer, 0, buffer.Length);
                uvsList[currentModel - 1].Add(BitConverter.ToSingle(buffer, 0));
                ms.Read(buffer, 0, buffer.Length);
                uvsList[currentModel - 1].Add(BitConverter.ToSingle(buffer, 0));
                ms.Seek(4, SeekOrigin.Current);
            }
        }
        private void ReadIndexes(MemoryStream ms, byte[] buffer)
        {
            isChildModel = true;
            uvSkip = true;
            int indexesCount = buffer[2];
            indexesCount = indexesCount * 3;
            for (int j = 0; j < indexesCount; j += 3)
            {
                if (j != 0)
                {
                    ms.Read(buffer, 0, 1);
                    facesIndicesList[currentModel - 1].Add(buffer[0]);
                    ms.Read(buffer, 0, 1);
                    normalsIndicesList[currentModel - 1].Add(buffer[0]);
                    ms.Read(buffer, 0, 1);
                    uvsIndicesList[currentModel - 1].Add(buffer[0]);
                }
                else
                {
                    ms.Seek(3, SeekOrigin.Current);
                }
            }
            for (long k = ms.Position; k % 4 != 0; k++)
            {
                ms.Read(buffer, 0, 1);
            }
        }
    }
}
