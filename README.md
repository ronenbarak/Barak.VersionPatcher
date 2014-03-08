Barak.VersionPatcher
====================

Increment change project file version

Simple utility to upgrade the assembly version file of only the changed projects.
The application will checkin the changes after upgrade is completed 

Support
====================

SourceControl: 
* Only Team foundation server(Tested on TFS 2012)
* Git (Todo)

Sources: 
* CSharp code only.


Usage:
====================

Command line options:

    patch|p (Default): Increment the AssemblyVersion attribute if change is detected
    	/comment            : comment to check-in (String) (Default = VersionPathcer auto increment)
    	/fspath             : file system checkout root directory (String) (Required)
    	/projectfiles       : path to the project(.csproj) file, leave empty to search the folder for all solutions (String[])
    	/recursive          : recursivly change projects depends on updated projects (Default = True)
    	/revision           : revision id, default will be latest (String)
    	/vc /versioncontrol : version control type  (VersionControl (Automatic/TFS/Git)) (Default = Automatic)
    	/vcpath             : version control path: ($/Barak/Dev/Main) (String) (Required)
    	/vcurl              : Url for the version control server (String) (Required)
    	/versionpart        : what part of the version to incress (VersionPart (Major/Minor/Build/Revision)) (Default = Build)

Sample:

    Barak.VersionPatcher.exe /vcPath:$/Barak/Dev/Branch/VersionPatcherTests /vcUrl:http://phenomii1090:8085/tfs/tfsrepository/  /fsPath:D:\Temp\VersionPatcher2\VersionPatcherTests /recursive:tru

or Teamcity:

    Barak.VersionPatcher.exe /vcPath:%vcsroot.tfs-root% /vcUrl:%vcsroot.tfs-url%  /fsPath:%teamcity.build.checkoutDir% /revision:%build.vcs.number% /recursive:true

How does it work:
====================
The application checks for all the files that has been changed between previous execution and current execution,
it will find the project that referance this files and update those project assembly info files.
when recursive paramter is set to ture, the version of all project that depends on the change will be change as well.

Note:
====================
We need help, use the application and report bugs(or better yet fix them). 




