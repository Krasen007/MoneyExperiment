@echo off
git add .
git commit -m "Update database" --quiet
echo *** Updating database ***
git push --quiet