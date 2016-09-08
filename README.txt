//to add existing to git repo
//create new repo "Stravarage" on GitHub.com
//from the solution directory
//open git bash

$ git init

//create .gitignore file with this content

/***************************************/
packages/

bin/

obj/

Stravarage.v12.suo

Stravarage/Stravarage.csproj.user

Stravarage/tmp/*.xlsx

.vs/config/applicationhost.config

.vs/Stravarage/v14/.suo

/***********************************/

$ git add .gitignore
$ git commit -m "gitignore"
$ git remote add origin https://github.com/vortenberg/Stravarage.git
$ git push -u origin master
$ git add .
$ git commit -m "adding all files"
$ git push -u origin master