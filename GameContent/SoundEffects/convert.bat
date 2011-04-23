pause
FOR %%v in (*.mp3) DO ffmpeg -i %%v  %%v.wav
pause