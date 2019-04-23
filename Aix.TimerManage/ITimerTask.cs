using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.TimerManage
{
    public interface ITimerTask
    {
        Task Start(MyJobContext context);

        Task Stop();

        event Func<string, Task> OnException;
    }
}
