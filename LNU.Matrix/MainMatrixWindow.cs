﻿using LNU.Matrix.UtilityClasses;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using NMMP.Triangulation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TriangleNet.Geometry;

namespace LNU.Matrix
{
    public partial class MainMatrixWindow : Form
    {
        List<double[,]> Ke;
        List<double[,]> Me;
        List<BasisStorage> Ne;
        List<Matrix<double>> ReMatrix;
        List<Vector<double>> ReVector;
        List<Vector<double>> Qe;
        DataStorage storage;
        double value = 1;

        public DenseMatrix A { get; set; }
        public DenseVector B { get; set; }

        Dictionary<int, List<Constants>> conditions = new Dictionary<int, List<Constants>>();

        public MainMatrixWindow()
        {
            InitializeComponent();
            storage = new DataStorage();
            Ke = new List<double[,]>();
            Me = new List<double[,]>();
            Ne = new List<BasisStorage>();
            ReMatrix = new List<Matrix<double>>();
            ReVector = new List<Vector<double>>();
            Qe = new List<Vector<double>>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //DataStorage storage = new DataStorage();
            List<double> squerArea = new List<double>();
            List<EquationStorage> equationStorage = new List<EquationStorage>();

            double a11 = double.Parse(tbA11.Text.Replace('.', ','));
            double a22 = double.Parse(tbA22.Text.Replace('.', ','));
            double a12 = double.Parse(tbA12.Text.Replace('.', ','));

            var sigma = tbSigma.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Convert.ToDouble(s.Replace(',', '.')));
            var sigmas = new double[6] {
                sigma.ToArray().Length >= 1 ? sigma.ToArray()[0] : 0,
                sigma.ToArray().Length >= 2 ? sigma.ToArray()[1] : 0,
                sigma.ToArray().Length >= 3 ? sigma.ToArray()[2] : 0,
                sigma.ToArray().Length >= 4 ? sigma.ToArray()[3] : 0,
                sigma.ToArray().Length >= 5 ? sigma.ToArray()[4] : 0,
                sigma.ToArray().Length >= 6 ? sigma.ToArray()[5] : 0,
            };
            var beta = tbBeta.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(b => Convert.ToDouble(b.Replace(',', '.')));
            var betas = new double[6] {
                beta.ToArray().Length >= 1 ? beta.ToArray()[0] : 0,
                beta.ToArray().Length >= 2 ? beta.ToArray()[1] : 0,
                beta.ToArray().Length >= 3 ? beta.ToArray()[2] : 0,
                beta.ToArray().Length >= 4 ? beta.ToArray()[3] : 0,
                beta.ToArray().Length >= 5 ? beta.ToArray()[4] : 0,
                beta.ToArray().Length >= 6 ? beta.ToArray()[5] : 0,
            };
            var ds = tbD.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(b => Convert.ToDouble(b.Replace(',', '.')));
            var d = new double[6] {
                ds.ToArray().Length >= 1 ? ds.ToArray()[0] : 0,
                ds.ToArray().Length >= 2 ? ds.ToArray()[1] : 0,
                ds.ToArray().Length >= 3 ? ds.ToArray()[2] : 0,
                ds.ToArray().Length >= 4 ? ds.ToArray()[3] : 0,
                ds.ToArray().Length >= 5 ? ds.ToArray()[4] : 0,
                ds.ToArray().Length >= 6 ? ds.ToArray()[5] : 0,
            };
            value = double.Parse(tbValue.Text.Replace('.', ','));

            GenerateConditions(sigmas, betas, d);

            double squareA11 = Math.Pow(double.Parse(tbA11.Text.Replace('.', ',')), 2);
            double squareA22 = Math.Pow(double.Parse(tbA22.Text.Replace('.', ',')), 2);
            double squareA12 = Math.Pow(double.Parse(tbA12.Text.Replace('.', ',')), 2);

            foreach (var item in storage.NT)
            {
                squerArea.Add(2 * HeronArea.getArea(item[0], item[1], item[2]));
                equationStorage.Add(MatrixKeConstants.getConstants(item[0], item[1], item[2]));
                Ne.Add(Basis.getBase(item[0], item[1], item[2]));
            }

