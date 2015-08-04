WorldLoaded = function()
	local tw_up = "tower.vulcan"

	if tower1.HasProperty("AcceptsUpgrade") then
		if tower1.AcceptsUpgrade(tw_up) then
			tower1.GrantUpgrade(tw_up)
		else
			Media.DisplayMessage("t1 doesn't accept " .. tw_up)
		end
	else
		Media.DisplayMessage("t1 doesn't accept upgrades")
	end

	if tower2.HasProperty("AcceptsUpgrade") then
		if tower2.AcceptsUpgrade(tw_up) then
			tower2.GrantUpgrade(tw_up)
		else
			Media.DisplayMessage("t2 doesn't accept " .. tw_up)
		end
	else
		Media.DisplayMessage("t2 doesn't accept upgrades")
	end
end
