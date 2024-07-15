using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NUCModelConverter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string fileName = "";
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Raw Files (*.raw, *.sraw)|*.raw;*.sraw";
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                fileName = ofd.FileName;
                ReadModel(ofd.FileName);
            }
        }

        private void ReadModel(string fileName)
        {
            using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(fileName)))
            {
                ms.Seek(4, SeekOrigin.Begin);
                Block block = new Block();
                byte[] buffer = new byte[0x4];
                block.CleanList();

                ms.Read(buffer, 0, 4);
                block.modelCount = BitConverter.ToInt32(buffer, 0);
                block.InitLists();

                for (int i = 0; i < ms.Length; i+=4)
                {
                    ms.Read(buffer, 0, buffer.Length);
                    block.ReadChunck(ms, buffer);
                }
                int f = 0;
                for (int i = 1; i < block.vertexList.Count; i++)
                {
                    f += block.vertexList[i - 1].Count / 3;
                    for(int j = 0; j < block.facesIndicesList[i].Count; j++)
                    {
                        block.facesIndicesList[i][j] += f;
                    }
                }
                ModelToOBJ(block.vertexList, block.vertexNormalsList, block.uvsList,
                           block.facesIndicesList, block.normalsIndicesList, block.uvsIndicesList, block.currentModel);
            }
        }
        private void ModelToOBJ(List<List<float>> vertexList, List<List<float>> normalsList, List<List<float>> uvsList, 
                                List<List<int>> facesIndicesList, List<List<int>> normalsIndicesList, List<List<int>> uvsIndicesList,
                                int modelCount)
        {
            List<List<string>> vertex = new List<List<string>>();
            List<List<string>> uvs = new List<List<string>>();
            List<List<string>> normals = new List<List<string>>();
            List<List<string>> facesIndices = new List<List<string>>();
            List<List<string>> uvsIndices = new List<List<string>>();
            List<List<string>> normalsIndices = new List<List<string>>();

            for (int i = 0; i < modelCount; i++)
            {
                vertex.Add(new List<string>());
                uvs.Add(new List<string>());
                normals.Add(new List<string>());
                facesIndices.Add(new List<string>());
                uvsIndices.Add(new List<string>());
                normalsIndices.Add(new List<string>());
            }
            for (int i = 0; i < modelCount; i++)
            {
                for (int j = 0; j < vertexList[i].Count; j += 3)
                {
                    float x = vertexList[i][j];
                    float y = vertexList[i][j + 1];
                    float z = vertexList[i][j + 2];

                    string formattedVertex = $"v {Math.Round(-x, 4):0.0000} {Math.Round(-y, 4):0.0000} {Math.Round(z, 4):0.0000}\n";
                    formattedVertex = formattedVertex.Replace(',', '.');

                    vertex[i].Add(formattedVertex);
                }
                for (int j = 0; j < uvsList[i].Count; j += 2)
                {
                    float x = uvsList[i][j];
                    float y = uvsList[i][j + 1];

                    string formattedUv = $"vt {Math.Round(x, 4):0.0000} {Math.Round(y, 4):0.0000}\n";
                    formattedUv = formattedUv.Replace(',', '.');

                    uvs[i].Add(formattedUv);
                }
                for (int j = 0; j < normalsList[i].Count; j += 3)
                {
                    float x = normalsList[i][j];
                    float y = normalsList[i][j + 1];
                    float z = normalsList[i][j + 2];

                    string formattedNormal = $"vn {Math.Round(-x, 4):0.0000} {Math.Round(-y, 4):0.0000} {Math.Round(z, 4):0.0000}\n";
                    formattedNormal = formattedNormal.Replace(',', '.');

                    normals[i].Add(formattedNormal);
                }
                for (int j = 0; j < facesIndicesList[i].Count; j += 3)
                {
                    facesIndices[i].Add("f " + Convert.ToString(facesIndicesList[i][j] + 1) + " " +
                                    Convert.ToString(facesIndicesList[i][j + 1] + 1) + " " +
                                    Convert.ToString(facesIndicesList[i][j + 2] + 1) + "\n");
                }
                /*for (int j = 0; j < facesIndicesList[i].Count; j += 3)
                {
                    facesIndices[i].Add("f " + Convert.ToString(facesIndicesList[i][j] + 1) + "/" + Convert.ToString(uvsIndicesList[i][j] + 1) + "/" + Convert.ToString(normalsIndicesList[i][j] + 1) + " " +
                                    Convert.ToString(facesIndicesList[i][j + 1] + 1) + "/" + Convert.ToString(uvsIndicesList[i][j + 1] + 1) + "/" + Convert.ToString(normalsIndicesList[i][j + 1] + 1) + " " +
                                    Convert.ToString(facesIndicesList[i][j + 2] + 1) + "/" + Convert.ToString(uvsIndicesList[i][j + 2] + 1) + "/" + Convert.ToString(normalsIndicesList[i][j + 2] + 1) + "\n");
                }*/
            }
            string output = "";
            for (int i = 0; i < modelCount; i++)
            {
                output += string.Join("", "mtllib " + Path.GetFileNameWithoutExtension(fileName) + ".mtl" + "\n");
                output += string.Join("", "o " + Path.GetFileNameWithoutExtension(fileName) + $"_{i}" + "\n");
                output += string.Join("", "usemtl material1 \n\n");
                output += string.Join("", vertex[i]) + "\n\n";
                output += string.Join("", normals[i]) + "\n\n";
                output += string.Join("", uvs[i]) + "\n\n";
                output += string.Join("", facesIndices[i]) + "\n\n";
            }

            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktop, Path.GetFileNameWithoutExtension(fileName));
            string mtlPath = Path.Combine(desktop, Path.GetFileNameWithoutExtension(fileName));
            File.WriteAllText(filePath + ".obj", output);
            string mtlOutput = "newmtl material1\n    map_Kd " + mtlPath + ".png";
            File.WriteAllText(mtlPath + ".mtl", mtlOutput);
        }
        private void ReadIDs(MemoryStream ms, int idsCount)
        {
            byte[] buffer = new byte[4];
            for (long j = 0; j < idsCount; j++)
            {
                ms.Read(buffer, 0, 1);
                if (j == idsCount - 1)
                {
                    for (j = ms.Position; j % 4 != 0; j++)
                    {
                        ms.Read(buffer, 0, 1);
                    }
                }
            }
        }
    }
}
