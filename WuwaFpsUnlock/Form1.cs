using System;
using System.Drawing;
using System.Windows.Forms;
﻿using Newtonsoft.Json.Linq;
using Microsoft.Data.Sqlite;

namespace WuwaFpsUnlock
{
    public partial class Form1 : Form
    {
        private Button? selectDirectoryButton;
        private Button? selectedDirectoryButton;
        private NumericUpDown? customNumberSelector;
        private Label? customNumberLabel;
        private string? selectedDirectoryPath;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "FPS Unlocker - Directory Selector";
            this.Size = new Size(400, 200);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 10);

            this.customNumberSelector = new NumericUpDown();
            this.customNumberSelector.Minimum = 1;
            this.customNumberSelector.Maximum = 120;
            this.customNumberSelector.Value = 120;
            this.customNumberSelector.Size = new Size(100, 25);
            this.customNumberSelector.Location = new Point(20, 60);

            this.customNumberLabel = new Label();
            this.customNumberLabel.Text = "Set Custom FPS";
            this.customNumberLabel.Size = new Size(140, 40);
            this.customNumberLabel.Location = new Point(20, 20);
            this.customNumberLabel.TextAlign = ContentAlignment.MiddleLeft;

            this.selectDirectoryButton = new Button();
            this.selectDirectoryButton.Text = "Select Directory";
            this.selectDirectoryButton.Size = new Size(150, 40);
            this.selectDirectoryButton.Location = new Point(220, 50);
            this.selectDirectoryButton.FlatStyle = FlatStyle.Flat;
            this.selectDirectoryButton.BackColor = Color.FromArgb(0, 120, 215);
            this.selectDirectoryButton.ForeColor = Color.White;
            this.selectDirectoryButton.Click += new EventHandler(this.SelectDirectoryButton_Click!);

            this.selectedDirectoryButton = new Button();
            this.selectedDirectoryButton.Text = "No directory selected";
            this.selectedDirectoryButton.Size = new Size(360, 40);
            this.selectedDirectoryButton.Location = new Point(20, 110);
            this.selectedDirectoryButton.FlatStyle = FlatStyle.Flat;
            this.selectedDirectoryButton.BackColor = Color.White;
            this.selectedDirectoryButton.ForeColor = Color.Black;
            this.selectedDirectoryButton.Click += new EventHandler(this.SelectedDirectoryButton_Click!);

            this.Controls.Add(this.selectDirectoryButton);
            this.Controls.Add(this.customNumberSelector);
            this.Controls.Add(this.selectedDirectoryButton);
            this.Controls.Add(this.customNumberLabel);
        }

        private void SelectDirectoryButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Select the directory to unlock FPS";
                folderBrowserDialog.UseDescriptionForTitle = true;

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedDirectoryPath = folderBrowserDialog.SelectedPath;
                    this.selectedDirectoryButton!.Text = "Set custom FPS limit";
                }
            }

        }

        private void SelectedDirectoryButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedDirectoryPath))
            {
                MessageBox.Show("Please select your Wuthering Waves folder!\nUsually located in \"C:\\Wuthering Waves\"");
                return;
            }

            try
            {
                string targetFilePath = Path.Combine(selectedDirectoryPath, "Wuthering Waves Game", "Client", "Saved", "LocalStorage", "LocalStorage.db");
                string targetFilePath2 = Path.Combine(selectedDirectoryPath, "Wuthering Waves Game", "Client", "Saved", "LocalStorage", "LocalStorage2.db");
                string targetFilePath3 = Path.Combine(selectedDirectoryPath, "Wuthering Waves Game", "Client", "Saved", "LocalStorage", "LocalStorage3.db");

                if (!File.Exists(targetFilePath))
                {
                    var targetFolder = Directory.GetDirectories(selectedDirectoryPath, "G?????")
                                            .FirstOrDefault();
                    if (targetFolder == null)
                    {
                        MessageBox.Show("Could not find the game files folder.");
                        return;
                    }
                    targetFilePath = Path.Combine(targetFolder, "Client", "Saved", "LocalStorage", "LocalStorage.db");
                    targetFilePath2 = Path.Combine(targetFolder, "Client", "Saved", "LocalStorage", "LocalStorage2.db");
                    targetFilePath3 = Path.Combine(targetFolder, "Client", "Saved", "LocalStorage", "LocalStorage3.db");
                }

                if (File.Exists(targetFilePath2)) 
                {
                    string connectionString2 = $"Data Source={targetFilePath2}";
                    PatchSQL(connectionString2);
                }
                if (File.Exists(targetFilePath))
                {
                    string connectionString = $"Data Source={targetFilePath}";
                    PatchSQL(connectionString);
                }

                if (File.Exists(targetFilePath3))
                {
                    string connectionString3 = $"Data Source={targetFilePath3}";
                    PatchSQL(connectionString3);
                }
                MessageBox.Show("You can close the program, the FPS should be unlocked.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void PatchSQL(string connectionString)
        {

            using (var connection = new SqliteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string selectQuery = "SELECT value FROM LocalStorage WHERE key = 'GameQualitySetting';";
                    string? gameQualitySettingJson = null;

                    using (var selectCommand = new SqliteCommand(selectQuery, connection))
                    {
                        using (var reader = selectCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                gameQualitySettingJson = reader!["value"].ToString()!;
                            }
                        }
                    }

                    if (gameQualitySettingJson == null)
                    {
                        // MessageBox.Show("Internal error, verify your game files and try again.");
                        return;
                    }

                    var gameQualitySetting = JObject.Parse(gameQualitySettingJson);
                    if (gameQualitySetting.ContainsKey("KeyCustomFrameRate"))
                    {
                        gameQualitySetting["KeyCustomFrameRate"] = int.Parse(customNumberSelector!.Value.ToString());
                    }
                    if (gameQualitySetting.ContainsKey("KeyPcVsync"))
                    {
                        gameQualitySetting["KeyPcVsync"] = 0;
                    }
                    string updatedGameQualitySettingJson = gameQualitySetting.ToString();

                    string updateQuery = "UPDATE LocalStorage SET value = @value WHERE key = 'GameQualitySetting';";

                    using (var updateCommand = new SqliteCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@value", updatedGameQualitySettingJson);
                        updateCommand.ExecuteNonQuery();
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }
    }
}
