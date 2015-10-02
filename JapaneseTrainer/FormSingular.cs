﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JapaneseTrainer
{
    public partial class FormSingular : Form
    {
        SingularCreator singularCreator;
        ConfigHandler config;
        System.Windows.Forms.Timer updateBarTimer;
        bool paused;
        bool hidden;

        public FormSingular(char sessionType, ConfigHandler configHandler)
        {
            singularCreator = new SingularCreator(sessionType, configHandler);
            config = configHandler;
            paused = false;
            InitializeComponent();
            setupConfig();
            newSingular();
            setVisualsLabels();            
        }

        private void FormSingular_Load(object sender, EventArgs e)
        {
            config.startTrainerTimer();
            if (config.getTrainerTimerEnabled())
            {
                updateBarTimer = new System.Windows.Forms.Timer();                
                updateBarTimer.Interval = 100;
                updateBarTimer.Start();
                updateBarTimer.Tick += new EventHandler(updateBarTimerTick);
                bar_trainer_timer.Maximum = (int)(config.getTrainerTimerInterval() * 1000);
            }
        }       

        private void newSingular()
        {
            string[] currentSingular = singularCreator.generateSingular();
            setVisualPriority();

            lbl_japanese.Text = currentSingular[1];            
            lbl_meaning.Text = currentSingular[2];
            lbl_furigana.Text = currentSingular[3];
            if (currentSingular[4] != "")
                lbl_extra.Text = "(" + currentSingular[4] + ")";
            else
                lbl_extra.Text = "";
            if (config.getTrainerFlags() % 2 == 1)
                lbl_extra.Text += " ID: " + currentSingular[0];

        }

        private void setupConfig()
        {
            byte flags = config.getTrainerFlags();
            showFuriganaToolStripMenuItem.Checked = ((flags >> 3) % 2 == 1);
            showKanjiToolStripMenuItem.Checked = ((flags >> 2) % 2 == 1);
            showMeaningsToolStripMenuItem.Checked = ((flags >> 1) % 2 == 1);
            showIDToolStripMenuItem.Checked = ((flags >> 0) % 2 == 1);
        }
        private void setVisualsLabels()
        {
            lbl_furigana.ForeColor = ((config.getTrainerFlags() >> 3) % 2 != 1) ? SystemColors.Control : SystemColors.ControlText;
            lbl_japanese.ForeColor = ((config.getTrainerFlags() >> 2) % 2 != 1) ? SystemColors.Control : SystemColors.ControlText;
            lbl_meaning.ForeColor = ((config.getTrainerFlags() >> 1) % 2 != 1) ? SystemColors.Control : SystemColors.ControlText;
            lbl_extra.ForeColor = ((config.getTrainerFlags() >> 1) % 2 != 1) ? SystemColors.Control : SystemColors.ControlText;
            hidden = true;
        }

        private void setVisualPriority()
        {
            PictureBox[] boxes = { pbx_star1, pbx_star2, pbx_star3, pbx_star4, pbx_star5 };
            int priority = singularCreator.getPriority();
            for (int cl = 0; cl<boxes.Length; cl++)
            {
                boxes[cl].Image = null;
            }
            for (int x = 0; x <= priority; x++)
            {
                if ((x == priority) && (x % 2 == 0))
                {
                    boxes[x / 2].Image = null;
                    boxes[x / 2].Image = JapaneseTrainer.Properties.Resources.star_half;
                }
                else
                {
                    boxes[x / 2].Image = JapaneseTrainer.Properties.Resources.star_full;
                }                
            }
        }

        private void revealHidden()
        {
            hidden = false;
            lbl_furigana.ForeColor = SystemColors.ControlText;
            lbl_japanese.ForeColor = SystemColors.ControlText;
            lbl_meaning.ForeColor = SystemColors.ControlText;
            lbl_extra.ForeColor = SystemColors.ControlText;
            updateBarTimer.Stop();
            updateBarTimer = new System.Windows.Forms.Timer();
            updateBarTimer.Tick += new EventHandler(updateBarTimerTick2);
            bar_trainer_timer.Maximum = (int)(config.getTrainerTimerInterval2() * 1000);

            updateBarTimer.Start();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void configToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // create SINGLE controller for accessing different forms and handlers
        }

        private void showFuriganaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            config.setTrainerFlags(config.getTrainerFlags() ^ 8);
        }
        private void showFuriganaToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            lbl_furigana.ForeColor = (((ToolStripMenuItem)sender).Checked) ? SystemColors.ControlText : SystemColors.Control;
        }
        private void showKanjiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            config.setTrainerFlags(config.getTrainerFlags() ^ 4);
        }
        private void showKanjiToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            lbl_japanese.ForeColor = (((ToolStripMenuItem)sender).Checked) ? SystemColors.ControlText : SystemColors.Control;
        }
        private void showMeaningsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            config.setTrainerFlags(config.getTrainerFlags() ^ 2);
        }
        private void showMeaningsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            lbl_meaning.ForeColor = lbl_extra.ForeColor = (((ToolStripMenuItem)sender).Checked) ? SystemColors.ControlText : SystemColors.Control;
        }
        private void showIDToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            config.setTrainerFlags(config.getTrainerFlags() ^ 1);
        }

        private void FormSingular_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space) // try to bind a key in config
            {             
                paused = !paused;
            }
            else if ((e.KeyCode == Keys.N))
            {
                if (hidden)
                {
                    bar_trainer_timer.Value = 0;
                    revealHidden();
                }
                else
                {
                    updateBarTimer.Stop();
                    bar_trainer_timer.Value = 0;
                    updateBarTimer = new System.Windows.Forms.Timer();
                    updateBarTimer.Tick += new EventHandler(updateBarTimerTick);
                    bar_trainer_timer.Maximum = (int)(config.getTrainerTimerInterval() * 1000);
                    newSingular();
                    setVisualsLabels();
                    updateBarTimer.Start();
                }
            }            
        }

        private void updateBarTimerTick(object sender, EventArgs e)
        {
            int interval = 100;
            if ((bar_trainer_timer.Value + interval) < bar_trainer_timer.Maximum)
            {
                if (!paused)
                    bar_trainer_timer.Value += interval;
            }
            else
            {
                bar_trainer_timer.Value = 0;
                revealHidden();
            }
        }

        private void updateBarTimerTick2(object sender, EventArgs e)
        {
            int interval = 100;
            if ((bar_trainer_timer.Value + interval) < bar_trainer_timer.Maximum)
            {
                if (!paused)
                    bar_trainer_timer.Value += interval;
            }                
            else
            {
                updateBarTimer.Stop();
                bar_trainer_timer.Value = 0;
                updateBarTimer = new System.Windows.Forms.Timer();
                updateBarTimer.Tick += new EventHandler(updateBarTimerTick);
                bar_trainer_timer.Maximum = (int)(config.getTrainerTimerInterval() * 1000);
                newSingular();
                setVisualsLabels();
                updateBarTimer.Start();
            }
        }

        public void remoteTest(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("s");
        }

        private void FormSingular_FormClosed(object sender, FormClosedEventArgs e)
        {
            updateBarTimer.Stop();
        }

        private void nextToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            newSingular();
            setVisualsLabels();
        }

        private void increasePriorityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            singularCreator.changePriority(singularCreator.getPriority() + 1);
        }

        private void decreasePriorityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            singularCreator.changePriority(singularCreator.getPriority() - 1);
        }
    }
}
