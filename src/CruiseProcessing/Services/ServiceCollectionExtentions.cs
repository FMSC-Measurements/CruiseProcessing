using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CruiseProcessing.Services
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection RegisterForm<tForm>(this IServiceCollection @this) where tForm : Form 
        {
            return @this.AddTransient<tForm>();
        }
    }
}
