using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Layout.Layered;

namespace GrafosPUC
{
    static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>


        [STAThread]
        static void Main()
        {

            Microsoft.Msagl.GraphViewerGdi.GViewer viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            Microsoft.Msagl.Drawing.Graph graph = new Microsoft.Msagl.Drawing.Graph("graph");

            var sugiyamaSettings = (SugiyamaLayoutSettings)graph.LayoutAlgorithmSettings;
            sugiyamaSettings.NodeSeparation *= 2;

            // dados para criar o grafo
            int verticesCount = 20;
            int edgesCount = 400;
            Graph graph1 = CreateGraph(verticesCount, edgesCount);


            List<Node> areasEstudo = new List<Node>();
            List<Node> alunos = new List<Node>();

            double[,] matriz_diss = new double[20, 20];
            int[] mediaDissimi = new int[20];
            

           

            //conexao 50 alunos materia
            try
            {
                string[] alunosTCC = System.IO.File.ReadAllLines(@"C:\Users\Marthinha\Source\repos\GrafosPUC\input\entrada50.txt", Encoding.Default);

                int ap = 0;
                
                foreach (string aluno in alunosTCC)

                {
                    string[] infoo;
                    infoo = aluno.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
                    // Use a tab to indent each line of the file.
                    Console.WriteLine("\t Aluno:" + infoo[0] + "TCC Aréa:" + infoo[1]);
                    alunos.Add(new Node(Int32.Parse(infoo[0]), "Aluno " + infoo[0], 1));
                    areasEstudo[Int32.Parse(infoo[1]) - 1].AddEdge(alunos[Int32.Parse(infoo[0])], 1);
                    graph.AddEdge(infoo[0], areasEstudo[Int32.Parse(infoo[1]) - 1].areaReturn()).Attr.AddStyle(Microsoft.Msagl.Drawing.Style.Bold);
                    ap++;

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read: entrada50.txt");
                Console.WriteLine(e.Message);
            }

            // Prencher graph1 com vertices e pesos de arestas AREAPESQUISA.TXT
            try
            {
                string[] materias = System.IO.File.ReadAllLines(@"C:\Users\Marthinha\Source\repos\GrafosPUC\input\areaPesquisa.txt", Encoding.Default);

                int linha = 0;
                int coluna = 0;
                int conexao = 0;
                foreach (string materia in materias)

                {
                    string[] pesos;
                    pesos = materia.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
                    foreach (string peso in pesos)
                    {
                        if (peso != "0")
                        {
                            graph1.edge[conexao].Source = linha;
                            graph1.edge[conexao].Destination = coluna;
                            graph1.edge[conexao].Weight = Int32.Parse(peso);
                        }
                        conexao++;
                        coluna++;
                    }
                    linha++;
                    coluna = 0;
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Não conseguiu ler: areaPesquisa.txt");
            }

            //implementa Kruskal para achar a arvore minima

            System.Windows.Forms.Form form1 = new Form1();
            form1.Size = new Size(600, 600);
            
             //bind o graph1 para a viewer e executa Kruskal
            viewer.Graph = Kruskal(graph1);
            viewer.Size = new Size(600, 600);

            //associate the viewer with the form
            form1.SuspendLayout();
            viewer.Dock = System.Windows.Forms.DockStyle.Fill;
            form1.Controls.Add(viewer);
            form1.ResumeLayout();

            //show the form
            form1.ShowDialog();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(form1);
        }
        

        private class Node
        {
            public int p;
            public string area;
            public int tipo;
            List<Edge> arestas = new List<Edge>();


            public override string ToString()
            {
                return "ID: " + p + "   Name: " + area;
            }
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                Node objAsPart = obj as Node;
                if (objAsPart == null) return false;
                else return Equals(objAsPart);
            }
            public bool Equals(Node other)
            {
                if (other == null) return false;
                return (this.p.Equals(other.p));
            }

            public Node AddEdge(Node child, int w)
            {
                arestas.Add(new Edge(this, child, w));

                if (!child.arestas.Exists(a => a.init == child && a.fim == this))
                {
                    child.AddEdge(this, w);
                }

                return this;
            }
            public Node(int p, string area, int tipo)
            {
                this.p = p;
                this.area = area;
                this.tipo = tipo;
            }

            public int idReturn()
            {
                return p;
            }

            public string areaReturn()
            {
                return area;
            }

            public int tipoReturn()
            {
                return tipo;
            }
        }
        private class Edge
        {
            public Node init;
            public Node fim;
            public int peso;
            private bool visitado;

            public Edge(Node init, Node fim, int peso)
            {
                this.init = init;
                this.fim = fim;
                this.peso = peso;
                this.visitado = false;
            }

            public Node initReturn()
            {
                return init;
            }

            public Node fimReturn()
            {
                return fim;
            }

            public string arestaReturn()
            {
                return "Aresta de " + init.areaReturn() + "para " + fim.areaReturn();
            }
            public int pesoReturn()
            {
                return peso;
            }

            public bool visitadoReturn()
            {
                return visitado;
            }
        }
        public struct EdgeB
        {
            public int Source;
            public int Destination;
            public int Weight;
        }

        public struct Graph
        {
            public int VerticesCount;
            public int EdgesCount;
            public EdgeB[] edge;
        }

        public struct Subset
        {
            public int Parent;
            public int Rank;
        }

        public static Graph CreateGraph(int verticesCount, int edgesCoun)
        {
            Graph graph = new Graph();
            graph.VerticesCount = verticesCount;
            graph.EdgesCount = edgesCoun;
            graph.edge = new EdgeB[graph.EdgesCount];

            return graph;
        }

        private static int Find(Subset[] subsets, int i)
        {
            if (subsets[i].Parent != i)
                subsets[i].Parent = Find(subsets, subsets[i].Parent);

            return subsets[i].Parent;
        }

        private static void Union(Subset[] subsets, int x, int y)
        {
            int xroot = Find(subsets, x);
            int yroot = Find(subsets, y);

            if (subsets[xroot].Rank < subsets[yroot].Rank)
                subsets[xroot].Parent = yroot;
            else if (subsets[xroot].Rank > subsets[yroot].Rank)
                subsets[yroot].Parent = xroot;
            else
            {
                subsets[yroot].Parent = xroot;
                ++subsets[xroot].Rank;
            }
        }

        //IMPRIMI KRUSKAL
        private static Microsoft.Msagl.Drawing.Graph Print(EdgeB[] result, int e)
        {
            Microsoft.Msagl.Drawing.Graph graph = new Microsoft.Msagl.Drawing.Graph("graph");
            List<Node> areasEstudo = new List<Node>();
            List<Node> alunos = new List<Node>();

            // ADICIONA nome ao vetor AreaPesquisaNome.TXT
            try
            {

                string[] areas = System.IO.File.ReadAllLines(@"C:\Users\Marthinha\Source\repos\GrafosPUC\input\areaPesquisaNome.txt", Encoding.Default);

                int p = 0;

                foreach (string area in areas)

                {
                    // Use a tab to indent each line of the file.
                    graph.AddNode(area);

                    graph.FindNode(area).Attr.Shape = Microsoft.Msagl.Drawing.Shape.Box;
                    graph.FindNode(area).Attr.XRadius = 3;
                    graph.FindNode(area).Attr.YRadius = 3;
                    graph.FindNode(area).Attr.FillColor = Microsoft.Msagl.Drawing.Color.Green;

                    Console.WriteLine("\t" + area);
                    // new Node(p, area, tipo);
                    areasEstudo.Add(new Node(p, area, 1));
                    p++;


                }

            }
            catch (Exception )
            {
                Console.WriteLine("The file could not be read: areaPesquisaNome.txt");
            }

            

            for (int i = 0; i < e; ++i) {


                graph.AddEdge(areasEstudo[result[i].Source].areaReturn(), result[i].Weight.ToString() , areasEstudo[result[i].Destination].areaReturn()).Attr.AddStyle(Microsoft.Msagl.Drawing.Style.Dashed);
                Console.WriteLine("{0} -- {1} == {2}", areasEstudo[result[i].Source].areaReturn(), areasEstudo[result[i].Destination].areaReturn(), result[i].Weight);
            }
            return graph;
        }

        public static Microsoft.Msagl.Drawing.Graph Kruskal(Graph graph)
        {
            int verticesCount = graph.VerticesCount;
            EdgeB[] result = new EdgeB[verticesCount];
            int i = 0;
            int e = 0;

            Array.Sort(graph.edge, delegate (EdgeB a, EdgeB b)
            {
                return a.Weight.CompareTo(b.Weight);
            });

            Subset[] subsets = new Subset[verticesCount];

            for (int v = 0; v < verticesCount; ++v)
            {
                subsets[v].Parent = v;
                subsets[v].Rank = 0;
            }

            while (e < verticesCount - 1)
            {
                EdgeB nextEdge = graph.edge[i++];
                int x = Find(subsets, nextEdge.Source);
                int y = Find(subsets, nextEdge.Destination);

                if (x != y)
                {
                    result[e++] = nextEdge;
                    Union(subsets, x, y);
                }
            }

            return Print(result, e);
        }
    }
}

