using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Awork.Enterprise.Event;
using Awork.Enterprise.Extensions;
using Awork.Enterprise.Helper;
using Awork.Enterprise.Message;
using Awork.Phobos;
using Awork.Phobos.Logging;
using FilesService.DAL;
using FilesService.Models.External;
using Microsoft.Extensions.Logging;

namespace FilesService.Events;

public class CommentEntityEventReceiver : IAworkEventReceiver
{
    private const string FileTagRegEx = @"(?<=~\[fileId\:)([\s\S]*?)(?=\])";
    private readonly ILogger<CommentEntityEventReceiver> _logger;
    private readonly IFileInfosRepository _fileInfosRepository;

    public CommentEntityEventReceiver(ILogger<CommentEntityEventReceiver> logger, IFileInfosRepository fileInfosRepository)
    {
        _logger = logger;
        _fileInfosRepository = fileInfosRepository;
    }

    public string GetTopicName()
    {
        return "CommentEntity";
    }

    public bool ShouldExecute(EnterpriseEventMessage message, PhobosContext pc)
    {
        return true;
    }

    public async Task Execute(EnterpriseEventMessage message, PhobosContext phobosContext)
    {
        using var pc = phobosContext.StartNewChild("UpdateCommentRelatedFlag");
        try
        {
            var comment = message.GetEntity<DefaultCommentModel>();
            if (message.IsUpdate()
                && message.IsPropertyChange()
                && Validations.ContainsPropertyChangeWithNewValue(message, false, "Message"))
            {
                message.Changes.GetPropertyValues("Message", out var oldMessage, out var newMessage);

                var oldFileTagsMatches = Regex.Matches(oldMessage, FileTagRegEx);
                var newFileTagsMatches = Regex.Matches(newMessage, FileTagRegEx);
                await UpdateCountOfFiles(oldFileTagsMatches, newFileTagsMatches, pc);
            }
            else
            {
                var fileTags = Regex.Matches(comment.Message, FileTagRegEx);
                if (fileTags.Count > 0)
                {
                    foreach (Match file in fileTags)
                    {
                        if (Guid.TryParse(file.Value, out var fileId))
                        {
                            await UpdateFileCount(fileId, message.EventType, pc);
                        }
                    }
                }
            }

            await _fileInfosRepository.SaveChanges(pc, executeMvUpdate: false, sendEnterpriseEvent: false);
        }
        catch (Exception ex)
        {
            PhobosHelper.AddEntityMetaInformationAsLog(pc, message, true);
            _logger.Error(pc, ex, "Error in UpdateCommentRelatedFlag.");
        }
    }

    private async Task UpdateCountOfFiles(
        MatchCollection oldFileTagsMatches,
        MatchCollection newFileTagsMatches,
        PhobosContext pc)
    {
        var files = new List<DAL.Models.FileInfoEntity>();
        if (oldFileTagsMatches.Count > 0)
        {
            foreach (Match fileMatch in oldFileTagsMatches)
            {
                if (Guid.TryParse(fileMatch.Value, out var fileId))
                {
                    var file = await _fileInfosRepository.GetById(fileId, pc);
                    if (file != null)
                    {
                        file.CommentFileCount -= 1;
                        if (file.CommentFileCount < 0)
                        {
                            file.CommentFileCount = 0;
                        }

                        await _fileInfosRepository.Update(file);
                        files.Add(file);
                    }
                }
            }

            foreach (Match fileMatch in newFileTagsMatches)
            {
                if (Guid.TryParse(fileMatch.Value, out var fileId))
                {
                    var file = files.FirstOrDefault(m => m.Id == fileId);
                    if (file == null)
                    {
                        file = await _fileInfosRepository.GetById(fileId, pc);
                    }

                    if (file != null)
                    {
                        file.CommentFileCount += 1;
                        await _fileInfosRepository.Update(file);
                    }
                }
            }
        }
    }

    private async Task UpdateFileCount(Guid fileId, string eventType, PhobosContext pc)
    {
        var file = await _fileInfosRepository.GetById(fileId, pc);
        if (file != null)
        {
            if (eventType == EnterpriseEventType.Added)
            {
                file.CommentFileCount += 1;
            }
            else if (eventType == EnterpriseEventType.Deleted)
            {
                file.CommentFileCount -= 1;
            }

            if (file.CommentFileCount < 0)
            {
                file.CommentFileCount = 0;
            }

            await _fileInfosRepository.Update(file);
        }
    }
}
