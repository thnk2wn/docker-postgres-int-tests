set home=%1
if "%home%" == "" (
    goto usage
)

docker run --name postgres-server -d -p 5432:5432 -v %home%:/home postgres

REM HACK: Ideally loop and delay until we've identified postgres is ready; see also pg_isready
REM TIMEOUT and SLEEP didn't always work
ping 192.0.2.1 -n 1 -w 8000 >nul

docker exec postgres-server psql -U postgres -e -f home/script.sql

goto exit


:usage
echo home volume to mount must be passed in
exit /b -1

:exit