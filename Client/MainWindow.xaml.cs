using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Client
{
    public partial class MainWindow : Window
    {
        HubConnection hubCon;
        bool userClosedConnection = false;

        public MainWindow()
        {
            InitializeComponent();

            //Create hub connection object
            hubCon = new HubConnectionBuilder()
                        .WithUrl("http://localhost:8090/chat")
                        .Build();            
        }
        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Subscribe to Closed event to set the retry reconnect mechanism
            hubCon.Closed += async (err) => {
                if (!userClosedConnection) {
                    //retry reconnection if server or network issue caused disconnection
                    await Task.Delay(new Random().Next(0, 5) * 1000);
                    await hubCon.StartAsync();
                }
                else {
                    //avoid reconnection if user explicitly closed the connect (pressed Disconnect button)
                    btnConnect.IsEnabled = true;
                    btnDisconnect.IsEnabled = false;
                    btnSendMessage.IsEnabled = false;
                    txtUser.IsEnabled = false;
                    txtMessage.IsEnabled = false;
                    lstMessages.Items.Clear();
                    lblStatus.Text = "Disconnected";
                }
            };

            //setup all listeners to receive messages from the server hub
            hubCon.On<string, string>("ReceiveMessage",
                (user, message) => {
                    this.Dispatcher.Invoke(() => lstMessages.Items.Add($"{user}: {message}"));
                });
            //hubCon.On("GetUser",
            //    () => {
            //        this.Dispatcher.Invoke(async () => await hubCon.InvokeAsync("SendUser", txtUser.Text));
            //    });
            hubCon.On<string, int>("ClientConnected",
                (message, onlineUsersCount) => {   //(message,onlineUsers)
                    this.Dispatcher.Invoke(() => UpdateUI(message, onlineUsersCount));
                });
            hubCon.On<string, int>("ClientDisconnected",
                (message, onlineUsersCount) => {
                    this.Dispatcher.Invoke(() => UpdateUI(message, onlineUsersCount));
                });
            hubCon.On("StopConnection", async () => await hubCon.StopAsync());

            txtUser.Focus();
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try {
                await hubCon.StartAsync();

                userClosedConnection = false;               
                lstMessages.Items.Add("Connected to server...");
                lblStatus.Text = "Connected";
                btnConnect.IsEnabled = false;
                btnDisconnect.IsEnabled = true;
                btnSendMessage.IsEnabled = true;
                txtUser.IsEnabled = true;
                txtMessage.IsEnabled = true;                
                lblStatus.Text = string.Empty;
            }
            catch (Exception ex) {
                lstMessages.Items.Add(ex.Message);
                lblStatus.Text = "Error...";
            }
        }

        private async void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            try {                
                await hubCon.StopAsync();
                userClosedConnection = true;                
            }
            catch (Exception ex) {
                lstMessages.Items.Add(ex.Message);
                lblStatus.Text = "Error...";
            }
        }

        private async void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            try {
                if (!string.IsNullOrEmpty(txtUser.Text) && !string.IsNullOrEmpty(txtMessage.Text)) {
                    await hubCon.InvokeAsync("SendMessage", txtMessage.Text, txtUser.Text);

                    lblStatus.Text = string.Empty;
                    txtMessage.Text = string.Empty;
                    txtMessage.Focus();
                } else 
                    MessageBox.Show("Please enter both the user name and message, then click send.");          
            }
            catch (Exception ex) {
                lstMessages.Items.Add(ex.Message);
                lblStatus.Text = "Error...";
            }            
        }

        private void UpdateUI(string message, int count) {
            lstMessages.Items.Add(message);
            lblStatus.Text = $"{count} user(s) online.";
            //lstOnlineUsers.DataContext = onlineUsers;
        }
    }
}
