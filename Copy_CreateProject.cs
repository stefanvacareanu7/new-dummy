using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutomationsService.Actions.ActionModels;
using AutomationsService.Actions.ActionValueModels;
using AutomationsService.DAL.Models;
using Awork.Orbit.Client;
using Awork.Orbit.Utils;
using Awork.Phobos;

namespace AutomationsService.Actions;

public class CreateProjectTaskAction : BaseCreateTaskAction
{
    public override string ActionName
    {
        get { return "task-create-project"; }
    }

    protected override async Task ExecuteActionInternal(
        List<ActionValueEntity> actionValues,
        IActionModel eventEntity,
        PhobosContext pc)
    {
        var taskToCreate = actionValues.GetValue<TaskActionModel>("task");

        // Fallback
        if (taskToCreate.AssigneeId.HasValue && taskToCreate.AssigneeIds.IsNullOrEmpty())
        {
            taskToCreate.AssigneeIds = new() { taskToCreate.AssigneeId.Value };
        }

        taskToCreate.BaseType = "projecttask";
        taskToCreate.EntityId = taskToCreate.ProjectId;

        taskToCreate.Description = taskToCreate.Description.ReplacePlaceholders(eventEntity, await GetTriggeringUsername(pc));

        var createdTask = await CreateTask(taskToCreate, pc);
        await AddInitialAssignees(taskToCreate.AssigneeIds, createdTask.Id, pc);
        await AddTags(taskToCreate.Tags, createdTask.Id, pc);
        await AddSubtasks(taskToCreate.Subtasks, createdTask.Id, pc);
    }

    public override Task<List<ActionValueEntity>> GetActionValuesForProject(
        IHellfireClient hellfireClient,
        Guid triggeringUserId,
        Guid projectId,
        ActionEntity templateAction,
        ActionEntity newAction,
        PhobosContext phobosContext)
    {
        HellfireClient = hellfireClient;
        HellfireContext = GetHellfireContext(newAction.WorkspaceId, phobosContext, triggeringUserId);

        // When creating a project from a template, we need to set
        // the projectId to the new project.
        return Task.FromResult(GetActionValues(projectId, templateAction, newAction));
    }

    public override Task<List<ActionValueEntity>> GetActionValuesForProjectTemplate(
        IHellfireClient hellfireClient,
        Guid triggeringUserId,
        Guid projectId,
        ActionEntity templateAction,
        ActionEntity newAction,
        PhobosContext phobosContext)
    {
        HellfireClient = hellfireClient;
        HellfireContext = GetHellfireContext(newAction.WorkspaceId, phobosContext, triggeringUserId);

        // When creating a template from a project, we need to reset the projectId.
        return Task.FromResult(GetActionValues(null, templateAction, newAction));
    }

    private List<ActionValueEntity> GetActionValues(Guid? newProjectId, ActionEntity templateAction, ActionEntity newAction)
    {
        var result = new List<ActionValueEntity>();

        foreach (var actionValue in templateAction.Values)
        {
            if (actionValue.Name == "task")
            {
                var task = actionValue.Value.DeserializeWithDefaultOptions<TaskActionModel>();

                task.ProjectId = newProjectId;
                task.Lists = new();

                var actionValueEntity = new ActionValueEntity
                {
                    Name = "task",
                    Action = newAction,
                    Value = task.SerializeWithDefaultOptions()
                };

                result.Add(actionValueEntity);
            }
            else
            {
                result.Add(NewActionValueFromActionTemplateValue(newAction, actionValue));
            }
        }

        return result;
    }
}
