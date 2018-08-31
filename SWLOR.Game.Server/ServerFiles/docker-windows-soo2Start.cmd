docker run --rm -it ^
-p 5121:5121 -p 5121:5121/udp ^
-e NWN_PORT=5121 ^
-v c:/NeverwinterNights/NWN:/nwn/home ^
-v /opt/nwn/logs.0:/nwn/run/logs.0 ^
-e NWN_SERVERNAME="Mono testing" ^
-e NWN_MODULE="Solar Odyssey Online 2" ^
-e NWN_PUBLICSERVER=1 ^
-e NWN_MAXCLIENTS=96 ^
-e NWN_MINLEVEL=1 ^
-e NWN_MAXLEVEL=40 ^
-e NWN_PAUSEANDPLAY=0 ^
-e NWN_PVP=2 ^
-e NWN_SERVERVAULT=1 ^
-e NWN_ELC=0 ^
-e NWN_ILR=0 ^
-e NWN_GAMETYPE=3 ^
-e NWN_ONEPARTY=0 ^
-e NWN_DIFFICULTY=3 ^
-e NWN_AUTOSAVEINTERVAL=0 ^
-e NWN_RELOADWHENEMPTY=0 ^
-e NWN_PLAYERPASSWORD=r ^
-e NWN_DMPASSWORD=r ^
-e NWN_ADMINPASSWORD=r ^
-e NWNX_CORE_LOG_LEVEL=6 ^
-e NWNX_JVM_LOG_LEVEL=6 ^
-e NWNX_MONO_LOG_LEVEL=6 ^
-e NWNX_ADMINISTRATION_SKIP=y ^
-e NWNX_BEHAVIOURTREE_SKIP=y ^
-e NWNX_CHAT_SKIP=n ^
-e NWNX_CREATURE_SKIP=n ^
-e NWNX_EVENTS_SKIP=n ^
-e NWNX_DATA_SKIP=y ^
-e NWNX_DAMAGE_SKIP=n ^
-e NWNX_JVM_SKIP=y ^
-e NWNX_METRICS_INFLUXDB_SKIP=y ^
-e NWNX_MONO_SKIP=n ^
-e NWNX_OBJECT_SKIP=n ^
-e NWNX_PLAYER_SKIP=n ^
-e NWNX_LUA_SKIP=y ^
-e NWNX_RUBY_SKIP=y ^
-e NWNX_SERVERLOGREDIRECTOR_SKIP=y ^
-e NWNX_SQL_SKIP=y ^
-e NWNX_THREADWATCHDOG_SKIP=y ^
-e NWNX_TRACKING_SKIP=y ^
-e NWNX_TWEAKS_SKIP=n ^
-e NWNX_MONO_ASSEMBLY=/nwn/home/mono/SOO2.Game.Server.dll ^
-e NWNX_MONO_BASE_DIRECTORY=/nwn/home/mono ^
-e NWNX_MONO_APP_CONFIG=/nwn/home/mono/SOO2.Game.Server.dll.config ^
-e NWNX_TWEAKS_HIDE_CLASSES_ON_CHAR_LIST=true ^
-e NWNX_TWEAKS_DISABLE_PAUSE=true ^
-e NWNX_TWEAKS_COMPARE_VARIABLES_WHEN_MERGING=true ^
-e SQL_SERVER_IP_ADDRESS=127.0.0.1 ^
-e SQL_SERVER_USERNAME=yourUsername ^
-e SQL_SERVER_PASSWORD=yourPassword ^
-e SQL_SERVER_DATABASE=soo2 ^
nwnxee/unified:latest-full