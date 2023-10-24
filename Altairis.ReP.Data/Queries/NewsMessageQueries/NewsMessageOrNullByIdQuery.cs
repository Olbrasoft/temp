﻿using Altairis.ReP.Data.Dtos.NewsMessageDtos;

namespace Altairis.ReP.Data.Queries.NewsMessageQueries;
public class NewsMessageOrNullByIdQuery : BaseQuery<NewsMessageDto?>
{
    public NewsMessageOrNullByIdQuery(IDispatcher dispatcher) : base(dispatcher)
    {
    }

    public int NewsMessageId { get; set; }
}
