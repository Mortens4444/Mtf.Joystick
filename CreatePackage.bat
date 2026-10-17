@echo off
setlocal enabledelayedexpansion

set "ProjectName=Mtf.Joystick"
set "TargetDir=C:\NuGetTest"

for %%F in ("%TargetDir%\%ProjectName%.*.nupkg") do (
    set "FileName=%%~nxF"
    if not "!FileName!" == "!FileName:.symbols=!" (
        echo Deleting: %%F
        del "%%F"
    )
)

for %%F in ("%TargetDir%\%ProjectName%.*.nupkg") do call :ProcessFile "%%F"
goto :UpdatePackages

:ProcessFile
set "FilePath=%~1"
set "FileName=%~nx1"

call set "RestPart=%%FileName:%ProjectName%.=%%"
echo !RestPart! | findstr /R "^[0-9][0-9]*\.[0-9][0-9]*\.[0-9][0-9]*" >nul

if !errorlevel! == 0 (
    echo Deleting: !FilePath!
    del "!FilePath!"
)

goto :eof

:UpdatePackages

for /R %%P in (*.csproj) do (
    echo Checking: %%~nxP
    pushd %%~dpP
    FOR /F "tokens=1,2,*" %%A IN ('dotnet list package --outdated --source "%TargetDir%"') DO (
        IF "%%A"==">" (
            set "PackageName=%%B"
            echo   Updating: !PackageName! in %%~nxP
            dotnet add package !PackageName! -s "%TargetDir%"
            IF ERRORLEVEL 1 (
                echo     Error: !PackageName! update failed.
            ) ELSE (
                echo     OK: !PackageName! update success.
            )
            echo.
        )
    )
    popd
)

REM Release only when the library itself changed; a dependency-only refresh is committed without a new version.
set "LibraryChanged="
for /f "delims=" %%L in ('git status --porcelain -- "%ProjectName%"') do set "LibraryChanged=1"
if defined LibraryChanged goto :Release

set "AnyChanged="
for /f "delims=" %%L in ('git status --porcelain') do set "AnyChanged=1"
if defined AnyChanged (
    git add -A
    git commit -m "Update package references"
    git push
)
echo No changes in %ProjectName%; skipping release.
goto :End

:Release
for /f %%V in ('powershell -ExecutionPolicy Bypass -File ".\IncrementPackageVersion.ps1" -CsprojFile "%ProjectName%\%ProjectName%.csproj"') do set "PackageVersion=%%V"
git add -A
git commit -m "%ProjectName% NuGet package release %PackageVersion%"
git push

dotnet pack %ProjectName%\%ProjectName%.csproj -c Release
move .\%ProjectName%\bin\Release\*.nupkg %TargetDir%

:End
REM pause