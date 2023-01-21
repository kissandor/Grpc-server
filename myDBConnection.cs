using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;


namespace Stock
{
    public class myDBConnection
    {
        public MySqlConnection cn;

        public void Connect() 
        {
            cn = new MySqlConnection("Datasource = 127.0.0.1; username=root; password=; database=grpc");    
        }
    }
}
