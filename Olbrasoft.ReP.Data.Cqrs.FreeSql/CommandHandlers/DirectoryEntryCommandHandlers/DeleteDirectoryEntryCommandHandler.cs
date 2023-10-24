﻿using Altairis.ReP.Data.Commands.DirectoryEntryCommands;

namespace Olbrasoft.ReP.Data.Cqrs.FreeSql.CommandHandlers.DirectoryEntryCommandHandlers;
public class DeleteDirectoryEntryCommandHandler : RepDbCommandHandler<DirectoryEntry, DeleteDirectoryEntryCommand, CommandStatus>
{
    public DeleteDirectoryEntryCommandHandler(IMapper mapper, RepDbContextFreeSql context) : base(mapper, context)
    {
    }

    protected override async Task<CommandStatus> GetResultToHandleAsync(DeleteDirectoryEntryCommand command, CancellationToken token)
    {
        var direcoryEntry = await GetOneOrNullAsync(de => de.Id == command.DirectoryEntryId, token);

        return direcoryEntry is null ? CommandStatus.NotFound : await RemoveAndSaveAsync(direcoryEntry, token);
    }
}