            #region GetMAtrixKe
            double constant = 0;
            for (int i = 0; i < equationStorage.Count; i++)
            {
                constant = 1d / (2d * squerArea[i]);
                Ke.Add(new double[3, 3]
                {
                    {
                        constant * (squareA11 * Math.Pow(equationStorage[i].X.B,2) + squareA22 * Math.Pow(equationStorage[i].X.C,2)),
                        constant * (squareA11 * equationStorage[i].X.B*equationStorage[i].Y.B + squareA22 * equationStorage[i].X.C*equationStorage[i].Y.C),
                        constant * (squareA11 * equationStorage[i].X.B*equationStorage[i].Z.B + squareA22 * equationStorage[i].X.C*equationStorage[i].Z.C)

                    },
                    {
                         constant * (squareA11 * equationStorage[i].X.B*equationStorage[i].Y.B + squareA22 * equationStorage[i].X.C*equationStorage[i].Y.C),
                         constant * (squareA11 * Math.Pow(equationStorage[i].Y.B,2)+ squareA22 * Math.Pow(equationStorage[i].Y.C,2)),
                         constant * (squareA11 * equationStorage[i].Y.B*equationStorage[i].Z.B + squareA22 * equationStorage[i].Y.C*equationStorage[i].Z.C)
                    },
                    {
                        constant * (squareA11 * equationStorage[i].X.B*equationStorage[i].Z.B + squareA22 * equationStorage[i].X.C*equationStorage[i].Z.C),
                        constant * (squareA11 * equationStorage[i].Y.B*equationStorage[i].Z.B + squareA22 * equationStorage[i].Y.C*equationStorage[i].Z.C) ,
                        constant * (squareA11 * Math.Pow(equationStorage[i].Z.B,2) + squareA22 * Math.Pow(equationStorage[i].Z.C,2))
                    }
                });
            }
            #endregion

            #region getMatrixMe
            constant = 0;
            for (int i = 0; i < squerArea.Count; i++)
            {
                constant = squerArea[i] / 24;
                Me.Add(new double[3, 3] {
                    {
                        2 * constant, 1 * constant, 1 * constant
                    },
                    {
                        1 * constant, 2 * constant, 1 * constant
                    },
                    {
                        1 * constant, 1 * constant, 2 * constant
                    }
                });

            }
            #endregion

            #region getMatrixRe
            var Uconst = 1d;
            var index = 0;
            var matrix = DenseMatrix.OfArray(new double[,] { { 1, 2 }, { 2, 1 } });
            foreach (var side in GetSegments())
            {
                var leftSide = sigmas.ToArray()[index] / betas.ToArray()[index] * matrix;
                var rigthSide = sigmas.ToArray()[index] / betas.ToArray()[index] * matrix;
                var vector = new[] { Uconst * side.UcCof, Uconst * side.UcCof };
                index++;
                foreach (var segment in side.Segments)
                {
                    var length = Math.Sqrt(Math.Pow(segment.Vertex1.X - segment.Vertex2.X, 2) +
                                            Math.Pow(segment.Vertex1.Y - segment.Vertex2.Y, 2));

                    var reLeft = (leftSide * length / 6);
                    var reRight = (rigthSide * length / 6) * (new DenseVector(vector));
                    ReMatrix.Add(reLeft);
                    ReVector.Add(reRight);
                }
            }
            #endregion

            #region getMatrixQe

            foreach (var item in storage.NT)
            {
                foreach (var me in Me)
                {

                    var vector = GenerateQeMatrixes(item[0], item[1], item[2]);
                    var result = DenseMatrix.OfArray(me).Multiply(vector);
                    Qe.Add(result);
                }

            }

            #endregion

            PrintMatrix();
            WriteMatrix();

            JoinAllMatrixes();

            var res = A.Solve(B);
            //PrintResult();

            JsonParser.Write(res, "Result.json");
            JsonParser.Write(A.ToArray(), "A.json");
            JsonParser.Write(B.ToArray(), "B.json");

            WriteResultsToFile(res.ToList(), "Results.txt");
        }


        private void WriteResultsToFile(List<double> z, string path)
        {
            var i = 0;
            using (var sr = new StreamWriter(path))
            {
                foreach (var vert in storage.CT)
                {
                    string toWrite = $"{vert.X.ToString("0.000").Replace(',', '.')} {vert.Y.ToString("0.000").Replace(',', '.')} {z[i++].ToString("0.000").Replace(',', '.')}";
                    sr.WriteLine(toWrite);
                }
            }

        }

        private void WriteMatrix()
        {
            JsonParser.Write(Me, "ME.json");
            JsonParser.Write(Qe, "QE.json");
            JsonParser.Write(Ke, "KE.json");
            JsonParser.Write(ReMatrix.Select(el => el.Storage).ToList(), "ReMatrix.json");
            JsonParser.Write(ReVector.Select(el => el.Storage).ToList(), "ReVector.json");
        }

