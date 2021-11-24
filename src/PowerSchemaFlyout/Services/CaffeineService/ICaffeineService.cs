using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerSchemaFlyout.Services.CaffeineService
{
    public interface ICaffeineService
    {
        void Start();
        void Stop();
        bool IsActive();
    }
}
