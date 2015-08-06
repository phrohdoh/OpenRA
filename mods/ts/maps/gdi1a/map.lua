CarryallType = "trnsport"
DropPath1 = { para_start.Location, para_end_1.Location }
DropPath2 = { para_start.Location, para_end_2.Location }
GDIReinforcements = { "e1", "e1", "smech" }

WorldLoaded = function()
	pPlayer = Player.GetPlayer("player")
	pNod = Player.GetPlayer("Nod")

	local tw_up = "tower"
	local tw_up_vulc = "tower.vulcan"

	tower1.GrantUpgrade(tw_up)
	tower1.GrantUpgrade(tw_up_vulc)
	tower2.GrantUpgrade(tw_up)
	tower2.GrantUpgrade(tw_up_vulc)

	UserInterface.SetMissionText("Eliminate all Nod presence.", pPlayer.Color)

	Trigger.AfterDelay(3, SendReinforcements)
end

SendReinforcements = function()
	Reinforcements.ReinforceWithTransport(pPlayer, CarryallType,
		GDIReinforcements, DropPath1, { para_start.Location })

	Reinforcements.ReinforceWithTransport(pPlayer, CarryallType,
		GDIReinforcements, DropPath2, { para_start.Location })
end
