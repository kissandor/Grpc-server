using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;


namespace Stock
{
    public class StockService : Stock.StockBase
    {
        static readonly List<string> sessions = new List<string>();
        static readonly List<StockItem> Items = new List<StockItem>();

        myDBConnection conn = new myDBConnection();
        MySqlCommand command;
        bool connected = false;

        public override async Task List(Empty vmi, Grpc.Core.IServerStreamWriter<StockItem> responseStream, Grpc.Core.ServerCallContext context)
        {
            List<StockItem> Itemss = Get();
            foreach (var StockItem in Itemss)
                await responseStream.WriteAsync(StockItem);
            //nincs lock mert async
        }
        public override Task<Result> ItemAdd(NewItem newItem, ServerCallContext context)
        {
            bool exist = false;
            lock (sessions)
            {
                if (sessions.Contains(newItem.Uid))
                {
                    exist = true;
                }
            }

            if (!exist)
            {
                return Task.FromResult(new Result { Success = "Please login first" });
            }
            else
            {

                if (!connected)
                {
                    conn.Connect();
                    connected = true;
                }
                try
                {
                    conn.cn.Open();
                    command = new MySqlCommand($"INSERT INTO `stock`(`itemCode`, `itemName`, `itemQuantity`) VALUES ('{newItem.Code}','{newItem.Name}','{newItem.Price}')", conn.cn);
                    command.ExecuteNonQuery();

                    return Task.FromResult(new Result { Success = "OK" }); ;
                }
                catch (Exception e)
                {
                    return Task.FromResult(new Result { Success = $"{e.Message}" }); ;
                }

                /*
                lock (Items)
                {
                    int i = 0;
                    for (i = 0; i < Items.Count && Items[i].Code != newItem.Code; i++) ;
                    if (i < Items.Count)
                    {
                        return Task.FromResult(new Result { Success = "Exists" });
                    }
                    else
                    {
                        StockItem temp = new StockItem();
                        temp.Name = newItem.Name; temp.Code = newItem.Code; temp.CurPrice = newItem.Price;
                        Items.Add(temp);
                        return Task.FromResult(new Result { Success = "OK!" });
                    }
                }
                */
            }
        }

        public override Task<Result> ItemDelete(DeleteStockItem item, ServerCallContext context)
        {
            bool exist = false;
            lock (sessions)
            {
                if (sessions.Contains(item.Uid))
                {
                    exist = true;
                }
            }

            if (!exist)
            {
                return Task.FromResult(new Result { Success = "login" });
            }
            else
            {
                if (!connected)
                {
                    conn.Connect();
                    connected = true;
                }
                try
                {
                    conn.cn.Open();
                    command = new MySqlCommand($"DELETE FROM `stock` WHERE itemCode='{item.Code}'", conn.cn);
                    command.ExecuteNonQuery();

                    return Task.FromResult(new Result { Success = "OK" }); ;
                }
                catch (Exception e)
                {
                    return Task.FromResult(new Result { Success = $"{e.Message}" }); ;
                }
            }
        }

        public override Task<Result> StockItemUpdate(UpdatedStockItem updatedStockItem, ServerCallContext context)
        {
            bool exist = false;
            lock (sessions)
            {
                if (sessions.Contains(updatedStockItem.Uid))
                {
                    exist = true;
                }
            }

            if (!exist)
            {
                return Task.FromResult(new Result { Success = "login" });
            }
            else
            {
                if (!connected)
                {
                    conn.Connect();
                    connected = true;
                }
                try
                {
                    conn.cn.Open();
                    command = new MySqlCommand($"UPDATE `stock` SET itemQuantity='{updatedStockItem.Price}' WHERE itemCode='{updatedStockItem.Code}'", conn.cn);
                    command.ExecuteNonQuery();

                    return Task.FromResult(new Result { Success = "OK" }); ;
                }
                catch (Exception e)
                {
                    return Task.FromResult(new Result { Success = $"{e.Message}" }); ;
                }
            }
        }

        public override Task<Result> Logout(Session_Id id, ServerCallContext context)
        {
            lock (sessions)
            {
                sessions.Remove(id.Id);
            }
            return Task.FromResult(new Result { Success = "Loged out" }); ;

        }


        public override Task<Session_Id> Login(User user, ServerCallContext context)
        {
            string id = "";
            if (isUserExist(user))
            {
                id = Guid.NewGuid().ToString();
                lock (sessions)
                {
                    sessions.Add(id);
                }

                return Task.FromResult(new Session_Id { Id = id });
            }
            else
                return Task.FromResult(new Session_Id { Id = "Login Faild" });
        }

        private List<StockItem> Get()
        {
            if (!connected)
            {
                conn.Connect();
                connected = true;
            }

            List<StockItem> items = new List<StockItem>();
            try
            {
                conn.cn.Open();
                command = new MySqlCommand("Select * from stock", conn.cn);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    StockItem item = new StockItem();
                    item.Code = reader.GetString(0);
                    item.Name = reader.GetString(1);
                    item.CurPrice = int.Parse(reader.GetString(2));
                    items.Add(item);
                }
                return items;
            }
            catch (Exception e)
            {
                return items;
            } 
        }
        private bool isUserExist(User user)
        {
            if (!connected)
            {
                conn.Connect();
                connected = true;
            }

            
            try
            {
                conn.cn.Open();
                command = new MySqlCommand($"Select Count(userName) from users where userName='{user.Name}' AND PASSWORD='{user.Passwd}'", conn.cn);
                
                return Convert.ToInt32(command.ExecuteScalar()) == 1;
                
            }
            catch (Exception e)
            {
                return false;
            }
        }
    } 
}


