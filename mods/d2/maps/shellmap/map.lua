WaveCount = 0

InfantryReinforcements = {
    "light_inf",
    "light_inf",
    "light_inf",
    "trooper",
    "trooper"
}

VehicleReinforcements = {
    "combat_tank_a"
}

InfantryAttackers = {
    "light_inf",
    "light_inf",
    "trooper"
}

VehicleAttackers = {
    "combat_tank_h"
}

AtreidesSpawns = { }
HarkonnenSpawns = { }
HarkonnenTargets = { }

BaseFootprint = { }

SendWave = function(unitTypes, nextDelayTime)
    WaveCount = WaveCount + 1

    if len_t(HarkonnenSpawns) == 0 then
        error("HarkonnenSpawns is empty")
    end

    if len_t(HarkonnenTargets) == 0 then
        error("HarkonnenTargets is empty")
    end

    local spawn = rand_t(HarkonnenSpawns).Location
    local destination = rand_t(HarkonnenTargets).Location

    Reinforcements.Reinforce(harkonnen, unitTypes, { spawn, destination }, 2, function(unit)
        Trigger.OnIdle(unit, function(_)
            if not unit.IsDead and unit.IsInWorld then
                unit.Hunt()
            end
        end)
    end)

    if nextDelayTime ~= nil and nextDelayTime > 0 then
        Trigger.AfterDelay(DateTime.Seconds(nextDelayTime), function() SendWave(unitTypes, nextDelayTime) end)
    end
end -- SendWave

ReinforceAtreides = function(unitTypes, nextDelayTime)
    if AtreidesBarracks == nil
       or AtreidesBarracks.IsDead
       or atreides.Resources >= 500
    then
        return
    end

    if len_t(AtreidesSpawns) == 0 then
        error("AtreidesSpawns is empty")
    end

    local spawn = rand_t(AtreidesSpawns).Location
    local destination = rand_t(HarkonnenTargets).Location

    Reinforcements.Reinforce(atreides, unitTypes, { spawn, destination }, 2, function(unit)
        Trigger.OnIdle(unit, function(u)
            if not unit.IsDead and unit.IsInWorld then
                u.AttackMove(rand_t(HarkonnenTargets).Location)
            end
        end)
        atreides.Resources = atreides.Resources - 500
    end)

    if nextDelayTime ~= nil and nextDelayTime > 0 then
        Trigger.AfterDelay(DateTime.Seconds(nextDelayTime + 1), function() ReinforceAtreides(unitTypes, nextDelayTime) end)
    end
end -- ReinforceAtreides

WorldLoaded = function()
    atreides = Player.GetPlayer("Atreides")
    harkonnen = Player.GetPlayer("Harkonnen")

    AtreidesSpawns = atreides.GetActorsByType("waypoint")
    HarkonnenSpawns = harkonnen.GetActorsByType("waypoint")
    HarkonnenTargets = Map.ActorsWithTag("harkonnen_target")

    BaseFootprint = get_cpos_rect(base_tl.Location, base_br.Location)

    Trigger.AfterDelay(DateTime.Seconds(5), function() ReinforceAtreides(InfantryReinforcements, 15) end)
    Trigger.AfterDelay(DateTime.Seconds(15), function() ReinforceAtreides(VehicleReinforcements, 59) end)
    Trigger.AfterDelay(DateTime.Seconds(35), function() SendWave(InfantryAttackers, 35) end)
    Trigger.AfterDelay(DateTime.Seconds(45), function() SendWave(VehicleAttackers, 55) end)

    Trigger.OnExitedFootprint(BaseFootprint, function(u, id)
        if not u.IsDead and u.HasProperty("AttackMove") then
            u.Stop()
            u.AttackMove(rand_t(HarkonnenTargets).Location)
        end
    end)
end -- WorldLoaded
