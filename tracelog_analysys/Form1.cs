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
            StreamReader sr = new StreamReader(
                fileNameTextBox.Text,Encoding.GetEncoding("utf-8"));

            int currentIndex = 0;
            int num_i = 0;
            int num_j = 0;
            int num_k = 0;
            int startIndex = 0;
            int endIndex = 0;
            int reverseIndex = 0;
            int dataFlag = 0;
            int tmpSize = 100;
            string[] stArray = new string[tmpSize];
            string text;

            //while(!sr.EndOfStream)
            for (num_i = 0; num_i < 60000; num_i++)
            {
                text = sr.ReadLine() + "\n";
                
                //検索し、該当したら行とデータ部を抜き出す
                if (0 <= text.IndexOf("10.22.227.7"))
                {
                    //データ部はバッファ配列を遡って確認する
                    for (num_j = 0; num_j < tmpSize; num_j++)
                    {
                        reverseIndex = currentIndex - num_j;
                        if(reverseIndex < 0){
                            reverseIndex += tmpSize;
                        }

                        //データ部は改行で区切られているものとする
                        if (dataFlag == 0)
                        {
                            //一つ目の改行　データ部開始フェイズ
                            if (stArray[reverseIndex] == "\n")
                            {
                                dataFlag = 1;
                                startIndex = reverseIndex;
                            }
                        }
                        else if (dataFlag == 1)
                        {
                            //二つ目の改行　データ部終了フェイズ
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
                            if(endIndex < startIndex){
                                for (num_k = endIndex; num_k <= startIndex; num_k++)
                                {
                                    textBox2.AppendText(stArray[num_k]);
                                }
                            }else{
                                //バッファが一周している時は2段階で遡る
                                for (num_k = endIndex; num_k <= tmpSize-1; num_k++)
                                {
                                    textBox2.AppendText(stArray[num_k]);
                                }

                                for (num_k = 0; num_k <= startIndex; num_k++)
                                {
                                    textBox2.AppendText(stArray[num_k]);
                                }
                            }

                            dataFlag = 0;
                            break;
                        }

                    }
                    reverseIndex = 0;
                    textBox2.AppendText(text + "\n");
                }

                //バッファ配列に格納
                stArray[currentIndex] = text;

                //確認用
                //logWindowTextBox.AppendText(text);
                //textBox1.AppendText(currentIndex.ToString() + "\n");

                //ループ処理
                if (currentIndex < tmpSize-1)
                {
                    currentIndex++;
                }
                else
                {
                    currentIndex = 0;
                }
                
            }
            
            sr.Close();
        }
    }
    



    /// <summary>
    /// ファイル関連のクラス
    /// </summary>

    //public class FileManage
    //{

    //}
}
