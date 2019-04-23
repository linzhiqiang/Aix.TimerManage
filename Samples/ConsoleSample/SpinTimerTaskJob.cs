using Aix.TimerManage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSample
{
   public class SpinTimerTaskJob : IMyJob
    {
        public async Task<MyJobResult> Execute(MyJobContext context)
        {
            var result = new MyJobResult();
            Console.WriteLine("SpinTimerTaskJob:" + DateTime.Now);
            await Task.Delay(5*1000);
            return result;
        }
    }
}
