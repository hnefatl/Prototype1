using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data
{
    public interface IExpandsData
    {
        bool Expand(IDataRepository Repo);
    }
}
