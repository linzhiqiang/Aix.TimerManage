using Aix.TimerManage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSample
{
    class CrontabTaskJob : IMyJob
    {
        public async Task<MyJobResult> Execute(MyJobContext context)
        {
            var result = new MyJobResult();
            Console.WriteLine("CrontabTaskJob:" + DateTime.Now);
            //await Task.Delay(2000);
            await Task.CompletedTask;
            return result;
        }
    }
}
