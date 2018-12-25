using System;
using System.Net.Sockets;
using System.Windows;
using System.Threading;
using System.Text;

namespace Client
{
    
    public partial class MainWindow : Window
    {
        int count=3;
        const int port = 8888;
        const string address = "127.0.0.1";
        TcpClient client;



        public MainWindow()
        {
            InitializeComponent();
            //btn_snd.Content = "Send";
        }

        private void Connect(string userName,string msg, string ip, int port)
        {
           // header = 0;
            client = new TcpClient();

            // Client connects to the server on the specified ip and port.
            try
            {
                client.Connect(ip, port);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                MessageBoxResult mbr = MessageBox.Show("IP was empty.");
                return;
                throw;
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                MessageBoxResult mbr = MessageBox.Show("Port isn't within the allowed range.");
                return;
                throw;
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                MessageBoxResult mbr = MessageBox.Show("Couldn't access the socket.");
                return;
                throw;
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                MessageBoxResult mbr = MessageBox.Show("TCP Client is closed.");
                return;
                throw;
            }

            // Get client stream for reading and writing.
            // Using is a try finally, if an exception occurs it disposes of the stream.
            NetworkStream stream = client.GetStream();
            
            Send(stream, userName, msg);
            while (true)
            {
                    if (client.Connected)
                    {
                        Receive(stream);
                    }
                    else
                    {
                        stream.Dispose();
                        client.Close();
                        client.Dispose();
                        Thread.CurrentThread.Join();
                    }
            }
            


        }

        private void Button_Connect(object sender, RoutedEventArgs e)
        {
            
            if (count == 0)
            {
                CHAT.Text += "\n Попытки законились";
                CHAT_SCROLL.ScrollToBottom();

            }
            else
            {

                try
                {
                    string userName = txt_nm.Text;
                    string message = txt_msg.Text;


                    if (string.IsNullOrWhiteSpace(message))
                    {
                        MessageBoxResult mbr = MessageBox.Show("Сообщение не может быть пустым!");
                    }
                    else
                    {
                        Thread t = new Thread(() => Connect(userName, message, address, port));
                        t.Start();

                    }
                }
                catch (FormatException p)
                {
                    Console.WriteLine(p.Message + "\n" + p.StackTrace);
                    MessageBoxResult mbr = MessageBox.Show("Неправильный формат сообщения!");
                    return;
                    throw;
                }
            }

        }


      

        private void Send(NetworkStream stream,string userName, string message)
        {

          
            message = String.Format("{0}: {1}", userName, message);

            // преобразуем сообщение в массив байтов
            byte[] data = Encoding.Unicode.GetBytes(message);
            // отправка сообщения
            

            try
            {
                if (stream.CanWrite)
                {
                    stream.Write(data, 0, data.Length);
                    this.Dispatcher.Invoke(() =>
                    {
                        CHAT.Text += "\n" + message;
                        CHAT_SCROLL.ScrollToBottom();
                    });
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                MessageBoxResult mbr = MessageBox.Show("The message size is too big for the buffer.");
                return;
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                MessageBoxResult mbr = MessageBox.Show("An error occurred when accessing the socket.");
                return;
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                MessageBoxResult mbr = MessageBox.Show("There was a failure reading from the network.");
                return;
            }
        }

        private void Receive(NetworkStream ns)
        {
         
            byte[] data =  new byte[64];


            string message;


            try
            {
                if (ns.CanRead)
                {
                    if (client.Connected)
                    {
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0;
                        do
                        {
                            bytes = ns.Read(data, 0, data.Length);
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        }
                        while (ns.DataAvailable);

                        message = builder.ToString();
                        message = String.Format("Сервер: {0}", message);
                        this.Dispatcher.Invoke(() =>
                        {
                            CHAT.Text += "\n" + message;
                            CHAT_SCROLL.ScrollToBottom();

                            if (builder.ToString() == "NACK"){
                                count--;
                                message = String.Format("У вас осталось {0} попыток", count);
                                CHAT.Text += "\n" + message;
                                CHAT_SCROLL.ScrollToBottom();
                            };
                            
                            
                        });
                       
                    }
                    else
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            CHAT.Text += "\n not connected anymore";
                            CHAT_SCROLL.ScrollToBottom();
                        });
                        
                        
                    }

                }
                else
                {
                    //return null;
                }
            }
            catch (System.IO.IOException e)
            {

                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                client.Close();
                //MessageBoxResult mbr = MessageBox.Show("TCP Client is closed.");
                //return null;

            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                MessageBoxResult mbr = MessageBox.Show("There was a failure reading from the network.");
                client.Close();
                //return null;
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            client.Close();
        }














    }
}
