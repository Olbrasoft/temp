﻿using Altairis.ReP.Data.Entities;

namespace Altairis.ReP.Data.Queries.DirectoryEntryQueries;
public class DirectoryEntriesQuery : BaseQuery<IEnumerable<DirectoryEntry>>
{
    public DirectoryEntriesQuery(IDispatcher dispatcher) : base(dispatcher)
    {
    }
}
