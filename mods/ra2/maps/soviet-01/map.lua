Tick = function()
	if (Utils.RandomInteger(1, 200) == 10) then
		local delay = Utils.RandomInteger(1, 10)
		Effect.Flash("lightning-strike", delay)
		Trigger.AfterDelay(delay, function()
			Media.PlaySound("thunder.aud")
		end)
	end
end
