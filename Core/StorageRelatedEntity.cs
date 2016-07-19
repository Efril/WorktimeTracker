using Core.Framework;
using Core.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public abstract class StorageRelatedEntity
    {
        protected TrackDbProvider TrackDb
        {
            get;
            private set;
        }
        protected virtual bool BoundToStorage
        {
            get { return TrackDb != null; }
        }

        protected void BindToStorageBase(TrackDbProvider TrackDb)
        {
            Contract.Requires(TrackDb != null);
            this.TrackDb = TrackDb;
        }

        protected static MethodCallResult CreateNotBoundToStorageYetMethodResult()
        {
            return MethodCallResult.CreateFail("Not bound to storage yet.");
        }
    }
}
