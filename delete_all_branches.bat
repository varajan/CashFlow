git checkout main
git branch > branches

for /f "delims=" %%x in (branches) do git branch -d %%x

git branch