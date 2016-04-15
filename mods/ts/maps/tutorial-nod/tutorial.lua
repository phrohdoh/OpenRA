ProducedUnitTypes =
{
    { nodhand1, { "e1", "e3" } },
    { gdibar1, { "e1", "e2" } }
}

ProduceUnits = function(t)
    local factory = t[1]
    if not factory.IsDead then
        local unitType = t[2][Utils.RandomInteger(1, #t[2] + 1)]
        factory.Wait(Actor.BuildTime(unitType))
        factory.Produce(unitType)
        factory.CallFunc(function() ProduceUnits(t) end)
    end
end

SetupFactories = function()
    Utils.Do(ProducedUnitTypes, function(pair)
        Trigger.OnProduction(pair[1], function(_, a) BindActorTriggers(a) end)
    end)
end

BindActorTriggers = function(a)
    if a.HasProperty("Hunt") then
        Trigger.OnIdle(a, a.Hunt)
    end

    if a.HasProperty("HasPassengers") then
        Trigger.OnDamaged(a, function()
            if a.HasPassengers then
                a.Stop()
                a.UnloadPassengers()
            end
        end)
    end
end

WorldLoaded = function()
    nod = Player.GetPlayer("Nod")
    gdi = Player.GetPlayer("GDI")
    
    SetupFactories()

    Utils.Do(ProducedUnitTypes, ProduceUnits)
end
