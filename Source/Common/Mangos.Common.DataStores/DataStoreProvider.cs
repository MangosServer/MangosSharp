using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Mangos.Common.DataStores
{
    public class DataStoreProvider
    {
        private readonly string dbcDirectory = "dbc";

        private Dictionary<string, DataStore> dataStores;

        public DataStoreProvider()
        {
            dataStores = new Dictionary<string, DataStore>();
        }

        public DataStore GetDataStore(string dbcFileName)
        {
            if (dataStores.ContainsKey(dbcFileName))
            {
                return dataStores[dbcFileName];
            }
            else
            {
                var path = Path.Combine(dbcDirectory, dbcFileName);
                var dataStore = new DataStore();
                dataStore.LoadFromFile(path);
                dataStores[dbcFileName] = dataStore;
                return dataStore;
            }
        }

        public async ValueTask<DataStore> GetDataStoreAsync(string dbcFileName)
        {
            if(dataStores.ContainsKey(dbcFileName))
            {
                return dataStores[dbcFileName];
            }
            else
            {
                var path = Path.Combine(dbcDirectory, dbcFileName);
                var dataStore = new DataStore();
                await dataStore.LoadFromFileAsync(path);
                dataStores[dbcFileName] = dataStore;
                return dataStore;
            }
        }
    }
}
