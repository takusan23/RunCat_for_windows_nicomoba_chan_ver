// 素敵な本家様：Copyright 2020 Takuto Nakamura
// ニコモバvar：Copyright 2020 takusan_23
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using RunCatNicomobaChanVar.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Resources;

namespace RunCatNicomobaChanVar
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new RunCatApplicationContext());
        }
    }

    public class RunCatApplicationContext : ApplicationContext
    {
        /// <summary>
        /// CPU使用率とる何か
        /// </summary>
        private PerformanceCounter cpuUsage;
        /// <summary>
        /// タスクバーに表示するやつ
        /// </summary>
        private NotifyIcon notifyIcon;
        /// <summary>
        /// 今表示してるアイコンの位置
        /// </summary>
        private int currentIconListPos = 0;
        /// <summary>
        /// アイコン配列。今回はニコモバちゃん（4枚）
        /// </summary>
        private Icon[] icons;
        /// <summary>
        /// 
        /// </summary>
        private Timer animateTimer = new Timer();
        private Timer cpuTimer = new Timer();


        public RunCatApplicationContext()
        {
            // CPU使用率取るなにか
            cpuUsage = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _ = cpuUsage.NextValue(); // 最初の戻り値を破棄します #って何

            // タスクバーにアイコンを出す。WPFだとめんどいんだっけ
            notifyIcon = new NotifyIcon()
            {
                Icon = Resources.nicomoba_chan_1,
                ContextMenu = new ContextMenu(new MenuItem[]
                {
                    new MenuItem("おつ（終了）", Exit)
                }),
                Text = "0.0%",
                Visible = true
            };
            // アイコン配列用意
            SetIcons();
            // アイコン切り替え関数を登録
            SetAnimation();
            // CPU使用率+アニメーション速度変更
            GetCPUUsageAndAnimationSpeedChange(null, EventArgs.Empty);
            // ↑これを定期的に呼ぶようにする
            StartObserveCPU();
            // 現在のアイコン配列の位置？
            currentIconListPos = 1;
        }

        /// <summary>
        /// パラパラ漫画で使うアイコンを配列に入れて用意する
        /// </summary>
        private void SetIcons()
        {
            ResourceManager rm = Resources.ResourceManager;
            icons = new List<Icon>
            {
                (Icon)rm.GetObject("nicomoba_chan_1"),
                (Icon)rm.GetObject("nicomoba_chan_2"),
                (Icon)rm.GetObject("nicomoba_chan_3"),
                (Icon)rm.GetObject("nicomoba_chan_4")
            }
            .ToArray();
        }

        /// <summary>
        /// 終了時にタイマー止めるなど
        /// </summary>
        /// <param name="sender">しらん</param>
        /// <param name="e">わからん</param>
        private void Exit(object sender, EventArgs e)
        {
            animateTimer.Stop();
            cpuTimer.Stop();
            notifyIcon.Visible = false;
            Application.Exit();
        }

        /// <summary>
        /// ChangeIconを定期的に呼ぶようにする
        /// </summary>
        private void SetAnimation()
        {
            animateTimer.Interval = 200;
            animateTimer.Tick += new EventHandler(ChangeIcon);
        }

        /// <summary>
        /// ここが定期的に呼ばれ、画像を切り替えている。
        /// どうやらGetCPUUsageAndAnimationSpeedChangeが更新頻度を変えてるらしい？
        /// </summary>
        /// <param name="sender">しらん</param>
        /// <param name="e">わからん</param>
        private void ChangeIcon(object sender, EventArgs e)
        {
            notifyIcon.Icon = icons[currentIconListPos];
            currentIconListPos = (currentIconListPos + 1) % icons.Length;
        }

        /// <summary>
        /// GetCPUUsageAndAnimationSpeedChange関数を定期的に呼ぶようにする
        /// </summary>
        private void StartObserveCPU()
        {
            cpuTimer.Interval = 3000;
            cpuTimer.Tick += new EventHandler(GetCPUUsageAndAnimationSpeedChange);
            cpuTimer.Start();
        }

        /// <summary>
        /// CPU使用率をとってアニメーションの速度を変更する
        /// </summary>
        /// <param name="sender">しらん</param>
        /// <param name="e">わからん</param>
        private void GetCPUUsageAndAnimationSpeedChange(object sender, EventArgs e)
        {
            float s = cpuUsage.NextValue();
            notifyIcon.Text = $"{s:f1}%";
            // パラパラ漫画の切替速度をここで変えてるらしい？
            s = 200.0f / (float)Math.Max(1.0f, Math.Min(20.0f, s / 5.0f));
            animateTimer.Stop();
            animateTimer.Interval = (int)s;
            animateTimer.Start();
        }

    }
}
