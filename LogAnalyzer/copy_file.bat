@echo off
set src_file=generated_log_big.txt
set dst_file=generated_log_big_dst.txt

echo Src File: %src_file%
echo Dst File: %dst_file%

rem Засекаем время
set start=%time%
copy "%src_file%" "%dst_file%"
set end=%time%

rem Выводим время выполнения
echo Start Time: %start%
echo End Time: %end%
