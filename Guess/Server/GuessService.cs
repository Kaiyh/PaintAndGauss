using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Server {
    public class GuessService : IGuessService {
        /// <summary>
        ///  1、登录
        /// </summary>
        public void Login(string name, string password) {
            OperationContext context = OperationContext.Current;
            IGuessCallback callback = context.GetCallbackChannel<IGuessCallback>();
            User user = new User(name, callback);

            //有重名，登录失败
            if (CC.users.ContainsKey(name)) {
                user.callback.ShowLogin(-1, null);
                return;
            }

            //初始化CC.users（当前用户添加到xxx）
            CC.users.Add(name, user);

            //回调登录
            user.callback.ShowLogin(0, user);
        }


        /// <summary>
        //// 2、进入大厅
        /// </summary>
        public void EnterHall(string name) {
            User user = CC.users[name];
            user.callback.ShowHall(name);
        }


        /// <summary>
        /// 3、进入房间
        /// </summary>
        public void EnterRoom(string name, int room) {
            //创建用户User
            User user = CC.users[name];
            user.inRoom = room;

            //初始化CC.rooms
            if (CC.rooms.ContainsKey(room) == false) {
                CC.rooms.Add(room, new Room());
                CC.rooms[room].users = new List<User>();
                CC.rooms[room].question = new Question();
            }
            
            //把该用户添加到房间中
            CC.rooms[room].users.Add(user);

            //给该房间的所有用户发送最新房间用户的信息
            foreach (var v in CC.rooms[room].users) {
                v.callback.ShowRoom(CC.rooms[room]);
                //v.callback.ShowMessage(name, name + "进入房间了！");
            }
        }


        /// <summary>
        /// 4、开始游戏
        /// </summary>
        public void StartGame(string user, int room) {
            //当前用户已准备好
            CC.users[user].redy = true;

            //判断当前房间是否都准备好
            foreach (var v in CC.rooms[room].users) {
                if (!v.redy) {
                    return;
                }
            }
            foreach (var v in CC.rooms[room].users) {
                v.callback.ShowStart(CC.rooms[room]);
            }
        }


        /// <summary>
        /// 5、发送数字墨迹
        /// </summary>
        public void SendInk(int room, string ink) {
            foreach (var v in CC.rooms[room].users) {
                v.callback.ShowInk(ink);
            }
        }


        /// <summary>
        /// 6、发消息，并判断答案
        /// </summary>
        public void SendMessage(int room, string user, string message) {
            string answer = CC.rooms[room].question.answer;
            if (message.Equals(answer)) {
                foreach (var v in CC.rooms[room].users) {
                    if (v.name == user) {
                        v.addScore();
                        v.callback.ShowMessage("系统信息", "你猜中了");
                    } else {
                        v.callback.ShowMessage("系统信息", string.Format("{0} 猜中了", v.name));
                    }
                    v.callback.ShowWin(CC.users[user]);
                }
                RollUserAndRestart(room);//开始新的一轮游戏
            } else {
                foreach (var v in CC.rooms[room].users) {
                    v.callback.ShowMessage(user, message);
                }
            }
        }
        private void RollUserAndRestart(int room) {//开始新的一轮游戏
            User u = CC.rooms[room].users[0];
            CC.rooms[room].users.RemoveAt(0);
            CC.rooms[room].users.Add(u);
            CC.rooms[room].question.Update();
            //CC.rooms[room].thisTurnBeginTime = DateTime.Now;
            //CC.rooms[room].thisTurnEndTime = DateTime.Now;

            foreach (var v in CC.rooms[room].users) {
                v.callback.ShowNewTurn(CC.rooms[room]);
            }
        }


        /// <summary>
        /// 7、退出游戏
        /// </summary>
        public void Logout(string name, int room) {
            //更新所在房间用户
            User user = CC.users[name];
            CC.rooms[room].users.Remove(user);
            foreach (var v in CC.rooms[room].users) {
                v.callback.ShowLogout(CC.rooms[room]);
            }

            // 删除用户
            CC.users.Remove(name);
            user = null;
        }

    }
}
