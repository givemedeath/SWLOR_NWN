@echo off
REM Packs the fresh "Erie Metroplex" module (ModuleSR) into a .mod, then deploys it
REM to the debugserver. Mirror of Module\PackModule.cmd for the two-module layout.
REM See ModuleSR\README.md for why this module exists.
setlocal
pushd "%~dp0"

REM The SWLOR.CLI packer enumerates all 16 module resource folders plus ncs/nss and
REM calls nwn_gff on every file it finds. Each folder must therefore EXIST, and the
REM empty ones must contain no stray files. Create any that are missing.
for %%d in (are dlg fac gic git ifo itp jrl utc utd uti utm utp uts utt utw ncs nss) do (
  if not exist "%%d" mkdir "%%d"
)

REM Script scaffolding (compiled .ncs event stubs + .nss sources) is identical to the
REM dormant SW module, so we mirror it in at pack time instead of duplicating 300+
REM files in git. /MIR keeps ncs\ and nss\ exactly matching the source.
echo Mirroring script scaffolding from ..\Module...
robocopy "..\Module\ncs" ".\ncs" /MIR /NFL /NDL /NJH /NJS /NP >nul
robocopy "..\Module\nss" ".\nss" /MIR /NFL /NDL /NJH /NJS /NP >nul

echo Packing Erie Metroplex.mod...
"..\tools\SWLOR.CLI\SWLOR.CLI.exe" -p ".\Erie Metroplex.mod"
if not exist ".\Erie Metroplex.mod" (
  echo ERROR: pack did not produce Erie Metroplex.mod
  popd & endlocal & exit /b 1
)

REM Deploy to the debugserver module directory if present.
if exist "..\debugserver\modules" (
  echo Deploying to ..\debugserver\modules...
  copy /Y ".\Erie Metroplex.mod" "..\debugserver\modules\Erie Metroplex.mod" >nul
)

echo Done.
popd
endlocal
