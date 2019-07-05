using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyIoTApp
{
    public class Data
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Time { get; set; }
        public string Value { get; set; }
        public string Result { get; set; }


    }
}
