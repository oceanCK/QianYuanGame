using System;
using System.Collections.Generic;

namespace CatBrotato.Data
{
    [Serializable]
    public class ConfigDatabase<T>
    {
        public List<T> items;
    }
}
