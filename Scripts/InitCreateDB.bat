@echo off
git init --quiet
git config user.name "Krasen Ivanov"
git config user.email "krasen007@gmail.com"
git add .
git commit -m "Create database"
git remote add origin https://github.com/Krasen007/MoneyExperimentDB.git
git push -u origin master --quiet
echo *** Creating remote database complete ***