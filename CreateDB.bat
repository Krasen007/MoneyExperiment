@echo off
git init
git add .
git commit -m "Create database"
echo *** Creating database ***
git remote add origin https://github.com/Krasen007/MoneyExperimentDB.git
git push -u origin master