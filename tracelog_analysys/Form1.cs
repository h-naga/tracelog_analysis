using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace tracelog_analysys
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //開くメニュー実行時
        private void 開くToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd = new OpenFileDialog();

            //はじめのファイル名を指定
            //ofd.FileName = "";
            //はじめに表示されるフォルダを指定する
            //ofd.InitialDirectory = @"C:\";
            ofd.InitialDirectory = System.Environment.CurrentDirectory;
            //[ファイルの種類]に表示される選択肢を指定する
            ofd.Filter = "すべてのファイル(*.*)|*.*";
            //[ファイルの種類]ではじめに選択されるものを指定する
            ofd.FilterIndex = 1;
            //タイトルを設定する
            ofd.Title = "開くファイルを選択してください";

            //ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //OKボタンがクリックされたとき、選択されたファイル名を表示する
                fileNameTextBox.Text = ofd.FileName;
            }
        }

        //読込ボタン
        private void loadButton_Click(object sender, EventArgs e)
        {
            //読み込み用ストリーム
            StreamReader sr = new StreamReader(
                fileNameTextBox.Text, Encoding.GetEncoding("utf-8"));

            //書き込み用ストリーム
            StreamWriter sw = new StreamWriter(
                Environment.CurrentDirectory + "\\output.txt", false, Encoding.GetEncoding("utf-8"));

            //ループ用変数
            long num_i = 0;
            long num_i2 = 0;
            int num_j = 0;
            int num_k = 0;

            //データ部探索用バッファ関連定義
            int bufferSize = 100;                           //バッファサイズ
            string[] stArray = new string[bufferSize];      //バッファ配列
            int currentIndex = 0;                           //バッファ書き込み位置
            int startIndex = 0;                             //データ部開始位置
            int endIndex = 0;                               //データ部終了位置
            int reverseIndex = 0;                           //バッファ遡り位置

            //遡り中の状態フラグ
            //　0:データ部開始位置探索フェイズ
            //　1:データ部終了位置探索フェイズ
            //　2:書き込みフェイズ
            int dataFlag = 0;

            string text;        //読込テキスト


            //プログレスバーを初期化する
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            System.IO.FileInfo fi = new System.IO.FileInfo(fileNameTextBox.Text);
            //ファイルのサイズを取得
            long filesize = fi.Length;

            MessageBox.Show("処理開始");

            //ファイルの初めからEOFまで探索
            while (!sr.EndOfStream)
            //for (num_i = 0; num_i < 60000; num_i++)
            {
                //プログレスバー更新
                progressBar1.Value = (int)(sr.BaseStream.Position / filesize)*100;

                text = sr.ReadLine() + "\n";

                //検索し、該当したら行とデータ部を抜き出す
                if (0 <= text.IndexOf("10.22.2.63"))
                {
                    //データ部はバッファ配列を遡って確認する
                    for (num_j = 0; num_j < bufferSize; num_j++)
                    {
                        reverseIndex = currentIndex - num_j;
                        if (reverseIndex < 0)
                        {
                            reverseIndex += bufferSize;
                        }

                        //データ部は改行で区切られているものとする
                        if (dataFlag == 0)
                        {
                            //一つ目の改行　データ部開始位置探索フェイズ
                            if (stArray[reverseIndex] == "\n")
                            {
                                dataFlag = 1;
                                startIndex = reverseIndex;
                            }
                        }
                        else if (dataFlag == 1)
                        {
                            //二つ目の改行　データ部終了位置探索フェイズ
                            if (stArray[reverseIndex] == "\n")
                            {
                                dataFlag = 2;
                                endIndex = reverseIndex;
                            }
                        }
                        else if (dataFlag == 2)
                        {
                            //書き込みフェイズ
                            //バッファを遡る
                            if (endIndex < startIndex)
                            {
                                for (num_k = endIndex; num_k <= startIndex; num_k++)
                                {
                                    sw.Write(stArray[num_k]);
                                    //textBox2.AppendText(stArray[num_k]);
                                }
                            }
                            else
                            {
                                //バッファが一周している時は2段階で遡る
                                for (num_k = endIndex; num_k <= bufferSize - 1; num_k++)
                                {
                                    sw.Write(stArray[num_k]);
                                    //textBox2.AppendText(stArray[num_k]);
                                }

                                for (num_k = 0; num_k <= startIndex; num_k++)
                                {
                                    sw.Write(stArray[num_k]);
                                    //textBox2.AppendText(stArray[num_k]);
                                }
                            }

                            dataFlag = 0;
                            break;
                        }

                    }
                    reverseIndex = 0;
                    sw.Write(text + "\n");
                    //textBox2.AppendText(text + "\n");
                }

                //バッファ配列に格納
                stArray[currentIndex] = text;

                //確認用
                //logWindowTextBox.AppendText(text);
                //textBox1.AppendText(currentIndex.ToString() + "\n");

                //バッファ書き込み位置のループ処理
                if (currentIndex < bufferSize - 1)
                {
                    currentIndex++;
                }
                else
                {
                    currentIndex = 0;
                }

            }

            sr.Close();
            sw.Close();

            MessageBox.Show("処理終了");
        }
    }
}