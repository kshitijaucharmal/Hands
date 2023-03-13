local sock = require("socket")

local HOST = 'localhost'
local PORT = 65432

local client = sock.connect(HOST, PORT)
while client do
    client:send('Hello World\n')
    local line, err = client:receive()
    if not err then
        print(line)
    end
end
