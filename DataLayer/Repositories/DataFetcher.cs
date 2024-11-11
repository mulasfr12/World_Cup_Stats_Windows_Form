using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Repositories
{
    public abstract class DataFetcher
    {
        public abstract Task<string> FetchData(string endpoint);

    }
}
