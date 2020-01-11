using Couchbase.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelloCouch.Data
{
    public interface IContactsBucketProvider: INamedBucketProvider
    {
    }
}
