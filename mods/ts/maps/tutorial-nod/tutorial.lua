McvIsIdle = function(mcv)
    if (not mcv.IsInWorld) or mcv.IsDead then
        Trigger.Clear(mcv, 'OnIdle')
        return
    end

    Media.DisplayMessage("To deploy a vehicle: select it, place the cursor over the vehicle, and right-click on it.")
    Trigger.AfterDelay(DateTime.Seconds(8), function()
        Media.DisplayMessage("Deploy your M.C.V. now that it has reached its destination.")
        Beacon.New(nod, MCV.CenterPosition, 750, false)
    end)

    Trigger.Clear(mcv, 'OnIdle')
end

WorldLoaded = function()
    nod = Player.GetPlayer("Nod")
    gdi = Player.GetPlayer("GDI")
    Camera.Position = MCV.CenterPosition

    Media.DisplayMessage("Select your forces by left-clicking then moving your cursor to draw a box around them, then releasing the held mouse button. "
        .. "You may select individual units by hovering your cursor over them then left-clicking.")

    Trigger.AfterDelay(DateTime.Seconds(12), function()
        Media.DisplayMessage("With your units selected, move them to the derelict base to the East by right-clicking on a destination cell.")
    end)

    Trigger.AfterDelay(DateTime.Seconds(24), function()
        local guards1 = { Actor436, Actor438, Actor439 }
        local guards2 = { Actor437, Actor442, Actor441 }

        if MCV.IsInWorld and not MCV.IsDead and MCV.IsIdle then
            MCV.Move(CPos.New(36, 3))
        end

        Trigger.OnIdle(MCV, McvIsIdle)

        Utils.Do(guards1, function(guard)
            if guard.IsInWorld and not guard.IsDead and guard.IsIdle then
                guard.AttackMove(CPos.New(45, 6), 1)
            end
        end)

        Utils.Do(guards2, function(guard)
            if guard.IsInWorld and not guard.IsDead and guard.IsIdle then
                guard.AttackMove(CPos.New(45, 8), 1)
            end
        end)

        if Buggy.IsInWorld and not Buggy.IsDead and Buggy.IsIdle then
            Buggy.Move(CPos.New(39, 9))
        end
    end)
end
