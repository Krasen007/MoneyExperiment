@echo off
git init --quiet
git add .
git commit -m "Create database"
echo *** Creating remote database ***
git remote add origin https://github.com/Krasen007/MoneyExperimentDB.git
git push -u origin master --quiet