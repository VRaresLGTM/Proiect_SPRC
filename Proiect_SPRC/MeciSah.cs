using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Proiect_SPRC
{
    internal class MeciSah
    {
        public string CodLobby { get; set; }
        public TcpClient Alb { get; set; }
        public TcpClient Negru { get; set; }
        public List<TcpClient> Spectatori { get; set; } = new List<TcpClient>();
        public string StareTabla { get; set; }
        public bool EsteActiv { get; set; }

        public MeciSah(string cod)
        {
            CodLobby = cod;
            EsteActiv = true;
        }
    }
}
