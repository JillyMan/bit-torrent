using System;
using System.Collections.Generic;
using System.Net;

namespace BitTorrent.Models
{
    public class Tracker
    {
        public event EventHandler<List<IPEndPoint>> PeerListUpdated;

        public string Address { get; private set; }   
 
        public Tracker(string address)
        {
            Address = address;
        }
    }
}