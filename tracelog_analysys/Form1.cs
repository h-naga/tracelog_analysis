using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
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

            //はじめに表示されるフォルダを指定する
            ofd.InitialDirectory = System.Environment.CurrentDirectory;
            //[ファイルの種類]に表示される選択肢を指定する
            ofd.Filter = "すべてのファイル(*.*)|*.*";
            //タイトルを設定する
            ofd.Title = "トレースログファイルを選択してください";

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

            //読み込み用ストリーム2
            //StreamReader sr2 = new StreamReader(
            //    fileNameTextBox.Text, Encoding.GetEncoding("utf-8"));

            //書き込み用ストリーム
            StreamWriter sw = new StreamWriter(
                Environment.CurrentDirectory + "\\output.txt", false, Encoding.GetEncoding("utf-8"));

            //ループ用変数
            double num_i = 0;
            double num_i2 = 0;
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

            //検索文字列の
            bool searchStringFlag = false;
            string text;        //読込テキスト

            //検索用文字列コレクション
            List<string> searchList = new List<string>();

            //検索用文字列コレクション初期化
            //searchList.Add(serchIpTextBox.Text);
            //searchList.Add("UP:RECV");
            foreach(CheckBox chBox in searchGroupBox.Controls){
                if (chBox.Checked)
                {
                    searchList.Add(chBox.Text);
                }
            }

            //プログレスバーを初期化する（再描画されず、未成功）
            //progressBar1.Minimum = 0;
            //progressBar1.Maximum = 100;
            //progressBar1.Value = 0;
            int progressNumber = 0;

            System.IO.FileInfo fi = new System.IO.FileInfo(fileNameTextBox.Text);
            //ファイルのサイズを取得
            long filesize = fi.Length;
            num_i2 = (int)filesize / 60;

            MessageBox.Show("処理開始");

            //ファイルの初めからEOFまで探索
            while (!sr.EndOfStream)
            //for (num_i = 0; num_i < 60000; num_i++)
            {
                //進捗表示用変数の更新
                num_i++;
                
                Application.DoEvents();

                //ストリームから一行読み込み
                text = sr.ReadLine() + "\n";

                //高速化のため、一文字目で判定
                if(text[0] == '['){
                    //検索文字列リストで読み込んだテキストを検索
                    for (int searchListCurrent = 0; searchListCurrent < searchList.Count; searchListCurrent++)
                    {
                        if (text.IsAny(serchIpTextBox.Text, "UP:RECV", searchList[searchListCurrent]))
                        {
                            searchStringFlag = true;
                            break;
                        }
                    }
                }

                //バッファ配列に格納
                stArray[currentIndex] = text;

                //検索し、該当したら行とデータ部を抜き出す
                //if (0 <= text.IndexOf(serchIpTextBox.Text) && 0 <= text.IndexOf("UP:RECV"))
                if(searchStringFlag)
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
                                }
                            }
                            else
                            {
                                //バッファが一周している時は2段階で遡る
                                for (num_k = endIndex; num_k <= bufferSize - 1; num_k++)
                                {
                                    sw.Write(stArray[num_k]);
                                }

                                for (num_k = 0; num_k <= startIndex; num_k++)
                                {
                                    sw.Write(stArray[num_k]);
                                }
                            }

                            //プログレスバー更新用処理
                            progressNumber = (int)((num_i / num_i2) * 100);
                            //label3.Text = progressNumber.ToString() + "%";
                            toolStripStatusLabel1.Text = progressNumber.ToString() + "%";
                            progressBar1.Value = (int)(num_i / num_i2) * 100;
                            progressBar1.Update();
                            
                            dataFlag = 0;
                            break;
                        }

                    }
                    reverseIndex = 0;
                    sw.Write(text);
                    searchStringFlag = false;
                }

                ////バッファ配列に格納
                //stArray[currentIndex] = text;

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
            num_i = 0;
            num_i2 = 0;
            MessageBox.Show("処理終了");
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}


/// <summary>
/// string 型の拡張メソッドを管理するクラス
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    /// 文字列が指定された文字列を全て含むかどうかを返します
    /// </summary>
    public static bool IsAny(this string self, params string[] values)
    {
        bool tmp_flag = true;
        //bool pre_tmp_flag = true;

        for (int num_l = 0; num_l < values.Length;num_l++ )
        {
            //文字列が指定された文字列を全て含む場合のみtrue
            if (0 <= self.IndexOf(values[num_l]) && tmp_flag == true)
            {
                tmp_flag = true;
            }
            else
            {
                tmp_flag = false;
                break;
            }

            //pre_tmp_flag = tmp_flag;
        }
        return tmp_flag;
    }
}