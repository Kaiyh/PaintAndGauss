using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Server {
    [ServiceContract(Namespace = "GuessServer",
                     SessionMode = SessionMode.Required,
                     CallbackContract = typeof(IGuessCallback))]
    public interface IGuessService {
        //1、登录
        [OperationContract(IsOneWay = true)]
        void Login(string user, string password);
        
        //2、进入大厅
        [OperationContract(IsOneWay = true)]
        void EnterHall(string user);
        
        //3、进入房间
        [OperationContract(IsOneWay = true)]
        void EnterRoom(string user, int roomNumber);

        //4、开始游戏
        [OperationContract(IsOneWay = true)]
        void StartGame(string user, int room);
        
        //5、发送数字墨迹
        [OperationContract(IsOneWay = true)]
        void SendInk(int room, string ink);

        //6、发送消息
        [OperationContract(IsOneWay = true)]
        void SendMessage(int room, string user, string message);
        
        //7、退出
        [OperationContract(IsOneWay = true)]
        void Logout(string user, int room);
    }
    
    [ServiceContract]
    public interface IGuessCallback {
        //1、回调登录
        [OperationContract(IsOneWay = true)]
        void ShowLogin(int loginCode, User user);
        
        //2、回调进入大厅
        [OperationContract(IsOneWay = true)]
        void ShowHall(string user);

        //3、回调进入房间
        [OperationContract(IsOneWay = true)]
        void ShowRoom(Room room);

        //4、回调开始游戏
        [OperationContract(IsOneWay = true)]
        void ShowStart(Room room);

        //5、回调显示墨迹 
        [OperationContract(IsOneWay = true)]
        void ShowInk(string ink);

        //6、回调显示信息
        [OperationContract(IsOneWay = true)]
        void ShowMessage(string who, string message);
        
        //7、回调胜利
        [OperationContract(IsOneWay = true)]
        void ShowWin(User user);

        //8、回调开始新游戏
        [OperationContract(IsOneWay = true)]
        void ShowNewTurn(Room room);

        //9、回调退出
        [OperationContract(IsOneWay = true)]
        void ShowLogout(Room room);
    }



    [DataContract]
    public class User {
        [DataMember]
        public string name {
            get; set;
        }
        [DataMember]
        public int score {
            get; set;
        }
        [DataMember]
        public int inRoom {
            get; set;
        }
        [DataMember]
        public bool redy {
            get; set;
        }
        [DataMember]
        public string icon;
        [DataMember]
        public readonly IGuessCallback callback;
        public void addScore() {
            score++;
        }
        public User(string name, IGuessCallback callback) {
            this.name = name;
            this.callback = callback;
        }
    }



    [DataContract]
    public class Question {
        [DataMember]
        public string answer {
            get; set;
        }
        [DataMember]
        public int level {
            get; set;
        }
        [DataMember]
        public List<string> tips {
            get; set;
        }
        private Random r;
        public Question() {
            r = new Random();
            Update();
        }
        public void Update() {
            int cas = r.Next(1, 8 + 1);
            switch (cas) {
                case 1: {
                        answer = "三明治";
                        level = 1;
                        tips = new List<string>();
                        tips.Add("三个字");
                        tips.Add("一种食物");
                    }
                    break;
                case 2: {
                        answer = "电脑";
                        level = 1;
                        tips = new List<string>();
                        tips.Add("两个字");
                        tips.Add("电器");
                    }
                    break;
                case 3: {
                        answer = "飞流直下三千尺";
                        level = 1;
                        tips = new List<string>();
                        tips.Add("七个字");
                        tips.Add("一句唐诗");
                    }
                    break;
                case 4: {
                        answer = "马克思主义";
                        level = 1;
                        tips = new List<string>();
                        tips.Add("五个字");
                        tips.Add("一种伟大的思想");
                    }
                    break;
                case 5: {
                        answer = "水龙头";
                        level = 1;
                        tips = new List<string>();
                        tips.Add("三个字");
                        tips.Add("常见的供水设备");
                    }
                    break;
                case 6: {
                        answer = "煤球";
                        level = 1;
                        tips = new List<string>();
                        tips.Add("两个字");
                        tips.Add("取暖");
                    }
                    break;
                case 7: {
                        answer = "藏羚羊";
                        level = 1;
                        tips = new List<string>();
                        tips.Add("三个字");
                        tips.Add("一种高原动物");
                    }
                    break;
                case 8: {
                        answer = "导弹";
                        level = 1;
                        tips = new List<string>();
                        tips.Add("两个字");
                        tips.Add("武器");
                    }
                    break;
            }
        }
    }



    [DataContract]
    public class Room {
        [DataMember]
        public int id;
        [DataMember]
        public string name;
        [DataMember]
        public DateTime thisTurnBeginTime;
        [DataMember]
        public DateTime thisTurnEndTime;
        [DataMember]
        public List<User> users;
        [DataMember]
        public Question question;
    }
}
