call ..\..\..\..\..\..\paths.cmd

set projectDir=%~dp0..\
set buildDir=%~dp0package-files\
set buildDirModLocation=%~dp0package-files\DesktopModules\DnnSharp\FaqMaster\
set packageName=DnnSharp.FaqMaster_%1_Install

"%OwnToolsPath%xpath-update\bin\release\xpath-update.exe" --value="%1" --xpath="//packages/package" -attr="version" --file="DnnSharp.FaqMaster.dnn"
"%OwnToolsPath%xpath-update\bin\release\xpath-update.exe" --value="%1" --xpath="//assemblies/assembly[1]/version" --file="DnnSharp.FaqMaster.dnn"
"%OwnToolsPath%xpath-update\bin\release\xpath-update.exe" --value="%1" --xpath="//assemblies/assembly[2]/version" --file="DnnSharp.FaqMaster.dnn"
"%OwnToolsPath%xpath-update\bin\release\xpath-update.exe" --value="%1" --xpath="//scripts/script[@type='Install']/version" --file="DnnSharp.FaqMaster.dnn"
"%OwnToolsPath%xpath-update\bin\release\xpath-update.exe" --value="%1" --xpath="//scripts/script[@type='UnInstall']/version" --file="DnnSharp.FaqMaster.dnn"
"%OwnToolsPath%xpath-update\bin\release\xpath-update.exe" --value="%1" --xpath="//eventMessage/attributes/upgradeVersionsList" --file="DnnSharp.FaqMaster.dnn"
echo %1 > version.txt

xcopy "%projectDir%templates\*.*" "%buildDirModLocation%templates\*.*" /e /s /y /q
xcopy "%projectDir%static\*.*" "%buildDirModLocation%static\*.*" /e /s /y /q
xcopy "%projectDir%RegCore\res\*.*" "%buildDirModLocation%RegCore\res\*.*" /e /s /y /q
xcopy "%projectDir%RegCore\*.aspx" "%buildDirModLocation%RegCore\*.aspx" /e /s /y /q
xcopy "%projectDir%RegCore\*.ascx" "%buildDirModLocation%RegCore\*.ascx" /e /s /y /q
copy "%projectDir%bin\release\DnnSharp.FaqMaster.dll" "%buildDir%DnnSharp.FaqMaster.dll"
copy "%projectDir%bin\release\DnnSharp.FaqMaster.Core.dll" "%buildDir%DnnSharp.FaqMaster.Core.dll"
xcopy "%projectDir%DataProviders\*.*" "%buildDir%DataProviders\*.*" /e /s /y /q
xcopy "%projectDir%*.css" "%buildDirModLocation%*.*" /y /q
xcopy "%projectDir%*.ascx" "%buildDirModLocation%*.*" /y /q
xcopy "%projectDir%*.aspx" "%buildDirModLocation%*.*" /y /q
xcopy "%projectDir%*.ashx" "%buildDirModLocation%*.*" /y /q
xcopy "%projectDir%*.txt" "%buildDir%*.*" /y /q

del "%buildDir%todo.txt"
del "%buildDir%changelog.txt"

xcopy "%projectDir%*.html" "%buildDir%*.*" /y /q
xcopy "%projectDir%*.dnn" "%buildDir%*.*" /y /q
xcopy "%projectDir%App_LocalResources\*.resx" "%buildDirModLocation%App_LocalResources\*.*" /y /q

rem minify our resources
rem java -jar "%buildDir%..\yuicompressor-2.4.2.jar" "%buildDir%/static/admin.js" -o "%buildDir%/static/admin.js" -v --charset utf8
rem java -jar "%buildDir%..\yuicompressor-2.4.2.jar" "%buildDir%/static/admin.css" -o "%buildDir%/static/admin.css" -v --charset utf8


cd "%buildDirModLocation%"
for %%i in (*.as?x) do "%ToolsPath%UnixTools\sed.exe" -e "s/CodeFile[^=]*=/CodeBehind=/ig" "%%i" > "%%i-1" && move "%%i-1" "%%i"

cd "%buildDir%"

"%ToolsPath%infozip\zip.exe" -r -9 Resources.zip DesktopModules  >>..\log.txt
"%ToolsPath%infozip\zip.exe" -r -9 "..\%packageName%.zip"  Resources.zip DataProviders *.txt Resources.zip *.dnn *.txt *.html *.dll >>..\log.txt
cd..


"%ToolsPath%s3.exe" put "dl.dnnsharp.com/faq-master/" %packageName%.zip /key:%S3Key% /secret:%S3Secret%
move %packageName%.zip "%BuildsFolder%\FAQ Master\Builds\Dev\%packageName%.zip"

echo http://dl.dnnsharp.com/faq-master/%packageName%.zip
echo http://dl.dnnsharp.com/faq-master/%packageName%.zip | clip

