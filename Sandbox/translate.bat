@echo off
set tt=..\Build\Debug\Topten.TranslateTool\net5.0\Topten.TranslateTool.exe
%tt% extract --json --t --out:extracted.json *.cs

for %%F in (strings*.json) do (
    %tt% update extracted.json %%F
    %tt% translate %%F --apikey:%1
)
