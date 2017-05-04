using Client.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IGuessServiceCallback {
        //客户端的服务器代理类
        private GuessServiceClient client;
        //当前用户
        private User user;
        //保存当前用户登录的状态
        private bool logined = false;

        //画板相关
        private DrawingAttributes inkDA;
        private DrawingAttributes highlighterDA;
        private Color currentColor;

        public MainWindow() {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            //创建客户端代理类
            InstanceContext context = new InstanceContext(this);
            client = new GuessServiceClient(context);

            //初始化墨迹和画板
            currentColor = Colors.Red;
            inkDA = new DrawingAttributes() {
                Color = currentColor,
                Height = 6,
                Width = 6,
                FitToCurve = false
            };
            highlighterDA = new DrawingAttributes() {
                Color = Colors.Orchid,
                IsHighlighter = true,
                IgnorePressure = false,
                StylusTip = StylusTip.Rectangle,
                Height = 30,
                Width = 10
            };
            ink1.DefaultDrawingAttributes = inkDA;
            ink1.EditingMode = InkCanvasEditingMode.Ink;
        }
        void MainWindow_Closed(object sender, EventArgs e) {
            //对直接关闭客户端窗口进行处理，保证游戏逻辑正确
            if (logined) {
                client.Logout(this.user.name, this.user.inRoom);
            }
        }

        
        /// <summary>
        /// 1、登录、回调登录
        /// </summary>
        private void btnLogin_MouseDown(object sender, MouseButtonEventArgs e) {
            client.Login(LoginName.Text, LoginPsw.Password);
        }

        public void ShowLogin(int num, User user) {
            //未重名
            if (num == 0) {
                logined = true;
                this.user = user;
                this.user.score = 0;
                MessageBox.Show("登录成功！即将进入游戏大厅……");
                client.EnterHall(user.name);
            }

            //重名
            else {
                MessageBox.Show("已存在此临时用户！");
            }
        }

        
        /// <summary>
        /// 2、进入大厅
        /// </summary>
        public void ShowHall(string user) {
            WindowLogin.Visibility = Visibility.Collapsed;//设置登录界面隐藏
            WindowMain.Visibility = Visibility.Visible;//大厅可见

            //显示用户名
            Info1.Visibility = Visibility;
            InfoName.Content = this.user.name;
        }


        /// <summary>
        /// 3、进入房间
        /// </summary>
        private void room_MouseDown(object sender, MouseButtonEventArgs e) {
            //设置大厅隐藏、房间可见
            InterfaceHall.Visibility = Visibility.Collapsed;
            InterfaceGame.Visibility = Visibility.Visible;

            //确定点击了几号房间（0~9）
            TextBlock tb = e.Source as TextBlock;
            int len = tb.Name.Length;
            int idx = (int)((tb.Name)[len - 1]) - 48;
            MessageBox.Show("进入" + idx + "号房间");

            //很重要！！记得更改本地user的房间号
            this.user.inRoom = idx;

            //回调进入房间
            client.EnterRoom(user.name, idx);
        }
        public void ShowRoom(Room room) {
            //更新用户列表
            UserList.Items.Clear();
            foreach (var v in room.users) {
                UserList.Items.Add(v.name + "----" + v.score + "分");
            }

            //显示积分
            Info2.Visibility = Visibility;
            InfoScore.Content = this.user.score;

            //只有游戏开始后、某一个玩家可以使用画板
            ink1.IsEnabled = false;
        }

        
        /// <summary>
        /// 4、准备--->开始游戏
        /// </summary>
        private void redy_Click(object sender, RoutedEventArgs e) {
            client.StartGame(user.name, user.inRoom);
        }
        public void ShowStart(Room room) {
            MessageList.Text = "";
            ink1.Strokes.Clear();

            //画图者
            if (user.name.Equals(room.users[0].name)) {
                ink1.IsEnabled = true;
                send.IsEnabled = false;
                tips1.Text = "画图任务：" + room.question.answer;
                tips2.Text = "";
                MessageBox.Show("请开始绘画");
            }
            
            //猜图者
            else {
                ink1.IsEnabled = false;
                send.IsEnabled = true;
                tips1.Text = "提示1：" + room.question.tips[0];
                tips2.Text = "提示2：" + room.question.tips[1];
                MessageBox.Show("请开始抢答");
            }
        }


        /// <summary>
        /// 5、画板
        /// </summary>
        private void ink1_MouseUp(object sender, MouseButtonEventArgs e) {
            //将InkCanvas的墨迹转换为String
            StrokeCollection sc = ink1.Strokes;
            string inkData = (new StrokeCollectionConverter()).ConvertToString(sc);

            client.SendInk(user.inRoom, inkData);
        }
        public void ShowInk(string inkData) {
            //删除原有的Strokes
            ink1.Strokes.Clear();

            //将String转换为InkCanvas的墨迹
            Type tp = typeof(StrokeCollection);
            StrokeCollection sc =
                (StrokeCollection)(new StrokeCollectionConverter()).ConvertFrom(inkData);

            //新Strokes添加到InkCanvas中
            ink1.Strokes = sc;
        }
        private void InitColor() {//初始化画笔和画板
            inkDA.Color = currentColor;
            rrbPen.IsChecked = true;
            ink1.DefaultDrawingAttributes = inkDA;
        }
        private void RibbonRadioButton_Checked(object sender, RoutedEventArgs e) {
            string name = (e.Source as RibbonRadioButton).Label;
            switch (name) {
                case "钢笔":
                    InitColor();
                    ink1.EditingMode = InkCanvasEditingMode.Ink;
                    break;
                case "荧光笔":
                    ink1.DefaultDrawingAttributes = highlighterDA;
                    break;
                case "红色":
                    currentColor = Colors.Red;
                    InitColor();
                    break;
                case "绿色":
                    currentColor = Colors.Green;
                    InitColor();
                    break;
                case "蓝色":
                    currentColor = Colors.Blue;
                    InitColor();
                    break;
                case "墨迹":
                    ink1.EditingMode = InkCanvasEditingMode.Ink;
                    break;
                case "手势":
                    ink1.EditingMode = InkCanvasEditingMode.GestureOnly;
                    break;
                case "套索选择":
                    ink1.EditingMode = InkCanvasEditingMode.Select;
                    break;
                case "点擦除":
                    ink1.EditingMode = InkCanvasEditingMode.EraseByPoint;
                    break;
                case "笔画擦除":
                    ink1.EditingMode = InkCanvasEditingMode.EraseByStroke;
                    break;
            }
        }


        /// <summary>
        /// 6、发消息
        /// </summary>
        private void send_Click(object sender, RoutedEventArgs e) {
            client.SendMessage(user.inRoom, user.name, input.Text);
        }
        public void ShowMessage(string who, string message) {
            MessageList.Text += string.Format("{0}说：{1}\n", who, message);
        }
        public void ShowWin(User user) {
            if (user.name.Equals(this.user.name)) {
                InfoScore.Content = user.score;
                MessageBox.Show("你赢了！\n\n点击开始下一轮游戏");
                //MessageList.Text += string.Format("系统信息：你赢了！\n");
            } else {
                MessageBox.Show("好遗憾…… \n\n点击开始下一轮游戏");
                //MessageList.Text += string.Format("系统信息：好遗憾……继续加油！\n");
            }
        }
        public void ShowNewTurn(Room room) {
            //更新用户列表和积分
            UserList.Items.Clear();
            foreach (var v in room.users) {
                UserList.Items.Add(v.name + "----" + v.score + "分");
            }

            //重新开始
            ShowStart(room);
        }


        /// <summary>
        /// 7、退出游戏
        /// </summary>
        public void ShowLogout(Room room) {
            //更新用户列表和积分
            UserList.Items.Clear();
            foreach (var v in room.users) {
                UserList.Items.Add(v.name + "----" + v.score + "分");
            }
        }
        
    }
}
