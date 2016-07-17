using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Storage
{
    internal enum SqliteJournalModes
    {
        Delete,
        Truncate,
        Persist,
        Memory,
        Wal,
        Off
    }
}
