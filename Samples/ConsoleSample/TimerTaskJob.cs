using Aix.TimerManage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSample
{
    public class TimerTaskJob : IMyJob
    {
        public async Task<MyJobResult> Execute(MyJobContext context)
        {
            var result = new MyJobResult();
            Console.WriteLine("TimerTaskJob:" + DateTime.Now);
            await Task.Delay(100);
            return result;
        }
    }
}