        private void JoinAllMatrixes()
        {
            int vertexesCount = storage.CT.Count;
            var a = new double[vertexesCount, vertexesCount];
            var b = new double[vertexesCount];
            var triangleIndex = 0;

            foreach (var triangle in storage.NT)
            {
                for (var i = 0; i < 3; i++)
                {
                    var ai = triangle[i].ID;
                    for (var j = 0; j < 3; j++)
                    {
                        var aj = triangle[j].ID;
                        a[ai, aj] += Ke[triangleIndex][i, j] + Me[triangleIndex][i, j];
                    }
                    b[ai] += Qe[triangleIndex][i];

                }
                triangleIndex++;
            }

            var ntg = storage.NTG.ToList();
            for (int k = 0; k < storage.NTG.Count; k++)
            {
                for (int i = 0; i < ntg[k].Count; i++)
                {
                    var ai = ntg[k][i].Vertex1.ID;
                    var aj = ntg[k][i].Vertex2.ID;
                    a[ai, ai] += ReMatrix[k][0, 0];
                    a[ai, aj] += ReMatrix[k][0, 1];
                    a[aj, ai] += ReMatrix[k][1, 0];
                    a[aj, aj] += ReMatrix[k][1, 1];
                    b[ai] += ReVector[k][0];
                    b[aj] += ReVector[k][1];
                }
            }
            A = DenseMatrix.OfArray(a);
            B = DenseVector.OfArray(b);
        }


        private void PrintResult()
        {
            textBox1.Text = "Matrix A " + System.Environment.NewLine;

            textBox1.Text += A.ToString("0.0000");

            textBox1.Text += "Vector B " + System.Environment.NewLine;

            textBox1.Text += B.ToString("0.0000");

        }
        private void PrintMatrix()
        {
            textBox1.Text = String.Empty;


            if (radioButton1.Checked)
            {
                textBox1.Text = "Matrix Ke" + System.Environment.NewLine;
                List<DenseMatrix> matrixKe = Ke.Select(item => DenseMatrix.OfArray(item)).ToList();

                radioButton4.Checked = false;
                radioButton2.Checked = false;
                radioButton3.Checked = false;
                radioButton5.Checked = false;

                foreach (var item in matrixKe)
                {
                    textBox1.Text += item.ToString();
                }
            }

            if (radioButton2.Checked)
            {
                textBox1.Text = "Matrix Me " + System.Environment.NewLine;

                List<DenseMatrix> matrixMe = Me.Select(item => DenseMatrix.OfArray(item)).ToList();

                radioButton1.Checked = false;
                radioButton4.Checked = false;
                radioButton3.Checked = false;
                radioButton5.Checked = false;

                foreach (var item in matrixMe)
                {
                    textBox1.Text += item.ToString();
                }
            }

            if (radioButton3.Checked)
            {
                textBox1.Text = "Matrix Qe " + System.Environment.NewLine;

                radioButton1.Checked = false;
                radioButton2.Checked = false;
                radioButton4.Checked = false;
                radioButton5.Checked = false;

                foreach (var item in Qe)
                {
                    textBox1.Text += item.ToString();
                }
            }

            if (radioButton4.Checked)
            {
                textBox1.Text = "Matrix Re " + System.Environment.NewLine;

                radioButton1.Checked = false;
                radioButton2.Checked = false;
                radioButton3.Checked = false;
                radioButton5.Checked = false;
                foreach (var item in ReMatrix)
                {
                    textBox1.Text += item.ToString();
                }
            }

            if (radioButton5.Checked)
            {
                textBox1.Text = "Vector Re " + System.Environment.NewLine;

                radioButton1.Checked = false;
                radioButton2.Checked = false;
                radioButton3.Checked = false;
                radioButton4.Checked = false;
                foreach (var item in ReVector)
                {
                    textBox1.Text += item.ToString();
                }
            }


        }
        private double Func(double x, double y) => value;

        private Vector<double> GenerateQeMatrixes(Vertex x, Vertex y, Vertex z)
        {
            var f = new double[3] {
                    Func(x.X, x.Y),
                    Func(y.X, y.Y),
                    Func(z.X, z.Y)
                };
            return Vector.Build.DenseOfArray(f);
        }

        private IEnumerable<Constants> GetSegments()
        {
            List<Constants> conds;
            if (!conditions.TryGetValue(storage.Figure[0], out conds)) return null;
            if (conds.Count != storage.NTG.Count) return conds;
            for (var i = 0; i < conds.Count; i++)
            {
                conds[i].Segments = storage.NTG[i];
            }
            return conds;
        }

