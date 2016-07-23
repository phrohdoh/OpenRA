len_t = function(t)
    local c = 0
    for _ in pairs(t) do c = c + 1 end
    return c
end

rand_t = function(t)
    if t == nil then
        error("rand_t: t must not be nil", 2)
    elseif len_t(t) == 0 then
        error("rand_t: t must not be empty", 2)
    end

    return Utils.Random(t)
end

get_cpos_rect = function(tl, br)
    local ret = { }
    for x=tl.X,br.X,1 do
        for y=tl.Y,br.Y,1 do
            ret[#ret+1] = CPos.New(x, y)
        end
    end

    return ret
end
