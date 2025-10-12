using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    public interface IDependencyInjection
    {
        void RegDependency(Type type, Type type1);

    }
}
