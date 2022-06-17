using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BotsTester
{
    public partial class Form1 : Form
    {
        BindingList<BotInfoRow> botInfoRows = new BindingList<BotInfoRow>();

        public Form1()
        {
            InitializeComponent();

            botInfoRows = JsonConvert.DeserializeObject<BindingList<BotInfoRow>>(File.ReadAllText(dataPath));
             
            dataGridView1.DataSource = botInfoRows;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            dataGridView1.CellMouseClick += DataGridView1_CellMouseClick;

            this.Load += Form1_Load;
            this.FormClosed += Form1_FormClosed;
            addBotButton.Click += AddBotButton_Click;
        }

        private void AddBotButton_Click(object? sender, EventArgs e)
        {
            botInfoRows.Add(new BotInfoRow("", "", ""));
            dataGridView1.DataSource = botInfoRows;
            dataGridView1.Update();
        }

        string dataPath = "data.json";
        private void Form1_FormClosed(object? sender, FormClosedEventArgs e)
        {
            File.WriteAllText(dataPath, JsonConvert.SerializeObject(botInfoRows, Formatting.Indented));
        }

        private void Form1_Load(object? sender, EventArgs e)
        {

        }

        private async void DataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

            var botInfo = botInfoRows[e.RowIndex];
            ClientSignalR client = null;

            if (dataGridView1.Columns[e.ColumnIndex].Name == "LocalToken")
            {
                client = botInfo.LocalClient;
            }
            else if(dataGridView1.Columns[e.ColumnIndex].Name == "RemoteToken")
            {
                client = botInfo.RemoteClient;
            }
            else return;

            if (client.IsConnected) await client.StopAsync();
            else await client.StartAsync();

            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = client.IsConnected ? Color.Green : Color.Red;
            dataGridView1.Refresh();
        }
    }

    public class BotInfoRow
    {
        public string UserName { get; set; }
        public string TypeBot { get; set; }
        public string LocalToken { get; set; }
        public string RemoteToken { get; set; }

        [NonSerialized]
        public ClientSignalR LocalClient;
        [NonSerialized]
        public ClientSignalR RemoteClient;

        public BotInfoRow(string userName, string localToken, string remoteToken, Func<IPlayer> createPlayer = null)
        {
            createPlayer ??= (() => new RandomBotPlayer(42));

            UserName = userName;
            TypeBot = createPlayer().GetType().Name;
            LocalToken = localToken;
            RemoteToken = remoteToken;

            LocalClient = new ClientSignalR(ClientSignalR.LocalUrl, localToken, createPlayer);
            RemoteClient = new ClientSignalR(ClientSignalR.RemoteUrl, remoteToken, createPlayer);
        }
    }
}
