using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace XO
{
  public partial class GameXO : Form
  {
    public bool isStartGame = false;  // игра еще не началась
    public bool isStartStep_X = true; // первыми ходят "Крестики"
    public bool isStep_X = true; // текущий ход "Крестиков"

    // время старта новый игры
    DateTime timeStart;

    // для текста игры XO
    Font  fontXO = new Font("Arial", 14, FontStyle.Bold);
    Brush brushX = new SolidBrush(Color.Blue); // для X
    Brush brushO = new SolidBrush(Color.Gold); // для O
    Color colorO = Color.Blue; // для X
    Color colorX = Color.Gold; // для O

    // двумерный массив ссылок на объекты типа Panel
    Panel[,] arrPanel; // перемнная класса

    public GameXO()
    {
      InitializeComponent();

      miMain_Restart.Enabled = false;
      Update_MI_StepTurn();
      // Положения панелей на окне
      // 1 2 3   ==>  1;1  1;2  1;3
      // 6 5 4   ==>  2;1  2;2  2;3
      // 9 8 7   ==>  3;1  3;2  3;3
panel1.Tag = " ;1;1"; panel2.Tag = " ;1;2"; panel3.Tag = " ;1;3";
panel6.Tag = " ;2;1"; panel5.Tag = " ;2;2"; panel4.Tag = " ;2;3";
panel9.Tag = " ;3;1"; panel8.Tag = " ;3;2"; panel7.Tag = " ;3;3";

      // двумерный массив ссылок на объекты типа Panel
      Panel[,] tmp = // временная переменная
      {
        { panel1, panel2, panel3 },
        { panel6, panel5, panel4 },
        { panel9, panel8, panel7 },
      };
      arrPanel = tmp;
    }

    private void Update_MI_StepTurn()
    {
      if (isStartStep_X)
      { // первыми ходят "Крестики"
        miGame_Start_X.Checked = true;
        miGame_Start_O.Checked = false;
      }
      else
      { // первыми ходят "Нолики"
        miGame_Start_X.Checked = false;
        miGame_Start_O.Checked = true;
      }
    }

    private void miGame_Start_X_Click(object sender, EventArgs e)
    {
      if (miGame_Start_X.Checked)
      { // инвертируем значение переменной isStartStep_X
        isStartStep_X = !isStartStep_X;
      }
      Update_MI_StepTurn(); // обновляем состояние меню
    }

    private void miGame_Start_O_Click(object sender, EventArgs e)
    {
      if(miGame_Start_O.Checked)
      { // инвертируем значение переменной isStartStep_X
        isStartStep_X = !isStartStep_X;
      }
      Update_MI_StepTurn(); // обновляем состояние меню
    }

    private void panel1_Click(object sender, EventArgs e)
    {
      Panel pn = sender as Panel;
      if (pn == null) return;
      // подсказка в статусной строке
      stbStepTurn.Text = isStep_X ? "Ходят X" : "Ходят O";
      // проверка на возможность хода
      if(isStep_X == isStartStep_X)
      { // это наш ход
        if((pn.Tag as string)[0] == ' ') // Tag = " ;Row;Col"
        { // плитка свободна
          string[] temp = (pn.Tag as string).Split(';');
          MyStep.Y = int.Parse(temp[1]) - 1; // Row
          MyStep.X = int.Parse(temp[2]) - 1; // Col
          temp[0] = (isStartStep_X) ? "X" : "O";
          pn.Tag = string.Join(";", temp);
          UserStep.Set(); // сигнал что мы сделали ход
          // переход хода - от нас к противнику
          isStep_X = !isStep_X;
        }
      }
      pn.Invalidate(true); // инвалидация окна для перепрорисовки
      pn.Update(); // сигнал на обновление окна панели
    }

    private void panel1_Paint(object sender, PaintEventArgs e)
    {
      Panel pn = sender as Panel;
      if(pn == null) return;
      PointF pt = new PointF(10, 10);

      // центрирование текста на панельке
      SizeF ptSize = e.Graphics.MeasureString("O", fontXO);

      pt.X = (pn.Width - ptSize.Width) / 2;
      pt.Y = (pn.Height - ptSize.Height) / 2;

      if ( (pn.Tag as string)[0] == 'X')
      { // вывод X
        e.Graphics.DrawString("X", fontXO, brushX, pt);
      }
      else if ((pn.Tag as string)[0] == 'O')
      { // вывод O
        e.Graphics.DrawString("O", fontXO, brushO, pt);
      }
      else
      { // прорисовка пустого поля
        e.Graphics.Clear(Color.DarkGray);
        e.Graphics.DrawString(" ", fontXO, brushO, pt);
      }
    }

    private void miText_Font_Click(object sender, EventArgs e)
    {
      fontDialog1.Font = fontXO;
      // показать диалог для выбора шрифта
      if (fontDialog1.ShowDialog(this) == DialogResult.OK)
      {
        // сохранить шрифт в объекте GameXO::fontXO
        fontXO = fontDialog1.Font;
      }
      // обновление для всей формы и ее элементов
      Invalidate(true);
      Update();
    }

    private void miText_Color_Click(object sender, EventArgs e)
    {
      if(colorDialog1.ShowDialog(this) == DialogResult.OK)
      {
        colorX = colorO = colorDialog1.Color;
        // обновление для всей формы и ее элементов
        Invalidate(true);
        Update();
      }
    }

    private void splitContainer2_Panel1_MouseClick(
      object sender, MouseEventArgs e)
    {
      if(e.Button == MouseButtons.Right)
      {
        if (isStartGame)
        {
          contMenu1.Items["pmiRestart"].Enabled = true;
        }
        else
        {
          contMenu1.Items["pmiRestart"].Enabled = false;
        }
        // вывод контекстного меню по текущим координатам "мыши"
        contMenu1.Show(sender as Control, e.Location);
      }
    }

    private void tmCurTime_Tick(object sender, EventArgs e)
    {
      (sender as System.Windows.Forms.Timer).Enabled = false; // временно выключаем таймер
      
      stbCurrTime.Text = DateTime.Now.ToShortDateString() +
                  "  " + DateTime.Now.ToShortTimeString();
      (sender as System.Windows.Forms.Timer).Enabled = true; // включаем таймер
    }

    private void miMain_NewGame_Click(object sender, EventArgs e)
    {
      isStartGame = true;

      // включить таймер игры
      tmGame.Enabled = true;
      timeStart = DateTime.Now;

      // прогресс-бар
      pbPastTime.Minimum = 0;
      pbPastTime.Maximum = 60 * 2; // максимум 2 минуты = 120 сек

      // перерисовать форму
      Invalidate(true);
      Update();
    }
    // таймер для текущей игры
    private void tmGame_Tick(object sender, EventArgs e)
    {
      tmGame.Enabled = false;

      stbGameTime.Text
        = (DateTime.Now - timeStart).ToString(@"hh\:mm\:ss");
      pbPastTime.Value = (DateTime.Now - timeStart).Seconds;

      tmGame.Enabled = true;
    }

    //--------------------------------------------
    // Подсистема сети

    TcpListener tcpServer = null; // сокет для сервера
    TcpClient   tcpClient = null; // сокет для клиента
    // для приема сообщений от удаленого хоста
    byte[]      buffer    = new byte[1024];

    string ipServer = "0.0.0.0"; // 127.0.0.1; 192.168.1.60
    int    portServer = 4123;
    string UserName = "Noname";
    //
    // событие хода игрока
    AutoResetEvent UserStep = new AutoResetEvent(false);
    Point MyStep    = new Point(); // координаты нашего хода
    Point EnemyStep = new Point(); // координаты хода противника

    private void miStartServer_Click(object sender, EventArgs e)
    {
      if(tcpServer == null)
      {
        ThreadPool.QueueUserWorkItem(ServerProc, this);
        miConnet.Enabled = false;
      }
      else
      {
        // ERROR - MsgBox
      }
    }

    // поток сервера
    private void ServerProc(object obj)
    {
      GameXO form = obj as GameXO;
      try
      {
        tcpServer = new TcpListener(
          IPAddress.Parse(ipServer), portServer);
        tcpServer.Start(100); // Bind(), Listen()

        while(true)
        {
          TcpClient client = tcpServer.AcceptTcpClient();
          if(tcpClient == null)
          { // первый игрок - играем с ним
            tcpClient = client;
            // Посылаем клиенту собщение с приглашением к игре
            string msg = "OK";
            // Выбор хода
            Random rnd = new Random(DateTime.Now.Millisecond);
            if(rnd.Next( 1, 100 ) % 2 == 0)
            { // серевер ходит X, клиент - O
              msg += "O"; // это для клиента O - "OKO"
              isStartStep_X = true; // это для сервера X
            }
            else
            { // серевер ходит O, клиент - X
              msg += "X"; // это для клиента X - "OKX"
              isStartStep_X = false; // это для сервера O
            }
            byte[] temp = Encoding.UTF8.GetBytes(msg);
            tcpClient.GetStream().Write(temp, 0, temp.Length);
            // Запускаем поток клиента
            ThreadPool.QueueUserWorkItem(ClientProc, form);
            //
          }
          else
          { // сервер уже занят игрой, этот клиент "лишний"
            string msg = "ERROR: Client is already conected!";
            // объект сетевого потока для приема-передачи данных в сети
            NetworkStream ns = client.GetStream();
            byte[] temp = Encoding.UTF8.GetBytes(msg);
            ns.Write(temp, 0, temp.Length); // Send(msg)
            client.Close(); // принудительно отключить клиента
          }
        } // while(true)
      } // try
      catch (Exception ex)
      { // ошибка при работе сервера
        MessageBox.Show(ex.Message);
      }
    } // void ServerProc(object obj)

    private void miConnet_Click(object sender, EventArgs e)
    {
      // запросить имя игрока: UserName
      // запросить адрес сервера: 192.168.1.60; server_XO 
      if(tcpClient != null)
      {
        // ERROR - этот клиент уже подключен к серверу
        return;
      }
      // подключаемся к серверу
      tcpClient = new TcpClient();
      try
      {
        // 1 - подключение к серверу игры
        ipServer = "127.0.0.1"; // локальное подключение
        tcpClient.Connect(ipServer, portServer);
        if (tcpClient.Connected)
        {
          // Получить приглашение от сервера на игру
          NetworkStream ns = tcpClient.GetStream();
          int size = ns.Read(buffer, 0, buffer.Length);
          string msg = Encoding.UTF8.GetString(buffer, 0, size);
          if(msg.Substring(0,2) == "OK")
          { // играем
            isStep_X = true; // всегда первыми ходят крестики
            //if (msg[2] == 'X') { isStartStep_X = true; /* X */ }
            //else               { isStartStep_X = false; /* O */ }
            isStartStep_X = (msg[2] == 'X');
            // Поток для работы игрока с удаленным сервером
            ThreadPool.QueueUserWorkItem(ClientProc, this);
          }
          else
          { // Отказ
            tcpClient.Close();
            tcpClient = null;
            MessageBox.Show(msg, "Отказ от сервера");
          }
        }
        // Поток для работы игрока с удаленным сервером
        // 2 - отправить имя игрока
        // 3 - получить имя игрока противника
        // 4 - игра в цикле
      }
      catch (Exception ex)
      {
        // MessageBox.Show()
        if (tcpClient != null)
        {
          tcpClient.Close();
          tcpClient = null;
        }
      }
    } // miConnet_Click()

    // ф-ция потока для работы клиента
    private void ClientProc(object par)
    {
      GameXO form = par as GameXO;
      form.isStep_X = true; // form.isStartStep_X;
      form.isStartGame = true;
      try
      {
        // Запускаем асинхронное чтение из сокета
        //  получить ходы противника
        form.tcpClient.Client.BeginReceive(
            form.buffer, 0, form.buffer.Length,
            SocketFlags.None,
            CallbackReceive, form );

        while(form.tcpClient.Connected) // while(true)
        {
          // Ждем события о ходе игрока
          UserStep.WaitOne();
          // "Row;Col" ==> MyStep.Y; MyStep.X
          //MyStep.Y.ToString() + ";" + MyStep.X.ToString();
          string msg = $"{MyStep.Y};{MyStep.X}";
          form.tcpClient.Client.Send(Encoding.UTF8.GetBytes(msg));
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "ERROR");
      }
      finally
      {
        form.tcpClient.Close(); // закрываем сокет
        form.tcpClient = null; // для следующего соединения
      }
    } // void ClientProc(object par)

    private void CallbackReceive( IAsyncResult ar )
    {
      if (ar.IsCompleted)
      {
        // наши параметры для асинхронного чтения
        GameXO form = ar.AsyncState as GameXO;
        try {
          // подтверждение завершения асинхронного чтения
          int size = form.tcpClient.Client.EndReceive( ar );
          // получить координаты хода противника "Row;Col"
          string temp = Encoding.UTF8.GetString(form.buffer, 0, size);
          string[] temp2 = temp.Split(';');
          Point pt = new Point( int.Parse(temp2[1]),  // X
                                int.Parse(temp2[0]) );// Y
          form.EnemyStep = pt; // координаты противника
          //
          // Вызвать метод для установки X или O по полученным координатам
          //
          Panel pn = form.arrPanel[ pt.Y, pt.X ];
          // INVOKE
          // "_;1;2"
          pn.Invoke( (Action) delegate ()
          {
            temp2 = (pn.Tag as string).Split(';');
            temp2[0] = (!form.isStartStep_X) ? "X" : "O";
            pn.Tag = string.Join(";", temp2); // "X;1;2"
            // "X;1;2"
            //pn.Tag = temp2[0] + ';' + temp2[1] + ';' + temp2[2] ;
            // Ниже foreach() не подойдет для формирования pn.Tag
            //string res = ""; // "X;1;2;"
            //foreach (string s in temp2) { res += s + ';'; }
            // Обновить внешний вид плитки
            pn.Invalidate(true);
            pn.Update();
          });
          // переход хода - от противника к нам
          form.isStep_X = !form.isStep_X;

          // Запустить очередное асинхронное чтение
          form.tcpClient.Client.BeginReceive(
                form.buffer, 0, form.buffer.Length,
                SocketFlags.None,
                CallbackReceive, form );
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message, "ERROR");
        }
      }
    } // void CallbackReceive( IAsyncResult ar )

  } // class GameXO
}
