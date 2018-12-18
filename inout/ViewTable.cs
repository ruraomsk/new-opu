using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inout
{
    public interface ViewTable
    {
        string[] ColumnsName();
        string[] Row(int row);
        int RowsCount();
    }
}
