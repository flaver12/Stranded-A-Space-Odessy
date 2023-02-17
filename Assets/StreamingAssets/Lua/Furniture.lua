-- TODO better way to define globals
Enterability_Yes = 0;
Enterability_No = 1;
Enterability_Soon = 2;

-- TODO move this to the mathf lib?
function Clamp01(value)
    if value > 1 then
        return 1
    elseif value < 0 then
        return 0
    end

    return value
end




function O2Generator_OnTickAction(furniture, deltaTime)

    -- check for room
    if furniture.Tile.room == nil then
        return "Furniture's room was null"
    end

    if furniture.Tile.room.GetGasAmount("O2") < 0.20 then
        furniture.Tile.room.ChangeGas("O2", 0.01 * deltaTime)
    else
        -- TODO power safe mode?
    end
end

function Door_OnTickAction(furniture, deltaTime)
    if furniture.GetParameteter("isOpening") >= 1 then
        furniture.SetParameteter("openness", furniture.GetParameteter("openness") + deltaTime * 4)

        if furniture.GetParameteter("openness") >= 1 then
            furniture.SetParameteter("isOpening", 0)
        end
    else
        furniture.SetParameteter("openness", furniture.GetParameteter("openness") - deltaTime * 4)
    end

    furniture.SetParameteter("openness", Clamp01(furniture.GetParameteter("openness")))

    -- call the change callback
    if furniture.onChanged ~= nil then
        furniture.onChanged(furniture)
    end
end

function Door_IsEnterable(furniture)
    furniture.SetParameteter("isOpening", 1);

    if furniture.GetParameteter("openness") >= 1 then
        return Enterability_Yes -- yes
    end

    return Enterability_Soon --soon
end

function Stockpile_OnTickAction(furniture, deltaTime)
    -- We need to ensure that we have a job on the queue
    -- Asking for: I am empty?: That all loos item come to us
    -- Asking for: We have something: Then if below max stack size, bring me more
    if furniture.Tile.Item ~= nil and furniture.Tile.Item.StackSize >= furniture.Tile.Item.maxStackSize then
        -- We are full
        furniture.CancelJobs()
        return
    end

    if furniture.JobCount() > 0 then
        -- all done
        return
    end

    if furniture.Tile.Item ~= nil and furniture.Tile.Item.StackSize == 0 then
        -- Something went wrong here!
        furniture.CancelJobs()
        return "Stockpile has a 0 sized stack!"
    end

    local desiredItems = {}
    if furniture.Tile.Item == nil then
        desiredItems = Stockpile_GetItemsFromFiler()
    else
        local desiredItem = furniture.Tile.Item.Clone()
        desiredItem.maxStackSize = desiredItem.maxStackSize - desiredItem.StackSize;
        desiredItem.StackSize = 0;
        desiredItems = { desiredItem }
    end

    -- Create the new job
    local job = Job.__new(
        furniture.Tile,
        nil,
        nil,
        0,
        desiredItems,
        false
    )
    job.canTakeFromStockpile = false
    job.RegisterJobWorkedCallback("Stockpile_JobWorked")
    furniture.AddJob(job)

end

function Stockpile_GetItemsFromFiler()
    return { Item.__new("Steel Plate", 50, 0) }
end

-- Callback for when the job for the stockpile got worked!
function Stockpile_JobWorked(job)
    job.CancelJob()

    for k, item in pairs(job.itemRequirements) do
        if item.StackSize > 0 then
            World.Instance.itemManager.InstallItem(job.Tile, item)
            return
        end
    end
end

return "LUA parsed! Bitches!"