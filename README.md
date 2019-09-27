# MoneyExperiment
 Experimental budgeting app.
 Sync a budget between PCs with the help of public Git repo.
 The trick is that your database of items is encrypted even if its in a public repo.
 ![Main view of the app.](Resources/MainView.png)


### How to Run:

* Install git as global app. (https://git-scm.com/downloads)
* If you want to build from source - Install dotnet core 3.0 (https://dotnet.microsoft.com/download/dotnet-core/3.0)
* Create empty folder and download and extract the latest version.
* Create new account and/or repository in github or gitlab.
* Open Scripts/InitCreateDB.bat and add your remote to match your repo, for example: git remote add origin https://github.com/Krasen007/MoneyExperimentDB.git
* If building from source open your folder with the app and open cmd/terminal and type "dotnet run".
* You are done! Enjoy your console .net core budgeting app.
* Optional: Repeat on any other PC you want to use the app and synchronize your settings.

### How to use:

* When the app start its going to ask you to set a password. Remember this. Its used to encrypt your database.
* After that its going to ask you to set a budget. Set the amount you wish to spent per month.
* The menu will appear. Use "y" to add entries. Enter what you spent for and how much did you spent.
* You can add new items and their balance will increase automatticly if you type the same item. Use -amount to decrease costs. 
* When you are done you can save your current account database on your PC, or you can upload it to your git repo.
* If you want to use the app on another PC, install git and run CloneDB.bat after you set your git repo in the file as explained above.
* It should automatically sync your database. Remember to use the same password or you will not be able to open the database.

### Credits:

* Main coding: Krasen Ivanov 2019
* Icon made by Freepik from www.flaticon.com
