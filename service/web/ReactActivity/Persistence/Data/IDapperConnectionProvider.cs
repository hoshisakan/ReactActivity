using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Persistence.Data
{
    public interface IDapperConnectionProvider
    {
        public IDbConnection Connect();
        public void Dispose(bool disposing);
    }
}