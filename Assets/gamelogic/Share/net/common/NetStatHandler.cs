using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum NetStat
{
    NotConnect = 0,
    StartConnect = 1,
    Connecting = 2,
    Connected = 3,
    FailedConnect = 4,
    BreakConnect = 5,
    ReConnect = 6,
}

public class NetStatHandler
{
    private NetStat stat;

    public NetStatHandler(NetStat stat)
    {
        this.stat = stat;
    }

    public void Update(NetStat stat)
    {
        this.stat = stat;
        switch (stat)
        {
            case NetStat.NotConnect:
                break;
            case NetStat.StartConnect:
                break;
            case NetStat.Connecting:
                break;
            case NetStat.Connected:
                break;
            case NetStat.BreakConnect:
                break;
        }
    }
}
