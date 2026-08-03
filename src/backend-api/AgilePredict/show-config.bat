@echo off
REM Script para mostrar o conteúdo dos arquivos de configuração

echo ==================================================
echo Conteúdo de appsettings.json
echo ==================================================
echo.
type AgilePredict\appsettings.json
echo.
echo.

echo ==================================================
echo Conteúdo de Program.cs (primeiras 100 linhas)
echo ==================================================
echo.
powershell -Command "Get-Content AgilePredict\Program.cs -TotalCount 100"
echo.
echo.

echo ==================================================
echo Verificação concluída!
echo ==================================================
pause
