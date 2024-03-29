--------------------------------------------------------------------------------
--      Copyright (c) 2015 , 蒙占志(topameng) topameng@gmail.com
--      All rights reserved.
--      Use, modification and distribution are subject to the "MIT License"
--------------------------------------------------------------------------------

--!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- 额外的Timer，我放到了Utils中，注意查看~！！！！
--!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


local setmetatable = setmetatable
local UpdateBeat = UpdateBeat
local CoUpdateBeat = CoUpdateBeat
local Time = Time

---@class Timer
Timer = {}

local Timer = Timer
local mt = {__index = Timer}

--unscaled false 采用deltaTime计时，true 采用 unscaledDeltaTime计时
function Timer.New(func, duration, loop, unscaled)
	unscaled = unscaled or false and true
	loop = loop or 1
	return setmetatable({func = func, duration = duration, time = duration, loop = loop, unscaled = unscaled, running = false}, mt)	
end

--新增[jaze]:为对象池初始化方法
function Timer:Ctor(func, duration, loop, unscaled)
    self:Reset(func, duration, loop, unscaled);
end
--[jaze]

function Timer:Start()
	self.running = true

	if not self.handle then
		self.handle = UpdateBeat:CreateListener(self.Update, self)
	end

	UpdateBeat:AddListener(self.handle)	
end

function Timer:Reset(func, duration, loop, unscaled)
	self.duration 	= duration
	self.loop		= loop or 1
	self.unscaled	= unscaled
	self.func		= func
	self.time		= duration		
end

function Timer:Stop()
	self.running = false

	if self.handle then
		UpdateBeat:RemoveListener(self.handle)	
	end
end

function Timer:Update()
	if not self.running then
		return
	end

	local delta = self.unscaled and Time.unscaledDeltaTime or Time.deltaTime	
	self.time = self.time - delta
	
	if self.time <= 0 then
		if self.loop > 0 then
			self.loop = self.loop - 1
			self.time = self.time + self.duration
		end

		if self.loop == 0 then
			self:Stop()
		elseif self.loop < 0 then
			self.time = self.time + self.duration
		end
		--后执行，防止在回调中，从池子中把自己拿出来，重置了loop = 1，导致执行了self:Stop()
		self.func()
	end
end

--给协同使用的帧计数timer
FrameTimer = {}

local FrameTimer = FrameTimer
local mt2 = {__index = FrameTimer}

function FrameTimer.New(func, count, loop)	
	local c = Time.frameCount + count
	loop = loop or 1
	return setmetatable({func = func, loop = loop, duration = count, count = c, running = false}, mt2)		
end

function FrameTimer:Reset(func, count, loop)
	self.func = func
	self.duration = count
	self.loop = loop
	self.count = Time.frameCount + count	
end

function FrameTimer:Start()		
	if not self.handle then
		self.handle = CoUpdateBeat:CreateListener(self.Update, self)
	end
	
	CoUpdateBeat:AddListener(self.handle)	
	self.running = true
end

function FrameTimer:Stop()	
	self.running = false

	if self.handle then
		CoUpdateBeat:RemoveListener(self.handle)	
	end
end

function FrameTimer:Update()	
	if not self.running then
		return
	end

	if Time.frameCount >= self.count then
		if self.loop > 0 then
			self.loop = self.loop - 1
		end

		if self.loop == 0 then
			self:Stop()
		else
			self.count = Time.frameCount + self.duration
		end

		--后执行，防止在回调中，从池子中把自己拿出来，重置了loop = 1，导致执行了self:Stop()
		self.func()
	end
end

CoTimer = {}

local CoTimer = CoTimer
local mt3 = {__index = CoTimer}

function CoTimer.New(func, duration, loop)	
	loop = loop or 1
	return setmetatable({duration = duration, loop = loop, func = func, time = duration, running = false}, mt3)			
end

function CoTimer:Start()		
	if not self.handle then	
		self.handle = CoUpdateBeat:CreateListener(self.Update, self)
	end
	
	self.running = true
	CoUpdateBeat:AddListener(self.handle)	
end

function CoTimer:Reset(func, duration, loop)
	self.duration 	= duration
	self.loop		= loop or 1	
	self.func		= func
	self.time		= duration		
end

function CoTimer:Stop()
	self.running = false

	if self.handle then
		CoUpdateBeat:RemoveListener(self.handle)	
	end
end

function CoTimer:Update()	
	if not self.running then
		return
	end

	if self.time <= 0 then
		if self.loop > 0 then
			self.loop = self.loop - 1
			self.time = self.time + self.duration
		end

		if self.loop == 0 then
			self:Stop()
		elseif self.loop < 0 then
			self.time = self.time + self.duration
		end
		--后执行，防止在回调中，从池子中把自己拿出来，重置了loop = 1，导致执行了self:Stop()
		self.func()
	end
	
	self.time = self.time - Time.deltaTime
end

---@class StampTimer
StampTimer = {}

local StampTimer = StampTimer
local mt = {__index = StampTimer}

function StampTimer.New(func, duration, loop)
	loop = loop or 1
	return setmetatable({func = func, duration = duration, time = duration, loop = loop, running = false}, mt)
end

function StampTimer:Ctor(func, duration, loop)
	self:Reset(func, duration, loop);
end

function StampTimer:Start()
	self.running = true
	self.currTime = TimerManager.GetServerTimeStamp();

	if not self.handle then
		self.handle = UpdateBeat:CreateListener(self.Update, self)
	end

	UpdateBeat:AddListener(self.handle)
end

function StampTimer:Reset(func, duration, loop)
	self.duration 	= duration
	self.loop		= loop or 1
	self.func		= func
	self.time		= duration
	self.currTime = TimerManager.GetServerTimeStamp();
end

function StampTimer:Stop()
	self.running = false

	if self.handle then
		UpdateBeat:RemoveListener(self.handle)
	end
end

function StampTimer:Update()
	if not self.running then
		return
	end
	local currTime = TimerManager.GetServerTimeStamp()
	self.time = self.time - (currTime - self.currTime)
	self.currTime = currTime

	if self.time <= 0 then
		if self.loop > 0 then
			self.loop = self.loop - 1
			self.time = self.time + self.duration
		end

		if self.loop == 0 then
			self:Stop()
		elseif self.loop < 0 then
			self.time = self.time + self.duration
		end
		--后执行，防止在回调中，从池子中把自己拿出来，重置了loop = 1，导致执行了self:Stop()
		self.func()
	end
end