        private void GenerateConditions(double[] sigmaValue, double[] bethaValue, double[] dValues)
        {
            conditions = new Dictionary<int, List<Constants>>
            {
                {1, new List<Constants>()
                    {
                        new Constants(bethaValue[0] == 0 ? Math.Pow(0.1, 6) : bethaValue[0], sigmaValue[0], dValues[0] == 0 ? Math.Pow(0.1, 6) : dValues[0]),
                        new Constants(bethaValue[1] == 0 ? Math.Pow(0.1, 6) : bethaValue[1], sigmaValue[1], dValues[1] == 0 ? Math.Pow(0.1, 6) : dValues[1]),
                        new Constants(bethaValue[2] == 0 ? Math.Pow(0.1, 6) : bethaValue[2], sigmaValue[2], dValues[2] == 0 ? Math.Pow(0.1, 6) : dValues[2])
                    }
                },
                {2, new List<Constants>()
                    {
                        new Constants(bethaValue[0] == 0 ? Math.Pow(0.1, 6) : bethaValue[0], sigmaValue[0], dValues[0] == 0 ? Math.Pow(0.1, 6) : dValues[0]),
                        new Constants(bethaValue[1] == 0 ? Math.Pow(0.1, 6) : bethaValue[1], sigmaValue[1], dValues[1] == 0 ? Math.Pow(0.1, 6) : dValues[1]),
                        new Constants(bethaValue[2] == 0 ? Math.Pow(0.1, 6) : bethaValue[2], sigmaValue[2], dValues[2] == 0 ? Math.Pow(0.1, 6) : dValues[2]),
                        new Constants(bethaValue[3] == 0 ? Math.Pow(0.1, 6) : bethaValue[3], sigmaValue[3], dValues[3] == 0 ? Math.Pow(0.1, 6) : dValues[3])
                    }
                },
                {3, new List<Constants>()
                    {
                        new Constants(bethaValue[0] == 0 ? Math.Pow(0.1, 6) : bethaValue[0], sigmaValue[0], dValues[0] == 0 ? Math.Pow(0.1, 6) : dValues[0]),
                        new Constants(bethaValue[1] == 0 ? Math.Pow(0.1, 6) : bethaValue[1], sigmaValue[1], dValues[1] == 0 ? Math.Pow(0.1, 6) : dValues[1]),
                        new Constants(bethaValue[2] == 0 ? Math.Pow(0.1, 6) : bethaValue[2], sigmaValue[2], dValues[2] == 0 ? Math.Pow(0.1, 6) : dValues[2]),
                        new Constants(bethaValue[3] == 0 ? Math.Pow(0.1, 6) : bethaValue[3], sigmaValue[3], dValues[3] == 0 ? Math.Pow(0.1, 6) : dValues[3])
                    }
                },
                {4, new List<Constants>()
                    {
                        new Constants(bethaValue[0] == 0 ? Math.Pow(0.1, 6) : bethaValue[0], sigmaValue[0], dValues[0] == 0 ? Math.Pow(0.1, 6) : dValues[0]),
                        new Constants(bethaValue[1] == 0 ? Math.Pow(0.1, 6) : bethaValue[1], sigmaValue[1], dValues[1] == 0 ? Math.Pow(0.1, 6) : dValues[1]),
                        new Constants(bethaValue[2] == 0 ? Math.Pow(0.1, 6) : bethaValue[2], sigmaValue[2], dValues[2] == 0 ? Math.Pow(0.1, 6) : dValues[2])
                    }
                },
                {5, new List<Constants>()
                    {
                        new Constants(bethaValue[0] == 0 ? Math.Pow(0.1, 6) : bethaValue[0], sigmaValue[0], dValues[0] == 0 ? Math.Pow(0.1, 6) : dValues[0]),
                        new Constants(bethaValue[1] == 0 ? Math.Pow(0.1, 6) : bethaValue[1], sigmaValue[1], dValues[1] == 0 ? Math.Pow(0.1, 6) : dValues[1]),
                        new Constants(bethaValue[2] == 0 ? Math.Pow(0.1, 6) : bethaValue[2], sigmaValue[2], dValues[2] == 0 ? Math.Pow(0.1, 6) : dValues[2]),
                        new Constants(bethaValue[3] == 0 ? Math.Pow(0.1, 6) : bethaValue[3], sigmaValue[3], dValues[3] == 0 ? Math.Pow(0.1, 6) : dValues[3])
                    }
                },
                {6, new List<Constants>()
                    {
                        new Constants(bethaValue[0] == 0 ? Math.Pow(0.1, 6) : bethaValue[0], sigmaValue[0], 1),
                        new Constants(bethaValue[1] == 0 ? Math.Pow(0.1, 6) : bethaValue[1], sigmaValue[1], 1),
                        new Constants(bethaValue[2] == 0 ? Math.Pow(0.1, 6) : bethaValue[2], sigmaValue[2], 1),
                        new Constants(bethaValue[3] == 0 ? Math.Pow(0.1, 6) : bethaValue[3], sigmaValue[3], 1),
                        new Constants(bethaValue[4] == 0 ? Math.Pow(0.1, 6) : bethaValue[4], sigmaValue[4], 1),
                        new Constants(bethaValue[5] == 0 ? Math.Pow(0.1, 6) : bethaValue[5], sigmaValue[5], 1)
                    }
                },
            };
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PrintResult();
        }
    }

}
